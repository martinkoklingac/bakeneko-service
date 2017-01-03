using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Monitor.Services
{
    internal partial class CommandFactory
    {
        #region INTERNAL TYPES
        private class StartCommand : 
            OptionCmd
        {
            #region CONSTRUCTORS
            public StartCommand(
                IConsoleManager consoleManager,
                ServiceController controller,
                string name, string command, string option) :
                base(consoleManager, controller, name, command, option)
            { }
            #endregion

            #region PRIVATE METHODS
            protected override async Task OnExecuteAsync()
            {
                this.Controller.Refresh();

                if (this.Controller.Status == ServiceControllerStatus.Running)
                {
                    this.ConsoleManager
                        .WriteStatus(this.Controller.Status, this.CommandLength, 1);
                }
                else
                {
                    using (this.ConsoleManager.BeginHiddenCursor())
                    {
                        var task = Task.Factory.StartNew(new Action(this.Controller.Start));

                        this.Refresh(task);

                        await task;

                        this.CheckStatus(ServiceControllerStatus.Running);
                        this.ConsoleManager
                            .WriteStatus(this.Controller.Status, this.CommandLength);
                    }//End using
                }//End else
            }
            #endregion
        }
        #endregion
    }
}
