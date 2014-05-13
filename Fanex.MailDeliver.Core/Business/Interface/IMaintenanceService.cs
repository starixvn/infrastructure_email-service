// --------------------------------------------------------------------------
// <copyright file="IMaintenanceService.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2014.5.13">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Business.Interface
{
    using Fanex.NAP.Core.Model.ValueObject;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IMaintenanceService
    {
        Downtime GetDowntimeInfo();
    }
}