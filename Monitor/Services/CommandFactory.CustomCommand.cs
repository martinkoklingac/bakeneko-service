using Commons.Data.Comm;
using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading.Tasks;
using __Command = Commons.Data.Comm.Command;

namespace Monitor.Services
{
    internal partial class CommandFactory
    {
        #region INTERNAL TYPES
        private class CustomCommand : 
            OptionCmd
        {
            #region PRIVATE FIELDS
            private readonly int _commandId;
            private const int MIN_COMMAND_ID = 128;
            private const int MAX_COMMAND_ID = 256;
            #endregion

            #region CONSTRUCTORS
            public CustomCommand(
                IConsoleManager consoleManager,
                ServiceController controller,
                string name, string command, string option) :
                base(consoleManager, controller, name, command, option)
            {
                this._commandId = int.Parse(this.Option.Split(':')[1]);
            }
            #endregion

            #region PUBLIC METHODS
            protected override async Task OnExecuteAsync()
            {
                if (this.Controller.Status != ServiceControllerStatus.Running)
                {
                    this.ConsoleManager
                        .WriteStatus(this.Controller.Status, this.CommandLength, 1);
                }
                else
                {
                    using (this.ConsoleManager.BeginHiddenCursor())
                    {
                        var task = Task.Factory.StartNew(ExecuteCommand);

                        this.ConsoleManager
                            .WriteStatus("Running", ConsoleColor.DarkYellow, this.CommandLength, 1);

                        await task;
                    }//End using
                }//End else
            }

            private void ExecuteCommand()
            {
                if (this._commandId < MIN_COMMAND_ID || this._commandId >= MAX_COMMAND_ID)
                    throw new ArgumentOutOfRangeException("cmd", this._commandId, $"Command value must be between {MIN_COMMAND_ID} & {MAX_COMMAND_ID}");

                this.Controller.ExecuteCommand(this._commandId);
                this.SocketConnect();
            }


            private void SocketConnect()
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                using (var sender = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp))
                {
                    sender.Connect(remoteEP);

                    using (var ns = new NetworkStream(sender, false))
                    {
                        Console.WriteLine();

                        var cp = new CommandPacket(__Command.Cmd.Status, null);

                        do
                        {
                            
                            ns.Write(cp.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                            cp = CommandPacket.Read(ns);
                            Console.WriteLine($"{cp.Cmd} --> {cp.Data}");

                            if(cp.Cmd == __Command.Cmd.Message)
                                cp = new CommandPacket(__Command.Cmd.Ack, null);

                        } while (cp.Cmd != __Command.Cmd.End && cp.Cmd != __Command.Cmd.Error);


                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                    }//End using
                }//End using
            }
            #endregion
        }
        #endregion
    }
}
