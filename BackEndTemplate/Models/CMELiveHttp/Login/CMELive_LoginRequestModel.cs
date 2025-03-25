namespace BackEndTemplate.Models.CMELiveHttp.Login;

public class CMELive_LoginRequestModel
 {
     public CMELive_LoginUserModel? user { get; set; }=new();
 }
 

public class CMELive_LoginUserModel()
{
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
}