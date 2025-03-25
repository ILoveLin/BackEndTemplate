using System.Runtime.CompilerServices;
using BackEndTemplate.Models.DataBaseModels.Context;
using Microsoft.EntityFrameworkCore;
using Polly;
using Yitter.IdGenerator;

namespace BackEndTemplate.Utils.UniqueKeyUtils
{
    public class UniqueKeyUtil
    {
        //该方法同一时间只允许一个线程使用
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string GetGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        //该方法同一时间只允许一个线程使用
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static long GetSnowID()
        {
            return YitIdHelper.NextId();
        }
        
        
        //用于在向数据库插入唯一值时，数据库返回重复键时，重试，重试指定次数后依然失败返回false，成功返回true
        //本函数只处理重复键异常，其他异常将会正常抛出
        public static async Task<bool> UniqeKeyRetry(ILogger logger, VideoShareContext ctx, Func<Task<bool>> action, int retryTotalCount = 10)
        {
            var retryPolicy = Policy.Handle<DbUpdateException>(exception =>
            {
                return exception.InnerException.Message.Contains("重复键");
            }).RetryAsync(retryTotalCount, (exception, retryCount) =>
            {
                ctx.ChangeTracker.Clear();
                logger.LogWarning($"重试第{retryCount}次,由于{exception.InnerException.Message}");
            });
            var res= await retryPolicy.ExecuteAndCaptureAsync(action);
            if (!res.Result)
            {
                if (res.FinalException is DbUpdateException && res.FinalException.InnerException.Message.Contains("重复键"))
                {
                    return false;
                }
                else
                {
                    throw res.FinalException;
                }
            }
            return res.Result;
        }
    }
}