using Lexa.Data.MetaData;
using System.Text;

namespace Lexa.Services
{
    [RegisterScoped]
    public class CodeCreateService
    {
        public ClassNameOptimizer _classNameOptimizer = new ClassNameOptimizer();
        public string _contextNamespace = "namespace Lexa.Data;";
        public string _entityNamespace = "namespace Lexa.Entities;";
        public string _entityOutputPath = @"E:\Codes\test1\Entities";
        public string _contextOutputPath = @"E:\Codes\test1\Data";
        public string _endpointOutputPath = @"E:\Codes\test1\Endpoints";
        public string _endpointnamespace = "namespace Lexa.Endpoints;";

        public void EntitiesGenerator(List<SystemTableInfo> tableInfos)
        {
            //根据tableInfos生成实体类代码，并保存到_entityOutputPath路径下
            foreach (var tableInfo in tableInfos)
            {
                var tablename = _classNameOptimizer.Optimize(tableInfo.TableName);
                string entityCode = @$"{_entityNamespace}
"+
                    @"{
"+
                    $@"    public prartial class {tablename}
"+
                    @"    {
"+
                    @"        "+string.Join("\n\t\t", tableInfo.Columns.Select(x => $"public {MapDataType(x.DataType)}{MapNullable(x.IsNullable)} {x.ColumnName} {{ get; set; }}"))+
                    @"    
    }
"+
                    @"}
";
                if (!Directory.Exists(_entityOutputPath))
                {
                    Directory.CreateDirectory(_entityOutputPath);
                }
                var filePath = Path.Combine(_entityOutputPath, $"{tablename}.cs");
                File.WriteAllText(filePath, entityCode);
            }

        }

        private string MapDataType(string sqlType)
        {
            // 简化映射，请根据您的数据库类型完善
            switch (sqlType)
            {
                case "int": return "int";
                case "smallint": return "short";
                case "tinyint": 
                    return "int";
                case "bigint":
                    return "long";
                case "uniqueidentifier":
                    return "Guid";
                case "bit":
                    return "bool";
                case "datetime": return "DateTime";
                case "datetime2": return "DateTime";
                case "date": return "date";
                case "smalldatetime":
                    return "DateTime";
                case "decimal": return "decimal";
                case "money": return "decimal";
                case "numeric":
                    return "decimal";
                case "float":
                    return "double";
                case "varchar": return "string";
                case "nvarchar":return "string";
                case "text": return "string";
                case "ntext": return "string";
                case "char": return "string";
                case "nchar": return "string";
                case "varbinary": return "byte[]";
                case "image":
                    return "byte[]";
                case "longtext": return "string";
                default:
                    return "object"; // 未知类型
            }
        }

        private string MapNullable(bool isNullAble)
        {
            if (isNullAble) return "?";
            else return "";
        }


        //生成DbContext代码，并保存到_contextOutputPath路径下
        public void DbContextGenerator(List<SystemTableInfo> tableInfos)
        {
            //生成的DbContext继承自ApplicationDbContext，并包含DbSet属性
            string contextCode = @"using Microsoft.EntityFrameworkCore;
using Lexa.Entities;
using Lexa.Data;
" +
 _contextNamespace+@"
{"+ @"
    public class AppDbContext : ApplicationDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
         }
        "+string.Join("\n\t\t", tableInfos.Select(x => { var tablename = _classNameOptimizer.Optimize(x.TableName); return $"public DbSet<{tablename}> {tablename}DbSet {{ get; set; }}"; }))
+"\n"
+ @"        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);"+ @"
            // 在这里可以添加额外的模型配置，例如表名、主键、关系等
            "+string.Join("\n\t\t\t", tableInfos.Select(x => $"modelBuilder.Entity<{_classNameOptimizer.Optimize(x.TableName)}>(entity=>\n\t\t\t{GenerateEntityColum(x)});"))
+ @"
        }
    }
}";

            if (!Directory.Exists(_contextOutputPath))
            {
                Directory.CreateDirectory(_contextOutputPath);
            }
            var filePath = Path.Combine(_contextOutputPath, "AppDbContext.cs");
            File.WriteAllText(filePath, contextCode);
        }

        private string GenerateEntityColum(SystemTableInfo tableInfo)
        {
            string EntityColumnCode = "{\n\t\t\t\t";
            EntityColumnCode += $"entity.ToTable(\"{tableInfo.TableName}\");\n";
            foreach(var colum in tableInfo.Columns)
            {

                if (colum.IsPrimaryKey)
                {
                    EntityColumnCode += $"\t\t\t\tentity.HasKey(e => e.{colum.ColumnName});\n";
                }

                EntityColumnCode += $"\t\t\t\tentity.Property(e => e.{colum.ColumnName})\n\t\t\t\t\t.HasComment(\"{colum.ColumnComment}\")\n\t\t\t\t\t.HasColumnName(\"{colum.ColumnName}\")";
                if(colum.CharacterMaximumLength != null && colum.CharacterMaximumLength > 0)
                {
                    EntityColumnCode += $"\n\t\t\t\t\t.HasMaxLength({colum.CharacterMaximumLength})";
                }
                EntityColumnCode += "\n;";

            }
            EntityColumnCode += "\n\t\t\t}";
            return EntityColumnCode;
        }

        //根据表信息生辰端点代码
        public void EndpointGenerator(List<SystemTableInfo> tableInfos)
        {
            foreach(var tableInfo in tableInfos)
            {
                var tableName = _classNameOptimizer.Optimize(tableInfo.TableName);

                GenerateGetEp(tableInfo);
                GenerateAddEp(tableInfo);
                GenerateUpdateEp(tableInfo);
                GenerateDeleteEp(tableInfo);


                
            }



            
        }
        //生成删除端点
        private void GenerateDeleteEp(SystemTableInfo tableInfo)
        {
        }
        //生成更新端点
        private void GenerateUpdateEp(SystemTableInfo tableInfo)
        {
        }
        //生成添加端点
        private void GenerateAddEp(SystemTableInfo tableInfo)
        {
        }
        //生成查询端点
        private void GenerateGetEp(SystemTableInfo tableInfo)
        {
            var entityName = _classNameOptimizer.Optimize(tableInfo.TableName);
            var endPointName = $"Get{entityName}Endpoint";
            var requestName = $"Get{entityName}Request";
            var responseName = $"Get{entityName}Response";

            //生成Rquest类
            StringBuilder RequestCode = new StringBuilder();
            RequestCode.AppendLine(@$"using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using {_entityNamespace};
using {_contextNamespace};");
            RequestCode.AppendLine(_endpointnamespace);
            RequestCode.AppendLine("{");
            RequestCode.AppendLine($"    public class {requestName}");
            RequestCode.AppendLine("    {");
            foreach(var colum in tableInfo.Columns)
            {
                RequestCode.AppendLine($"        public {MapDataType(colum.DataType)}? {colum.ColumnName} {{get;set;}} ");
            }
            RequestCode.AppendLine("    }");
            RequestCode.AppendLine("}");


            //生成Response类
            StringBuilder ResponseCode = new StringBuilder();
            ResponseCode.AppendLine(@$"using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using {_entityNamespace};
using {_contextNamespace};");
            ResponseCode.AppendLine(_endpointnamespace);
            ResponseCode.AppendLine("{");
            ResponseCode.AppendLine($"    public class {requestName}");
            ResponseCode.AppendLine("    {");
            foreach (var colum in tableInfo.Columns)
            {
                ResponseCode.AppendLine($"        public {MapDataType(colum.DataType)}? {colum.ColumnName} {{get;set;}} ");
            }
            ResponseCode.AppendLine("    }");
            ResponseCode.AppendLine("}");


            //生成Endpoint类
            StringBuilder EndpointCode = new StringBuilder();
            EndpointCode.AppendLine("using FastEndpoints;");
            EndpointCode.AppendLine("using Microsoft.AspNetCore.Authorization;");
            EndpointCode.AppendLine($"using {_entityNamespace}");
            EndpointCode.AppendLine($"using {_contextNamespace}");
            EndpointCode.AppendLine($"using {_endpointnamespace}");
            EndpointCode.AppendLine($"namespace {_endpointnamespace}");
            EndpointCode.AppendLine("{");
            EndpointCode.AppendLine($"   public class {endPointName}(AppDbContext context) : Endpoint<{requestName},{responseName}>");
            EndpointCode.AppendLine("    {");
            EndpointCode.AppendLine("       public override void Configure()");
            EndpointCode.AppendLine("        {");
            EndpointCode.AppendLine($"           Get(\"/api/get{entityName}\");");
            EndpointCode.AppendLine($"           AllowAnonymous();\r\n            // 更改swagger tag\r\n            Summary(s => s.Summary = \"获取数据库表信息\");\r\n            //设置swagger标签\r\n            Description(d =>d.WithTags(\"{entityName}\"));");
            EndpointCode.AppendLine("        }");
            EndpointCode.AppendLine($"        public override async Task HandleAsync({requestName} req, CancellationToken ct)");
            EndpointCode.AppendLine("        {");
            EndpointCode.AppendLine($"           var query = context.{entityName}DbSet.AsQueryable();");
            EndpointCode.AppendLine("           if(req.Id != null)");
            EndpointCode.AppendLine("           {");
            foreach(var colum in tableInfo.Columns)
            {
                if(colum.IsPrimaryKey)
                {
                    EndpointCode.AppendLine($"              query = query.Where(x => x.{colum.ColumnName} == req.{colum.ColumnName});");
                }
            }
            EndpointCode.AppendLine("           }");
            EndpointCode.AppendLine("           var data = await query.ToListAsync(ct);");
            EndpointCode.AppendLine($"           var response = data.select(d=>new {responseName} ( ");
            //EndpointCode.AppendLine(string.Join(",\n", tableInfo.Columns.Select(x => $"d.{x.ColumnName}")));

            EndpointCode.AppendLine("           await SendAsync(response, cancellation: ct);");
            EndpointCode.AppendLine("        }");
            EndpointCode.AppendLine("     }");
             EndpointCode.AppendLine("}");
        }
    }
}
