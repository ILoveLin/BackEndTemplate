using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VideoShare_BackEnd.Models.Filters;

namespace VideoShare_BackEnd.Controllers
{
    [ApiController]
    [NoNeedLogin]
    [ApiVersionNeutral]
    //-------------------------------------------------> 注释下一行来启用WebSocketServer，同时取消注释Program.cs文件里的app.UseWebSockets();
    [NonController]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> logger;

        public WebSocketController(ILogger<WebSocketController> _logger)
        {
            this.logger = _logger;
        }


        [Route("/ws")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                if (App.WebSocketServerManager.OnMsgAsync == null)
                {
                    App.WebSocketServerManager.OnMsgAsync = async (a, b) =>
                    {
                        Console.WriteLine(b);
                        await App.WebSocketServerManager.SendAsync(a.HttpContext.TraceIdentifier, b);
                    };
                }
                await App.WebSocketServerManager.AcceptWebSocketAsync(HttpContext, CancellationToken.None);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}