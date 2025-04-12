using System.ServiceProcess;
using System.Configuration.Install;
using System.ComponentModel;

namespace ZebraRFIDReaderGUI
{
    [RunInstaller(true)]
    public class RFIDReaderServiceInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public RFIDReaderServiceInstaller()
        {
            processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = "ZebraRFIDReader",
                DisplayName = "Zebra RFID Reader Service",
                Description = "Provides RFID tag reading service with REST API"
            };

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }

    public class RFIDReaderService : ServiceBase
    {
        private Form1 mainForm;

        public RFIDReaderService()
        {
            ServiceName = "ZebraRFIDReader";
        }

        protected override void OnStart(string[] args)
        {
            mainForm = new Form1(true); // Pass true to indicate service mode
        }

        protected override void OnStop()
        {
            mainForm?.Close();
            mainForm?.Dispose();
        }

        public void StartInteractive()
        {
            OnStart(null);
        }
    }
}
