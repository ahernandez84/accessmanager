using System;

namespace AccessManager.Models
{
    internal class Event
    {
        public DateTime TimeStamp { get; set; }
        public int PersonIdHi { get; set; }
        public int PersonIdLo { get; set; }
        public int DoorIdHi { get; set; }
        public int DoorIdLo { get; set; }
        public int CardNumber{ get; set; }
        public string ControllerPath { get; set; }
        public string Message { get; set; }
        public string DoorIDString { get; set; }
    }
}
