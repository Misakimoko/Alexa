using FastEndpoints;
using Lexa.Data.MetaData;
using Lexa.Services;
using SqlSugar;

namespace Lexa.Endpoints
{
    public class GenarateCodeEp(CodeCreateService codeCreateService,ISqlSugarClient _client):EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/api/genaratecode");
            AllowAnonymous();
        }
        public override async Task HandleAsync(CancellationToken ct)
        {

            var tables = _client.DbMaintenance.GetTableInfoList();

            var tableInfos = tables.Select(table => new SystemTableInfo
            {
                TableName = table.Name,
                TableComment = table.Description,
                Columns = _client.DbMaintenance.GetColumnInfosByTableName(table.Name).Select(column => new SystemColumnInfo
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
            codeCreateService.EntitiesGenerator(tableInfos);
            codeCreateService.DbContextGenerator(tableInfos);


            await SendAsync("代码生成成功");
        }
    }
}
