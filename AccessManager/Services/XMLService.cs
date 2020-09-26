using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using AccessManager.Models;
using NLog;

namespace AccessManager.Services
{
    internal class XMLService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public List<Point> LoadFile()
        {
            try
            {
                logger.Info($"App Directory:  {AppDomain.CurrentDomain.BaseDirectory}");

                XDocument doc = XDocument.Load($@"{AppDomain.CurrentDomain.BaseDirectory}\monitor.xml");

                var points = from p in doc.Descendants("Point")
                             select new Point()
                             {
                                 DoorsToMonitor = p.Element("DoorsToMonitor").Value.Split(','),
                                 AreasToManage = p.Element("AreasToManage").Value.Split(',')
                             };

                logger.Info($"XML Service found {points.Count()} point(s) to monitor.");

                return points.ToList<Point>();
            }
            catch (Exception ex) { logger.Error(ex, "XMLService <LoadFile> method."); return null; }
        }
    }
}
