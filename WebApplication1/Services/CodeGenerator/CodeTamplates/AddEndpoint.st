
using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using <$data.Namespace$>.DTOs.Requests;
using <$data.Namespace$>.DTOs.Responses;
using <$data.Namespace$>.Services;

namespace <$data.Namespace$>.Endpoints
{
    /// <summary>
    /// 创建<$data.EntityName$>端点
    /// </summary>
    public class Add<$data.EntityName$>Endpoint : Endpoint<<$data.EntityName$>Request, <$data.EntityName$>Response>
    {
        private readonly I<$data.EntityName$>Service _<$data.EntityNameLower$>Service;

        public Create<$data.EntityName$>Endpoint(I<$data.EntityName$>Service <$data.EntityNameLower$>Service)
        {
            _<$data.EntityNameLower$>Service = <$data.EntityNameLower$>Service ?? throw new ArgumentNullException(nameof(<$data.EntityNameLower$>Service));
        }

        public override void Configure()
        {
            Post("/api/<$data.EntityNamePluralLower$>");
            Description(b => b
                .WithName("Create<$data.EntityName$>")
                .WithDescription("<$data.Description$>")
                .Produces<<$data.EntityName$>Response>(201, "application/json")
                .ProducesProblem(400)
                .ProducesProblem(500));
            AllowAnonymous();
        }

        public override async Task HandleAsync(<$data.EntityName$>Request req, CancellationToken ct)
        {
            var response = await _<$data.EntityNameLower$>Service.CreateAsync(req, ct);
            await SendCreatedAtAsync<Get<$data.EntityName$>ByIdEndpoint>(new { id = response.Id }, response, cancellation: ct);
        }
    }
}