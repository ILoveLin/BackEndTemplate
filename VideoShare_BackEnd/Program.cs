using Asp.Versioning;
using Casbin;
using Casbin.AspNetCore.Authorization;
using Casbin.AspNetCore.Authorization.Transformers;
using Casbin.Persist.Adapter.EFCore;
using Casbin.Persist.Adapter.EFCore.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VideoShare_BackEnd.Models.Conventions;
using VideoShare_BackEnd.Models.DataBaseModels;
using VideoShare_BackEnd.Models.DataBaseModels.Context;
using VideoShare_BackEnd.Models.Filters;
using VideoShare_BackEnd.Models.Middlewares;
using VideoShare_BackEnd.Models.Providers;
using VideoShare_BackEnd.Models.User;
using VideoShare_BackEnd.Utils.DirectoryUtils;
using VideoShare_BackEnd.Utils.NullUtils;
using VideoShare_BackEnd.Utils.SecurityUtil;
using Yitter.IdGenerator;
using ILogger = Serilog.ILogger;

namespace VideoShare_BackEnd
{
    public partial class Program
    {
        private static WebApplicationBuilder? builder;

        private static WebApplication? app;

        public static void Main(string[] args)
        {
            App.AppPath = AppDomain.CurrentDomain.BaseDirectory;
            App.AppConfigFileName = "appsettings.json";
            App.AppConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, App.AppConfigFileName);
            App.LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");


            ConfigureSupportApiVersion();


            try
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(new ConfigurationBuilder().SetBasePath(App.AppPath)
                        .AddJsonFile(App.AppConfigFileName)
                        .Build())
                    .CreateBootstrapLogger();
            }
            catch (Exception e)
            {
                DirectoryUtil.EnsurePathExist(App.LogPath);
                //因为日志没有初始化成功，所以手动写入日志文件
                File.WriteAllText(Path.Combine(App.LogPath, "logError.txt"), e.Message + "\n" + e.StackTrace);
                Console.Write(e.Message + "\n" + e.StackTrace);
                return;
            }

            try
            {
                BuilderConfig(args);
                
                if (!AppConfig()) Log.Logger.Fatal("程序配置异常！");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "程序异常退出！");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        private static void BuilderConfig(string[] args)
        {
            builder = WebApplication.CreateBuilder(args);

            #region 配置日志

            builder.Services.AddSerilog((serviceProvider, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(serviceProvider);
            });

            #endregion

            #region 配置HTTPS

            var pfxpath = builder.Configuration.GetValue<string>("PfxPath");
            var pfxkey = builder.Configuration.GetValue<string>("PfxKey");
            if (!NullUtilities.IsNullOrEmpty(pfxkey) && !NullUtilities.IsNullOrEmpty(pfxpath))
                builder.WebHost.UseKestrel(opts => { opts.ConfigureHttpsDefaults(options => { options.ServerCertificate = new X509Certificate2(pfxpath, SecurityUtil.Decrypt(pfxkey)); }); });

            #endregion

            #region 配置本地化

            builder.Services.AddJsonLocalization(opts => { opts.ResourcesPath = "I18N"; });
            builder.Services.AddRequestLocalization(opts =>
            {
                opts.DefaultRequestCulture = new RequestCulture("zh-cn");
                opts.SupportedCultures = [new CultureInfo("zh-cn"), new CultureInfo("en-us")];
                opts.SupportedUICultures = [new CultureInfo("zh-cn"), new CultureInfo("en-us")];
                opts.RequestCultureProviders = [new StringRequestCultureProvider()];
            });

            #endregion

            #region 配置版本管理

            var apiVersioningBuilder = builder.Services.AddApiVersioning(opts =>
            {
                opts.DefaultApiVersion = new ApiVersion(1, 0);
                opts.AssumeDefaultVersionWhenUnspecified = true;
                // 支持通过URL路径、查询字符串、请求头来指定版本,此处使用的是字符串
                opts.ApiVersionReader = new QueryStringApiVersionReader(App.BaseParamVerName);
                opts.ReportApiVersions = true;
            }).AddMvc(opts =>
            {
                //使用自定义的ApiVersionConventionBuilder替代默认的ApiVersionConventionBuilder
                opts.Conventions = new ApiVersionsBuilder();
            });

            #endregion

            #region 配置Filter，中间件

            builder.Services.Configure<ApiBehaviorOptions>(opt => { opt.SuppressModelStateInvalidFilter = true; });

            builder.Services.AddControllers(opts =>
            {
                opts.Filters[0] = new UnsupportMediaTypeFilter(); //使用自定义MediaType类型处理
                opts.Filters.Add<ResultFilter>();
                //如果使用insert方法将自定义过滤器插入首位，此时会违反内部过滤器执行顺序,这是内部会根据过滤器变量order大小来判断谁先执行(order越小，执行顺序越靠前)
                //因需要此过滤器的OnActionExecuted最后执行，所以使用insert插入首位
                opts.Filters.Add<ExceptionFilter>(); //使用自定义异常处理
                // opts.Filters.Add<BaseParamsFilter>(); //全局增加公共参数
                opts.Filters.Add<InputModelFilter>(); //使用自定义模型输入处理
                opts.Filters.Add<NeedLoginAttribute>(); //全局启用登录
            }).AddNewtonsoftJson(opts =>
            {
                opts.AllowInputFormatterExceptionMessages = true;
                opts.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"; // 设置UTC时间格式
                opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // 忽略循环引用
                opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 返回的格式按照小驼峰形式
                opts.SerializerSettings.NullValueHandling = NullValueHandling.Include; // 包含空值
            });

            #endregion

            #region 配置跨域

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
                        .SetIsOriginAllowed(str => { return true; });
                });
            });

            #endregion

            #region 配置MediatR,MediatR是一个实现中介者模式的库
            //向.NET Core的依赖注入容器注册MediatR服务
            builder.Services.AddMediatR(cfg =>
            {   //会扫描指定程序集（这里是Program类所在的程序集）中的所有MediatR处理程序（比如IRequestHandler<>和INotificationHandler<>的实现），并将它们自动注册到依赖注入容器中
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            #endregion

            #region 配置数据库

            builder.Services.AddDbContext<VideoShareContext>(op =>
            {
                op.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                // op.EnableSensitiveDataLogging();//postgresql使用 日志记录具体数据
            });

            #endregion

            #region 配置JwtBearer Token

            // 取消注释以下内容和app.UseAuthorization();来启用登录验证，同时可取消注释本文件中的opts.Filters.Add<NeedLoginAttribute>();来启用全局登录

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true, //是否验证Issuer
                        ValidateAudience = true, //是否验证Audience
                        ValidateIssuerSigningKey = true, //是否验证SecurityKey
                        ValidateLifetime = true, //是否验证失效时间
                        ValidIssuer = builder.Configuration["Jwt:Issuer"], //发行人Issuer
                        ValidAudience = builder.Configuration["Jwt:Audience"], //订阅人Audience
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])), //SecurityKey
                        ClockSkew = TimeSpan.FromSeconds(30), //过期时间容错值，解决服务器端时间不同步问题（秒）
                        RequireExpirationTime = true,
                    };
                });

            #endregion

            #region 配置Casbion权限控制

            //创建默认casbin_rule表
            // var context = new CasbinDbContext<long>(new DbContextOptionsBuilder<CasbinDbContext<long>>()
            //     .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection1"))
            //     .Options);
            // context.Database.EnsureCreated();
            
            builder.Services.AddDbContext<CasbinDbContext<long>>(opts =>
            {
                opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddScoped<IEnforcerProvider, MyEnforcerProvider>();
            builder.Services.AddCasbinAuthorization(opts =>
            {
                opts.PreferSubClaimType = UserClaimType.UserIdentifier;
                opts.DefaultModelPath = "./rbac.conf"; 
                opts.DefaultRequestTransformerType = typeof(BasicRequestTransformer);
            });

            #endregion

#if DEBUG

            #region 配置Swagger

            if (builder.Environment.IsDevelopment())
            {
                apiVersioningBuilder.AddApiExplorer(opts => { opts.GroupNameFormat = "'v'VVV"; });
                builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
                builder.Services.AddSwaggerGen();
            }

            #endregion

#endif
        }

        private static bool AppConfig()
        {
            app = builder.Build();
            //注册自定义中间件，GetSnowID替换TraceIdentifier并注入到 HTTP 请求上下文中
            app.UseMiddleware<TraceIdMiddleware>();

            #region 使Request可重复读取，启用请求体缓冲功能
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next(context);
            });

            #endregion

#if DEBUG

            #region 添加Swagger

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in app.DescribeApiVersions())
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = description.GroupName.ToUpperInvariant();
                        options.SwaggerEndpoint(url, name);
                    }
                });
            }

            #endregion

#endif
            // 取消注释下一行来启用WebSocketServer，同时注释项目目录Contollers/WebSocket文件夹下的[NonController]
            // app.UseWebSockets();
            app.UseSerilogRequestLogging(SerilogRequestLoggingConfigure);
            app.UseHttpsRedirection();//Https重定向
            app.UseRequestLocalization();//请求本地化
            app.UseRouting();//启用端点路由匹配，将请求映射到控制器/API 端点
            app.UseCors();//启用跨域资源共享（CORS）
            // 取消注释下一行和本文件中Region名为(配置JwtBearer Token)里的内容来启用登录验证，同时可取消注释本文件中的opts.Filters.Add<NeedLoginAttribute>();来启用全局登录
            app.UseAuthentication();//启用身份认证机制，解析请求中的身份凭证（如JWT Token、Cookies）
            app.UseCasbinAuthorization();//基于Casbin策略的细粒度权限验证
            app.MapControllers();//注册控制器路由映射
            if (!AppBeforeRunningFun()) return false;

            app.Run();
            return true;
        }

        private static bool AppBeforeRunningFun()
        {
            var logger = app.Services.GetRequiredService<ILogger>();
            logger.Information($"启动时间{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

            App.Service = app.Services;

            App.AppSettings = app.Configuration;

            #region 配置雪花算法

            #region 获取雪花算法WorkerID

            var snowConfiguration = new ConfigurationBuilder().SetBasePath(App.AppPath)
                .AddJsonFile("SnowFlake.json").Build();
            var snowWorkerIDStr = snowConfiguration["SnowFlakeWorkerID"];
            if (!ushort.TryParse(snowWorkerIDStr, out var snowWorkerID))
            {
                logger.Error("雪花算法WorkerID解析失败！请检查SnowFlake.json！");
                return false;
            }

            if (snowWorkerID < 0 || snowWorkerID > 63)
            {
                logger.Error("雪花算法WorkerID允许的值为0-63！请检查SnowFlake.json！");
                return false;
            }

            var options = new IdGeneratorOptions(snowWorkerID);

            // 默认值6，限定 WorkerId 最大值为2^6-1，即默认最多支持64个节点。
            //注意该值在已有雪花id的数据存储后，只能增大，禁止减小
            // options.WorkerIdBitLength = 10;

            // 默认值6，限制每毫秒生成的ID个数。若生成速度超过5万个/秒，建议加大 SeqBitLength 到 10。
            //注意该值在已有雪花id的数据存储后，只能增大，禁止减小
            options.SeqBitLength = 10;
            //设置雪花算法基准时间
            options.BaseTime= DateTime.Parse("2025-03-14 00:00:00");
            logger.Information($"当前程序雪花算法WorkerID为{snowWorkerID}，SeqBitLength为{options.SeqBitLength}，基准时间为{options.BaseTime}");
            YitIdHelper.SetIdGenerator(options);

            #endregion

            #region 确保程序启动时间大于上一次停止时间

            //确保程序启动时间大于上一次停止时间
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/start_lock"))
            {
                var timeStr = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/start_lock");
                if (!DateTime.TryParse(timeStr, out var time))
                {
                    logger.Error("时间检验失败！请检查start_lock文件！");
                    return false;
                }

                var now = DateTime.Now;
                if (now.CompareTo(time) <= 0)
                {
                    logger.Error("时间检验失败！程序启动的时间必须大于上次程序停止的时间，请检查电脑时间是否正确！");
                    logger.Error($"上次程序停止时间为{time.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                    return false;
                }
            }

            #endregion

            #region 程序退出时将当前时间保存进start_lock文件中,注册一个全局事件，用于在进程退出时执行特定操作
            
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            #endregion

            #endregion

            return true;
        }
    }
}