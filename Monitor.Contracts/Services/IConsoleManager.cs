using System;
using System.ServiceProcess;

namespace Monitor.Services
{
    public interface IConsoleManager
    {
        #region PROPERTIES
        string Prompt { get; }
        #endregion

        #region METHODS
        void Write(string text = null);
        void Write(string text, ConsoleColor color);
        void WriteLine(string text = null);
        void WriteLine(string text, ConsoleColor color);

        void WriteStatus(ServiceControllerStatus status, int startPosition, int topPositionOffset = 0);
        void WriteStatus(string status, ConsoleColor color, int startPosition, int topPositionOffset = 0);
        void WriteErrorStatus(int startPosition, int topPositionOffset = 0);
        IDisposable BeginHiddenCursor();
        IDisposable BeginForegroundColor(ConsoleColor color);
        #endregion
    }
}
