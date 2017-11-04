using MicroService.Core;
using Nancy.TinyIoc;
namespace MicroService.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new MicroServiceBase("MyService");
            service.OnServiceStarting += Service_OnServiceStarting;
            service.OnServiceStatusUpdating += Service_OnServiceStatusUpdating;
            service.Run(args);
        }
        /// <summary>
        /// 服务启动之前执行事件
        /// </summary>
        /// <param name="service"></param>
        /// <param name="container"></param>
        private static void Service_OnServiceStarting(
            MicroServiceBase service, 
            TinyIoCContainer container)
        {
            var redisService = new RedisService();
            container.Register(redisService);
        }
        /// <summary>
        /// 服务状态更新事件
        /// </summary>
        /// <param name="service"></param>
        /// <param name="serviceStatus"></param>
        private static void Service_OnServiceStatusUpdating(
            MicroServiceBase service, 
            ServiceStatus serviceStatus)
        {
            var redisService = service.GetComponent<RedisService>();
            if (redisService != null)
            {
                serviceStatus.AddOrUpdate("RedisStatus", redisService.RedisServiceStatus);
            }
        }
    }
}