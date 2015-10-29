// --------------------------------------------------------------------------
// <copyright file="WinMailService.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.MailServer.Service
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using Fanex.MailDeliver.Business.TableCaching;
    using Fanex.MailDeliver.Business.Worker;
    using Fanex.MailDeliver.Core.Business.Worker;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;

    /// <summary>
    /// Mail windows service.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1400:AccessModifierMustBeDeclared", Justification = "Reviewed.")]
    partial class WinMailService : ServiceBase
    {
        private ServiceHost _serviceHost;
        private Task _task;
        private SendMailWorker _sendMailWorker;
        private ResendMailWorker _resendMailWoker;
        private string RealServiceName;

        public WinMailService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Function will run when service start.
        /// </summary>
        /// <param name="args">Start arguments.</param>
        protected override void OnStart(string[] args)
        {
            // Start WCF service
            try
            {
                // Open Ipc service
                if (_serviceHost != null)
                {
                    _serviceHost.Close();
                }

                _serviceHost = new ServiceHost(typeof(MailService));
                _serviceHost.Opened += new EventHandler(delegate(object sender, EventArgs ev)
                {
                    //// Init Fanex.Data
                    Fanex.Data.Configuration.DbSettingProviderManager.Current.Start();
                    //// init internal cache
                    MemInstance.Initialize();

                    _sendMailWorker = new SendMailWorker();
                    _task = Task.Factory.StartNew(() =>
                    {
                        _sendMailWorker.Run(true);
                    });

                    _resendMailWoker = new ResendMailWorker();
                    Task.Factory.StartNew(() =>
                    {
                        _resendMailWoker.Run();
                    });
                });
                _serviceHost.Faulted += new EventHandler(delegate(object sender, EventArgs ev)
                {
                    ////TODO
                });
                //// Open new service
                _serviceHost.Open();

                RealServiceName = GetServiceName();
                LogService.Info(string.Format("Service {0} starts at {1}.", RealServiceName, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        protected String GetServiceName()
        {
            try
            {
                // Calling System.ServiceProcess.ServiceBase::ServiceNamea allways returns
                // an empty string,
                // see https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=387024

                // So we have to do some more work to find out our service name, this only works if
                // the process contains a single service, if there are more than one services hosted
                // in the process you will have to do something else

                int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                String query = "SELECT * FROM Win32_Service where ProcessId = " + processId;
                System.Management.ManagementObjectSearcher searcher =
                    new System.Management.ManagementObjectSearcher(query);

                foreach (System.Management.ManagementObject queryObj in searcher.Get())
                {
                    return queryObj["Name"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return "Fanex.Email.Service";
        }

        protected override void OnStop()
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
            }

            _task = null;
            _sendMailWorker = null;
            _resendMailWoker = null;
            LogService.Info(string.Format("Service {0} stops at {1}.", RealServiceName, DateTime.Now));
        }
    }
}