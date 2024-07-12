using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public class ScheduleModel
{
    public ScheduleModel()
    {
        SelectedDaysOfMonth = new List<int>();
        SelectedDaysOfWeek = new List<int>();
        AvailableDaysOfMonth = new List<SelectListItem>();
        AvailableDaysOfWeek = new List<SelectListItem>();
        AvailableScheduleTypes = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.AvaliableDateTimeFromUtc")]
    [UIHint("DateTimeNullable")]
    public DateTime? AvaliableDateTimeFromUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.AvaliableDateTimeToUtc")]
    [UIHint("DateTimeNullable")]
    public DateTime? AvaliableDateTimeToUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.ScheduleType")]
    public int ScheduleTypeId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.ScheduleType")]
    public string ScheduleTypeString { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.TimeOfDayFromUtc")]
    [UIHint("TimeNullable")]
    public DateTime? TimeOfDayFromUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.TimeOfDayToUtc")]
    [UIHint("TimeNullable")]
    public DateTime? TimeOfDayToUtc { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.SelectedDaysOfWeek")]
    public IList<int> SelectedDaysOfWeek { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.Schedules.Fields.SelectedDaysOfMonth")]
    public IList<int> SelectedDaysOfMonth { get; set; }

    public IList<SelectListItem> AvailableDaysOfWeek { get; set; }
    public IList<SelectListItem> AvailableDaysOfMonth { get; set; }
    public IList<SelectListItem> AvailableScheduleTypes { get; set; }
}
