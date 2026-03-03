using WebApplication1.Data;
using FastEndpoints;
using FastEndpoints.Swagger;
using WebApplication1.Data.MetaData;
namespace WebApplication1.Endpoints.MetaEps.GetMeta
{
    //实现一个endpoint类，没有请求和响应模型
    public class GetMetaEp(MetaService metaService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/api/getMetaData");
            AllowAnonymous();
            // 更改swagger tag
            Summary(s => s.Summary = "获取数据库表信息");
             Tags("Meta");
        }
        public override async Task HandleAsync(CancellationToken ct)
        {
            var res = await metaService.GetAllTablesAsync();
            if (res != null) { await SendAsync(res); }
        }
    }
}
