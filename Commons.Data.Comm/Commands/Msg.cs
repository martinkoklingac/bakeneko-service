using System;
using System.IO;
using System.Text;
using static Commons.Data.Comm.Command;

namespace Commons.Data.Comm.Commands
{
    public class Msg :
        Packet
    {
        #region PRIVATE FIELDS
        private string _message;
        #endregion

        #region CONSTRUCTORS
        public Msg(Stream stream) :
            base(stream)
        { }

        public Msg(string message) :
            base(PacCmd.Message, Encoding.ASCII.GetBytes(message ?? string.Empty))
        {
            this._message = message ?? string.Empty;
        }
        #endregion

        #region PUBLIC PROPERTIES
        public string Message { get { return this._message; } }
        #endregion


        #region PUBLIC METHODS
        public override byte[] GetBytes()
        {
            var buffer = base.GetBytes();
            SetMessage(buffer, this._message);

            return buffer;
        }

        public override bool Equals(object obj)
        {
            var msg = obj as Msg;
            return base.Equals(msg)
                && msg.Message.Equals(this.Message);
        }

        public static void SetMessage(byte[] buffer, string message)
        {
            var messageBuffer = Encoding.ASCII.GetBytes(message);
            Array.Copy(messageBuffer, 0, buffer, DATA_OFFSET, messageBuffer.Length);
        }
        #endregion


        #region PROTECTED METHODS
        protected override void ParseData(byte[] buffer)
        {
            this._message = Encoding.ASCII.GetString(buffer);
        }
        #endregion
    }
}
