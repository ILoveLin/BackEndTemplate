using VideoShare_BackEnd.Utils.UniqueKeyUtils;

namespace VideoShare_BackEnd.Models.Middlewares
{
    //将原来的TraceIdentifier类型替换为Guid类型
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate next;

        public TraceIdMiddleware(RequestDelegate _next)
        {
            next = _next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.TraceIdentifier = UniqueKeyUtil.GetSnowID().ToString();
            var id = context.TraceIdentifier;
            context.Response.Headers["X-Trace-Id"] = id;
            // context.Request.Headers["X-Trace-Id"] = id;
            await next(context);
        }
    }
}