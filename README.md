## MicroService.Core <a name="top" href="#top"></a>

> `MicroService.Core` 的初衷是为了方便的创建一个微服务，
> 可作为 Windows Service 或者控制台模式启动。
> 它底层使用了 Nancy，使得开发过程很简单，很舒服！
> 
><b style='color:red'>注意：最新版本不再有 OWIN 了</b>

* [快速入门](#quick-start)
* [框架构成](#principle)
* [进阶](#advance)
* [扩展](#extensions)
* [实例](#samples)

## <a name="quick-start" href="#quick-start">快速入门</a>
#### 一、创建控制台项目（需要.net 4.5以上）
#### 二、安装Nuget包
```
PM> Install-Package MicroService.Core -Version 1.2
```
或在 Nuget 包管理器中搜索 `MicroService.Core` 安装。
#### 三、编写 Program.cs，输入以下代码
添加引用：`using MicroService.Core;`
Main 方法中写如下代码：
```
var service = new MicroServiceBase("MyService");
service.Run(args);
```
这时 Program.cs 文件内容如下
```
using MicroService.Core;
namespace MicroService.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new MicroServiceBase("MyService");
            service.Run(args);
        }
    }
}
```
生成项目，然后在生成的 EXE 文件目录下打开一个命令行窗口，执行：
> 如生成的 EXE 文件为 `MicroService.Samples.exe`
###### 1、命令行模式运行：
```
MicroService.Samples.exe --console
```
即可看到如下的运行效果：
![Sample1](http://res.mrhuo.com/github/microservice-sample1.png)

###### 2、Windows 服务模式
```
MicroService.Samples.exe --install
```
运行结果如果出现下面这个情况：
![Sample2](http://res.mrhuo.com/github/microservice-sample2.png)
是因为权限不够，无法安装windows服务，以管理员权限运行命令行窗口再次执行以上命令，
截图如下：
![Sample3](http://res.mrhuo.com/github/microservice-sample3.png)
查看 `Windows 服务` 列表，可以看到此服务已经正常运行：
![Sample4](http://res.mrhuo.com/github/microservice-sample4.png)
#### 四、如何测试此服务是否已经正常运行？
此微服务框架默认提供了服务状态地址 `/health`，如图，默认绑定地址是 
`http://127.0.0.1:8080`，从浏览器访问地址 
[http://127.0.0.1:8080/health](http://127.0.0.1:8080/health)
可以得到如下的输出：
![Sample5](http://res.mrhuo.com/github/microservice-sample5.png)
可以看出服务的一些情况，运行时常，暴露端点等等...
#### 五、是否被惊到？
仅仅3行代码，抛去 using 引用，就只有2行代码，就可以做一个可以运行在命令行
/windows 服务的对外提供 `API` 能力的程序。

## <a name="principle" href="#principle">框架构成</a>
* 通过集成 Owin 使得她有了自托管的能力
* 通过实现了一个内部的 ServiceBase 类使得她有了能作为 windows 服务运行的能力
* 通过集成 Nancy （[http://nancyfx.org/](http://nancyfx.org/)） 使得她有了很容易开放 Api 服务的能力

> 有人可能会说，这有个鸟用？先别急，我们来扩展一下，做个什么呢？
> 就先来个加法器吧，输入两个数字，返回两个数字之和。

## <a name="advance" href="#advance">进阶</a>
#### 一、写一个 Nancy 模块：
> 关于 Nancy 模块的编写，请自行前往链接 [http://nancyfx.org/](http://nancyfx.org/)
> 学习，作为技术人员，学习是必不可少的技能。当然，你看到这里，先不用着急去学习 Nancy，
> 继续往下看，很简单的！

###### 1、新增一个类
在项目中新建一个名为 `AddModule` 的 Nancy 模块，代码如下：
```
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
```
这段代码可能对没有 Nancy 模块经验的人，稍微有点难度，但是代码通俗易懂，一看就会！

> 停掉上面运行的 Windows 服务，然后重新生成项目，启动服务。

访问浏览器：[http://127.0.0.1:8080/add?num1=1&num2=2](http://127.0.0.1:8080/add?num1=1&num2=2)
![Sample6](http://res.mrhuo.com/github/microservice-sample6.png)

如果不出意外，你的运行结果应该和我无异！

> 到这里可能你有点想法了，那我不能用 VS 调试吗？
> 答案是：可以！

*在 VS 项目上右键，进入项目属性页面，在调试选项卡下的启动参数里输入 --console，然后启动项目*
如图：
![Sample7](http://res.mrhuo.com/github/microservice-sample7.png)

现在重新看看控制台的运行效果：
![Sample8](http://res.mrhuo.com/github/microservice-sample8.png)

看红色箭头处，新增了两行，加载刚刚新建的模块成功！
> 连暴露的URL都列出来了，是否很贴心？

到这里我感觉还是没有体现出他的优势，我们再将这个项目处理处理！

###### 2、再单独新建一个类 RedisService.cs
代码如下：
```
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
```
代码很简单，只有一个只读属性，获取了 Redis 服务的状态（模拟）。
下面，我们把他加入到上面的 `/health` 端点中，让我们的服务更加直观的展示出整体服务的状态。

修改 Program.cs 类中的代码：
```
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
```

> 有人可能要骂我了，你 TM 逗我啊，这叫简单？其实仔细看看不难！

加了两个事件，服务启动之前执行事件和服务状态更新事件，在服务启动之前执行事件代码中，
使用 Nancy 内置的 TinyIoCContainer IoC容器注册了一个 RedisService 的实例。
在服务状态更新事件中，通过 `service.GetComponent<RedisService>()` 获取到了这个
RedisService 实例，在 serviceStatus 添加进去。

现在，再次运行，看看结果：
![Sample9](http://res.mrhuo.com/github/microservice-sample9.png)

## <a name="extensions" href="#extensions">扩展</a>
> 正在整理

## <a name="samples" href="#samples">实例</a>
> 正在整理

> QQ: 491217650  邮件：admin@mrhuo.com

<a href="#top">回到顶部</a>