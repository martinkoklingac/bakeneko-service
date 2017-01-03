using Commons.Data.Comm;
using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BakenekoService
{
    public class CommService : 
        IDisposable
    {
        #region PRIVATE FIELDS
        private static ILog _log = LogManager.GetLogger(typeof(CommService));

        private readonly object _lock;
        private readonly int _port;
        private readonly int _queueSize;
        private readonly ConcurrentDictionary<IAsyncResult, byte> _activeConnectionsTable;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private CommServiceState _commServiceState;
        private Socket _socketListener;
        #endregion


        #region CONSTRUCTORS
        public CommService()
        {
            _log.Info("CommService Constructor");
            this._lock = new object();
            this._port = 11000;
            this._queueSize = 10;
            this._activeConnectionsTable = new ConcurrentDictionary<IAsyncResult, byte>(10, 10);
            this._cancellationTokenSource = new CancellationTokenSource();
        }
        #endregion

        #region PUBLIC PROPERTIES
        public bool IsCommInterfaceAvailable { get; private set; }
        public int ActiveConnectionCount { get { return this._activeConnectionsTable.Count; } }
        #endregion

        #region PUBLIC METHODS
        public void Start()
        {
            try
            {
                StartListener();
                StartAccept();
                this._commServiceState = CommServiceState.Running;
            }
            catch (Exception ex)
            {
                this.IsCommInterfaceAvailable = false;
                _log.Error("Start - Opening socket failed", ex);
            }
        }

        public void Stop()
        {
            lock (this._lock)
            {
                if (this._commServiceState == CommServiceState.Stopped)
                    return;

                this._commServiceState = CommServiceState.StoppingCycle;
            }//End lock


            _log.Info("Stop -> Sending cancel signal");
            this._cancellationTokenSource.Cancel();

            var waitHandles = this._activeConnectionsTable
                .Select(x => x.Key.AsyncWaitHandle)
                .ToArray();

            _log.Info($"Stop -> handles: {waitHandles.Length}");
            if (waitHandles.Any())
            {
                _log.Info($"Stop -> waiting on handles to finish");
                WaitHandle.WaitAll(waitHandles);
                _log.Info($"Stop -> closing all handles");
                foreach (var handle in waitHandles)
                    handle.Close();
            }
            
            _log.Info($"Stop -> there should be 0 active connection processors : {this.ActiveConnectionCount}");

            _log.Info($"Stop -> killing listener socket");
            lock (_lock)
            {
                this._commServiceState = CommServiceState.Stopped;
                this._socketListener.Close();
                this._socketListener.Dispose();
                this._socketListener = null;
            }//End lock
        }

        public void Dispose()
        {
            _log.Info("Dispose");
            this.Stop();

            this._cancellationTokenSource.Dispose();
        }
        #endregion

        #region PRIVATE METHODS
        private void StartListener()
        {
            _log.Info("StartListener - Opening socket");
            var hostName = Dns.GetHostName();
            var ipHostEntry = Dns.GetHostEntry(hostName);
            var ipAddress = ipHostEntry.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddress, this._port);

            _log.Info($"StartListener - host name: [{hostName}]");
            _log.Info($"StartListener - ip: [{ipAddress.ToString()}]");
            _log.Info($"StartListener - port: [{this._port}]");
            _log.Info($"StartListener - queue size: [{this._queueSize}]");

            this._socketListener = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            this._socketListener.Bind(ipEndPoint);
            this._socketListener.Listen(this._queueSize);

            this.IsCommInterfaceAvailable = true;
            _log.Info("StartListener - Socket opened");
        }

        private void StartAccept()
        {
            //Begin waiting for incoming connections to be processed
            this._socketListener.BeginAccept(
                this.OnAccept, null);
            
            _log.Info($"Active connections: {this.ActiveConnectionCount}");
        }

        private void OnAccept(IAsyncResult ar)
        {
            //This lock will enforce atomic creation of a connection processor thread
            //and will ensure that the IAsyncResult will be added to the _activeConnectionsTable
            //so that durring shutdown, all active connection threads will be present and can be awaited.
            lock (this._lock)
            {
                _log.Info($"-> _isCommServiceStarted : {_commServiceState}");


                //The CommService is shut down (or in shut down cycle) 
                //so we should not process any new connections
                if (this._commServiceState == CommServiceState.Stopped)
                    return;

                //Initiate new waiter
                if (this._commServiceState == CommServiceState.Running)
                    this.StartAccept();


                if (this._commServiceState == CommServiceState.Running 
                    || this._commServiceState == CommServiceState.Paused)
                {
                    _log.Info("-> Begin creating connection processor");

                    //Spin up connection processor thread
                    var handler = this._socketListener.EndAccept(ar);   //Obtain connection Socket
                    var processor = new Action<Socket, CancellationToken>(ProcessConnection);
                    var processResult = processor
                        .BeginInvoke(handler, _cancellationTokenSource.Token, this.OnProcessConnectionCompleted, null);

                    this._activeConnectionsTable
                        .TryAdd(processResult, byte.MinValue);

                    _log.Info($"-> Active connections : {this.ActiveConnectionCount}");
                }

                if(this._commServiceState == CommServiceState.StoppingCycle)
                {
                    _log.Error($"-> connection killed in stopping cycle");
                    //TODO : We received a connection after stop cycle has been initiated - tell the connection to stop and kill it
                }
            }//End lock

            _log.Info("-> Done OnAccept");
        }


        private void OnProcessConnectionCompleted(IAsyncResult ar)
        {
            byte dummy;
            this._activeConnectionsTable
                .TryRemove(ar, out dummy);
        }

        private void ProcessConnection(Socket handlerSocket, CancellationToken ct)
        {
            using (handlerSocket)
            using (var ns = new NetworkStream(handlerSocket, false))
            {
                ns.ReadTimeout = 10000; //10 seconds

                try
                {    
                    var messages = 10;
                    var messageCounter = 1;
                    var timeoutCounter = 0;
                    var exit = false;
                    var requestCmd = default(CommandPacket);
                    var responseCmd = default(CommandPacket);

                    do
                    {
                        try
                        {
                            _log.Info($"->> Before cancel check Is data avilable : {ns.DataAvailable}");

                            if (ct.IsCancellationRequested)
                            {
                                _log.Info("Cancelled!");
                                responseCmd = new CommandPacket(Command.Cmd.End, null);
                                ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                                break;
                            }

                            _log.Info($"->> After cancel check Is data avilable : {ns.DataAvailable}");

                            requestCmd = CommandPacket.Read(ns);

                            _log.Info($"Processing ({messageCounter}) : {requestCmd.Cmd}");

                            if (requestCmd.Cmd == Command.Cmd.Status)
                            {
                                responseCmd = new CommandPacket(Command.Cmd.Message, $"Msg: {messageCounter}");
                                ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                            }
                            else if (requestCmd.Cmd == Command.Cmd.Ack 
                                && messageCounter <= messages)
                            {
                                responseCmd = new CommandPacket(Command.Cmd.Message, $"Msg: {messageCounter}");
                                ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                            }
                            else if (requestCmd.Cmd == Command.Cmd.Ack 
                                && messageCounter > messages)
                            {
                                responseCmd = new CommandPacket(Command.Cmd.End, $"Mesg: {messageCounter}");
                                ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                                exit = true;
                            }
                            else if (requestCmd.Cmd == Command.Cmd.End)
                            {
                                responseCmd = new CommandPacket(Command.Cmd.End, null);
                                ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                                exit = true;
                            }
                            else
                            {
                                responseCmd = new CommandPacket(Command.Cmd.Error, "Unknown");
                                ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                                exit = true;
                            }

                            timeoutCounter = 0; //reset timeouts
                            messageCounter++;
                        }//End try
                        catch(IOException ioException)
                        {
                            var socketException = ioException.InnerException as SocketException;
                            if(socketException?.SocketErrorCode == SocketError.TimedOut)
                            {
                                if (timeoutCounter < 5)
                                {
                                    responseCmd = new CommandPacket(Command.Cmd.Timeout, null);
                                    ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                                    timeoutCounter++;
                                }
                                else
                                {
                                    responseCmd = new CommandPacket(Command.Cmd.End, "Timeout!");
                                    ns.Write(responseCmd.GetPacketBuffer(), 0, CommandPacket.PACKET_SIZE);
                                    exit = true;
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }//End catch
                    } while (!exit);
                }//End try
                catch(Exception ex)
                {
                    _log.Error("-> Soc error occured", ex);
                }
                finally
                {
                    handlerSocket.Shutdown(SocketShutdown.Both);
                    handlerSocket.Close();
                    _log.Info("Closed");
                }
            }//End using
        }
        #endregion

        #region INTERNAL TYPES
        private enum CommServiceState
        {
            Stopped,
            StoppingCycle,
            Running,
            Paused
        }
        #endregion
    }
}
