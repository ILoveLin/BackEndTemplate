using BackEndTemplate.Models.ControllerModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace BackEndTemplate.Models.Filters
{
    //异常处理 返回400
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public int Order { get; } = 0;

        private readonly IStringLocalizer<I18N> localizer;

        public ExceptionFilter(IStringLocalizer<I18N> _localizer)
        {
            localizer = _localizer;
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            ResponseMsg responseMsg = new ResponseMsg();
            responseMsg.code = ResponseCode.InnerException;
            responseMsg.msg = localizer["内部异常！"];
            responseMsg.result = new { exceptionMsg = context.Exception.ToString() };

            context.Result = new BadRequestObjectResult(responseMsg);
            context.HttpContext.Items.TryAdd("ExceptionMsg", context.Exception.ToString());
            context.ExceptionHandled = true; //为true时代表此异常已被处理，后续处理程序将不会处理此异常
            return Task.CompletedTask;
        }
    }
}