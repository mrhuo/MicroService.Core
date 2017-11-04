using Nancy;
using System.Threading.Tasks;

namespace MicroService.Samples
{
    public class AddModule : NancyModule
    {
        public AddModule()
        {
            Get["/add", true] = async (_, ctx) =>
            {
                return await Task.Run(() =>
                {
                    int? num1 = Request.Query.num1;
                    int? num2 = Request.Query.num2;
                    if (num1.HasValue && num2.HasValue)
                    {
                        return $"{num1} + {num2} = {num1 + num2}";
                    }
                    return "Paramters num1 and num2 missing!";
                });
            };
        }
    }
}
