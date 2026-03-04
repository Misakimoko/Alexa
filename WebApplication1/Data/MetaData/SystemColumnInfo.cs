namespace Lexa.Data.MetaData
{


    // Entities/SystemColumnInfo.cs
    public class SystemColumnInfo
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int? CharacterMaximumLength { get; set; }
        public bool IsNullable { get; set; }
        public string ColumnComment { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
    }
}
