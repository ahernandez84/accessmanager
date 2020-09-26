using System.ServiceProcess;

namespace AccessManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new AccessService()
            };
            ServiceBase.Run(ServicesToRun);
        }

    }
}
