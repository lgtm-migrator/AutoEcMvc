using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CodeGeneration
{
    public static class BuildMethods
    {
        private static List<Table> _tables;

        public static List<Table> GetJsonFilesAsTables(string path)
        {
            if (true == _tables?.Any()) return _tables;

            _tables = new List<Table>();
            var settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include
            };
            try
            {
                var dir = new DirectoryInfo(path);

                foreach (var f in dir.GetFiles("*.json"))
                {
                    try
                    {
                        Trace.WriteLine("Starting parse of " + f.Name);
                        using (var reader = new StreamReader(f.FullName, true))
                        {
                            var table = JsonConvert.DeserializeObject<Table>(reader.ReadToEnd(), settings);
                            reader.Close();
                            _tables.Add(table);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return _tables;
        }
    }
}