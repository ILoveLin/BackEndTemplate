using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VideoShare_BackEnd.Models.DataBaseModels;

[Table("casbin_rule")]
[Index("Ptype", Name = "IX_casbin_rule_ptype")]
[Index("V0", Name = "IX_casbin_rule_v0")]
[Index("V1", Name = "IX_casbin_rule_v1")]
[Index("V2", Name = "IX_casbin_rule_v2")]
[Index("V3", Name = "IX_casbin_rule_v3")]
[Index("V4", Name = "IX_casbin_rule_v4")]
[Index("V5", Name = "IX_casbin_rule_v5")]
public partial class CasbinRule
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ptype")]
    public string? Ptype { get; set; }

    [Column("v0")]
    public string? V0 { get; set; }

    [Column("v1")]
    public string? V1 { get; set; }

    [Column("v2")]
    public string? V2 { get; set; }

    [Column("v3")]
    public string? V3 { get; set; }

    [Column("v4")]
    public string? V4 { get; set; }

    [Column("v5")]
    public string? V5 { get; set; }
}
