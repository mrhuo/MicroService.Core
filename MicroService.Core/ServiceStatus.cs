using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroService.Core
{
    /// <summary>
    /// 服务状态类，用于向外界汇报服务情况
    /// </summary>
    public class ServiceStatus
    {
        /// <summary>
        /// 服务状态 StartTime KEY
        /// </summary>
        public const string KEY_START_TIME = "StartTime";
        /// <summary>
        /// 服务状态 ServiceUrls KEY
        /// </summary>
        public const string KEY_SERVICE_URLS = "ServiceUrls";
        /// <summary>
        /// 服务状态 ServiceName KEY
        /// </summary>
        public const string KEY_SERVICE_NAME = "ServiceName";
        /// <summary>
        /// 服务状态 RunTime KEY
        /// </summary>
        public const string KEY_RUN_TIME = "RunTime";
        private readonly ConcurrentDictionary<string, dynamic> statusDict;

        /// <summary>
        /// 默认构造函数，仅允许初始化一次
        /// </summary>
        public ServiceStatus(string serviceName,  string urls)
        {
            this.statusDict = new ConcurrentDictionary<string, dynamic>
            {
                [KEY_START_TIME] = DateTime.Now,
                [KEY_SERVICE_URLS] = urls,
                [KEY_SERVICE_NAME] = serviceName
            };
        }

        /// <summary>
        /// 增加或者更新服务状态中的字段
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(string key, dynamic value)
        {
            if (KEY_START_TIME.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception($"名称为 {KEY_START_TIME} 的键已存在，不允许更新！");
            }
            if (KEY_RUN_TIME.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception($"名称为 {KEY_RUN_TIME} 的键为预留字段，不允许手动更新！");
            }
            if (KEY_SERVICE_URLS.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception($"名称为 {KEY_SERVICE_URLS} 的键为预留字段，不允许手动更新！");
            }
            if (KEY_SERVICE_NAME.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception($"名称为 {KEY_SERVICE_NAME} 的键为预留字段，不允许手动更新！");
            }
            this.statusDict[key] = value;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            if (this.statusDict.ContainsKey(key))
            {
                return (T)this.statusDict[key];
            }
            return default(T);
        }

        /// <summary>
        /// 获取当前服务状态
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, dynamic> GetStatus()
        {
            this.statusDict[KEY_RUN_TIME] = DateTime.Now.Subtract(this.statusDict[KEY_START_TIME]);
            return this.statusDict;
        }
    }
}
