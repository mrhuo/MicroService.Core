using System;
namespace MicroService.Samples
{
    public class RedisService
    {
        public string RedisServiceStatus
        {
            get
            {
                var rnd = new Random().Next(1, 10);
                if (rnd % 3 == 0)
                {
                    return "DOWN";
                }
                return "UP";
            }
        }
    }
}
