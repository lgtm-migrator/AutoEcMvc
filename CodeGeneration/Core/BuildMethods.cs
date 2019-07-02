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
        public static string GetTabs(int tabs)
        {
            var i = 0;
            var pad = "";
            while (i < tabs)
            {
                i += 1;
                pad += "    ";
            }
            return pad;
        }

        #region File Methods

        private static List<Table> _tables;

        public static List<Table> GetJsonFilesAsTables(string path)
        {
            if (true == _tables?.Any()) return _tables;

            _tables = new List<Table>();
            var settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
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

        private static SortedDictionary<string, SortedDictionary<string, string>> _enums;

        public static SortedDictionary<string, SortedDictionary<string, string>> GetEnumFilesAsDictionaries(string path)
        {
            if (true == _enums?.Any()) return _enums;

            _enums = new SortedDictionary<string, SortedDictionary<string, string>>();
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
                            _enums[f.Name.Replace(".json", "")] =
                                JsonConvert.DeserializeObject<SortedDictionary<string, string>>(reader.ReadToEnd(),
                                    settings);
                            reader.Close();
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

            return _enums;
        }

        #endregion File Methods

        public static string GetModelProperty(Column column)
        {
            var ret = new List<string>();
            var attributes = string.Empty;
            var type = column.Type;
            switch (column.Type)
            {
                case "byte[]":
                    if (true == column.Timestamp) attributes = $"[Timestamp]";
                    break;
                case "DateTime":
                    var dateTimeAttributes = new List<string>();
                    if (!string.IsNullOrEmpty(column.DataType)) dateTimeAttributes.Add($"[DataType(DataType.{column.DataType})]");
                    if (!string.IsNullOrEmpty(column.DataFormatString) || true == column.ApplyFormatInEditMode)
                    {
                        var attr = "[DisplayFormat(";
                        if (!string.IsNullOrEmpty(column.DataFormatString)) attr += $"DataFormatString = \"{column.DataFormatString}\"";
                        if (true == column.ApplyFormatInEditMode) attr += ", ApplyFormatInEditMode = true";
                        attr += ")]";
                        dateTimeAttributes.Add(attr);
                    }
                    if (dateTimeAttributes.Any()) attributes = string.Join(Environment.NewLine + GetTabs(2), dateTimeAttributes);
                    break;
                case "decimal":
                    var decimalAttributes = new List<string>();
                    if (!string.IsNullOrEmpty(column.DataType)) decimalAttributes.Add($"[DataType(DataType.{column.DataType})]");
                    if (!string.IsNullOrEmpty(column.ColumnTypeName)) decimalAttributes.Add($"[Column(TypeName = \"{column.ColumnTypeName}\")]");
                    if (decimalAttributes.Any()) attributes = string.Join(Environment.NewLine + GetTabs(2), decimalAttributes);
                    break;
                case "enum":
                    type = column.Enum;
                    break;
                case "enum?":
                    type = $"{column.Enum}?";
                    break;
                case "int":
                    if (!string.IsNullOrEmpty(column.Range)) attributes = $"[Range({column.Range})]";
                    break;
                case "string":
                    if (column.Length > 0) attributes = $"[StringLength({column.Length}";
                    if (column.MinimumLength > 0) attributes += $", MinimumLength = {column.MinimumLength}";
                    if (attributes.Length > 0) attributes += ")]";
                    break;
                case "relationship":
                    var idColumnName = $"{column.Name}ID";
                    if (!string.IsNullOrEmpty(column.TargetIdName))
                    {
                        idColumnName = column.TargetIdName;
                    }

                    var nullable = "";
                    if (false == column.IsRequired)
                    {
                        nullable = "?";
                    }
                    attributes = $"public int{nullable} {idColumnName} {{ get; set; }}";
                    type = column.Target;
                    break;
                case "relationships":
                    type = $"ICollection<{column.Target}>";
                    break;
            }

            if (!string.IsNullOrEmpty(attributes))
            {
                ret.Add(GetTabs(2) + attributes);
            }

            if (!string.IsNullOrEmpty(column.DisplayName))
            {
                ret.Add(GetTabs(2) + $"[Display(Name = \"{column.DisplayName}\")]");
            }
            ret.Add(GetTabs(2) + $"public {type} {column.Name} {{ get; set; }}");
            return string.Join(Environment.NewLine, ret);
        }
    }
}