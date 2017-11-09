using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    /// <summary>
    /// 默认提供的服务健康模块
    /// </summary>
    public class HealthModule : Nancy.NancyModule
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="serviceStatus"></param>
        public HealthModule(ServiceStatus serviceStatus)
        {
            //注册 /health 接口用于输出服务状态
            Get[serviceStatus.GetValue<string>(ServiceStatus.KEY_HEALTH_URL)] = _ => Response.AsJson(serviceStatus.GetStatus());
        }
    }
}
