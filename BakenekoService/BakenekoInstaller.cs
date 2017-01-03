using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace BakenekoService
{
    [RunInstaller(true)]
    public class BakenekoInstaller :
        Installer
    {
        #region PRIVATE FIELDS
        private ServiceInstaller _serviceInstaller;
        private ServiceProcessInstaller _processInstaller;
        #endregion

        #region CONSTRUCTORS
        public BakenekoInstaller()
        {
            //Instantiate installers for process and services.
            _processInstaller = new ServiceProcessInstaller();
            _serviceInstaller = new ServiceInstaller();

            //The services run under the system account.
            //_processInstaller.Account = ServiceAccount.LocalSystem;
            _processInstaller.Account = ServiceAccount.LocalService;
            _processInstaller.Username = "hollym";
            _processInstaller.Password = "mw9xcg75ff";

            //The services are started manually.
            _serviceInstaller.StartType = ServiceStartMode.Manual;

            //ServiceName must equal those on ServiceBase derived classes.
            _serviceInstaller.ServiceName = BakenekoService.NAME;

            // Add installers to collection. Order is not important.
            this.Installers.Add(_serviceInstaller);
            this.Installers.Add(_processInstaller);
        }
        #endregion
    }
}
