namespace BackEndTemplate.Models.ControllerModels
{
    public enum ResponseCode
    {
        ParamsCannotUse = -3, //参数不可用，原因 ：1.参数互相依赖，需要同时传入2.参数不能同时传入，冲突，不知使用哪个参数 3.多个参数中必须传入其中一个
        InvalidInput = -2, //无效输入。
        InnerException = -1, //内部异常。
        Success = 0,
        Failure = 1,
        Failure_NoData = 2, //中途参数查询无数据 1.传入的参数有误2.某些原因导致无法查出数据
        Failure_Operation = 3, //操作失败1.调用接口想要达成的目的无法完成
        NoDataCausedByUnknownReason = 4, //未知原因导致的无数据，1.连表查询无法确定原因
        TimeError = 5, //时间错误
        InsufficientPermissions = 6, //权限不足1.当前登录角色无权限做某事
        UniqueKeyError = 7, //唯一键错误，1.产生唯一键失败
        LoginInvalid = 8, //登录失效。
        UserCannotUse = 9, //用户不存在或禁用,可能原因： 1：用户不存在。2：用户被禁用。
    }
}