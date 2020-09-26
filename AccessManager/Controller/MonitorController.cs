using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text; 

using AccessManager.Models;
using AccessManager.Services;
using NLog;

namespace AccessManager.Controllers
{
    public class MonitorController
    {
        [DllImport(".\\DLLs\\se.Homewood.HandOverLibrary.dll",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void GetConnectionString(StringBuilder sb, int size);

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private AppConfigService appConfigService;
        private SQLService sqlService; 
        private XMLService xmlService;

        private List<Point> pointsToMonitor = new List<Point>();

        public (int InitCode, string Status) Initialize()
        {
            logger.Info("Initializing Access Manager v1.0.0");

            var status = $"Access Manager Service: X | Monitoring 0 Points"; // default status message

            appConfigService = new AppConfigService();
            if (appConfigService.ReadSettings() == false)
                return (-1, status);

            var sb = new StringBuilder(1024);
            GetConnectionString(sb, sb.Capacity);

            var connectionString = sb.ToString();
            if (string.IsNullOrEmpty(connectionString))
            { 
                logger.Info("Failed to read Continuum's connection string.  Please make sure to run this program on a cyberstation.");
                return (-2, status);
            }

            if (!string.IsNullOrEmpty(appConfigService.FailoverPartner))
                connectionString += $";Failover Partner={appConfigService.FailoverPartner};";

            logger.Info("HandOver found connection string.");

            sqlService = new SQLService()
            {
                ConnectionString = connectionString,
            };

            if (!sqlService.SetMRE()) return (-3, status);

            xmlService = new XMLService();
            pointsToMonitor = xmlService.LoadFile();

            if (pointsToMonitor == null || pointsToMonitor.Count == 0) return (-4, status);

            return (0, $"Access Manager Service: OK | Monitoring {pointsToMonitor.Count} Points");
        }

        public void CheckForNewEvents()
        {
            var events = sqlService.CheckForAccessEvents();

            if (events.Count > 0)
            {
                var eventsToManage = new Dictionary<Event, string[]>();

                events.ForEach(e =>
                {
                    pointsToMonitor.ForEach(p =>
                    {
                        foreach (var d in p.DoorsToMonitor.ToList())
                        {
                            if (e.DoorIDString == d)
                                eventsToManage.Add(e, p.AreasToManage);
                        }
                    });
                });

                if (eventsToManage.Count > 0)
                {
                    logger.Info($"Found {eventsToManage.Count} event groupings to process.");

                    foreach (var kp in eventsToManage)
                    {
                        logger.Info($"Event: {kp.Key.DoorIDString}, Person: {kp.Key.PersonIdLo},  Areas: {string.Join(",", kp.Value)}");
                    }

                    //todo:  update area link table to enable the listed areas.
                }

            }

        }

        public void StopMonitoring()
        {
            try
            {
                logger.Info($"Monitoring Service: Stopped at {DateTime.Now}");
            }
            catch (Exception ex) { logger.Error(ex, "MonitorController <StopMonitoring> method."); }
        }

    }
}
