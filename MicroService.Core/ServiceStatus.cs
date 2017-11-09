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
        /// <summary>
        /// 服务状态 RunMode KEY
        /// </summary>
        public const string KEY_RUN_MODE = "RunMode";
        /// <summary>
        /// 服务状态 Health KEY
        /// </summary>
        public const string KEY_HEALTH_URL = "Health";
        private readonly ConcurrentDictionary<string, dynamic> statusDict;

        /// <summary>
        /// 默认构造函数，仅允许初始化一次
        /// </summary>
        public ServiceStatus(MicroServiceBase service)
        {
            this.statusDict = new ConcurrentDictionary<string, dynamic>
            {
                [KEY_START_TIME] = DateTime.Now,
                [KEY_SERVICE_URLS] = service.RunningUrls,
                [KEY_SERVICE_NAME] = service.ServiceName,
                [KEY_RUN_MODE] = service.ServiceRunningMode.ToString(),
                [KEY_HEALTH_URL] = service.HealthUrl
            };
        }

        /// <summary>
        /// 增加或者更新服务状态中的字段
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(string key, dynamic value)
        {
            var disAllowUpdateKeys = new List<string>() {
                KEY_START_TIME,
                KEY_RUN_TIME,
                KEY_SERVICE_URLS,
                KEY_SERVICE_NAME,
                KEY_RUN_MODE
            };
            foreach (var item in disAllowUpdateKeys)
            {
                if (item.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new Exception($"名称为 {item} 的键为内置属性，不允许更新！");
                }
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
