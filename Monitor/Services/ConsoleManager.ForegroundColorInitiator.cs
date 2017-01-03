using System;

namespace Monitor.Services
{
    public partial class ConsoleManager
    {
        #region INTERNAL TYPES
        /// <summary>
        /// Controlls the <see cref="Console.ForegroundColor"/> and stores the original
        /// value which can then be reset by disposing of this instance.
        /// </summary>
        private class ForegroundColorInitiator :
            IDisposable
        {
            #region PRIVATE FIELDS
            private readonly ConsoleColor _originalForegroundColor;
            #endregion

            #region CONSTRUCTORS
            public ForegroundColorInitiator(ConsoleColor color)
            {
                this._originalForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
            }
            #endregion

            #region PUBLIC METHODS
            public void Dispose()
            {
                Console.ForegroundColor = this._originalForegroundColor;
            }
            #endregion
        }
        #endregion
    }
}
