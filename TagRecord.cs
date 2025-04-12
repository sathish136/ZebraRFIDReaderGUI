using System;

namespace ZebraRFIDReaderGUI
{
    public class TagRecord
    {
        public string TagID { get; set; }
        public int SeenCount { get; set; }
        public short PeakRSSI { get; set; }
        public int AntennaID { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
