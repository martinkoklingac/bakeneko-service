using Monitor.Services;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using System;
using System.ServiceProcess;

namespace Monitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var kernel = new StandardKernel(
                new NinjectSettings { AllowNullInjection = false },
                new ServiceModule()))
            {
                new AppRunner(kernel).Run();
            }
        }

        class ServiceModule : 
            NinjectModule
        {
            #region PUBLIC METHODS
            public override void Load()
            {
                this.Bind<IConsoleManager>().To<ConsoleManager>().InSingletonScope();
                this.Bind<IAppSettings>().To<AppSettings>().InSingletonScope();
                this.Bind<ServiceController>().ToMethod(CreateServiceController).InSingletonScope();
                this.Bind<CommandFactory>().To<CommandFactory>().InSingletonScope();
            }
            #endregion

            #region PRIVATE METHODS
            private static ServiceController CreateServiceController(IContext context)
            {
                var appSettings = context
                    .Kernel
                    .Get<IAppSettings>();

                return new ServiceController(
                    appSettings.ServiceName, 
                    appSettings.ServiceMachineName);
            }
            #endregion
        }

        class AppRunner
        {
            #region PRIVATE FIELDS
            private readonly IKernel _kernel;
            #endregion

            #region CONSTRUCTORS
            public AppRunner(
                IKernel kernel)
            { this._kernel = kernel; }
            #endregion

            #region PUBLIC METHODS
            public void Run()
            {
                var cm = this.GetConsoleManager();

                do { cm.Write(cm.Prompt); }
                while (this.ProcessRequest(Console.ReadLine().Trim()));
            }
            #endregion

            #region PRIVATE METHODS
            private bool ProcessRequest(string command)
            {
                var cm = this.GetConsoleManager();
                var continueExecution = true;
                Cmd cmd;

                if (this.GetCommandFactory().Create(command, out cmd))
                {
                    cmd.Execute();
                    cm.WriteLine();
                    continueExecution = cmd.Continue;
                }
                else if (!string.IsNullOrWhiteSpace(command))
                {
                    cm.WriteStatus($"Unknown command : {command}", ConsoleColor.DarkGray, command.Length, 1);
                    cm.WriteLine();
                }

                return continueExecution;
            }

            private CommandFactory GetCommandFactory() { return this._kernel.Get<CommandFactory>(); }

            private IConsoleManager GetConsoleManager() { return this._kernel.Get<IConsoleManager>(); }
            #endregion
        }
    }
}

