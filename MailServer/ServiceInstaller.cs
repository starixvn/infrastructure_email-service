// --------------------------------------------------------------------------
// <copyright file="ServiceInstaller.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.21">Created</change>
// </history>

namespace Fanex.MailDeliver.MailServer
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.ServiceProcess;

    /// <summary>
    /// Service installer.
    /// </summary>
    [RunInstaller(true)]
    public partial class ServiceInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller _process;
        private System.ServiceProcess.ServiceInstaller _service;

        public ServiceInstaller()
        {
            InitializeComponent();
            _process = new ServiceProcessInstaller();
            _process.Account = ServiceAccount.LocalSystem;
            _service = new System.ServiceProcess.ServiceInstaller();

            string[] commandlineArgs = Environment.GetCommandLineArgs();

            string serviceName;
            ParseServiceNameSwitches(commandlineArgs, out serviceName);

            _service.ServiceName = serviceName;
            _service.Description = "Fanex - Email - Mail Service - Name " + serviceName;
            _service.StartType = ServiceStartMode.Automatic;
            Installers.Add(_process);
            Installers.Add(_service);
        }

        /// <summary>
        /// Get Command line arguments.
        /// </summary>
        /// <param name="commandlineArgs">Command line arguments.</param>
        /// <param name="serviceName">Out service name.</param>
        private void ParseServiceNameSwitches(string[] commandlineArgs, out string serviceName)
        {
            try
            {
                var servicenameswitch = (from s in commandlineArgs where s.StartsWith("/servicename") select s).FirstOrDefault();

                if (servicenameswitch == null)
                {
                    throw new ArgumentException("Argument 'servicename' is missing");
                }

                if (!(servicenameswitch.Contains('=') || servicenameswitch.Split('=').Length < 2))
                {
                    throw new ArgumentNullException("The /servicename switch is malformed");
                }

                serviceName = servicenameswitch.Split('=')[1];

                serviceName = serviceName.Trim('"');
            }
            catch (Exception ex)
            {
                serviceName = "Fanex Email Service";
                Console.Write(ex.Message);
            }
        }
    }
}