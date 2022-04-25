using System;

namespace Pdbc.Cli.App.Model
{
    public class PropertyItem
    {
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