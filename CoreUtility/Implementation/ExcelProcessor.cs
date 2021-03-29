using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreUtility
{
    internal class ExcelProcessor : IExcelProcessor
    {
        #region Members
        private readonly ILogger<ExcelProcessor> _logger;
        #endregion


        #region Constructor
        public ExcelProcessor(ILogger<ExcelProcessor> logger)
        {
            _logger = logger;
        }
        #endregion


        #region Public Methods
        public IEnumerable<T> Process<T>(byte[] excelFileContent, IEnumerable<KeyValuePair<string, string>> cellPropertyMap)
        {
            ExcelWorksheet excelWorksheet = GetFirstExcelWorksheet(excelFileContent);
            return ConvertSheetToEntityList<T>(excelWorksheet, cellPropertyMap);
        }
        #endregion


        #region Private Methods
        private ExcelWorksheet GetFirstExcelWorksheet(byte[] fileContent)
        {
            if (fileContent?.Any() == true)
            {
                using Stream stream = new MemoryStream(fileContent);
                ExcelPackage excelPackage = new ExcelPackage(stream);
                if (excelPackage?.Workbook?.Worksheets?.Any() == true)
                    return excelPackage.Workbook.Worksheets.FirstOrDefault();
            }
            return null;
        }

        private IEnumerable<T> ConvertSheetToEntityList<T>(ExcelWorksheet xlsWorksheet,
                    IEnumerable<KeyValuePair<string, string>> columnPropertyMap)
        {
            IEnumerable<KeyValuePair<int, string>> colCntAndPropMap = GetColumnMapping(xlsWorksheet, columnPropertyMap);
            if (colCntAndPropMap != null)
            {
                ICollection<T> list = new List<T>();
                for (var rowNumber = 2; rowNumber <= xlsWorksheet.Dimension.End.Row; rowNumber++)
                {
                    T objModel = ConvertRowToEntity<T>(xlsWorksheet, colCntAndPropMap, rowNumber);
                    if (objModel != null)
                        list.Add(objModel);
                }
                return list;
            }
            return null;
        }

        private IEnumerable<KeyValuePair<int, string>> GetColumnMapping(ExcelWorksheet xlsWorksheet,
                                            IEnumerable<KeyValuePair<string, string>> columnPropertyMap)
        {
            if (xlsWorksheet != null && xlsWorksheet.Dimension != null && xlsWorksheet.Dimension.End != null &&
                        xlsWorksheet.Dimension.End.Column > 0 && xlsWorksheet.Dimension.End.Row > 0 &&
                        columnPropertyMap != null && columnPropertyMap.Any())
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
                            columnPropertyMap.Any(m => HelperFunc.IsEqualString(m.Key, columnHeader)) &&
                            !colIndexAndPropMap.Select(c => c.Value).Contains(columnHeader))
                        {
                            string propName = columnPropertyMap
                                                .Where(m => HelperFunc.IsEqualString(m.Key, columnHeader))
                                                .Select(m => m.Value).First();
                            colIndexAndPropMap.Add(new KeyValuePair<int, string>(colIndex, propName));
                        }
                    }
                }
                if (colIndexAndPropMap.Count == columnPropertyMap.Count())
                    return colIndexAndPropMap;
                else
                {
                    IEnumerable<string> missingColumns = columnPropertyMap
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
                        HelperFunc.SetValue(objModel, item.Value, value);
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
