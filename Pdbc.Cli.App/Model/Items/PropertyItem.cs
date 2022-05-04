using System;

namespace Pdbc.Cli.App.Model.Items
{
    public class PropertyItem
    {
        public static PropertyItem CancellationTokenPropertyItem =
            new PropertyItem("CancellationToken", "cancellationToken");

        public PropertyItem()
        {
            
        }

        public PropertyItem(String type, string name)
        {
            Type = type;
            Name = name;
        }

        public String Type { get; set; }
        public String Name { get; set; }
    }
}