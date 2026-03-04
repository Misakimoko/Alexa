using Microsoft.EntityFrameworkCore;
using SqlSugar;
using System.Data;

namespace Lexa.Data.MetaData
{
    [RegisterScoped]
    public class MetaService(ApplicationDbContext _context, ISqlSugarClient _client)
    {

        //使用SQL sugar查询数据库lexa中的所有表和列信息
        public async Task<List<SystemTableInfo>> GetAllTableInfoAsync()
        {
            //var tables = await _client.Queryable<DbTableInfo>().ToListAsync();
            var tables =  _client.DbMaintenance.GetTableInfoList();
            
            var tableInfos = tables.Select(table => new SystemTableInfo
            {
                TableName = table.Name,
                TableComment = table.Description,
                Columns =  _client.DbMaintenance.GetColumnInfosByTableName(table.Name).Select(column => new SystemColumnInfo
                {
                    ColumnName = column.DbColumnName,
                    DataType = column.DataType,
                    IsNullable = column.IsNullable,
                    ColumnComment = column.ColumnDescription,
                    CharacterMaximumLength = column.Length,
                    IsPrimaryKey = column.IsPrimarykey,
                    IsIdentity = column.IsIdentity
                }).ToList()
            }).ToList();
            return tableInfos;
        }


        //根据表信息和列信息修改表的属性



        //删除表



        //新增数据表


        //新增列


    }
}
