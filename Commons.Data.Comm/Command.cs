using System;
using System.Linq;

namespace Commons.Data.Comm
{
    public static class Command
    {
        #region PRIVATE FIELDS
        private static readonly Cmd[] COMMANDS = Enum.GetValues(typeof(Cmd))
            .Cast<Cmd>()
            .ToArray();
        #endregion

        #region PUBLIC METHODS
        public static Cmd Convert(byte cmd)
        {
            return COMMANDS.Any(x => cmd == (byte)x)
                ? (Cmd)cmd
                : Cmd.Undefined;
        }

        public static Cmd Convert(int cmd) { return Convert((byte)cmd); }

        public static byte Convert(Cmd cmd) { return (byte)cmd; }
        #endregion

        #region INTERNAL TYPES
        public enum Cmd : byte
        {
            Undefined = 0,

            /// <summary>
            /// Used to acknowledge command reception
            /// </summary>
            Ack = 128,

            /// <summary>
            /// Used to notify client that message stream has ended
            /// </summary>
            End = 129,

            /// <summary>
            /// Gets the status of the service
            /// </summary>
            Status = 130,

            /// <summary>
            /// Gets the configuration of the service
            /// </summary>
            DumpConfig = 131,

            /// <summary>
            /// Gets service state statistics information
            /// </summary>
            DumpStats = 140,

            /// <summary>
            /// Command with string message payload
            /// </summary>
            Message = 141,

            /// <summary>
            /// Used to notify client that message was expected but not received within specified window
            /// </summary>
            Timeout = 142,

            /// <summary>
            /// Used to notify client that an error was encoutered
            /// </summary>
            Error = 143
        }
        #endregion
    }
}
