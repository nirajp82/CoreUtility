using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreUtility
{
    public class ExcelUtil : IExcelUtil
    {
        #region Members
        private readonly ILogger<ExcelUtil> _logger;
        #endregion


        #region Constructor
        public ExcelUtil(ILogger<ExcelUtil> logger)
        {
            _logger = logger;
        }
        #endregion


        #region Public Methods
        public IEnumerable<T> ConvertXlsToEntityList<T>(byte[] excelFileContent,
                    IEnumerable<KeyValuePair<string, string>> xlsColumnAndPropertyMap)
        {
            ExcelWorksheet excelWorksheet = GetXslWorksheet(excelFileContent);
            return ConvertSheetToEntityList<T>(excelWorksheet, xlsColumnAndPropertyMap);
        }
        #endregion


        #region Private Methods
        private ExcelWorksheet GetXslWorksheet(byte[] fileContent)
        {
            if (fileContent != null && fileContent.Any())
            {
                ExcelPackage excelPackage = null;
                using (Stream stream = new MemoryStream(fileContent))
                {
                    excelPackage = new ExcelPackage(stream);
                }
                if (excelPackage != null && excelPackage.Workbook != null &&
                    excelPackage.Workbook.Worksheets != null && excelPackage.Workbook.Worksheets.Any())
                {
                    return excelPackage.Workbook.Worksheets.FirstOrDefault();
                }
            }
            return null;
        }

        private IEnumerable<T> ConvertSheetToEntityList<T>(ExcelWorksheet xlsWorksheet,
            IEnumerable<KeyValuePair<string, string>> xlsColumnAndPropertyMap)
        {
            IEnumerable<KeyValuePair<int, string>> colCntAndPropMap = GetColumnMapping(xlsWorksheet, xlsColumnAndPropertyMap);
            if (colCntAndPropMap != null)
            {
                Type type = typeof(T);
                ICollection<T> list = new List<T>();
                for (var rowNumber = 2; rowNumber <= xlsWorksheet.Dimension.End.Row; rowNumber++)
                {
                    T objModel = ConvertRowToEntity<T>(xlsWorksheet, colCntAndPropMap, rowNumber);
                    if (objModel != null)
                    {
                        list.Add(objModel);
                    }
                }
                return list;
            }
            return null;
        }

        private IEnumerable<KeyValuePair<int, string>> GetColumnMapping(ExcelWorksheet xlsWorksheet,
                                            IEnumerable<KeyValuePair<string, string>> xlsColumnAndPropertyMap)
        {
            if (xlsWorksheet != null && xlsWorksheet.Dimension != null && xlsWorksheet.Dimension.End != null &&
                        xlsWorksheet.Dimension.End.Column > 0 && xlsWorksheet.Dimension.End.Row > 0 &&
                        xlsColumnAndPropertyMap != null && xlsColumnAndPropertyMap.Any())
            {
                IList<KeyValuePair<int, string>> colIndexAndPropMap = new List<KeyValuePair<int, string>>();
                //Use For Loop instead of ForEach to avoid NULL RowHeadler.Text issue. 
                for (int colIndex = 1; colIndex <= xlsWorksheet.Dimension.End.Column; colIndex++)
                {
                    var xlsCell = xlsWorksheet.Cells[1, colIndex].FirstOrDefault();
                    if (xlsCell != null)
                    {
                        string columnHeader = xlsCell.Text;
                        if (!string.IsNullOrWhiteSpace(columnHeader) &&
                            xlsColumnAndPropertyMap.Any(m => HelperFunc.DoesStringMatch(m.Key, columnHeader)) &&
                            !colIndexAndPropMap.Select(c => c.Value).Contains(columnHeader))
                        {
                            string propName = xlsColumnAndPropertyMap
                                                .Where(m => HelperFunc.DoesStringMatch(m.Key, columnHeader))
                                                .Select(m => m.Value).First();
                            colIndexAndPropMap.Add(new KeyValuePair<int, string>(colIndex, propName));
                        }
                    }
                }
                if (colIndexAndPropMap.Count == xlsColumnAndPropertyMap.Count())
                {
                    return colIndexAndPropMap;
                }
                else
                {
                    IEnumerable<string> missingColumns = xlsColumnAndPropertyMap
                           .Where(e => !colIndexAndPropMap.Select(c => c.Value).Contains(e.Value))
                           .Select(e => e.Value);

                    _logger.LogError($"Following required columns are missing:{string.Join(", ", missingColumns)}");
                    throw new Exception("Following required columns are missing:" + string.Join(", ", missingColumns));
                }
            }
            return null;
        }

        private T ConvertRowToEntity<T>(ExcelWorksheet xlsWorksheet,
                IEnumerable<KeyValuePair<int, string>> colCntAndPropMap, int rowNumber)
        {
            try
            {
                T objModel = (T)Activator.CreateInstance(typeof(T));
                foreach (var item in colCntAndPropMap)
                {
                    string value = (xlsWorksheet.Cells[rowNumber, item.Key]).Text;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        HelperFunc.SetValue(objModel, item.Value, value);
                    }
                }
                return objModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ConvertRowToEntity:{typeof(T).Name} RowNum:{rowNumber}");
            }
            return default;
        }
        #endregion
    }
}
