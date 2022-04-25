﻿using System;
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

        public void WriteParameters()
        {
            // TODO Parse args

            Console.WriteLine("Startup Parameters");
            Console.WriteLine($"EntityName: {EntityName}");
            Console.WriteLine($"PluralEntityName: {PluralEntityName}");

            Console.WriteLine($"Action: {Action}");
            Console.WriteLine($"ActionName: {ActionName}");
            Console.WriteLine($"FullActionName: {FullActionName}");
        }

        public string ActionName
        {
            get
            {
                if (Action is "Store" or "Create" or "Update" or "List" or "Get")
                {
                    return Action;
                }

                return Action;
            }
        }

        public string FullActionName
        {
            get
            {
                return $"{ActionName}{EntityName}";
            }
        }

        public Boolean ShouldGenerateCqrs()
        {
            return ActionName != null;
        }

        public String CqrsInputType
        {
            get
            {
                if (ActionName == "Get" ||
                    ActionName == "List")
                {
                    return "Query";
                }

                return "Command";
            }
        }

        public string CqrsInputClassName => $"{ActionName}{EntityName}{CqrsInputType}";
        public string CqrsOutputClassName => $"{ActionName}{EntityName}{CqrsOutputType}";

        public String CqrsOutputType
        {
            get
            {
                if (ActionName == "Get" ||
                    ActionName == "List")
                {
                    return "ViewModel";
                }

                return "Result";
            }
        }
        public string ActionEntityDtoName => $"{EntityName}{ActionName}Dto";
        public string ActionEntityInterfaceDtoName => $"I{ActionEntityDtoName}";



        public string EntityConfigurationName=> $"{EntityName}Configuration";

        public string EntitySpecificationName => $"{EntityName}Specification";

        public string EntityRepositoryClassName => $"{EntityName}Repository";

        public string EntityRepositoryInterfaceName => $"I{EntityRepositoryClassName}";

        public string EntityRepositorySpecificationName => $"{EntityRepositoryClassName}Specification";

        public string EntityRepositorySpecificationQueriesName => $"{EntityRepositoryClassName}QueriesSpecification";

        public string EntityBuilderName => $"{EntityName}Builder";

        public string EntityTestDataBuilderName => $"{EntityName}TestDataBuilder";
        public string DatabaseContextName { get; set; }
    }
}
