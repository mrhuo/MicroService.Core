using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    /// <summary>
    /// Api 通用返回值
    /// </summary>
    public class RestResult
    {
        /// <summary>
        /// 默认构造方法
        /// </summary>
        public RestResult() { }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="msg"></param>
        public RestResult(bool ret, string msg) : this(ret, msg, null) { }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public RestResult(bool ret, string msg, object data)
        {
            this.ret = ret;
            this.msg = msg;
            this.data = data;
        }
        /// <summary>
        /// 成功标识
        /// </summary>
        public bool ret { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 附加数据
        /// </summary>
        public object data { get; set; }
    }

    /// <summary>
    /// Api 返回值扩展方法
    /// </summary>
    public static class RestResultExtensions
    {
        /// <summary>
        /// 使用一个消息字符串初始化 RestResult
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static RestResult ToErrorResult(this string msg)
        {
            return new RestResult(false, msg);
        }

        /// <summary>
        /// 使用一个消息字符串初始化 RestResult
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static RestResult ToOkResult(this string msg)
        {
            return new RestResult(true, msg);
        }

        /// <summary>
        /// 使用一个消息字符串初和附加数据始化 RestResult
        /// </summary>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static RestResult ToOkResult(this object data, string msg = "OK")
        {
            return new RestResult(true, msg, data);
        }
    }
}
