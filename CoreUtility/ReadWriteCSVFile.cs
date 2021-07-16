
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
