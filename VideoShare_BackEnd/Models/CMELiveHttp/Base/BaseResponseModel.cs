namespace VideoShare_BackEnd.Models.CMELiveHttp.Base;

public class CMELive_ResponseModelBase
{
    public int? code { get; set; }
    
    public string? msg { get; set; }
}

public class CMELive_BaseResponseModel : CMELive_ResponseModelBase
{
    public object? result { get; set; }
}