using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeGeneration
{
    public static class BuildMethods
    {
        #region Utility Methods

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

        public static string GetPrimaryKeyName(Table thisTable, List<Table> allTables)
        {
            var ret = (!string.IsNullOrEmpty(thisTable.PrimaryKeyName))
                ? thisTable.PrimaryKeyName
                : $"{thisTable.Name}ID";

            if (string.IsNullOrEmpty(thisTable.Base)) return ret;

            var baseTable = allTables?.FirstOrDefault(t => t.Name == thisTable.Base);
            ret = GetPrimaryKeyName(baseTable, null);
            return ret;
        }

        public static string GetColumnName(Column column)
        {
            var ret = string.Empty;
            if (!string.IsNullOrEmpty(column.TargetIdName))
            {
                ret = column.TargetIdName;
            }
            else
            {
                ret = column.Name;
            }
            return ret;
        }

        public static string GetDisplayName(Table thisTable, List<Table> allTables)
        {
            var ret = string.Empty;
            if (!string.IsNullOrEmpty(thisTable.DisplayName))
            {
                return thisTable.DisplayName;
            }

            if (string.IsNullOrEmpty(thisTable.Base)) return thisTable.Columns.FirstOrDefault(c => c.Type == "string")?.Name ?? thisTable.Columns.FirstOrDefault()?.Name;

            var baseTable = allTables?.FirstOrDefault(t => t.Name == thisTable.Base);
            ret = GetDisplayName(baseTable, null);
            return ret;
        }

        #endregion Utlity Methods

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
                        using (var reader = new StreamReader(f.FullName, true))
                        {
                            var table = JsonConvert.DeserializeObject<Table>(reader.ReadToEnd(), settings);
                            reader.Close();
                            _tables.Add(table);
                        }
                    }
                    catch { }
                }
            }
            catch { }
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
                        using (var reader = new StreamReader(f.FullName, true))
                        {
                            _enums[f.Name.Replace(".json", "")] =
                                JsonConvert.DeserializeObject<SortedDictionary<string, string>>(reader.ReadToEnd(),
                                    settings);
                            reader.Close();
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return _enums;
        }

        #endregion File Methods

        #region Controller Methods

        public static string GetIncludeColumns(Table thisTable, List<Table> allTables)
        {
            var ret = new List<string> { };
            foreach (var col in thisTable.Columns.Where(c => c.MaterializeByDefault))
            {
                ret.Add($"{GetTabs(4)}.Include(c => c.{col.Name})");
                if (col.Type.StartsWith("relationship"))
                {
                    var targetTable = allTables?.FirstOrDefault(t => t.Name == col.Target);
                    if (true != targetTable?.Columns?.Where(c => c.MaterializeByDefault)?.Any()) continue;

                    foreach (var subCol in targetTable?.Columns?.Where(c => c.MaterializeByDefault))
                    {
                        if (subCol.Type.StartsWith("relationship"))
                        {
                            var subTargetTable = allTables?.FirstOrDefault(t => t.Name == subCol.Target);
                            if (true != subTargetTable?.Columns?.Where(c => c.MaterializeByDefault)?.Any()) continue;

                            foreach (var subSubCol in subTargetTable.Columns.Where(c => c.MaterializeByDefault))
                            {
                                ret.Add($"{GetTabs(4)}.Include(c => c.{col.Name}).ThenInclude(c => c.{subCol.Name}).ThenInclude(c => c.{subSubCol.Name})");
                            }
                        }
                        else
                        {
                            ret.Add($"{GetTabs(4)}.Include(c => c.{col.Name}).ThenInclude(c => c.{subCol.Name})");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(thisTable.OrderBy))
            {
                ret.Add($"{GetTabs(4)}.OrderBy(c => c.{thisTable.OrderBy})");
            }
            else if (!string.IsNullOrEmpty(thisTable.Base))
            {
                var baseTable = allTables?.FirstOrDefault(t => t.Name == thisTable.Base);
                if (!string.IsNullOrEmpty(baseTable?.OrderBy))
                {
                    ret.Add($"{GetTabs(4)}.OrderBy(c => c.{baseTable.OrderBy})");
                }
            }
            return string.Join(Environment.NewLine, ret);
        }

        public static string GetBindingColumns(Table thisTable, List<Table> allTables)
        {
            if (null == thisTable) return string.Empty;

            var values = new List<string> { GetPrimaryKeyName(thisTable, allTables) };
            foreach (var col in thisTable.Columns.Where(c => c.Type != "relationships"))
            {
                values.Add(GetColumnName(col));
                if (col.Type.StartsWith("relationship") && string.IsNullOrEmpty(col.TargetIdName))
                {
                    values.Add($"{col.Target}ID");
                }
            }
            var ret = string.Join(",", values.Distinct());
            if (!string.IsNullOrEmpty(thisTable.Base))
            {
                var baseTable = allTables?.FirstOrDefault(t => t.Name == thisTable.Base);
                var baseProps = GetBindingColumns(baseTable, null);
                if (!string.IsNullOrEmpty(baseProps))
                {
                    ret += $",{baseProps}";
                }
            }
            return ret;
        }

        public static string GetAssignColumns(Table thisTable, List<Table> allTables)
        {
            var ret = new List<string> { };
            foreach (var col in thisTable.Columns.Where(c => c.Type != "relationships" && c.Type != "version"))
            {
                ret.Add($"s => s.{GetColumnName(col)}");
            }
            return string.Join(",", ret);
        }

        public static string GetValidateColumns(Table thisTable, List<Table> allTables)
        {
            var ret = new List<string> { };
            foreach (var column in thisTable.Columns.Where(c => c.Type != "relationships" && c.Type != "version"))
            {
                var name = GetColumnName(column);
                ret.Add($"{GetTabs(6)}if (databaseValues.{name} != clientValues.{name})");
                ret.Add($"{GetTabs(6)}{{");
                if (column.Type.StartsWith("relationship"))
                {
                    var targetTable = allTables?.FirstOrDefault(t => t.Name == column.Target);
                    ret.Add($"{GetTabs(7)}var en = await _context.{column.Target}s.FirstOrDefaultAsync(i => i.{GetPrimaryKeyName(targetTable, allTables)} == databaseValues.{GetColumnName(column)});");
                    ret.Add($"{GetTabs(7)}ModelState.AddModelError(\"{GetColumnName(column)}\", $\"Current value: {{ en.{GetDisplayName(targetTable, allTables)}}}\");");
                }
                else
                {
                    var formatter = string.Empty;
                    if (column.Type.StartsWith("decimal"))
                    {
                        formatter = ":c";
                    }

                    if (column.Type.StartsWith("DateTime"))
                    {
                        formatter = ":d";
                    }
                    ret.Add($"{GetTabs(7)}ModelState.AddModelError(\"{GetColumnName(column)}\", $\"Current value: {{ databaseValues.{name}{formatter}}}\");");
                }

                ret.Add($"{GetTabs(6)}}}");
            }
            return string.Join(Environment.NewLine, ret);
        }

        public static string GetCreateBinding(Column column, Table thisTable, List<Table> allTables)
        {
            var ret = string.Empty;

            switch (column.Type)
            {
                case "relationship":
                case "relationship?":
                    var targetTable = allTables?.FirstOrDefault(t => t.Name == column.Target);
                    ret = $"ViewData[\"{GetColumnName(column)}\"] = new SelectList(_context.{column.Target}s, \"{GetPrimaryKeyName(targetTable, allTables)}\", \"{GetDisplayName(targetTable, allTables)}\");";
                    break;

                case "relationships":

                    break;
            }
            return $"{GetTabs(3)}{ret}";
        }

        public static string GetAfterUpdateBinding(Table thisTable, List<Table> allTables)
        {
            var ret = new List<string>();
            foreach (var column in thisTable.Columns.Where(c => true == c.MaterializeByDefault))
            {
                var viewData = string.Empty;

                switch (column.Type)
                {
                    case "relationship":
                    case "relationship?":
                        var targetTable = allTables?.FirstOrDefault(t => t.Name == column.Target);
                        var targetIdName = GetColumnName(column);
                        viewData =
                            $"ViewData[\"{targetIdName}\"] = new SelectList(_context.{column.Target}s, \"{GetPrimaryKeyName(targetTable, allTables)}\", \"{GetDisplayName(targetTable, allTables)}\", obj.{targetIdName});";
                        break;

                    case "relationships":

                        break;
                }
                ret.Add($"{GetTabs(3)}{viewData}");
            }
            return string.Join(Environment.NewLine, ret);
        }

        #endregion Controller Methods

        #region Model Methods

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
                    if (decimalAttributes.Any()) attributes = string.Join(Environment.NewLine + GetTabs(2), decimalAttributes);
                    break;
                case "enum":
                    type = column.Enum;
                    break;
                case "enum?":
                    if (!string.IsNullOrEmpty(column.NullDisplayText))
                    {
                        attributes = $"[DisplayFormat(NullDisplayText = \"{column.NullDisplayText}\")]";
                    }
                    type = $"{column.Enum}?";
                    break;
                case "int":
                    if (!string.IsNullOrEmpty(column.Range)) attributes = $"[Range({column.Range})]";
                    break;
                case "string":
                    if (column.Length > 0) attributes = $"[StringLength({column.Length}";
                    if (column.MinimumLength > 0) attributes += $", MinimumLength = {column.MinimumLength}";
                    if (!string.IsNullOrEmpty(column.ErrorMessage)) attributes += $", ErrorMessage = \"{column.ErrorMessage}\"";
                    if (attributes.Length > 0) attributes += ")]";
                    break;
                case "relationship":
                case "relationship?":
                    var idColumnName = $"{column.Name}ID";
                    if (!string.IsNullOrEmpty(column.TargetIdName))
                    {
                        idColumnName = column.TargetIdName;
                    }

                    var nullable = "";
                    if (column.Type == "relationship?")
                    {
                        nullable = "?";
                    }

                    if (true == column.IsKey)
                    {
                        attributes = $"[Key]{Environment.NewLine}{GetTabs(2)}";
                    }
                    attributes += $"public int{nullable} {idColumnName} {{ get; set; }}";
                    type = column.Target;
                    break;

                case "relationships":
                    type = $"ICollection<{column.Target}>";
                    break;
                case "version":
                    type = "byte[]";
                    attributes = "[Timestamp]";
                    break;
            }

            if (!string.IsNullOrEmpty(column.ColumnTypeName) || !string.IsNullOrEmpty(column.ColumnName))
            {
                var col = "[Column(";
                if (!string.IsNullOrEmpty(column.ColumnName))
                {
                    col += $"\"{column.ColumnName}\"";
                    if (!string.IsNullOrEmpty(column.ColumnTypeName)) col += ", ";
                }
                if (!string.IsNullOrEmpty(column.ColumnTypeName))
                {
                    col += $"TypeName = \"{column.ColumnTypeName}\"";
                }
                col += ")]";
                attributes += $"{Environment.NewLine}{GetTabs(2)}{col}";
            }

            if (true == column.IsRequired)
            {
                ret.Add(GetTabs(2) + "[Required]");
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

        #endregion Model Methods
    }
}