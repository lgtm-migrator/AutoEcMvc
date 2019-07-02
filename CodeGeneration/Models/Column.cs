namespace CodeGeneration
{
    public class Column
    {
        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public string DisplayName { get; set; }

        public string Enum { get; set; }

        public bool? IsAudited { get; set; }

        public bool? IsCompressed { get; set; }

        public bool? IsIndexed { get; set; }

        public bool? IsInsertOnly { get; set; }

        public bool? IsJSON { get; set; }

        public bool? IsReadOnly { get; set; }

        public bool? IsRelationship { get; set; }

        public bool? IsRequired { get; set; }

        public bool? IsUnique { get; set; }

        public string JsonType { get; set; }
        public int? Length { get; set; }
        public int? MinimumLength { get; set; }
        public string Name { get; set; }

        public int Order { get; set; }

        public int? Precision { get; set; }

        public string Range { get; set; }

        public string RelationshipOnOwnerRemove { get; set; }

        public string RelationshipOnTargetRemove { get; set; }

        public string RelationshipPairTo { get; set; }

        public int? Scale { get; set; }
        public string Target { get; set; }
        public string TargetAlias { get; set; }

        public string Type { get; set; }
    }
}
