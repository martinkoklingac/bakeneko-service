using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace Monitor.Services
{
    /// <summary>
    /// Used to create executable commands from command line utility input
    /// </summary>
    internal partial class CommandFactory :
        IDisposable
    {
        #region PRIVATE FIELDS
        private readonly IConsoleManager _consoleManager;
        private ServiceController _controller;

        private readonly IReadOnlyDictionary<string, Func<IConsoleManager, ServiceController, string, Match, Cmd>> _commandStructureTable;
        #endregion

        #region CONSTRUCTORS
        public CommandFactory(
            IConsoleManager consoleManager, 
            ServiceController controller)
        {
            this.CheckInitParams(consoleManager, controller);

            this._consoleManager = consoleManager;
            this._controller = controller;

            this._commandStructureTable = new Dictionary<string, Func<IConsoleManager, ServiceController, string, Match, Cmd>>
                {
                    {@"^\W*(?<CMD>q|Q)\W*$", (cm, ctl, cmd, match)=> {return new ExitCommand(cm, ctl, match.Groups["CMD"].Value, cmd); } },
                    {"^.*?(?<CMD>status).*--(?<OPTION>stop).*$", (cm, ctl, cmd, match) => { return new StopCommand(cm, ctl, match.Groups["CMD"].Value, cmd, match.Groups["OPTION"].Value); } },
                    {"^.*?(?<CMD>status).*--(?<OPTION>start).*$", (cm, ctl, cmd, match) => { return new StartCommand(cm, ctl, match.Groups["CMD"].Value, cmd, match.Groups["OPTION"].Value); } },
                    {"^.*?(?<CMD>status).*$", (cm, ctl, cmd, match) => { return new StatusCommand(cm, ctl, match.Groups["CMD"].Value, cmd); } },
                    {"^.*?(?<CMD>exec).*--(?<OPTION>cmd:\\d+).*$", (cm, ctl, cmd, match) => { return new CustomCommand(cm, ctl, match.Groups["CMD"].Value, cmd, match.Groups["OPTION"].Value); } }
                };
        }
        #endregion

        #region PUBLIC METHODS
        public bool Create(string command, out Cmd cmd)
        {
            var commandStructureEntry = _commandStructureTable
                .FirstOrDefault(x => Regex.Match(command, x.Key).Success);

            var constructor = commandStructureEntry.Value;

            cmd = constructor == null
                ? null
                : constructor(
                    _consoleManager, _controller, 
                    command, 
                    Regex.Match(command, commandStructureEntry.Key));

            return cmd != null;
        }

        public void Dispose()
        {
            _controller = null;
        }
        #endregion

        #region PRIVATE METHODS
        private void CheckInitParams(IConsoleManager consoleManager, ServiceController controller)
        {
            if (consoleManager == null)
                throw new ArgumentNullException("consoleManager");

            if (controller == null)
                throw new ArgumentNullException("controller");
        }
        #endregion
    }
}
