using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core.ServiceInternal
{
    /// <summary>
    /// 内部服务，用来注册 windows service
    /// </summary>
    public class InternalService : ServiceBase
    {
        private readonly MicroServiceBase service;
        /// <summary>
        /// Windows 服务构造方法
        /// </summary>
        /// <param name="service"></param>
        public InternalService(MicroServiceBase service)
        {
            this.service = service;
            this.ServiceName = this.service.ServiceName;
        }

        /// <summary>
        /// 服务启动时执行
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            this.service.InternalRun();
        }

        /// <summary>
        /// 服务停止时执行
        /// </summary>
        protected override void OnStop()
        {
            this.service.Stop();
        }
    }
}
