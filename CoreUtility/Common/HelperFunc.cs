﻿using System;
using System.Reflection;

namespace CoreUtility
{
    public class HelperFunc
    {
        public static string TrimString(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? value.Trim() : value;
        }

        public static bool IsEqualString(string string1, string string2)
        {
            if (string1 == default && string2 == default)
                return true;
            if (string1 == string.Empty && string2 == string.Empty)
                return true;
            if (!string.IsNullOrWhiteSpace(string1) && !string.IsNullOrWhiteSpace(string2))
                return string.Compare(TrimString(string1), TrimString(string2), true) == 0;
            return false;
        }

        public static void SetValue(object objModel, string propertyName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && objModel != null && value != Double.NaN.ToString())
            {
                var propertyInfo = objModel.GetType().GetProperty(propertyName, BindingFlags.GetProperty |
                                                              BindingFlags.IgnoreCase |
                                                              BindingFlags.Instance |
                                                              BindingFlags.Public);
                if (propertyInfo != null)
                {
                    Type toType = propertyInfo.PropertyType;

                    if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        toType = Nullable.GetUnderlyingType(toType);

                    if (toType == typeof(bool) && !string.IsNullOrWhiteSpace(value))
                    {
                        if (value.ToUpper() == "Y" || value == "1")
                            value = true.ToString();
                        else if (value.ToUpper() == "N" || value == "0")
                            value = false.ToString();
                    }

                    propertyInfo.SetValue(objModel, Convert.ChangeType(value, toType));
                }
            }
            
       public static string[] CSVParser(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var parser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            var fields = parser.Split(value);
            return fields;
            //return value.Split(new[] { separator }, StringSplitOptions.None);
        }
            
       static async Task ReadLargeFiles(string inputfile, string destFile)
        {
            try
            {
                using (var writer = new StreamWriter(destFile))
                {
                    using (FileStream fs = File.Open(inputfile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (BufferedStream bs = new BufferedStream(fs))
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string line;
                        var cnt = 0;
                        while ((line = await sr.ReadLineAsync()) != null)
                        {
                            writer.WriteLine(line);
                            cnt++;
                            if (cnt > 10000)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
            
             /// <summary>
        /// string createdTimePropName = Util.GetMemberName((BaseDataModel m) => m.CreatedDate);
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetMemberName<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression)
        {
            if (expression.Body is MemberExpression member && member.Member != null)
            {
                return member.Member.Name;
            }
            throw new ArgumentException("Invalid argument");
        }
        }
    }
}
