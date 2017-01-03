using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Monitor.Services
{
    /// <summary>
    /// Defines the interface and basic implementation of BakenekoService command
    /// that can be run from the command line utility.
    /// </summary>
    abstract internal class Cmd
    {
        #region PRIVATE FIELDS
        private readonly IConsoleManager _consoleManager;
        private readonly ServiceController _controller;
        private readonly string _name;
        private readonly string _command;
        #endregion

        #region CONSTRUCTORS
        protected Cmd(
            IConsoleManager consoleManager,
            ServiceController controller,
            string name, string command)
        {
            _consoleManager = consoleManager;
            _controller = controller;

            _name = name;
            _command = command;
            this.Continue = true;
        }
        #endregion

        #region PUBLIC PROPERTIES
        /// <summary>
        /// Indicates wheather the command line utility should continue processing
        /// additional commands. If this property return <c>False</c>, then the command line
        /// utility will simply exit upon the command completion.
        /// </summary>
        public bool Continue { get; protected set; }

        /// <summary>
        /// Command name
        /// </summary>
        public string Name { get { return this._name; } }

        /// <summary>
        /// The original command as read from the command line utility
        /// </summary>
        public string Command { get { return _command; } }

        /// <summary>
        /// Length of <see cref="Command"/> string
        /// </summary>
        public int CommandLength { get { return _command.Length; } }
        #endregion

        #region PRIVATE PROPERTIES
        /// <summary>
        /// Exposes the underlying <see cref="IConsoleManager"/> instance
        /// </summary>
        protected IConsoleManager ConsoleManager { get { return this._consoleManager; } }

        /// <summary>
        /// Exposes the underlying <see cref="ServiceController"/> instance
        /// </summary>
        protected ServiceController Controller { get { return _controller; } }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Abstract definition of the main method with which the client can execute this command instance
        /// </summary>
        public void Execute()
        {
            try { OnExecuteAsync().Wait(); }
            catch (AggregateException aex)
            {
                _consoleManager.WriteErrorStatus(this.CommandLength);

                foreach(var ex in aex.InnerExceptions)
                {
                    _consoleManager.WriteLine();
                    _consoleManager.Write($"{ex.GetType().FullName} (", ConsoleColor.DarkGray);
                    _consoleManager.Write($"{ex.Message}", ConsoleColor.DarkRed);
                    _consoleManager.WriteLine(")", ConsoleColor.DarkGray);
                    _consoleManager.WriteLine();

                    using (_consoleManager.BeginForegroundColor(ConsoleColor.DarkRed))
                    {
                        foreach (var line in ex.StackTrace.Split('\n'))
                            _consoleManager.WriteLine(line.Trim());
                    }
                }//End foreach
            }//End catch
        }
        #endregion

        #region PRIVATE METHODS
        protected abstract Task OnExecuteAsync();
        #endregion
    }
}
