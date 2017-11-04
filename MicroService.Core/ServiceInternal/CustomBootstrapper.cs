using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    /// <summary>
    /// Nancy 默认启动器
    /// </summary>
    internal class CustomBootstrapper : DefaultNancyBootstrapper
    {
        private readonly MicroServiceBase service;
        private readonly IEnumerable<ModuleRegistration> modules;
        private readonly Action<MicroServiceBase, TinyIoCContainer> prepareBeforeRun;
        private readonly Action<MicroServiceBase, IPipelines, NancyContext> processPipelines;
        private readonly bool useCors;

        public CustomBootstrapper(
            MicroServiceBase service,
            IEnumerable<ModuleRegistration> modules,
            Action<MicroServiceBase, TinyIoCContainer> prepareBeforeRun,
            Action<MicroServiceBase, IPipelines, NancyContext> processPipelines,
            bool useCors)
        {
            this.service = service;
            this.modules = modules;
            this.prepareBeforeRun = prepareBeforeRun;
            this.useCors = useCors;
            this.processPipelines = processPipelines;
        }

        /// <summary>
        /// 请求开始时，设置跨域请求
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pipelines"></param>
        /// <param name="context"></param>
        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            this.processPipelines(service, pipelines, context);
            if (this.useCors)
            {
                pipelines.AfterRequest.AddItemToEndOfPipeline(x =>
                {
                    x.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    x.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT,OPTIONS");
                });
            }
        }

        /// <summary>
        /// Nancy 框架启动执行
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pipelines"></param>
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            //将 ServiceStatus 注册
            container.Register(this.service.ServiceStatus);
            //将自定义的 Module 注册到 Nancy 容器中
            if (this.modules != null && this.modules.Count() != 0)
            {
                base.RegisterRequestContainerModules(container, this.modules);
            }
            MicroServiceBase.container = container;
            this.prepareBeforeRun(service, container);
            base.ApplicationStartup(container, pipelines);
        }
    }
}
