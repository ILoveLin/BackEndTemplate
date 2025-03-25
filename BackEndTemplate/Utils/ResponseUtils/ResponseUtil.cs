using BackEndTemplate.Models.ControllerModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BackEndTemplate.Utils.ResponseUtils
{
    public class ResponseUtil
    {

        private readonly static IStringLocalizer<I18N> localizer = App.Service.GetRequiredService<IStringLocalizer<I18N>>();

        //todo:国际化
        public readonly static Dictionary<ResponseCode, string> responseCodeMap = new Dictionary<ResponseCode, string>
        {
            { ResponseCode.ParamsCannotUse, "参数不可用！" },
            { ResponseCode.InvalidInput, "无效输入！" },
            { ResponseCode.InnerException, "内部异常！" },
            { ResponseCode.Success, "请求成功！" },
            { ResponseCode.Failure, "请求失败！" },
            { ResponseCode.Failure_NoData, "暂无数据！" },
            { ResponseCode.Failure_Operation, "操作失败！" },
            { ResponseCode.NoDataCausedByUnknownReason, "未知原因导致的无数据！" },
            { ResponseCode.TimeError, "时间错误！" },
            { ResponseCode.InsufficientPermissions, "权限不足！" },
            { ResponseCode.UniqueKeyError, "唯一值错误！" },
            { ResponseCode.LoginInvalid, "登录失效！" },
            { ResponseCode.UserCannotUse, "用户不可用！" },
        };

        public static IActionResult Response(ResponseCode code, object? result = null, string? msg = null)
        {
            ResponseMsg responseMsg = new ResponseMsg();
            responseMsg.code = code;
            responseMsg.msg = msg ?? localizer[responseCodeMap[code]];
            responseMsg.result = result ?? new object();
            return new OkObjectResult(responseMsg);
        }

        public static IActionResult ResponseMsg(ResponseCode code, string? msg = null)
        {
            ResponseMsg responseMsg = new ResponseMsg();
            responseMsg.code = code;
            responseMsg.msg = msg ?? localizer[responseCodeMap[code]];
            responseMsg.result = new object();
            return new OkObjectResult(responseMsg);
        }
    }
}