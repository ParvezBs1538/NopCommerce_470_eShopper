using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public class TreeUPdateModel
{
    public TreeUPdateModel()
    {
        Data = new List<Child>();
    }

    public int MegaMenuId { get; set; }

    public IList<Child> Data { get; set; }

    public class Child
    {
        public Child()
        {
            Children = new List<Child>();
        }

        public int Id { get; set; }

        public IList<Child> Children { get; set; }
    }
}

public class NodeDeleteResponseModel
{
    public NodeDeleteResponseModel()
    {
        Nodes = new List<NodeModel>();
    }

    public bool Result { get; set; }

    public IList<NodeModel> Nodes { get; set; }

    public class NodeModel
    {
        public int EntityId { get; set; }

        public int MenuItemTypeId { get; set; }
    }
}
