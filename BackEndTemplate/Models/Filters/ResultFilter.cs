using BackEndTemplate.Models.ControllerModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackEndTemplate.Models.Filters
{
    public class ResultFilter : Attribute, IOrderedFilter, IAlwaysRunResultFilter
    {
        public int Order { get; } = -9999;

        //在即将响应前此函数将会执行
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result != null)
            {
                var responseMsg = (context.Result as ObjectResult)?.Value as ResponseMsg;
                if (responseMsg != null)
                {
                    responseMsg.traceID = context.HttpContext.TraceIdentifier;
                    context.HttpContext.Items.TryAdd("ResponseMsg", responseMsg);
                }
            }
        }

        //响应完成后此函数执行
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}