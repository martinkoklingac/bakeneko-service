using System.ServiceProcess;
using System.Threading.Tasks;

namespace Monitor.Services
{
    internal partial class CommandFactory
    {
        #region INTERNAL TYPES
        private class StatusCommand :
            Cmd
        {
            #region CONSTRUCTORS
            public StatusCommand(
                IConsoleManager consoleManager,
                ServiceController controller,
                string name,
                string command) :
                base(consoleManager, controller, name, command)
            { }
            #endregion

            #region PRIVATE METHODS
            protected override async Task OnExecuteAsync()
            {
                await Task.Factory.StartNew(() =>
                {
                    this.Controller.Refresh();
                    this.ConsoleManager
                        .WriteStatus(this.Controller.Status, CommandLength, 1);
                });
            }
            #endregion
        }
        #endregion
    }
}
