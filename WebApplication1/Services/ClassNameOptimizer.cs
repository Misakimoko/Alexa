using System.Globalization;

namespace Lexa.Services
{
    /// <summary>
    /// 类名优化器配置选项
    /// </summary>
    public class ClassNameOptimizerOptions
    {
        /// <summary>
        /// 要移除的前缀列表（按长度降序排列）
        /// </summary>
        public List<string> PrefixesToRemove { get; set; } = new List<string>
        {
            "tbl_", "t_", "tab_", "tb_", "sys_", "app_", "base_", "biz_", "dt_", "md_", "mt_", "rt_", "st_", "tm_", "ut_"
        };

        /// <summary>
        /// 要移除的后缀列表（按长度降序排列）
        /// </summary>
        public List<string> SuffixesToRemove { get; set; } = new List<string>
        {
            "_tab", "_table", "_info", "_data", "_master", "_detail", "_temp", "_tmp", "_bak", "_log", "_hist", "_config"
        };

        /// <summary>
        /// 区域性设置
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// 类名优化器
    /// 用于将数据库表名优化为C#实体类名
    /// </summary>
    public class ClassNameOptimizer
    {
        private readonly ClassNameOptimizerOptions _options;
        private readonly TextInfo _textInfo;

        public ClassNameOptimizer(ClassNameOptimizerOptions options = null)
        {
            _options = options ?? new ClassNameOptimizerOptions();
            _textInfo = _options.Culture.TextInfo;
        }

        /// <summary>
        /// 优化表名为实体类名
        /// </summary>
        /// <param name="tableName">原始表名</param>
        /// <returns>优化后的实体类名</returns>
        public string Optimize(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return tableName;

            // 1. 转换为小写，统一处理
            string name = tableName.ToLowerInvariant();

            // 2. 移除前缀
            name = RemovePrefixes(name);

            // 3. 移除后缀
            name = RemoveSuffixes(name);

            // 4. 转换为PascalCase
            name = ConvertToPascalCase(name);

            return name;
        }

        /// <summary>
        /// 批量优化表名
        /// </summary>
        /// <param name="tableNames">表名列表</param>
        /// <returns>优化后的类名字典（原始表名 -> 优化类名）</returns>
        public Dictionary<string, string> OptimizeBatch(IEnumerable<string> tableNames)
        {
            var result = new Dictionary<string, string>();
            foreach (var tableName in tableNames)
            {
                result[tableName] = Optimize(tableName);
            }
            return result;
        }

        #region 私有方法

        private string RemovePrefixes(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            // 按前缀长度降序排序，优先匹配长前缀
            foreach (var prefix in _options.PrefixesToRemove
                .OrderByDescending(p => p.Length)
                .Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return name[prefix.Length..];
                }
            }
            return name;
        }

        private string RemoveSuffixes(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            // 按后缀长度降序排序，优先匹配长后缀
            foreach (var suffix in _options.SuffixesToRemove
                .OrderByDescending(s => s.Length)
                .Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return name[..^suffix.Length];
                }
            }
            return name;
        }

        private string ConvertToPascalCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            // 分割下划线和连字符
            var separators = new[] { '_', '-' };
            var parts = name.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            // 每个部分首字母大写
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    // 处理数字开头的部分
                    if (char.IsDigit(parts[i][0]))
                    {
                        parts[i] = "_" + _textInfo.ToTitleCase(parts[i]);
                    }
                    else
                    {
                        parts[i] = _textInfo.ToTitleCase(parts[i]);
                    }
                }
            }

            return string.Concat(parts);
        }

        #endregion
    }

    #region 测试和示例代码

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("类名优化器测试（无单数转换）");
            Console.WriteLine("==========================================");

            // 创建优化器实例
            var options = new ClassNameOptimizerOptions
            {
                PrefixesToRemove = { "sys_", "tbl_", "t_", "tab_", "app_", "base_" },
                SuffixesToRemove = { "_tab", "_table", "_info", "_data", "_master" }
            };

            var optimizer = new ClassNameOptimizer(options);

            // 测试用例
            var testCases = new[]
            {
                "sys_user",              // 用户表
                "tbl_orders",            // 订单表
                "user_roles",            // 用户角色
                "t_categories",          // 分类
                "tab_products",          // 产品
                "sys_menu_info",         // 菜单信息
                "app_config_data",       // 配置数据
                "base_department_master", // 部门主表
                "person",                // 人员
                "people",                // 人们
                "mouse",                 // 鼠标
                "mice",                  // 鼠标（复数）
                "order_items",           // 订单项
                "customer_addresses",    // 客户地址
                "t_2024_sales_stats",    // 2024销售统计
                "1st_quarter_report",    // 第一季度报告
                "USER_LOG",              // 用户日志
                "Sys_User_Group",        // 用户组
                "tbl_invoice_details",   // 发票详情
                "product-categories",    // 产品分类（带连字符）
                "sys_audit_logs",        // 审计日志
                "data_migration_history", // 数据迁移历史
                "temp_user_sessions",    // 用户会话临时表
                "api_request_log",       // API请求日志
                "payment_transactions",  // 支付交易
            };

            Console.WriteLine("| 原始表名                    | 优化后的类名             |");
            Console.WriteLine("|-----------------------------|-------------------------|");

            foreach (var tableName in testCases)
            {
                var className = optimizer.Optimize(tableName);
                Console.WriteLine($"| {tableName,-27} | {className,-23} |");
            }

            Console.WriteLine("==========================================");

            // 批量处理示例
            Console.WriteLine("\n批量处理示例：");
            var tableNames = new List<string>
            {
                "sys_user",
                "tbl_order_items",
                "customer_addresses"
            };

            var result = optimizer.OptimizeBatch(tableNames);
            foreach (var kvp in result)
            {
                Console.WriteLine($"表名: {kvp.Key,-25} -> 类名: {kvp.Value}");
            }

            // 自定义配置示例
            Console.WriteLine("\n自定义配置示例：");
            var customOptions = new ClassNameOptimizerOptions
            {
                PrefixesToRemove = { "myapp_", "temp_", "tmp_" },
                SuffixesToRemove = { "_tmp", "_old", "_backup" }
            };

            var customOptimizer = new ClassNameOptimizer(customOptions);
            var testCases2 = new[]
            {
                "myapp_user_logs_tmp",
                "temp_2024_data_backup",
                "tmp_calculations_old"
            };

            foreach (var testTable in testCases2)
            {
                var customResult = customOptimizer.Optimize(testTable);
                Console.WriteLine($"表名: {testTable,-25} -> 类名: {customResult}");
            }

            // 测试空值和特殊值
            Console.WriteLine("\n边界条件测试：");
            Console.WriteLine($"空字符串: \"{optimizer.Optimize("")}\"");
            Console.WriteLine($"null: \"{optimizer.Optimize(null)}\"");
            Console.WriteLine($"全大写: \"{optimizer.Optimize("TEST_TABLE")}\"");
            Console.WriteLine($"全小写: \"{optimizer.Optimize("test_table")}\"");
            Console.WriteLine($"混合大小写: \"{optimizer.Optimize("Test_Table_Name")}\"");
            Console.WriteLine($"纯数字: \"{optimizer.Optimize("123_table")}\"");
            Console.WriteLine($"数字开头: \"{optimizer.Optimize("2024_report")}\"");
            Console.WriteLine($"多个分隔符: \"{optimizer.Optimize("sys__user___info")}\"");

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }

    #endregion
}
