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

        [Option('r', "withoutResponse", Required = false, HelpText = "Indicates that the action should not return a response (only for commands).")]
        public Boolean WithoutResponse { get; set; }


        public void WriteParameters()
        {
            Console.WriteLine("Mandatory Startup Parameters");
            Console.WriteLine($" == EntityName: {EntityName}");

            Console.WriteLine("Optional Startup Parameters");
            Console.WriteLine($" == PluralEntityName: {PluralEntityName}");
            Console.WriteLine($" == Action: {Action}");
        }

    }
}
