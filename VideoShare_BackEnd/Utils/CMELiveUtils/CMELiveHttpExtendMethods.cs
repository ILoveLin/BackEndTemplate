using VideoShare_BackEnd.Models.CMELiveHttp.Base;

namespace VideoShare_BackEnd.Utils.CMELiveUtils;

public static class CMELiveHttpExtendMethods
{
    public static bool CheckResponse(this CMELive_ResponseModelBase responseModel, bool ThrowException = true)
    {
        if (responseModel.code is not 0)
        {
            if (ThrowException)
            {
                throw new HttpRequestException(responseModel.msg);
            }

            return false;
        }
        return true;
    }
}