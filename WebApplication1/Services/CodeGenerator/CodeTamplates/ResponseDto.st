
using System;

namespace <$data.Namespace$>.DTOs.Responses
{
    /// <summary>
    /// <$data.EntityName$> 响应DTO
    /// </summary>
    public class <$data.EntityName$>Response
    {
        <$each:data.Properties prop$>
        /// <summary>
        /// <$prop.Description$>
        /// </summary>
        public <$prop.Type$> <$prop.Name$> { get; set; }
        <$endeach$>
    }
}