using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace Lexa.Services.CodeGenerator
{
    public class TemplateEngine
    {
        private readonly TemplateGroupDirectory _templateGroup;

        /// <summary>
        /// 构造函数：初始化模板组，加载目录下.st模板文件
        /// </summary>
        /// <param name="templateDirectory">模板文件根目录</param>
        public TemplateEngine(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
                throw new DirectoryNotFoundException("模板目录不存在，请检查路径");

            // 初始化文件模板组，设置分隔符为$$，避免与C#语法冲突
            _templateGroup = new TemplateGroupDirectory(templateDirectory, '$', '$');
            // 加载模板
            _templateGroup.Load();
        }

        /// <summary>
        /// 渲染模板：绑定参数+生成可编译代码
        /// </summary>
        /// <param name="templateName">模板名称（不含.st后缀）</param>
        /// <param name="data">渲染参数（强类型对象）</param>
        /// <returns>渲染后的可编译C#代码</returns>
        public string Render(string templateName, object data)
        {
            try
            {
                // 获取模板实例
                var template = _templateGroup.GetInstanceOf(templateName);
                if (template == null)
                    throw new KeyNotFoundException($"未找到对应模板：{templateName}.st，请检查模板目录");

                // 绑定参数，模板中用 data.属性 调用
                template.Add("data", data);

                // 执行渲染并返回代码
                return template.Render();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"模板【{templateName}】渲染失败，检查占位符与参数是否匹配", ex);
            }
        }

        /// <summary>
        /// 清空模板缓存，修改模板后调用生效
        /// </summary>
        public void ClearCache()
        {
            _templateGroup.Unload();
            _templateGroup.Load();
        }
    }
}



