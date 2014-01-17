// --------------------------------------------------------------------------
// <copyright file="GetMailErrorCriteria.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.8.5">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Criterias
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Fanex.NAP.Framework.Data;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class GetMailErrorCriteria : CriteriaBase
    {
        public long MailID { get; set; }

        public override string GetSettingKey()
        {
            return "MailRecorder_GetError";
        }

        public override bool IsValid()
        {
            return MailID > 0;
        }
    }
}
