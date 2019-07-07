using System.Collections.Generic;
using System.Linq;

namespace CodeGeneration
{
    public class Table
    {
        public string Base { get; set; }

        public List<string> HttpOps { get; set; }

        public string Description { get; set; }

        public int CacheDuration { get; set; }

        public bool? IsBase { get; set; }

        public string Name { get; set; }

        public string DatabaseGeneratedOption { get; set; }

        public string DisplayName { get; set; }

        public string PrimaryKeyDisplayName { get; set; }
        public string PrimaryKeyName { get; set; }

        public bool? JunctionTable { get; set; }

        public List<Column> Columns { get; set; }

        public List<Column> GetColumns() => Columns;

        public List<Column> GetAllColumns()
        {
            var ret = new List<Column>();
            ret.AddRange(GetColumns());
            return ret.Where(p => null != p).OrderBy(p => p.Name).ToList();
        }
    }
}
