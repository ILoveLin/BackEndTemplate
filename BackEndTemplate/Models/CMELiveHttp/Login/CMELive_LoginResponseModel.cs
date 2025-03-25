using BackEndTemplate.Models.CMELiveHttp.Base;

namespace BackEndTemplate.Models.CMELiveHttp.Login;

public class CMELive_LoginResponseModel : CMELive_ResponseModelBase
{
    public CMELive_LoginResponseModelResult result { get; set; }
}

public class CMELive_LoginResponseModelResult
{
    public string token { get; set; }
}