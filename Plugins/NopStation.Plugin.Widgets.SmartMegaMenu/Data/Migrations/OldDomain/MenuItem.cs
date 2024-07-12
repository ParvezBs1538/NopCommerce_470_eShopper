namespace NopStation.Plugin.Widgets.SmartMegaMenu.Data.Migrations.OldDomain;

public class MenuItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string MenuType { get; set; }
    public string Url { get; set; }
    public int MenuId { get; set; }
    public string MenuItemId { get; set; }
    public string ParentItem { get; set; }
    public bool SubjectToAcl { get; set; }
    public string RibbonText { get; set; }
    public string RibbonColor { get; set; }
    public string RibbonTextColor { get; set; }
    public int PictureId { get; set; }
}
