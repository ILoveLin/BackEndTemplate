
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Polly;
using VideoShare_BackEnd.Utils.NullUtils;

namespace VideoShare_BackEnd.Utils;

public static class HttpExtendMethods
{
     /// <summary>
    /// 发送Http请求
    /// </summary>
    /// <param name="Request">Http请求体</param>
    /// <param name="RestClient">Http请求Client</param>
    /// <param name="RetryCount">重试次数</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>返回响应结果</returns>
    /// <exception cref="Exception">全部次数重试完成后依然无法获取http响应，抛出异常</exception>
    public static async Task<T> SendAsync<T>(this RestRequest Request, RestClient RestClient, int RetryCount = 3)
    {
        if (RestClient is null)
        {
            return default;
        }
        
        //获取一个日志记录器
        var logger = App.GetService<ILogger<T>>();
        //记录请求的执行时间
        var stopwatch = new Stopwatch();
        //获取请求的日志信息
        var RequestParamsStr = GetRequestLogStr<T>(Request,RestClient);
        //获取请求的日志信息
        var result = await Policy.Handle<Exception>()//处理所有异常。
            .OrResult<T>(result => { return result == null; })//处理返回结果为null的情况
            .WaitAndRetryAsync(
                retryCount: RetryCount,
                //重试间隔时间，每次重试间隔时间递增
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 200),
                //每次重试时记录日志
                onRetry: (delegateResult, delayTime, retryCount, context) =>
                {
                    string str = delegateResult.Exception != null ? $"{delegateResult.Exception.Message}" : JsonConvert.SerializeObject(delegateResult.Result);
                    logger.LogWarning($"[HttpUtil] [{Request.Resource}] {delayTime.TotalSeconds}秒后进行第{retryCount}次重试，原因: {str}");
                })
            //执行请求并捕获结果
            .ExecuteAndCaptureAsync(async () =>
            {
                stopwatch.Restart();
                //执行请求并获取响应
                var restResponse = await RestClient.ExecuteAsync<JObject>(Request);
                stopwatch.Stop();

                if (restResponse?.IsSuccessful is false)
                {
                    if (restResponse.ErrorException is not null)
                    {
                        throw restResponse.ErrorException;
                    }
                    return default;
                }

                logger.LogInformation(
                    $"[HttpUtil] [SendAsync] [Url: {(restResponse.ResponseUri.Query.IsNullOrEmpty() ? restResponse.ResponseUri.OriginalString : restResponse.ResponseUri.OriginalString.Replace(restResponse.ResponseUri.Query, ""))}] [Scheme: {restResponse.ResponseUri.Scheme}] [Method: {Request.Method}] [StatusCode: {(int)restResponse.StatusCode}] [ConsumingTime: {stopwatch.Elapsed}] {RequestParamsStr} [ResponseContent: ({restResponse.Content})]");

                return JsonConvert.DeserializeObject<T>(restResponse.Content);
            });
        //处理最终结果
        if (result.FinalException != null)
        {
            logger.LogError($"[HttpUtil] [SendAsync] [Url:{Request.Resource}] {RequestParamsStr} 请求失败，原因: {result.FinalException.Message}");
            throw result.FinalException;
        }

        return result.Result;
    }
     
    private static string GetRequestLogStr<T>(RestRequest Request,RestClient RestClient)
    {
        var queryStr = RestClient.GetRequestQuery(Request);
        queryStr = queryStr == null ? "null" : queryStr;
        var jsonParams = Request.Parameters.OfType<JsonParameter>().FirstOrDefault()?.Value;
        string body = "null";
        if (jsonParams != null)
        {
            body = JsonConvert.SerializeObject(jsonParams);
        }

        return $"[RequestParams: (Url：{queryStr},Body：{body})]";
    }
}