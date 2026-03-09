
using System;
using System.ComponentModel.DataAnnotations;

namespace <$data.Namespace$>.DTOs.Requests
{
    /// <summary>​
    /// <$data.EntityName$> 请求DTO​
    /// </summary>​
    public class <$data.EntityName$>Request​
    {​
        <$each:data.Properties prop$>​
        /// <summary>​
        /// <$prop.Description$>​
        /// </summary>​
        <$if:prop.IsRequired$>[Required(ErrorMessage = "<$prop.Description$>不能为空")]​
        <$endif$>​
        public <$prop.Type$> <$prop.Name$> { get; set; }​
        <$endeach$>​
    }
}
