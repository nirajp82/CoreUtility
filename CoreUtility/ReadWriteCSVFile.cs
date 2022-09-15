
//Reference: https://cc.davelozinski.com/c-sharp/fastest-way-to-read-text-files

static async Task AddUniqueNumColumn(string headerFilePath)
        {
            using (var writer = new StreamWriter(headerFilePath.Replace(".csv","_temp.csv")))
            {
                using (var fs = File.OpenRead(headerFilePath))
                using (var bs = new BufferedStream(fs))
                using (var sr = new StreamReader(bs))
                {
                    string line;
                    var rowNum = 0;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        //we're just testing read speeds    
                        line = rowNum == 0 ? $"{line},UniqueNumber" : $",{rowNum}";
                        await writer.WriteLineAsync(line);
                        rowNum++;
                    }
                }
            }
        }
 static void Upload(string csvFile, string tableName, DataTable dt)
        {
            string csvData = File.ReadAllText(csvFile);
            foreach (string row in csvData.Split('\n'))
            {
                if (!string.IsNullOrEmpty(row))
                {
                    dt.Rows.Add();
                    int i = 0;
                    try
                    {
                        foreach (string cell in CsvParser(row))
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell;
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }

            using (SqlConnection con = new SqlConnection(_CONN_STRING))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                {
                    sqlBulkCopy.DestinationTableName = tableName;
                    con.Open();
                    sqlBulkCopy.WriteToServer(dt);
                    con.Close();
                }
            }
        }

        static string[] CsvParser(string csvText)
        {
            List<string> tokens = new List<string>();

            int last = -1;
            int current = 0;
            bool inText = false;

            while (current < csvText.Length)
            {
                switch (csvText[current])
                {
                    case '"':
                        inText = !inText; break;
                    case ',':
                        if (!inText)
                        {
                            tokens.Add(csvText.Substring(last + 1, (current - last)).Trim(' ', ','));
                            last = current;
                        }
                        break;
                    default:
                        break;
                }
                current++;
            }

            if (last != csvText.Length - 1)
            {
                tokens.Add(csvText.Substring(last + 1).Trim());
            }

            return tokens.ToArray();
        }

        static DataTable GetMasterSchema()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[3] {
                new DataColumn("Id",typeof(string)),
                new DataColumn("Key",typeof(string)),
                new DataColumn("Data",typeof(string))
                      });
            return dt;
        }
