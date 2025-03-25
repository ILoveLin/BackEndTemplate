using VideoShare_BackEnd.Models.CMELiveHttp.Base;

namespace VideoShare_BackEnd.Models.CMELiveHttp.GetUserInfo;

public class CMELive_GetUserInfoResponseModel: CMELive_ResponseModelBase
{
    public CMELive_GetUserInfoResponseModelResult result { get; set; }
}

public class CMELive_GetUserInfoResponseModelResult
{
    public CMELive_User user { get; set; }
    
    public List<CMELive_AddressManage> AddressManages { get; set; }
}

public class CMELive_User
{
    public long Id { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 地区管理号
    /// </summary>
    public long? AmNumber { get; set; }

    /// <summary>
    /// 品牌
    /// </summary>
    public int Brand { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// 负责人
    /// </summary>
    public string? Manager { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 是否使用
    /// </summary>
    public bool IsUse { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDelete { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}

public class CMELive_AddressManage
{
    public long Id { get; set; }

    /// <summary>
    /// 地区管理号
    /// </summary>
    public long? AmNumber { get; set; }

    /// <summary>
    /// 地区
    /// </summary>
    public string Address { get; set; } = null!;

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDelete { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}