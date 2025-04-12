using System;

namespace ZebraRFIDReaderGUI
{
    public class TagRecord
    {
        public string TagID { get; set; }
        public DateTime LastSeenTime { get; set; }
        public int AntennaID { get; set; }
        public int RSSI { get; set; }
        public int SeenCount { get; set; }
    }
}
