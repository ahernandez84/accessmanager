using System;
using System.Configuration;

using NLog;

namespace AccessManager.Services
{
    public class AppConfigService
    {
        public string FailoverPartner { get; private set; } = "";
        public int MonitorTimeout { get; private set; } = 1000;
        public int CmdLineTimeout { get; private set; } = 1000;
        public bool ActivateContinuum { get; private set; } = false;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool ReadSettings()
        {
            try
            {
                MonitorTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["MonitorTimeout"]);
                CmdLineTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["CmdLineTimeout"]);
                ActivateContinuum = ConfigurationManager.AppSettings["ActivateContinuum"] == "True";
                FailoverPartner = ConfigurationManager.AppSettings["FailoverPartner"];

                return true;
            }
            catch (Exception ex) { logger.Error(ex, "AppConfigService <ReadSettings> method."); return false; }
        }
    }
}
