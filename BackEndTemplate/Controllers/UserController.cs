using BackEndTemplate.Controllers.BaseController;
using BackEndTemplate.Models.Attributes;
using BackEndTemplate.Models.CMELiveHttp.Login;
using BackEndTemplate.Models.ControllerModels;
using BackEndTemplate.Models.ControllerModels.RequestModels.User;
using BackEndTemplate.Models.DataBaseModels;
using BackEndTemplate.Models.DataBaseModels.Context;
using BackEndTemplate.Models.Filters;
using BackEndTemplate.Models.User;
using BackEndTemplate.Utils.CMELiveUtils;
using BackEndTemplate.Utils.ResponseUtils;
using BackEndTemplate.Utils.UniqueKeyUtils;
using BackEndTemplate.Utils.UserClaimUtils;
using BackEndTemplate.Utils.UserUtils;
using Casbin.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BackEndTemplate.Controllers;

public class UserController : BaseController<UserController>
{
    public UserController(ILogger<UserController> _logger, IStringLocalizer<I18N> _localizer, VideoShareContext _ctx, IEnforcerProvider _enforcerProvider) : base(_logger, _localizer, _ctx,
        _enforcerProvider)
    {
    }


    /// <summary>
    /// 登录接口
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [NoNeedLogin] //代表此接口不需要登录
    [Route("login")]
    [ApiVersionToLatest(1.0)]
    public async Task<IActionResult> login([FromBody] LoginRequestModel model)
    {
        model.AuthType ??= UserAuthType.Default;

        UserAuth userAuth = null;

        switch (model.AuthType)
        {
            case UserAuthType.Default:
            {
                userAuth = await ctx.UserAuths.Where(a => a.AuthType == (int)model.AuthType && a.AuthCredential == model.Password && a.AuthAccount == model.Account).FirstOrDefaultAsync();
                break;
            }
            case UserAuthType.CMELive:
            {
                //调用CMELive登录接口
                CMELive_LoginRequestModel requestModel = new CMELive_LoginRequestModel();
                requestModel.user.Username = model.Account;
                requestModel.user.Password = model.Password;
                var loginReponseMsg = await CMELiveHttpUtil.LoginAsync(requestModel);
                if (!loginReponseMsg.CheckResponse(false))
                {
                    return ResponseUtil.ResponseMsg(ResponseCode.LoginInvalid);
                }
                //获取CMELive用户信息
                var cmeLiveGetUserInfoResponseModel = await CMELiveHttpUtil.GetUserInfoAsync(loginReponseMsg.result.token);
                if (!cmeLiveGetUserInfoResponseModel.CheckResponse(false))
                {
                    return ResponseUtil.ResponseMsg(ResponseCode.LoginInvalid);
                }

                //查询数据库是否存在当前第三方登录信息，存在则更新Token，不存在则新增
                userAuth = await ctx.UserAuths.Where(a => a.AuthType == (int)model.AuthType && a.AuthAccount == model.Account).FirstOrDefaultAsync();
                if (userAuth is null)
                {
                    await using var dbContextTransaction = await ctx.Database.BeginTransactionAsync();

                    User dstNewUser = null;
                    var IdentifierUniqeKeyflag = await UniqueKeyUtil.UniqeKeyRetry(logger, ctx, async () =>
                    {
                        User newUser = new();
                        newUser.Identifier = UniqueKeyUtil.GetSnowID().ToString();
                        newUser.Username = $"{model.Account}{Random.Shared.NextInt64() % 99999}";
                        newUser.Level = 0;//层级关系
                        newUser.UserType = (int)CMELiveUtil.MapRoleIDToUserType(cmeLiveGetUserInfoResponseModel.result.user.RoleId);
                        newUser.Group = "default";
                        newUser.CreateTime = DateTime.UtcNow;
                        await ctx.Users.AddAsync(newUser);
                        await ctx.SaveChangesAsync();
                        dstNewUser = newUser;
                        return true;
                    });
                    if (!IdentifierUniqeKeyflag)
                    {
                        return Ok(ResponseUtil.ResponseMsg(ResponseCode.UniqueKeyError, localizer["Identifier唯一键产生失败！"]));
                    }

                    UserAuth dstNewUserAuth = null;
                    var AuthAccountUniqeKeyflag = await UniqueKeyUtil.UniqeKeyRetry(logger, ctx, async () =>
                    {
                        UserAuth newUserAuth = new();
                        newUserAuth.UserId = dstNewUser.Id;
                        newUserAuth.AuthAccount = $"{model.Account}{Random.Shared.NextInt64() % 99999}";
                        newUserAuth.AuthType = (int)UserAuthType.Default;
                        newUserAuth.AuthCredential = model.Password;
                        await ctx.UserAuths.AddAsync(newUserAuth);
                        await ctx.SaveChangesAsync();
                        dstNewUserAuth = newUserAuth;
                        return true;
                    });
                    if (!AuthAccountUniqeKeyflag)
                    {
                        return Ok(ResponseUtil.ResponseMsg(ResponseCode.UniqueKeyError, localizer["AuthAccount唯一键产生失败！"]));
                    }

                    UserAuth newUserAuth1 = new();
                    newUserAuth1.UserId = dstNewUser.Id;
                    newUserAuth1.AuthAccount = model.Account;
                    newUserAuth1.AuthType = (int)UserAuthType.CMELive;
                    newUserAuth1.AuthCredential = loginReponseMsg.result.token;

                    await ctx.UserAuths.AddAsync(newUserAuth1);
                    await ctx.SaveChangesAsync();
                    await UserUtil.ConfigurePermissionByUserType((UserType)dstNewUser.UserType, dstNewUser.Identifier);
                    await dbContextTransaction.CommitAsync();

                    userAuth = dstNewUserAuth;
                }
                else
                {
                    userAuth.AuthCredential = loginReponseMsg.result.token;
                    await ctx.SaveChangesAsync();
                }

                break;
            }
        }

        if (userAuth is null)
        {
            return ResponseUtil.ResponseMsg(ResponseCode.LoginInvalid);
        }

        var token = await UserUtil.GetTokenFromUserIDAsync(ctx, userAuth.UserId);
        if (token is null)
        {
            return ResponseUtil.ResponseMsg(ResponseCode.LoginInvalid);
        }

        LoginResponseModel responseModel = new();
        responseModel.token = token;
        return ResponseUtil.Response(ResponseCode.Success, responseModel, localizer["登录成功！"]);
    }

    /// <summary>
    /// 登出接口
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("loginOut")]
    [ApiVersionToLatest(1.0)]
    public async Task<IActionResult> loginOut()
    {
        return ResponseUtil.ResponseMsg(ResponseCode.Success);
    }

    /// <summary>
    /// 注册接口
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [NoNeedLogin]
    [Route("register")]
    [ApiVersionToLatest(1.0)]
    public async Task<IActionResult> register(RegisterRequestModel model)
    {
        User newUser = new();
        newUser.Username = model.Username;
        newUser.Level = 0;
        newUser.Group = "default";
        newUser.UserType =(int)model.UserType;
        newUser.Avatar = model.Avatar;
        newUser.Phone = model.Phone;
        newUser.CreateTime = DateTime.UtcNow;

        UserAuth newUserAuth = new();
        newUserAuth.AuthAccount = model.Account;
        newUserAuth.AuthCredential = model.Password;
        newUserAuth.AuthType = (int)UserAuthType.Default;

        var res = await UserUtil.AddNewUserAsync(ctx, newUser, newUserAuth);

        switch (res.Item1)
        {
            case -1:
            {
                return ResponseUtil.ResponseMsg(ResponseCode.Failure_Operation, localizer["Identifier唯一键产生失败！"]);
            }
            case -2:
            {
                return ResponseUtil.ResponseMsg(ResponseCode.Failure_Operation, localizer["账号已存在！"]);
            }
        }

        RegisterResponseModel responseModel = new();
        return ResponseUtil.Response(ResponseCode.Success, responseModel);
    }


    /// <summary>
    /// 绑定父级账户
    /// 想要调用此接口用户，必须拥有"parentAccount","bind"权限：拥有父账号的绑定权限
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiVersionToLatest(1.0)]
    [Route("bindParentAccount")]
    [CasbinAuthorize("parentAccount","bind")] 
    public async Task<IActionResult> bindParentAccount(BindParentAccountRequestModel model)
    {
        var userClaim = UserClaimUtil.GetUserClaim(HttpContext);
        int res = await UserUtil.BindParentAccountAsync(ctx, userClaim.UserID, model.ParentIdentifier);
        if (res != 0)
        {
            return ResponseUtil.ResponseMsg(ResponseCode.Failure_Operation, localizer["绑定失败！"]);
        }

        BindParentAccountResponseModel responseModel = new();
        return ResponseUtil.Response(ResponseCode.Success, responseModel);
    }
}