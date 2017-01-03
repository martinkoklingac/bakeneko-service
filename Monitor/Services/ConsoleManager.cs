using System;
using System.ServiceProcess;

namespace Monitor.Services
{
    public partial class ConsoleManager : 
        IConsoleManager
    {
        #region PRIVATE FIELDS
        private const string PROPT = "> ";
        #endregion

        #region PUBLIC PROPERTIES
        public string Prompt { get { return PROPT; } }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Wrapper for <see cref="Console.Write"/> method
        /// </summary>
        public virtual void Write(string text = null) { Console.Write(text); }

        /// <summary>
        /// Wrapper for <see cref="Console.Write"/> method with ability to add foreground color
        /// via the <paramref name="color"/> argument
        /// </summary>
        /// <param name="text">Text to write to <see cref="Console"/></param>
        /// <param name="color"><see cref="ConsoleColor"/> foreground color</param>
        public virtual void Write(string text, ConsoleColor color)
        {
            var originalForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalForegroundColor;
        }

        /// <summary>
        /// Wrapper for <see cref="Console.WriteLine"/> method
        /// </summary>
        public virtual void WriteLine(string text = null) { Console.WriteLine(text); }

        /// <summary>
        /// Wrapper for <see cref="Console.WriteLine"/> method with ability to add foreground color
        /// via the <paramref name="color"/> argument
        /// </summary>
        /// <param name="text">Text to write to <see cref="Console"/></param>
        /// <param name="color"><see cref="ConsoleColor"/> foreground color</param>
        public virtual void WriteLine(string text, ConsoleColor color)
        {
            var originalForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalForegroundColor;
        }

        /// <summary>
        /// Writes formatted <see cref="ServiceControllerStatus"/> <paramref name="status"/> to standard output
        /// </summary>
        /// <param name="status"><see cref="ServiceControllerStatus"/> status to format</param>
        /// <param name="startPosition">Horizontal leftmost position, in standard output, where to begin writing formatted <paramref name="status"/></param>
        /// <param name="topPositionOffset">Vertical position, in standard output, where to begin writing formatted <paramref name="status"/></param>
        public void WriteStatus(ServiceControllerStatus status, int startPosition, int topPositionOffset = 0)
        {
            var color = status == ServiceControllerStatus.Running
                ? ConsoleColor.DarkGreen
                : status == ServiceControllerStatus.Stopped
                    ? ConsoleColor.DarkRed
                    : ConsoleColor.DarkGray;

            WriteStatus(status.ToString(), color, startPosition, topPositionOffset);
        }

        /// <summary>
        /// Writes formatted <see cref="string"/> <paramref name="status"/> to standard output
        /// </summary>
        /// <param name="status"><see cref="string"/> status to format</param>
        /// <param name="color"><see cref="ConsoleColor"/> format color</param>
        /// <param name="startPosition">Horizontal leftmost position, in standard output, where to begin writing formatted <paramref name="status"/></param>
        /// <param name="topPositionOffset">Vertical position, in standard output, where to begin writing formatted <paramref name="status"/></param>
        public void WriteStatus(string status, ConsoleColor color, int startPosition, int topPositionOffset = 0)
        {
            var startCursor = Console.CursorLeft;
            Console.SetCursorPosition(startPosition + PROPT.Length, Console.CursorTop - topPositionOffset);
            this.Write(" [");
            this.Write($"{status}", color);
            this.Write("]");

            var endCursor = Console.CursorLeft;

            //We need to overwrite the rest of the buffer that might have garbage in it at this point
            var delta = startCursor - endCursor;
            for (var j = 0; j < delta; j++) //If delta is negative, this for loop will not execute
                this.Write(" ");
        }

        /// <summary>
        /// Writes a standard formatted error to standard output
        /// </summary>
        /// <param name="startPosition">Horizontal leftmost position, in standard output, where to begin writing formatted error status</param>
        /// <param name="topPositionOffset">Vertical position, in standard output, where to begin writing formatted error status</param>
        public void WriteErrorStatus(int startPosition, int topPositionOffset = 0)
        {
            WriteStatus("ERROR", ConsoleColor.DarkRed, startPosition, topPositionOffset);
        }

        public IDisposable BeginHiddenCursor() { return new HideCursorInitiator(); }
        public IDisposable BeginForegroundColor(ConsoleColor color) { return new ForegroundColorInitiator(color); }
        #endregion
    }
}
