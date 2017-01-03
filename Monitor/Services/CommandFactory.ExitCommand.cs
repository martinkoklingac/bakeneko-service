using System.ServiceProcess;
using System.Threading.Tasks;

namespace Monitor.Services
{
    internal partial class CommandFactory
    {
        #region INTERNAL TYPES
        private class ExitCommand : 
            Cmd
        {
            #region CONSTRUCTORS
            public ExitCommand(
                IConsoleManager consoleManager,
                ServiceController controller,
                string name,
                string command) :
                base(consoleManager, controller, name, command)
            {
                this.Continue = false;
            }
            #endregion

            #region PRIVATE METHODS
            protected override Task OnExecuteAsync() { return Task.FromResult(0); }
            #endregion
        }
        #endregion
    }
}
