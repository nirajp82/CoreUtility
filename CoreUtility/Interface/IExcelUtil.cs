using System.Collections.Generic;

namespace CoreUtility
{
    public interface IExcelUtil
    {
        IEnumerable<T> ConvertXlsToEntityList<T>(byte[] excelFileContent, 
                IEnumerable<KeyValuePair<string, string>> xlsColumnAndPropertyMap);
    }
}