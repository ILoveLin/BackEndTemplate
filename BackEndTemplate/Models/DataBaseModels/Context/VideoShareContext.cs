using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using BackEndTemplate.Models.DataBaseModels;

namespace BackEndTemplate.Models.DataBaseModels.Context;

public partial class VideoShareContext : DbContext
{
    public VideoShareContext(DbContextOptions<VideoShareContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CasbinRule> CasbinRules { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAuth> UserAuths { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pk");

            entity.ToTable("user", tb => tb.HasComment("用户表"));

            entity.Property(e => e.Avatar).HasComment("头像");
            entity.Property(e => e.CreateTime).HasComment("创建时间");
            entity.Property(e => e.Group).HasComment("用户组");
            entity.Property(e => e.Identifier).HasComment("用户唯一标识符");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasComment("是否删除");
            entity.Property(e => e.IsDeny)
                .HasDefaultValue(false)
                .HasComment("是否禁用");
            entity.Property(e => e.Level).HasComment("用户层级");
            entity.Property(e => e.Phone).HasComment("手机号");
            entity.Property(e => e.Pid).HasComment("父ID");
            entity.Property(e => e.UserType).HasComment("用户类型");
            entity.Property(e => e.Username).HasComment("用户名");
        });

        modelBuilder.Entity<UserAuth>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_auth_pk");

            entity.Property(e => e.AuthAccount).HasComment("认证账户");
            entity.Property(e => e.AuthCredential).HasComment("登录凭证");
            entity.Property(e => e.AuthType).HasComment("认证类型");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
