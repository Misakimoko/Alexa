namespace WebApplication1.Data.MetaData
{

    public class SystemTableInfo
    {
        public string TableName { get; set; }
        public string TableComment { get; set; }
        public DateTime CreateTime { get; set; }
        public List<SystemColumnInfo> Columns { get; set; } = new();
    }


}
