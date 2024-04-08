using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Datadog.Trace;
using log4net;
using log4net.Config;
using log4net.Repository;

namespace BuggyAmb
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(typeof(Program).Assembly);
            var configFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "log4net.config");
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));

            log.Debug("Application starting...");

            try
            {
                LogicalThreadContext.Properties["order-number"] = 1024;
                log.Info("Message before a trace.");

                using (var scope = Tracer.Instance.StartActive("BuggyAmb - Main()"))
                {
                    // Set the properties for log4net
                    LogicalThreadContext.Properties["dd.env"] = CorrelationIdentifier.Env;
                    LogicalThreadContext.Properties["dd.service"] = CorrelationIdentifier.Service;
                    LogicalThreadContext.Properties["dd.version"] = CorrelationIdentifier.Version;
                    LogicalThreadContext.Properties["dd.trace_id"] = CorrelationIdentifier.TraceId.ToString();
                    LogicalThreadContext.Properties["dd.span_id"] = CorrelationIdentifier.SpanId.ToString();

                    log.Info("Message during a trace.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Host terminated unexpectedly", ex);
            }
            finally
            {
                LogicalThreadContext.Properties.Remove("order-number");
            }

            log.Info("Message after a trace.");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
// using System.IO;
// using Datadog.Trace;
// using log4net;
// using log4net.Config;

// namespace BuggyAmb
// {
//     class Program
//     {
//         private static readonly ILog log = LogManager.GetLogger(typeof(Program));

//         static void Main(string[] args)
//         {
//             var logRepository = LogManager.GetRepository(typeof(Program).Assembly);
//             // Uncomment this line if you want to debug your log4net setup
//             // log4net.Util.LogLog.InternalDebugging = true;
//             var configFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "log4net.config");
//             XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));
//             try
//             {
//                 LogicalThreadContext.Properties["order-number"] = 1024;
//                 log.Info("Message before a trace.");
//                 using (var scope = Tracer.Instance.StartActive("BuggyAmb - Main()"))
//                 {
//                     log.Info("Message during a trace.");
//                 }
//             }
//             finally
//             {
//                 LogicalThreadContext.Properties.Remove("order-number");
//             }

//             log.Info("Message after a trace.");
//         }
//     }
// }