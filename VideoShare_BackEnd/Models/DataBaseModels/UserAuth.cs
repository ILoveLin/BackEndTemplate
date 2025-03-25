using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VideoShare_BackEnd.Models.DataBaseModels;

[Table("user_auth")]
[Index("AuthType", "AuthAccount", Name = "user_auth_pk_2", IsUnique = true)]
[Index("UserId", Name = "user_auth_user_id_index")]
public partial class UserAuth
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    /// <summary>
    /// 认证类型
    /// </summary>
    [Column("auth_type")]
    public int AuthType { get; set; }

    /// <summary>
    /// 认证账户
    /// </summary>
    [Column("auth_account")]
    public string AuthAccount { get; set; } = null!;

    /// <summary>
    /// 登录凭证
    /// </summary>
    [Column("auth_credential")]
    public string AuthCredential { get; set; } = null!;
}
