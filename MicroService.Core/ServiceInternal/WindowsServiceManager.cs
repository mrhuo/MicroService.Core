using System;
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace MicroService.Core.ServiceInternal
{
    /// <summary>
    /// Windows 服务管理器
    /// </summary>
    internal class WindowsServiceManager
    {
        private readonly MicroServiceBase service;
        /// <summary>
        /// 默认构造方法
        /// </summary>
        /// <param name="service"></param>
        public WindowsServiceManager(MicroServiceBase service)
        {
            this.service = service;
        }

        #region Internal
        /// <summary>
        /// 安装服务
        /// </summary>
        internal void Install()
        {
            if (IsInstalled())
            {
                this.service.WriteToLog($"服务 {this.service.ServiceName} 已经安装，退出安装服务过程。");
                return;
            }
            using (var installer = GetInstaller())
            {
                IDictionary state = new Hashtable();
                try
                {
                    installer.Install(state);
                }
                catch (Exception ex)
                {
                    try
                    {
                        installer.Rollback(state);
                    }
                    catch { }
                    this.service.WriteToLog("服务安装失败", ex);
                }
            }
        }

        /// <summary>
        /// 卸载服务
        /// </summary>
        internal void UnInstall()
        {
            if (!IsInstalled())
            {
                this.service.WriteToLog($"服务 {this.service.ServiceName} 没有安装，退出卸载服务过程。");
                return;
            }
            using (var installer = GetInstaller())
            {
                installer.Uninstall(null);
            }
        }

        /// <summary>
        /// 运行服务
        /// </summary>
        internal void Start()
        {
            if (!IsInstalled())
            {
                this.service.WriteToLog($"服务 {this.service.ServiceName} 没有安装，无法启动。");
                return;
            }
            using (var controller = new ServiceController(this.service.ServiceDisplayName))
            {
                if (controller.Status == ServiceControllerStatus.Running) return;
                controller.Start();

                controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
            }
            this.service.WriteToLog("服务启动成功");
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        internal void Stop()
        {
            if (!IsInstalled())
            {
                this.service.WriteToLog($"服务 {this.service.ServiceName} 没有安装，无法停止。");
                return;
            }
            using (var controller = new ServiceController(this.service.ServiceDisplayName))
            {
                if (controller.Status == ServiceControllerStatus.Stopped) return;
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }
            this.service.WriteToLog("服务停止成功");
        }
        #endregion

        #region Private
        /// <summary>
        /// 构造安装程序
        /// </summary>
        /// <returns></returns>
        private TransactedInstaller GetInstaller()
        {
            var transactedInstaller = new TransactedInstaller()
            {
                Context = new InstallContext()
            };
            transactedInstaller.Context.Parameters["assemblypath"] = $"\"{Assembly.GetEntryAssembly().Location}\"";

            var serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
                Password = null,
                Username = null
            };

            var serviceInstaller = new ServiceInstaller
            {
                DisplayName = this.service.ServiceDisplayName,
                Description = this.service.ServiceDisplayName,
                ServiceName = this.service.ServiceName,
                StartType = ServiceStartMode.Automatic
            };

            transactedInstaller.Installers.Add(serviceProcessInstaller);
            transactedInstaller.Installers.Add(serviceInstaller);

            return transactedInstaller;
        }

        /// <summary>
        /// 判断服务是否安装
        /// </summary>
        /// <returns></returns>
        private bool IsInstalled()
        {
            using (var controller = new ServiceController(this.service.ServiceDisplayName))
            {
                try
                {
                    var status = controller.Status;
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }
        #endregion
    }
}
