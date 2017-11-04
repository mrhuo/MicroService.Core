using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    /// <summary>
    /// 更新频率常量类
    /// </summary>
    public class UpdateRate
    {
        /// <summary>
        /// 表示1秒
        /// </summary>
        public static readonly int SECOND = 1000;
        /// <summary>
        /// 表示1分钟
        /// </summary>
        public static readonly int MINUTE = 60 * SECOND;
        /// <summary>
        /// 表示1小时
        /// </summary>
        public static readonly int HOUR =  60 * MINUTE;
        /// <summary>
        /// 表示半小时
        /// </summary>
        public static readonly int HALF_HOUR = HOUR >> 1;
        /// <summary>
        /// 表示1天
        /// </summary>
        public static readonly int DAY =  24 * HOUR;
    }
}
