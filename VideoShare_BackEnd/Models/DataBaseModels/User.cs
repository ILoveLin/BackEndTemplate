using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VideoShare_BackEnd.Models.DataBaseModels;

/// <summary>
/// 用户表
/// </summary>
[Table("user")]
[Index("Group", Name = "user_group_index")]
[Index("Level", Name = "user_level_index")]
[Index("Pid", Name = "user_pid_index")]
[Index("Identifier", Name = "user_pk_2", IsUnique = true)]
[Index("UserType", Name = "user_user_type_index")]
public partial class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 父ID
    /// </summary>
    [Column("pid")]
    public long? Pid { get; set; }

    /// <summary>
    /// 用户层级
    /// </summary>
    [Column("level")]
    public int Level { get; set; }

    /// <summary>
    /// 用户组
    /// </summary>
    [Column("group")]
    public string Group { get; set; } = null!;

    /// <summary>
    /// 用户唯一标识符
    /// </summary>
    [Column("identifier")]
    public string Identifier { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    [Column("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// 头像
    /// </summary>
    [Column("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Column("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("create_time")]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    [Column("is_deny")]
    public bool IsDeny { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    [Column("is_delete")]
    public bool IsDelete { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    [Column("user_type")]
    public int UserType { get; set; }
}
