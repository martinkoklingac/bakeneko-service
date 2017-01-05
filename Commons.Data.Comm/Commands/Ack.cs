using System.IO;
using static Commons.Data.Comm.Command;

namespace Commons.Data.Comm.Commands
{
    public class Ack :
        Packet
    {
        #region CONSTRUCTORS
        public Ack(Stream stream) : base(stream) { }

        public Ack() : 
            base(PacCmd.Ack)
        { }
        #endregion
    }
}
