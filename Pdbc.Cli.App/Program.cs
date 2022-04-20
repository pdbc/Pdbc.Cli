using System.Threading.Tasks;

namespace Pdbc.Cli.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var context = new RoslynGenerationContext();
            var fileHelperService = new FileHelperService();
            var codeGenerationService = new CodeGenerationService(context);


            // configuration
            //var basePath= @"C:\repos\Development\IM.Scharnier\";
            //var rootNamespace = "IM.Scharnier";

            var basePath= @"C:\repos\Development\IM.FuelMgmt\";
            var rootNamespace = "IM.FuelMgmt";
            var entityName = "Person";


            // setup context
            await context.Initialize(rootNamespace, basePath, fileHelperService.GetSolutionPathFrom(basePath));

            // generation
            await codeGenerationService.SetupEntity(entityName);
            


        }

    }

}