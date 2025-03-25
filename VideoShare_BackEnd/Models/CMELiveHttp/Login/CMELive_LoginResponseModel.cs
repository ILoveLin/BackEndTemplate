using VideoShare_BackEnd.Models.CMELiveHttp.Base;

namespace VideoShare_BackEnd.Models.CMELiveHttp.Login;

public class CMELive_LoginResponseModel : CMELive_ResponseModelBase
{
    public CMELive_LoginResponseModelResult result { get; set; }
}

public class CMELive_LoginResponseModelResult
{
    public string token { get; set; }
}