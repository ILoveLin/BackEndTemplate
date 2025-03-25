using Casbin;
using Casbin.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackEndTemplate.Controllers.BaseController;
using BackEndTemplate.Models.Attributes;
using BackEndTemplate.Models.ControllerModels;
using BackEndTemplate.Models.DataBaseModels.Context;
using BackEndTemplate.Models.Filters;
using BackEndTemplate.Utils.ResponseUtils;
using BackEndTemplate.Utils.UserClaimUtils;
using BackEndTemplate.Models.DataBaseModels;
using BackEndTemplate.Models.User;
using BackEndTemplate.Utils.NullUtils;

namespace BackEndTemplate.Controllers;


#if RELEASE
[NonController]
#endif
public class TestController : BaseController<TestController>
{
    public TestController(ILogger<TestController> _logger, IStringLocalizer<I18N> _localizer, VideoShareContext _ctx, IEnforcerProvider _enforcerProvider) : base(_logger, _localizer, _ctx,
        _enforcerProvider)
    {
    }

    [HttpGet]
    [Route("test")]
    [ApiVersionToLatest(1.0)]
    [NoNeedLogin] //代表此接口不需要登录
    public async Task<IActionResult> testv1()
    {
        var erforcer = enforcerProvider.GetEnforcer();

        erforcer.AddGroupingPolicy("Admin426825", "admin");
        var a = erforcer.GetAllRoles().ToList();
        var aa = erforcer.GetAllObjects().ToList();
        var aaa = erforcer.GetAllSubjects().ToList();
        return ResponseUtil.ResponseMsg(ResponseCode.Success, localizer["请求成功！"] + "v1.0");
    }


    [HttpGet]
    [Route("test")]
    [NoNeedLogin] //代表此接口不需要登录
    public async Task<IActionResult> test3([FromQuery] Test3RequestModel requestRequestModel)
    {
        return ResponseUtil.Response(ResponseCode.Success, new { requestRequestModel.str, requestRequestModel.id }, localizer["请求成功！"]);
    }

    //WebSokcet 测试接口 给所有客户端发送消息
    [HttpGet]
    [Route("sendAll")]
    [NoNeedLogin] //代表此接口不需要登录
    public async Task<IActionResult> sendAll([FromQuery] string data)
    {
        await App.WebSocketServerManager.SendAllAsync(data);
        return ResponseUtil.Response(ResponseCode.Success);
    }


    //登录测试接口  测试登录获取的token是否验证 开启了登录验证才有用 登录验证开启方式在Program.cs
    [HttpGet]
    [Route("testToken")]
    public async Task<IActionResult> testToken()
    {
        //进入此处代表Token已经验证成功，若未验证成功，在项目目录Models/Filters/LoginFilter.cs文件中将返回登录失效消息 
        var userClaim = UserClaimUtil.GetUserClaim(HttpContext);
        return ResponseUtil.Response(ResponseCode.Success, userClaim, "成功验证Token！");
    }


    //登录测试接口  测试登录获取的token是否验证 开启了登录验证才有用 登录验证开启方式在Program.cs
    [HttpGet]
    [Route("testCasbin")]
    [CasbinAuthorize("res", "read")]
    public async Task<IActionResult> testCasbin()
    {
        //进入此处代表Token已经验证成功，若未验证成功，在项目目录Models/Filters/LoginFilter.cs文件中将返回登录失效消息 
        //获取 Casbin 执行器:enforcerProvider 是一个依赖注入的服务，它负责提供 Casbin 的执行器（Enforcer）实例
        //GetEnforcer() 方法返回一个 Enforcer 对象，该对象用于执行 Casbin 的访问控制规则
        var enforcer = enforcerProvider.GetEnforcer();
        var userClaim = UserClaimUtil.GetUserClaim(HttpContext);
        //执行权限检查：方法返回 true，表示用户具有写入权限
        if (await enforcer.EnforceAsync(userClaim.UserInentifier, "res", "write"))
        {
            return ResponseUtil.ResponseMsg(ResponseCode.Success, "成功写入资源！");
        }
        else
        {
            return ResponseUtil.Response(ResponseCode.Success, userClaim, "成功验证Token！");
        }
    }
}