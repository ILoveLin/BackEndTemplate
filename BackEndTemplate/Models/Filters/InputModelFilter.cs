using BackEndTemplate.Models.ControllerModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using BackEndTemplate.Utils.ResponseUtils;


namespace BackEndTemplate.Models.Filters
{
    //模型输入验证 返回200
    public class InputModelFilter : ActionFilterAttribute
    {
        private readonly IStringLocalizer<I18N> localizer;


        public InputModelFilter(IStringLocalizer<I18N> _localizer)
        {
            Order = 2;
            localizer = _localizer;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                List<JObject> jobjects = new List<JObject>();
                var ModelStates = context.ModelState.ToList();
                for (int i = 0; i < ModelStates.Count; i++)
                {
                    if (ModelStates[i].Value.Errors.Count > 0)
                    {
                        JObject jobject = new JObject();
                        jobject.Add("key", ModelStates[i].Key);
                        jobject.Add("ErrorMessages", JToken.FromObject(ModelStates[i].Value.Errors.Select(e => e.ErrorMessage).ToList()));
                        jobjects.Add(jobject);
                    }
                }

                ResponseMsg responseMsg = new ResponseMsg();
                responseMsg.code = ResponseCode.InvalidInput;
                responseMsg.msg = localizer["参数有误！"];
                responseMsg.result = jobjects;
                context.Result = new OkObjectResult(responseMsg);
            }
        }
    }
}