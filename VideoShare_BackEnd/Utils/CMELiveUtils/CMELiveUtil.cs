using VideoShare_BackEnd.Models.User;

namespace VideoShare_BackEnd.Utils.CMELiveUtils;

public class CMELiveUtil
{
    /// <summary>
    /// 将CMELive用户角色ID转换为用户类型
    /// </summary>
    /// <param name="role">CMELive用户角色ID</param>
    /// <returns></returns>
    public static UserType MapRoleIDToUserType(long role)
    {
        switch (role)
        {
            case 0:
            case 1:
            {
                return UserType.Unknown;
            }
            case 2:
            {
                return UserType.Hospital;
            }
            case 3:
            {
                return UserType.ServiceCenter;
            }
            case 4:
            {
                return UserType.Agency;
            }
        }
        return UserType.Unknown;
    }
}