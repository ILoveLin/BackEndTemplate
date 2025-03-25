namespace VideoShare_BackEnd.Models.User;


public enum UserType
{
    Unknown = -1, //未知
    Hospital = 1, //医院
    Doctor = 2, //医生
    ServiceCenter = 3, //服务中心
    Agency = 4 //代理商
}

public enum UserAuthType
{
    Default = 1,
    CMELive = 2,
}

public static class UserClaimType
{
    public const string UserID = "UserID";
    public const string UserPID = "UserPID";
    public const string UserLevel = "UserLevel";
    public const string UserGroup = "UserGroup";
    public const string UserIdentifier = "UserIdentifier";
}