﻿using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NopStation.Plugin.SMS.TeleSign.Areas.Admin.Models
{
    public record QueuedSmsListModel : BasePagedListModel<QueuedSmsModel>
    {
    }
}