// --------------------------------------------------------------------------
// <copyright file="MaintenanceService.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2014.5.13">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Business.Implement
{
    using Fanex.MailDeliver.Core.Business.Interface;
    using Fanex.MailDeliver.Core.Infrastructure.Configurations;
    using Fanex.NAP.Core.Model.ValueObject;
    using Sunplus.UMControllerLibrary;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class MaintenanceService : IMaintenanceService
    {
        /// <summary>
        /// Get down time information.
        /// </summary>
        /// <returns>Down time object.</returns>
        public Downtime GetDowntimeInfo()
        {
            var downTime = new Downtime
            {
                From = "0:00",
                To = "0:00",
                IsDowntime = false
            };

            if (AppSettings.EnableCheckUM)
            {
                string timeFrom = string.Empty;
                string timeTo = string.Empty;
                var umctrl = new UMBusiness();

                // gets Agent Site UM Status.
                downTime.IsDowntime = umctrl.isUnderMaintainence(
                    Sites.AccountSite,
                    AppSettings.UMAddress,
                    ref timeFrom,
                    ref timeTo);

                if (downTime.IsDowntime)
                {
                    downTime.From = timeFrom;
                    downTime.To = timeTo;
                }
            }

            return downTime;
        }
    }
}