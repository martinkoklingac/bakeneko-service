using Castle.DynamicProxy;
using System.ServiceProcess;
using System;
using log4net;
using System.Reflection;
using System.Linq;

namespace BakenekoService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var proxyGenerator = new ProxyGenerator();

            var serviceProxy = (ServiceBase)proxyGenerator.CreateClassProxy(
                typeof(BakenekoService),
                new ProxyGenerationOptions { Selector = new BakenekoServiceLoggingSelector() },
                new object[] { new CommService() },    //BakenekoService constructor args
                new LoggingInterceptor());

            //var serviceProxy = proxyGenerator.CreateClassProxy<BakenekoService>(
            //    new ProxyGenerationOptions { Selector = new BakenekoServiceLoggingSelector() }, 
            //    new LoggingInterceptor());

            var servicesToRun = new ServiceBase[] { serviceProxy };
            ServiceBase.Run(servicesToRun);
        }
    }

    public class LoggingInterceptor : 
        IInterceptor
    {
        #region PUBLIC METHODS
        public void Intercept(IInvocation invocation)
        {
            var log = LogManager.GetLogger(invocation.TargetType);
            log.DebugFormat("Method Call intercepted: {0}", invocation.Method.Name);

            invocation.Proceed();
        }
        #endregion
    }

    public class BakenekoServiceLoggingSelector : 
        IInterceptorSelector
    {
        #region PRIVATE FIELDS
        private readonly ILog _logger;
        private static readonly string[] methods =
        {
            "OnStart",
            "OnStop",
            "OnPause",
            "OnContinue",
            "OnCustomCommand",
            "OnShutdown",
            "OnSessionChange"
        };
        #endregion

        #region CONSTRUCTORS
        public BakenekoServiceLoggingSelector()
        {
            _logger = LogManager
                .GetLogger(typeof(BakenekoServiceLoggingSelector));
        }
        #endregion

        #region PUBLIC METHODS
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            var isMethodMatch = methods.Any(x => x == method.Name);
            _logger.DebugFormat("method:[{0}] isMethodMatch:[{1}]", method.Name, isMethodMatch);
            return isMethodMatch
                ? interceptors 
                : new IInterceptor[0];
        }
        #endregion
    }

}
