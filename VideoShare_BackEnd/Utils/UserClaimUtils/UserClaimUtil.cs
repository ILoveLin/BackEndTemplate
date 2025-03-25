using VideoShare_BackEnd.Models.User;

namespace VideoShare_BackEnd.Utils.UserClaimUtils
{
    public class UserClaimUtil
    {
        public static UserClaim GetUserClaim(HttpContext httpContext)
        {
            httpContext.Items.TryGetValue("UserClaim", out object value);
            return value as UserClaim;
        }
    }
}