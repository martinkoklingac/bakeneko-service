using System;

namespace Monitor.Services
{
    public partial class ConsoleManager
    {
        #region INTERNAL TYPES
        /// <summary>
        /// Controlls the <see cref="Console.CursorVisible"/> property
        /// </summary>
        private class HideCursorInitiator :
            IDisposable
        {
            #region CONSTRUCTORS
            public HideCursorInitiator() { Console.CursorVisible = false; }
            #endregion

            #region PUBLIC METHODS
            public void Dispose() { Console.CursorVisible = true; }
            #endregion
        }
        #endregion
    }
}
