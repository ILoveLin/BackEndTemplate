using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using BackEndTemplate.Utils.NullUtils;

namespace BackEndTemplate
{
    public partial class Program
    {
        private static void ConfigureSupportApiVersion()
        {
            App.ApiVersions.Add(1.0);
        }
        
        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}/start_lock", now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            Log.Information("程序退出!");
        }

        private static void SerilogRequestLoggingConfigure(RequestLoggingOptions options)
        {
            // options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.MessageTemplate =
                "[{IP}] [{Scheme}] [{RequestMethod}] [{RequestPath}] responded {StatusCode} in {Elapsed:0.0000} ms {RequestParams}{ResponseParams}{ExceptionMsg}";

            options.GetLevel = (httpContext, arg2, arg3) =>
            {
                if (httpContext.Items.ContainsKey("ExceptionMsg")) return LogEventLevel.Error;

                return LogEventLevel.Information;
            };
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.Items.TryGetValue("ExceptionMsg", out var value) && !((string)value).IsNullOrEmpty())
                {
                    var dst = $"\nException: {value}  ";
                    diagnosticContext.Set("ExceptionMsg", dst);
                }
                else
                {
                    diagnosticContext.Set("ExceptionMsg", "");
                }


                diagnosticContext.Set("IP", $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}");
                diagnosticContext.Set("Scheme", $"{httpContext.Request.Scheme}");

                #region RequestParams

                var UrlRequestParams = httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.ToString() : null;
                string BodyRequestParams = null;
                if (httpContext.Request.Body.Length != 0)
                {
                    httpContext.Request.Body.Position = 0;
                    var streamReader = new StreamReader(httpContext.Request.Body, Encoding.UTF8);
                    var result = streamReader.ReadToEndAsync().Result;
                    httpContext.Request.Body.Position = 0;
                    try
                    {
                        var jToken = JToken.Parse(result);
                        BodyRequestParams = jToken.HasValues ? jToken.ToString(Formatting.None) : null;
                    }
                    catch (Exception e)
                    {
                        BodyRequestParams = result;
                    }
                }

                if (UrlRequestParams != null || BodyRequestParams != null)
                {
                    var RequestParams = $"RequestParams【url:{UrlRequestParams ?? "null"},body:{BodyRequestParams ?? "null"}】  ";
                    diagnosticContext.Set("RequestParams", RequestParams);
                }
                else
                {
                    diagnosticContext.Set("RequestParams", "");
                }

                #endregion

                #region ResponseParams

                if (httpContext.Items.TryGetValue("ResponseMsg", out var responseMsg) && responseMsg != null)
                {
                    var ResponseParams = $"ResponseParams【{JObject.FromObject(responseMsg).ToString(Formatting.None)}】  ";
                    diagnosticContext.Set("ResponseParams", ResponseParams);
                }
                else
                {
                    diagnosticContext.Set("ResponseParams", "");
                }

                #endregion
            };
        }

        public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            private readonly IApiVersionDescriptionProvider provider;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
            {
                this.provider = provider;
            }

            public void Configure(SwaggerGenOptions options)
            {
                foreach (var description in provider.ApiVersionDescriptions)
                    options.SwaggerDoc(
                        description.GroupName,
                        new OpenApiInfo
                        {
                            Title = $"Example API {description.ApiVersion}",
                            Version = description.ApiVersion.ToString()
                        });
            }
        }
    }
}