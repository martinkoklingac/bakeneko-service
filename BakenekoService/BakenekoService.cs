using Commons.Data.Comm;
using log4net;
using System.ServiceProcess;

namespace BakenekoService
{
    public class BakenekoService :
        ServiceBase
    {
        #region PUBLIC FIELDS
        public const string NAME = "BakenekoService";
        public static ILog _log = LogManager.GetLogger(typeof(BakenekoService));
        #endregion

        #region PRIVATE FIELDS
        private readonly CommService _commService;
        #endregion

        #region CONSTRUCTORS
        public BakenekoService(
            CommService commService)
        {
            _commService = commService;
        }
        #endregion
        
        #region PRIVATE METHODS
        protected override void OnStart(string[] args)
        {
            _log.Info("OnStart");
            _commService.Start();
        }

        protected override void OnStop()
        {
            _log.Info("OnStop");
            _commService.Stop();
        }

        protected override void OnPause()
        {
            _log.Info("OnPause");
        }

        protected override void OnContinue()
        {
            _log.Info("OnContinue");
        }

        protected override void OnCustomCommand(int command)
        {
            {
                var cmd = Command.Convert(command);

                //TODO : Invoke the inteded command process

                _log.InfoFormat("OnCustomCommand - command:[{0}]", cmd);
                _log.Info("OnCustomCommand - Waiting for a connection");
            }//End block
        }

        protected override void OnShutdown()
        {
            _log.Info("OnShutdown");
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            _log.Info("OnSessionChange");
            base.OnSessionChange(changeDescription);
        }

        protected override void Dispose(bool disposing)
        {
            //if (this.IsEndpointActive)
            //{
            //    _log.Info("Dispose - disposing endpoint");
            //    this._socketListener.Disconnect(false);
            //    this._socketListener.Dispose();
            //}

            base.Dispose(disposing);
        }
        #endregion
    }
}
