using RestSharp;
using VideoShare_BackEnd.Models.CMELiveHttp.GetUserInfo;
using VideoShare_BackEnd.Models.CMELiveHttp.Login;

namespace VideoShare_BackEnd.Utils.CMELiveUtils;

public static class CMELiveHttpUtil
{
    public static async Task<CMELive_LoginResponseModel> LoginAsync(CMELive_LoginRequestModel requestModel)
    {
        var request = new RestRequest("api/user/login");
        request.Method = Method.Post;
        request.AddBaseParam();
        request.AddJsonBody(requestModel);
        //this是扩展方法的标志
        //表示该方法扩展RestRequest类型，所有RestRequest实例均可调用此方法。
        //参数App.CMELive_HttpClient代表调用该方法的RestRequest实例（即原始RestRequest对象）
        return await request.SendAsync<CMELive_LoginResponseModel>(App.CMELive_HttpClient);
    }
    
    public static async Task<CMELive_GetUserInfoResponseModel> GetUserInfoAsync(string token)
    {
        var request = new RestRequest("api/user/getUserInfo");
        request.Method = Method.Get;
        request.AddBaseParam();
        request.AddOrUpdateHeader("Authorization", $"Bearer {token}");
        
        return await request.SendAsync<CMELive_GetUserInfoResponseModel>(App.CMELive_HttpClient);
    }


    private static void AddBaseParam(this RestRequest request)
    {
        request.AddQueryParameter("ver","1.1");
        request.AddQueryParameter("lang","zh-cn");
        request.AddQueryParameter("plf","199");
    }
}