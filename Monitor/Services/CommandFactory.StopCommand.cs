using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Monitor.Services
{
    internal partial class CommandFactory
    {
        #region INTERNAL TYPES
        private class StopCommand : 
            OptionCmd
        {
            #region CONSTRUCTORS
            public StopCommand(
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
                if (this.Controller.Status == ServiceControllerStatus.Stopped)
                {
                    this.ConsoleManager
                        .WriteStatus(this.Controller.Status, this.CommandLength, 1);
                }
                else
                {
                    using (this.ConsoleManager.BeginHiddenCursor())
                    {
                        var task = Task.Run(new Action(this.Controller.Stop));

                        this.Refresh(task);

                        await task;

                        this.CheckStatus(ServiceControllerStatus.Stopped);
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
