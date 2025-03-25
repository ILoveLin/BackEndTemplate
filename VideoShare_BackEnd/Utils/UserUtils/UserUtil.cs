using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Casbin;
using Casbin.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VideoShare_BackEnd.Models.ControllerModels;
using VideoShare_BackEnd.Models.ControllerModels.RequestModels.User;
using VideoShare_BackEnd.Models.DataBaseModels;
using VideoShare_BackEnd.Models.DataBaseModels.Context;
using VideoShare_BackEnd.Models.User;
using VideoShare_BackEnd.Utils.ResponseUtils;
using VideoShare_BackEnd.Utils.UniqueKeyUtils;

namespace VideoShare_BackEnd.Utils.UserUtils;

public class UserUtil
{
    /// <summary>
    /// 通过用户ID获取Token
    /// </summary>
    /// <param name="ctx">数据库Context</param>
    /// <param name="UserID">用户ID</param>
    /// <returns>返回token，可能为null</returns>
    public static async Task<string> GetTokenFromUserIDAsync(VideoShareContext ctx, long UserID)
    {
        var user = await ctx.Users.FirstOrDefaultAsync(a => a.Id == UserID && a.IsDeny == false && a.IsDelete == false);
        if (user is null)
        {
            return null;
        }

        //加入要在Token中保存的信息，以便使用
        var Claims = new List<Claim>()
        {
            new(UserClaimType.UserID, user.Id.ToString()),
            new(UserClaimType.UserPID, user.Pid.ToString()),
            new(UserClaimType.UserLevel, user.Level.ToString()),
            new(UserClaimType.UserGroup, user.Group),
            new(UserClaimType.UserIdentifier, user.Identifier),
        };

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(App.AppSettings["Jwt:SecretKey"])),
            SecurityAlgorithms.HmacSha256);

        // 生成token
        var jwtSecurityToken = new JwtSecurityToken(
            App.AppSettings["Jwt:Issuer"],
            App.AppSettings["Jwt:Audience"],
            Claims,
            DateTime.Now,
#if DEBUG
            DateTime.Now.AddMinutes(5),
#elif RELEASE
                    DateTime.Now.AddDays(1),
#endif
            signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    /// <summary>
    /// 添加新用户
    /// 注意用户标识符不允许手动赋值，赋值会返回-10,本函数会自动生成唯一标识，尝试一定次数的插入数据库，插入失败返回-2
    /// UserAuth中的AuthAccount若有值会检查数据库是否存在，存在返回-1
    /// UserAuth中的AuthAccount若无值会自动产生uuid，尝试一定次数的插入数据库，插入失败返回-1
    /// </summary>
    /// <param name="ctx">数据库Context</param>
    /// <param name="newUser">新用户</param>
    /// <param name="userType">用户类型</param>
    /// <returns>0 成功,-1 用户标识符产生失败,-2 用户账户已存在, -10 用户标识符不允许手动赋值</returns>
    public static async Task<(int, User, UserAuth)> AddNewUserAsync(VideoShareContext ctx, User newUser, UserAuth newUserAuth)
    {
        //用户标识符不允许手动赋值
        if (!newUser.Identifier.IsNullOrEmpty())
        {
            return (-10, null, null);
        }

        await using var dbContextTransaction = await ctx.Database.BeginTransactionAsync();

        User dstNewUser = null;
        var Identifierflag = await UniqueKeyUtil.UniqeKeyRetry(App.GetService<ILogger<UserUtil>>(), ctx, async () =>
        {
            newUser.Identifier = UniqueKeyUtil.GetSnowID().ToString();
            await ctx.Users.AddAsync(newUser);
            await ctx.SaveChangesAsync();
            dstNewUser = newUser;
            return true;
        });
        if (!Identifierflag)
        {
            return (-1, null, null);
        }


        UserAuth dstNewUserAuth = null;
        if (!newUserAuth.AuthAccount.IsNullOrEmpty())
        {
            bool accoutIsExist = await ctx.UserAuths.AnyAsync(a => a.AuthAccount == newUserAuth.AuthAccount && a.AuthType == newUserAuth.AuthType);
            if (accoutIsExist)
            {
                return (-2, null, null);
            }
            newUserAuth.UserId = dstNewUser.Id;
            await ctx.UserAuths.AddAsync(newUserAuth);
            await ctx.SaveChangesAsync();
            dstNewUserAuth = newUserAuth;
        }
        else
        {
            newUserAuth.UserId = dstNewUser.Id;
            var AuthAccountflag = await UniqueKeyUtil.UniqeKeyRetry(App.GetService<ILogger<UserUtil>>(), ctx, async () =>
            {
                newUserAuth.AuthAccount = UniqueKeyUtil.GetGuid();
                await ctx.UserAuths.AddAsync(newUserAuth);
                await ctx.SaveChangesAsync();
                dstNewUserAuth = newUserAuth;
                return true;
            });
            if (!AuthAccountflag)
            {
                return (-2, null, null);
            }
        }

        await ConfigurePermissionByUserType((UserType)dstNewUser.UserType, dstNewUser.Identifier);
        await ctx.SaveChangesAsync();
        await dbContextTransaction.CommitAsync();
        return (0, dstNewUser, dstNewUserAuth);
    }

    /// <summary>
    /// 绑定用户父级ID
    /// </summary>
    /// <param name="ctx">数据库Context</param>
    /// <param name="UserID">待绑定的用户ID</param>
    /// <param name="ParentIndentifier">父级账户唯一标识符</param>
    /// <returns>0 成功,-1 绑定失败</returns>
    public static async Task<int> BindParentAccountAsync(VideoShareContext ctx, long? UserID, string? ParentIndentifier)
    {
        if (UserID is null || ParentIndentifier.IsNullOrEmpty())
        {
            return -1;
        }

        var user = await ctx.Users.FirstOrDefaultAsync(a => a.Id == UserID && a.IsDeny == false && a.IsDelete == false);
        if (user is null)
        {
            return -1;
        }

        var parentUser = await ctx.Users.FirstOrDefaultAsync(a => a.Identifier == ParentIndentifier && a.IsDeny == false && a.IsDelete == false);
        if (parentUser is null)
        {
            return -1;
        }

        user.Pid = parentUser.Id;
        user.Level = parentUser.Level + 1;
        await ctx.SaveChangesAsync();

        return 0;
    }

    /// <summary>
    /// 根据用户类型添加权限
    /// </summary>
    /// <param name="userType">用户类型</param>
    /// <param name="UserIdentifier">用户标识符</param>
    public static async Task ConfigurePermissionByUserType(UserType userType, string UserIdentifier)
    {
        switch (userType)
        {
            case UserType.Doctor:
            {
                //todo: 添加相应权限，例如医生可以关联父账号
                //获取作用域服务,通过 IEnforcerProvider,获取具体的权限执行器,获取 Enforcer 实例,最后异步添加策略
                await App.GetScopeService<IEnforcerProvider>().GetEnforcer().AddGroupingPolicyAsync(UserIdentifier, "doctor");
                break;
            }
            case UserType.Hospital:
            case UserType.ServiceCenter:
            case UserType.Agency:
            {
                break;
            }
        }
    }
}