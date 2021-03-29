using System.Collections.Generic;

namespace CoreUtility
{
    public interface IExcelProcessor
    {
        IEnumerable<T> Process<T>(byte[] excelFileContent,
                            IEnumerable<KeyValuePair<string, string>> cellPropertyMap);
    }
}
