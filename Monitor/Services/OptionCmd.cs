using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor.Services
{
    /// <summary>
    /// Extension of <see cref="Cmd"/> that also encapsulates the option
    /// parameter as parsed from the original command.
    /// </summary>
    internal abstract class OptionCmd : 
        Cmd
    {
        #region PUBLIC FIELDS
        /// <summary>
        /// Maximum number of times the refresh loop can execute
        /// </summary>
        public const int MAX_REFRESH_ITERATION_COUNT = 15;

        /// <summary>
        /// Maximum milliseconds to wait between refresh iterations
        /// </summary>
        public const int MAX_REFRESH_ITERATION_WAIT_TIME = 200;
        #endregion

        #region PRIVATE FIELDS
        private readonly string _option;
        private static readonly string[] _spinTokens = 
            new string[] { @"|", @"/", @"-", @"\", @"|", @"/", @"-", @"\" };
        private const string PROGRESS_TOKEN = ".";
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Instantiates underlying components and properties
        /// </summary>
        /// <param name="consoleManager"><see cref="IConsoleManager"/> instance</param>
        /// <param name="controller"><see cref="ServiceController"/> instance</param>
        /// <param name="name">Command name</param>
        /// <param name="command">Raw command as read from command line utility</param>
        /// <param name="option">Parsed option section of the command</param>
        protected OptionCmd(
            IConsoleManager consoleManager, 
            ServiceController controller, 
            string name, string command, string option) : 
            base(consoleManager, controller, name, command) { _option = option; }
        #endregion

        #region PUBLIC PROPERTIES
        /// <summary>
        /// Provides the parsed option component of the <see cref="Cmd.Command"/> 
        /// </summary>
        public string Option { get { return this._option; } }
        #endregion

        #region PRIVATE METHODS
        protected void Refresh(Task task)
        {
            var spinner = this.CreateSpinner()
                .Start();
            
            var iteration = 0;

            do
            {
                spinner.Spin();
                this.Controller.Refresh();
                Thread.Sleep(MAX_REFRESH_ITERATION_WAIT_TIME);
                iteration++;
            }
            while (!task.IsCompleted && iteration <= MAX_REFRESH_ITERATION_COUNT);
        }

        protected void CheckStatus(ServiceControllerStatus expectedStatus)
        {
            if (this.Controller.Status == expectedStatus)
                return;

            this.ConsoleManager
                .WriteErrorStatus(this.CommandLength);

            throw new StatusException(this.Controller.Status, ServiceControllerStatus.Running);
        }

        private StatusSpinner CreateSpinner()
        {
            return new StatusSpinner(
                this.ConsoleManager,
                this.CommandLength,
                PROGRESS_TOKEN,
                _spinTokens);
        }
        #endregion

        #region INTERNAL TYPES
        public class StatusException :
                Exception
        {
            #region CONSTRUCTORS
            public StatusException(ServiceControllerStatus currentStatus, ServiceControllerStatus expectedStatus) :
                base($"The current status[{currentStatus}] is incongruent with expected status [{expectedStatus}]")
            { }
            #endregion
        }

        private class StatusSpinner
        {
            #region PRIVATE FIELDS
            private int _spinCount;
            private string _progress;

            private readonly IConsoleManager _consoleManager;
            private readonly int _leftOffset;
            private readonly Queue<string> _spinTokenQueue;
            private readonly string _progressToken;
            #endregion

            #region CONSTRUCTORS
            public StatusSpinner(
                IConsoleManager consoleManager,
                int leftOffset,
                string progressToken,
                string[] spinTokens)
            {
                this._consoleManager = consoleManager;
                this._leftOffset = leftOffset;
                this._progressToken = progressToken;
                this._spinTokenQueue = new Queue<string>(spinTokens);

                this._progress = string.Empty;
            }
            #endregion
            
            #region PUBLIC METHODS
            public StatusSpinner Start()
            {
                this._consoleManager
                    .WriteStatus(_spinTokenQueue.Peek(), ConsoleColor.Gray, this._leftOffset, 1);
                _spinTokenQueue.Enqueue(_spinTokenQueue.Dequeue());

                return this;
            }

            public StatusSpinner Spin()
            {
                this._consoleManager
                    .WriteStatus(this._progress + _spinTokenQueue.Peek(), ConsoleColor.Gray, this._leftOffset);
                _spinTokenQueue.Enqueue(_spinTokenQueue.Dequeue());

                this._spinCount++;
                if (this._spinCount % 3 == 0)
                    this._progress += this._progressToken;

                return this;
            }
            #endregion
        }
        #endregion
    }
}
