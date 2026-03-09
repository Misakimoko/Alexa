using Lexa.Services.CodeGenerator;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Lexa.Services.CodeGenerator
{
    public class FastEndpointsGenerator
    {
        private readonly TemplateEngine _templateEngine;

        /// <summary>
        /// 构造函数：绑定模板目录，初始化模板引擎
        /// </summary>
        /// <param name="templateDirectory">模板文件根目录</param>
        public FastEndpointsGenerator(string templateDirectory)
        {
            _templateEngine = new TemplateEngine(templateDirectory);
        }

        /// <summary>
        /// 生成完整CRUD端点代码（Request+Response+增删改查列表）
        /// </summary>
        /// <param name="options">生成配置参数（强类型）</param>
        /// <returns>生成的可编译代码文件集合</returns>
        public List<GeneratedFile> GenerateCrudEndpoints(CrudEndpointOptions options)
        {
            var generatedFiles = new List<GeneratedFile>();

            // 1. 生成请求DTO
            var requestCode = _templateEngine.Render("RequestDto", options);
            generatedFiles.Add(new GeneratedFile($"{options.EntityName}Request.cs", requestCode));

            // 2. 生成响应DTO
            var responseCode = _templateEngine.Render("ResponseDto", options);
            generatedFiles.Add(new GeneratedFile($"{options.EntityName}Response.cs", responseCode));

            // 3. 生成新增端点
            var createCode = _templateEngine.Render("CreateEndpoint", options);
            generatedFiles.Add(new GeneratedFile($"Create{options.EntityName}Endpoint.cs", createCode));

            // 4. 生成单查端点
            var getByIdCode = _templateEngine.Render("GetByIdEndpoint", options);
            generatedFiles.Add(new GeneratedFile($"Get{options.EntityName}ByIdEndpoint.cs", getByIdCode));

            // 5. 生成更新端点
            var updateCode = _templateEngine.Render("UpdateEndpoint", options);
            generatedFiles.Add(new GeneratedFile($"Update{options.EntityName}Endpoint.cs", updateCode));

            // 6. 生成删除端点
            var deleteCode = _templateEngine.Render("DeleteEndpoint", options);
            generatedFiles.Add(new GeneratedFile($"Delete{options.EntityName}Endpoint.cs", deleteCode));

            // 7. 生成列表端点
            var listCode = _templateEngine.Render("ListEndpoint", options);
            generatedFiles.Add(new GeneratedFile($"List{options.EntityNamePlural}Endpoint.cs", listCode));

            return generatedFiles;
        }

        /// <summary>
        /// 刷新模板缓存（模板修改后调用，无需重启程序）
        /// </summary>
        public void RefreshTemplates()
        {
            _templateEngine.ClearCache();
        }
    }

    #region 生成器模型（强类型参数，无改动，兼容原有逻辑）
    /// <summary>
    /// 生成的代码文件模型
    /// 
    public class GeneratedFile
    {
        public string FileName { get; }
        public string Content { get; }

        public GeneratedFile(string fileName, string content)
        {
            FileName = fileName;
            Content = content;
        }
    }

    /// <summary>
    /// CRUD端点生成配置项（强类型参数，StringTemplate直接绑定）
     public class CrudEndpointOptions
    {
        /// <summary>
        /// 命名空间
        /// 
        public string Namespace { get; set; }
        /// <summary>
        /// 实体名称（单数，如User）
        /// 
        public string EntityName { get; set; }
        /// <summary>
        /// 实体名称复数（如Users）
        /// 
        public string EntityNamePlural { get; set; }
        /// <summary>
        /// 实体名称小写（如user）
        /// 
        public string EntityNameLower => EntityName?.ToLower() ?? string.Empty;
        /// <summary>
        /// 实体复数小写（如users）
        /// 
        public string EntityNamePluralLower => EntityNamePlural?.ToLower() ?? string.Empty;
        /// &lt;summary&gt;
        /// 开发作者
        /// 
        public string Author { get; set; }
        /// <summary>
        /// 接口描述
        /// 
        public string Description { get; set; }
        /// <summary>
        /// 实体属性集合（支持循环渲染）
        /// 
        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();

    }

/// <summary>
/// 实体属性信息
/// 
public class PropertyInfo
{
    /// <summary>
    /// 属性名
    /// 
    public string Name { get; set; }
    /// <summary>
    /// 属性类型
    ///
    public string Type { get; set; }
    /// <summary>
    /// 属性描述
    ///  public string Description { get; set; }
    /// <summary>
    /// 是否必填（支持条件渲染）
    ///  public bool IsRequired { get; set; }
}
    #endregion
}


