using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Pdbc.Cli.App.Model
{
    public class StartupParameters
    {
        private string _pluralEntityName;

        [Option('e', "entityName", Required = true, HelpText = "The entityname you want to generate actions for.")]
        public String EntityName { get; set; }

        [Option('p', "plural", Required = false,
            HelpText =
                "The plural name of the entity, this will be used as namespaces and everywhere more than one entity is used.")]
        public String PluralEntityName
        {
            get
            {
                if (_pluralEntityName != null)
                    return _pluralEntityName;

                return $"{EntityName}Items";
            }
            set { _pluralEntityName = value; }
        }

        [Option('a', "action", Required = false, HelpText = "The action you want to generate for the entity.")]
        public String Action { get; set; }

        [Option('r', "returnDataDto", Required = false, HelpText = "Indicates that the action should return a response (only for commands, which by default return nothing).", Default = false)]
        public Boolean ReturnDataDto { get; set; }

        [Option('m', "mode", Required = false, HelpText = "The mode you want this generation to run in (single/crud).", Default = "single")]
        public string Mode { get; set; }


        public void WriteParameters()
        {
            Console.WriteLine("Mandatory Startup Parameters");
            Console.WriteLine($" == EntityName: {EntityName}");

            Console.WriteLine("Optional Startup Parameters");
            Console.WriteLine($" == PluralEntityName: {PluralEntityName}");
            Console.WriteLine($" == Action: {Action}");
            Console.WriteLine($" == ReturnDataDto: {ReturnDataDto}");
            Console.WriteLine($" == Mode: {Mode}");
        }

    }
}
