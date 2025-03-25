using Newtonsoft.Json;

namespace BackEndTemplate.Models.ControllerModels
{
    public class ResponseMsg
    {
        /// <summary>
        /// 响应代码
        /// </summary>
        public ResponseCode code;

        /// <summary>
        /// 响应说明
        /// </summary>
        public string msg;

        /// <summary>
        /// 响应唯一标识符
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] //当为null时json序列化时忽略此变量
        public string traceID;

        /// <summary>
        /// 响应结果
        /// </summary>
        public object result = new object();
    }
}