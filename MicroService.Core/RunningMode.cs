using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    /// <summary>
    /// 服务运行模式
    /// </summary>
    public enum RunningMode
    {
        /// <summary>
        /// 控制台模式
        /// </summary>
        Console,
        /// <summary>
        /// Windows 服务模式
        /// </summary>
        WindowsService
    }
}
