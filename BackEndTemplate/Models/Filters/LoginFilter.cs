using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BackEndTemplate.Models.ControllerModels;
using BackEndTemplate.Models.DataBaseModels.Context;
using BackEndTemplate.Models.User;
using BackEndTemplate.Utils.ResponseUtils;
using BackEndTemplate.Models.DataBaseModels;

namespace BackEndTemplate.Models.Filters
{
    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NoNeedLoginAttribute : Attribute, IFilterMetadata, IOrderedFilter
    {
        public int Order => 1003;
    }

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NeedLoginAttribute : Attribute, IActionFilter, IOrderedFilter
    {
        public int Order => 3;

        private readonly VideoShareContext ctx;

        public NeedLoginAttribute(VideoShareContext _ctx)
        {
            ctx = _ctx;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            IList<IFilterMetadata> filters = context.Filters;
            foreach (IFilterMetadata filter in filters)
            {
                if (filter is NoNeedLoginAttribute)
                {
                    return;
                }
            }

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = ResponseUtil.ResponseMsg(ResponseCode.LoginInvalid);
            }


            UserClaim userClaim = CheckAuthenticateAsync(context.HttpContext.User, ctx).GetAwaiter().GetResult();
            if (userClaim != null)
            {
                context.HttpContext.Items.TryAdd("UserClaim", userClaim);
                return;
            }
            else
            {
                context.Result = ResponseUtil.ResponseMsg(ResponseCode.LoginInvalid);
            }
        }

        public async Task<UserClaim> CheckAuthenticateAsync(ClaimsPrincipal claimsPrincipal, VideoShareContext ctx)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                UserClaim userClaim = new UserClaim();
                var list = claimsPrincipal.Claims.ToList();
                foreach (var iter in list)
                {
                    switch (iter.Type)
                    {
                        case UserClaimType.UserID:
                            {
                                userClaim.UserID = -1;
                                if (long.TryParse(iter.Value, out long id))
                                {
                                    userClaim.UserID = id;
                                }
                                break;
                            }
                        case UserClaimType.UserPID:
                            {
                                userClaim.UserPID = -1;
                                if (long.TryParse(iter.Value, out long pid))
                                {
                                    userClaim.UserPID = pid;
                                }
                                break;
                            }
                        case UserClaimType.UserLevel:
                            {
                                userClaim.UserLevel = -1;
                                if (int.TryParse(iter.Value, out int level))
                                {
                                    userClaim.UserLevel = level;
                                }
                                break;
                            }
                        case UserClaimType.UserGroup:
                            {
                                userClaim.UserGroup = iter.Value;
                                break;
                            }
                        case UserClaimType.UserIdentifier:
                        {
                            userClaim.UserInentifier = iter.Value;
                            break;
                        }
                    }
                }

                bool flag = await ctx.Users.AnyAsync(a => a.Id == userClaim.UserID&&a.IsDelete==false&&a.IsDeny==false);
                return flag ? userClaim : null;
            }

            return null;
        }
    }
}