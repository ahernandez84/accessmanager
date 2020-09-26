using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using AccessManager.Models;
using NLog;

namespace AccessManager.Services
{
    internal class SQLService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public string ConnectionString { get; set; } = string.Empty;
        public string ContinuumPointToSet { get; set; } = "strEvent";

        private DateTime mre = DateTime.Now;

        public void SetConnectionString(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        #region Set MRE
        public bool SetMRE()
        {
            try
            {
                var query = @"SELECT TOP 1
	                            ae.[timeoflog]
                            FROM 
	                            accessevent ae 
                            ORDER BY timeoflog desc";

                using (SqlConnection connection = new SqlConnection(this.ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                mre = reader.GetDateTime(0);

                                logger.Info($"MRE set to {mre}.");
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex) { logger.Info(ex, "SQLService <SetMRE> method."); return false; }
        }
        #endregion

        #region Check for Access Events
        public List<Event> CheckForAccessEvents()
        {
            var events = new List<Event>();

            try
            {
                var query = @"SELECT 
	                            ae.[timeoflog], ae.personidhi, ae.personidlo, ae.dooridhi, ae.dooridlo, ae.nonabacardnumber 
	                            ,d3.controllername as network_for_infinet, d1.controllername as network_or_controller
	                            ,d2.controllername as controller_or_infinet, d.controllername as door
                                ,ae.[message]
                            FROM 
	                            accessevent ae with(nolock)
                            inner join
	                            dictionary d on (d.ObjectIdHi = ae.DoorIdHi and d.ObjectIdLo = ae.DoorIdLo)
                            inner join 
                                dictionary d2 on (d2.objectidhi=d.DeviceIdHi and d2.objectidlo=d.DeviceIdLo)
                            inner join 
                                dictionary d1 on (d1.ObjectIdHi=d2.DeviceIdHi and d1.ObjectIdLo=d2.DeviceIdLo)
                            left join 
                                dictionary d3 on (d3.ObjectIdHi=d1.DeviceIdHi and d3.ObjectIdLo=d1.DeviceIdLo)
                            WHERE 
	                            [timeoflog] > @mre and eventclass=0 
                            ORDER BY timeoflog asc";

                using (SqlConnection connection = new SqlConnection(this.ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@mre", mre);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows) return events;

                            while (reader.Read())
                            {
                                // Build controller path
                                var netForInf = reader.IsDBNull(6) ? "" : reader.GetString(8);
                                var netOrCntrl = reader.GetString(7);
                                var cntrlOrInf = reader.GetString(8);
                                //var point = this.ContinuumPointToSet;  point to set is selected from xml file

                                var cp = string.Join(@"\", netForInf, netOrCntrl, cntrlOrInf).TrimStart('\\');
                                // *****

                                events.Add(new Event()
                                {
                                    TimeStamp = reader.GetDateTime(0)
                                    , PersonIdHi = reader.GetInt32(1)
                                    , PersonIdLo = reader.GetInt32(2)
                                    , DoorIdHi = reader.GetInt32(3)
                                    , DoorIdLo = reader.GetInt32(4)
                                    , CardNumber = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                                    , ControllerPath = cp
                                    , Message = reader.GetString(10)
                                    , DoorIDString = $"{reader.GetInt32(3)}.{reader.GetInt32(4)}"
                                });

                                mre = reader.GetDateTime(0); // Update for next check
                            }

                            return events;
                        }
                    }
                }
            }
            catch (Exception ex) { logger.Info(ex, "SQLService <CheckForAccessEvents> method."); return events; }
        }
        #endregion

    }
}
