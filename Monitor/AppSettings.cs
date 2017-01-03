using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor
{
    public class AppSettings :
        IAppSettings
    {
        #region CONSTRUCTORS
        public AppSettings()
        {
            this.ServiceMachineName = ConfigurationManager.AppSettings["ServiceMachineName"];
            this.ServiceName = ConfigurationManager.AppSettings["ServiceName"];
        }
        #endregion

        #region PUBLIC PROPERTIES
        public string ServiceMachineName { get; }
        public string ServiceName { get; }
        #endregion
    }
}
