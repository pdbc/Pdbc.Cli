using System;

namespace Pdbc.Cli.App.Model
{
    public class MethodItem
    {
        public MethodItem()
        {

        }

        public MethodItem(String returnType, string name)
        {
            ReturnType = returnType;
            Name = name;
        }


        public String ReturnType { get; set; }
        public String Name { get; set; }
    }
}