using MicroService.Core.ServiceInternal;
using Microsoft.Owin.Hosting;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Nancy.TinyIoc;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace MicroService.Core
{
    /// <summary>
    /// 微服务类
    /// </summary>
    public class MicroServiceBase : IDisposable
    {
        #region [Properties]
        /// <summary>
        /// 启动画面文字
        /// </summary>
        private string banner = Properties.Resources.DefaultBanner;
        /// <summary>
        /// 服务名称
        /// </summary>
        private string serviceName = "";
        /// <summary>
        /// 服务显示名称（windows service）
        /// </summary>
        private string serviceDisplayName = "";
        /// <summary>
        /// 服务绑定的 URL
        /// </summary>
        private string[] runningUrls;
        /// <summary>
        /// 服务状态
        /// </summary>
        private ServiceStatus serviceStatus;
        /// <summary>
        /// 启动画面开关
        /// </summary>
        private bool enableBanner = true;
        /// <summary>
        /// API 访问跨域开关
        /// </summary>
        private bool enableCors = true;
        /// <summary>
        /// 服务状态更新频率，默认10秒
        /// </summary>
        private int updateServiceStatusRate = 10 * UpdateRate.SECOND;

        /// <summary>
        /// 获取或者设置一个值，该值表示服务名称
        /// <para>仅在调用 Run 方法前可设置</para>
        /// </summary>
        public string ServiceName
        {
            get
            {
                return this.serviceName;
            }
            set
            {
                if (serviceRunning)
                {
                    WriteToLog("服务已运行，无法设置值 ServiceName");
                    return;
                }
                this.serviceName = value;
            }
        }

        /// <summary>
        /// 获取或者设置一个值，该值表示服务显示名称
        /// <para>仅在调用 Run 方法前可设置</para>
        /// </summary>
        public string ServiceDisplayName
        {
            get
            {
                return this.serviceDisplayName;
            }
            set
            {
                if (serviceRunning)
                {
                    WriteToLog("服务已运行，无法设置值 ServiceDisplayName");
                    return;
                }
                this.serviceDisplayName = value;
            }
        }

        /// <summary>
        /// 获取或者设置一个值，该值表示当前微服务运行对外开放的URL
        /// <para>格式如：http://localhost:9000;http://+:9001</para>
        /// <para>意思为：内网可访问9000端口，外网可访问9001端口。</para>
        /// <para>仅在调用 Run 方法前可设置</para>
        /// </summary>
        public string[] RunningUrls
        {
            get
            {
                var urls = this.runningUrls;
                if (urls != null && urls.Length > 0)
                {
                    //如果设置了，那么直接返回
                    return urls;
                }
                List<string> useUrls = new List<string>();
                var configUrls = this.GetConfig("server.urls");
                //既没有通过属性设置，也没有配置URL，就使用默认地址
                if (string.IsNullOrWhiteSpace(configUrls))
                {
                    WriteToLog("警告：appSettings/server.urls 没有配置，使用默认地址 http://127.0.0.1:8080");
                    useUrls.Add("http://127.0.0.1:8080");
                }
                //如果配置了，那么就从配置里读取
                else
                {
                    foreach (var item in configUrls.Split(';'))
                    {
                        useUrls.Add(item);
                    }
                }
                this.runningUrls = useUrls.ToArray();
                return this.runningUrls;
            }
            set
            {
                if (serviceRunning)
                {
                    WriteToLog("服务已运行，无法设置值 RunningUrls");
                    return;
                }
                this.runningUrls = value;
            }
        }

        /// <summary>
        /// 获取或者设置一个值，该值表示程序启动画面
        /// <para>仅在调用 Run 方法前可设置</para>
        /// </summary>
        public string Banner
        {
            get
            {
                return this.banner;
            }
            set
            {
                if (serviceRunning)
                {
                    WriteToLog("服务已运行，无法设置值 Banner");
                    return;
                }
                this.banner = value;
            }
        }

        /// <summary>
        /// 获取或者设置一个值，该值表示是否启用程序启动画面，默认 true
        /// <para>仅在调用 Run 方法前可设置</para>
        /// </summary>
        public bool EnableBanner
        {
            get
            {
                return this.enableBanner;
            }
            set
            {
                if (serviceRunning)
                {
                    WriteToLog("服务已运行，无法设置值 EnableBanner");
                    return;
                }
                this.enableBanner = value;
            }
        }

        /// <summary>
        /// 获取或者设置一个值，该值表示是否允许跨域请求，默认 true
        /// </summary>
        public bool EnableCors
        {
            get
            {
                return this.enableCors;
            }
            set
            {
                if (serviceRunning)
                {
                    WriteToLog("服务已运行，无法设置值 EnableCors");
                    return;
                }
                this.enableCors = value;
            }
        }

        /// <summary>
        /// 获取一个值，该值表示服务是否正在运行
        /// </summary>
        public bool IsServiceRunning
        {
            get
            {
                return serviceRunning;
            }
        }

        /// <summary>
        /// 获取或者设置一个值，该值表示服务状态的刷新时间，默认10秒，最小值5秒，最大1天
        /// <para>仅在调用 Run 方法前可设置</para>
        /// </summary>
        public int UpdateServiceStatusRate
        {
            get
            {
                return this.updateServiceStatusRate;
            }
            set
            {
                if (serviceRunning)
                {
                    WriteToLog("服务已运行，无法设置值 UpdateServiceStatusRate");
                    return;
                }
                if (value <= 5 * UpdateRate.SECOND)
                {
                    this.updateServiceStatusRate = 5 * UpdateRate.SECOND;
                    WriteToLog("警告：设置的服务状态更新间隔小于5秒，被重置为5秒");
                    return;
                }
                //如果更新时间超过1天，则设置为1天
                if (value >= UpdateRate.DAY)
                {
                    this.updateServiceStatusRate = UpdateRate.DAY;
                    WriteToLog("警告：设置的服务状态更新间隔超过1天，被重置为1天");
                    return;
                }
                this.updateServiceStatusRate = value;
            }
        }

        /// <summary>
        /// 获取一个值，该值表示当前运行的 Exe 文件名
        /// </summary>
        public string ExeFile
        {
            get
            {
                return Process.GetCurrentProcess().MainModule.FileName;
            }
        }

        /// <summary>
        /// 获取一个值，该值表示当前服务运行的目录
        /// </summary>
        public string RootPath
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
        }

        /// <summary>
        /// 获取一个值，该值表示服务当前的运行状态
        /// <para>每个微服务中都有一个 ServiceStatus 类，可以通过 /health 路径访问服务状态</para>
        /// </summary>
        public ServiceStatus ServiceStatus
        {
            get
            {
                return this.serviceStatus;
            }
        }

        /// <summary>
        /// 服务运行模式
        /// </summary>
        public RunningMode ServiceRunningMode { get; set; }

        /// <summary>
        /// 内部 OWIN 启动时使用的启动参数
        /// </summary>
        private StartOptions StartOptions
        {
            get
            {
                var options = new StartOptions();
                foreach (var item in this.RunningUrls)
                {
                    options.Urls.Add(item.Trim());
                }
                return options;
            }
        }
        #endregion

        #region [Events]
        /// <summary>
        /// 服务正在启动时执行的事件，可在这个时候对服务做一些扩展
        /// </summary>
        public event Action<MicroServiceBase, TinyIoCContainer> OnServiceStarting;
        /// <summary>
        /// 服务启动成功后执行的事件
        /// </summary>
        public event Action<MicroServiceBase> OnServiceStarted;
        /// <summary>
        /// 服务停止之前执行的事件
        /// </summary>
        public event Action<MicroServiceBase> OnServiceStoping;
        /// <summary>
        /// 服务停止之后执行的事件
        /// </summary>
        public event Action<MicroServiceBase> OnServiceStoped;
        /// <summary>
        /// 服务状态更新时执行的事件
        /// </summary>
        public event Action<MicroServiceBase, ServiceStatus> OnServiceStatusUpdating;
        /// <summary>
        /// 当请求到达Nancy模块时执行的事件
        /// </summary>
        public event Action<MicroServiceBase, IPipelines, NancyContext> OnRequestStart;
        #endregion

        #region [Fields]
        /// <summary>
        /// 一个标记
        /// </summary>
        private static bool serviceRunning = false;
        /// <summary>
        /// OWIN
        /// </summary>
        private IDisposable serverDisposable;
        /// <summary>
        /// Nancy bootstrapper override
        /// </summary>
        private CustomBootstrapper bootstrap;
        /// <summary>
        /// 日志记录器
        /// </summary>
        private log4net.ILog logger;
        /// <summary>
        /// 已注册模块
        /// </summary>
        private IEnumerable<ModuleRegistration> registedModules;
        /// <summary>
        /// Window 服务管理器
        /// </summary>
        private WindowsServiceManager serviceManager;
        /// <summary>
        /// 扩展的可用于构造 OWIN IAppBuilder 的方法列表
        /// </summary>
        private List<Action<IAppBuilder>> configActionList = new List<Action<IAppBuilder>>();

        /// <summary>
        /// Nancy ioc container
        /// </summary>
        protected internal static TinyIoCContainer container = new TinyIoCContainer();
        /// <summary>
        /// 停止服务器时使用的 TokenSource
        /// </summary>
        protected CancellationTokenSource cancellationTokenSource;
        /// <summary>
        /// Token
        /// </summary>
        private CancellationToken cancellationToken;
        #endregion

        #region [Constructors]
        /// <summary>
        /// 构造方法
        /// <para>默认使用 app.config/appSettings/server.urls 配置或 http://127.0.0.1:8080 作为 API 入口</para>
        /// </summary>
        /// <param name="serviceName"></param>
        public MicroServiceBase(string serviceName) : this(null, serviceName, null)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="urls">设置绑定 URL</param>
        /// <param name="serviceName">设置服务名称</param>
        public MicroServiceBase(string urls, string serviceName) : this(urls, serviceName, null)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="urls">包含多个IP时分号分割，如：http://localhost:8080;http://localhost:8081</param>
        /// <param name="serviceName"></param>
        /// <param name="serviceDisplayName"></param>
        public MicroServiceBase(string urls, string serviceName, string serviceDisplayName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                var ex = new Exception("ServiceName 不能为空！");
                WriteToLog("启动服务失败", ex);
                throw ex;
            }
            if (!string.IsNullOrWhiteSpace(urls))
            {
                this.runningUrls = urls.Split(';');
            }

            //设置日志记录器
            this.logger = log4net.LogManager.GetLogger(serviceName);

            this.serviceName = serviceName;
            this.serviceDisplayName = serviceDisplayName ?? this.serviceName;
            this.serviceStatus = new ServiceStatus(serviceName, string.Join(",", this.RunningUrls));

            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = this.cancellationTokenSource.Token;

            //默认绑定一些事件的处理方法，用于直接在子类中重写达到调用事件的目的
            this.OnServiceStarting += this.ServiceStartingDefaultAction;
            this.OnServiceStarted += this.ServiceStartedDefaultAction;
            this.OnServiceStoping += this.ServiceStopingDefaultAction;
            this.OnServiceStoped += this.ServiceStopedDefaultAction;
            this.OnServiceStatusUpdating += this.ServiceStatusUpdateDefaultAction;
            this.OnRequestStart += this.RequestStartDefaultAction;
            this.serviceManager = new WindowsServiceManager(this);
        }
        #endregion

        #region [Virtuals]
        /// <summary>
        /// 服务启动之前执行的事件
        /// </summary>
        protected virtual void ServiceStartingDefaultAction(
            MicroServiceBase service,
            TinyIoCContainer container)
        { }
        /// <summary>
        /// 服务启动后执行的事件
        /// </summary>
        protected virtual void ServiceStartedDefaultAction(
            MicroServiceBase service)
        { }
        /// <summary>
        /// 服务停止后执行的事件
        /// </summary>
        protected virtual void ServiceStopingDefaultAction(
            MicroServiceBase service)
        { }
        /// <summary>
        /// 服务停止后执行的事件
        /// </summary>
        protected virtual void ServiceStopedDefaultAction(
            MicroServiceBase service)
        { }
        /// <summary>
        /// 更新服务状态时执行的事件，可以通过此事件，向 /health 接口汇报服务状态
        /// </summary>
        protected virtual void ServiceStatusUpdateDefaultAction(
            MicroServiceBase service,
            ServiceStatus status)
        { }
        /// <summary>
        /// 在子类重写时，在调用 OWIN 初始化方法时会调用此方法，用于向 OWIN 增加新功能
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureOwin(IAppBuilder app) { }
        /// <summary>
        /// 在子类中重写时，可处理 Nancy Pipelines 完成扩展功能
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pipelines"></param>
        /// <param name="nancyContext"></param>
        protected virtual void RequestStartDefaultAction(
            MicroServiceBase service,
            IPipelines pipelines,
            NancyContext nancyContext)
        { }
        #endregion

        #region [Privates]
        /// <summary>
        /// 输出当前需要注册的模块
        /// </summary>
        private void OutputRegistedModules()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var item in this.registedModules)
                {
                    WriteToLog($"模块[{item.ModuleType.Name}]加载成功...");

                    if (container != null && container.TryResolve(item.ModuleType, out object module))
                    {
                        var m = (NancyModule)module;
                        foreach (var route in m.Routes)
                        {
                            WriteToLog($"URL：{route.Description.Method} {route.Description.Path}");
                        }
                    }
                }
            }, cancellationToken);
        }
        /// <summary>
        /// 启动服务状态更新，触发 OnUpdateServiceStatus 事件
        /// </summary>
        private void StartUpdateServiceStatusThread()
        {
            Task.Factory.StartNew(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested && serviceRunning)
                {
                    try
                    {
                        this.OnServiceStatusUpdating?.Invoke(this, serviceStatus);
                    }
                    catch (Exception ex)
                    {
                        WriteToLog("更新服务状态发生异常：" + ex.Message, ex);
                    }
                    finally
                    {
                        Task.Delay(this.UpdateServiceStatusRate).Wait();
                    }
                }
            }, cancellationToken);
            WriteToLog("自动更新服务状态线程启动成功");
        }
        #endregion

        #region [Public Methods]
        /// <summary>
        /// 从 App.config/AppSettings 配置中获取配置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 从 IoC 容器中获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
            where T : class
        {
            container.TryResolve<T>(out T outType);
            return outType;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="args"></param>
        public void Run(string[] args)
        {
            if (this.EnableBanner)
            {
                Console.WriteLine(this.banner);
                Console.WriteLine();
            }
            if (serviceRunning)
            {
                throw new Exception($"服务[{serviceName}]已运行，不能重复运行！");
            }

            this.registedModules =
                 from t in AppDomainAssemblyTypeScanner
                           .TypesOf<INancyModule>(ScanMode.ExcludeNancyNamespace)
                 select new ModuleRegistration(t);

            if (args.Contains("--console"))
            {
                WriteToLog($"服务[{serviceName}]正在使用控制台模式启动");
                //run in console
                this.InternalRun();
                this.ServiceRunningMode = RunningMode.Console;
                this.OnServiceStarted?.Invoke(this);
                WriteToLog($"服务[{serviceName}]使用控制台模式启动成功，按 ENTER 退出控制台程序！");
                Console.ReadLine();
                this.Stop();
            }
            else if (args.Contains("--install"))
            {
                serviceManager.Install();
                serviceManager.Start();
            }
            else if (args.Contains("--uninstall"))
            {
                serviceManager.Stop();
                serviceManager.UnInstall();
            }
            else
            {
                //window service main entry
                var servicesToRun = new ServiceBase[]
                {
                    new InternalService(this)
                };
                WriteToLog($"服务[{serviceName}]正在使用Window服务模式启动");
                ServiceBase.Run(servicesToRun);
                this.ServiceRunningMode = RunningMode.WindowsService;
                this.OnServiceStarted?.Invoke(this);
                WriteToLog($"服务[{serviceName}]使用Window服务模式启动成功");
            }
        }

        /// <summary>
        /// 写出到日志，并输出到控制台
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public void WriteToLog(string msg, Exception ex = null)
        {
            lock (this)
            {
                if (ex != null)
                {
                    this.logger.Error(msg, ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} ERROR {msg}");
                    Console.WriteLine(ex.ToString());
                    Console.ResetColor();
                }
                else
                {
                    this.logger.Info(msg);
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} INFO  {msg}");
                }
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop(string reason = "")
        {
            try
            {
                this.OnServiceStoping?.Invoke(this);
                WriteToLog("正在停止服务...");
                this.OnServiceStoped?.Invoke(this);
                this.cancellationTokenSource.Cancel();
            }
            finally
            {
                WriteToLog("服务停止成功.");
                serviceRunning = false;
                this.Dispose();
                GC.Collect();
            }
        }

        /// <summary>
        /// 核心运行方法
        /// </summary>
        internal void InternalRun()
        {
            this.bootstrap = new CustomBootstrapper(
                this,
                this.registedModules,
                this.OnServiceStarting,
                this.OnRequestStart,
                this.EnableCors);

            this.serverDisposable = WebApp.Start(this.StartOptions, (app) =>
            {
                try
                {
                    //使用 Nancy 框架
                    app.UseNancy((options) =>
                    {
                        options.Bootstrapper = bootstrap;
                        options.PassThroughWhenStatusCodesAre(
                            HttpStatusCode.NotFound,
                            HttpStatusCode.InternalServerError
                        );
                    });
                }
                catch (Exception ex)
                {
                    WriteToLog(ex.Message, ex);
                }

                //配置 OWIN，预留作为扩展
                try
                {
                    this.ConfigureOwin(app);
                }
                catch (Exception ex)
                {
                    WriteToLog("ConfigurationOwin(app) 发生异常！", ex);
                }
            });
            serviceRunning = true;

            //输出已注册的模块
            this.OutputRegistedModules();
            this.StartUpdateServiceStatusThread();
            this.WriteToLog($"服务 {serviceName} 已运行，端口 {string.Join(", ", this.RunningUrls)}");
        }
        #endregion

        #region [IDisposable Support]
        // 要检测冗余调用
        private bool disposedValue = false; 
        /// <summary>
        /// 释放系统资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    if (disposing)
                    {
                        this.cancellationTokenSource.Dispose();
                        this.bootstrap.Dispose();
                        this.serverDisposable.Dispose();
                        log4net.LogManager.Shutdown();
                    }
                    
                    this.logger = null;
                }
                catch { }
                disposedValue = true;
            }
        }

        /// <summary>
        /// 释放系统资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
