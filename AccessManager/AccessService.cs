using System.ServiceProcess;
using System.Timers;

using AccessManager.Controllers;
using AccessManager.Services;
using NLog;

namespace AccessManager
{
    public partial class AccessService : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MonitorController monitor = new MonitorController();
        private Timer tmrCheckForEvents;

        public AccessService()
        {
            InitializeComponent();

            AppConfigService app = new AppConfigService();
            app.ReadSettings();

            tmrCheckForEvents = new Timer();
            tmrCheckForEvents.Interval = app.MonitorTimeout;
            tmrCheckForEvents.Elapsed += TmrCheckForEvents_Elapsed;
        }

        #region DELETE WHEN READY
        ////public void MockOnStart()
        ////{
        ////    logger.Info("Service started.");

        ////    var result = monitor.Initialize();

        ////    if (result.InitCode != 0)
        ////    {
        ////        logger.Warn($"Monitor controller failed to initialize:  Error Code = {result.InitCode},  {GetFailedInitMessage(result.InitCode)}");
        ////        this.Stop();
        ////        return;
        ////    }

        ////    tmrCheckForEvents.Start();

        ////    logger.Info(result.Status);
        ////}
        #endregion

        protected override void OnStart(string[] args)
        {
            logger.Info("Service started.");
             
            var (InitCode, Status) = monitor.Initialize();

            if (InitCode != 0)
            {
                logger.Warn($"Monitor controller failed to initialize:  Error Code = {InitCode },  {GetFailedInitMessage(InitCode)}");
                this.Stop();
                return;
            }

            tmrCheckForEvents.Start(); // start polling

            logger.Info(Status);
        }

        protected override void OnStop()
        {
            monitor.StopMonitoring();

            logger.Info($"Service stopped.");
        }

        private void TmrCheckForEvents_Elapsed(object sender, ElapsedEventArgs e)
        {
            monitor.CheckForNewEvents();
        }

        #region Get Failed Init Message
        private string GetFailedInitMessage(int initCode)
        {
            var message = "Failed to initialize";

            switch (initCode)
            {
                case -1:
                    message = "Failed to read app config file.";
                    break;

                case -2:
                    message = "Failed to get Continuum Connection string.";
                    break;

                case -3:
                    message = "SQL Connection Failed.";
                    break;

                case -4:
                    message = "Failed to load xml configuration file.";
                    break;
            }

            return message;
        }
        #endregion

    }
}
