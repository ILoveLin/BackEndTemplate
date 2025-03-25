namespace BackEndTemplate.Models.User;

public class UserClaim
{
    public long UserID { get; set; } = -1;

    public long? UserPID { get; set; } = null;

    public int? UserLevel { get; set; } = null;

    public string UserGroup { get; set; }
    
    public string UserInentifier { get; set; }
}