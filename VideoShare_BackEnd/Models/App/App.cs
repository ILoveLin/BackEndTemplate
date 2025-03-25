using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using VideoShare_BackEnd.Models.WebSocket;

namespace VideoShare_BackEnd
{
    public static class App
    {
        //当前程序接口支持的版本
        public static List<double> ApiVersions { get;  set; } = new();

        //当前程序的工作目录
        public static string AppPath { get;  set; } = AppDomain.CurrentDomain.BaseDirectory;

        //当前程序的配置文件名
        public static string AppConfigFileName { get;  set; } = "appsettings.json";

        //当前程序的配置文件路径
        public static string AppConfigPath { get;  set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConfigFileName);

        //当前程序的日志文件路径
        public static string LogPath { get;  set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        //服务提供
        public static IServiceProvider Service { get;  set; } = null;

        //当前程序的设置
        public static IConfiguration AppSettings { get;  set; } = null;

        //WebSocketServer管理器
        public static WebSocketServerManager WebSocketServerManager { get;  set; } = new();

        
        public static RestClient CMELive_HttpClient { get;  set; } = new("https://www.cmejx.com:55000",configureSerialization: cfg => cfg.UseNewtonsoftJson());

        // 机密字符串使用此类型保存
        // public static SecureString SecureString;

        //全局参数
        public static string BaseParamVerName { get; private set; } = "ver";
        public static string BaseParamLangName { get; private set; } = "lang";

        // 从根容器解析服务
        public static T GetService<T>()
            where T : class
        {
            // 如果 Service 为 null 或服务未注册，可能返回 null 或抛出异常
            return Service?.GetRequiredService<T>();
        }
        // 从新作用域解析服务（需注意作用域释放）
        public static T GetScopeService<T>()
            where T : class
        {
            // 创建作用域，确保释放（此处并未做释放处理）
            // using (var scope = Service?.CreateScope())
            // {
            //     return scope?.ServiceProvider.GetRequiredService<T>();
            // }
            return Service?.CreateScope().ServiceProvider.GetRequiredService<T>();
        }
    }
}