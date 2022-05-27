using System;

namespace Pdbc.Cli.App.Extensions
{
    public static class NamingExtensions
    {
        public static String ToSpecification(this string name, string subname = null)
        {
            if (subname == null)
                return $"{name}Specification";

            return $"{name}{subname}Specification";
        }

        public static String ToDbContext(this string name)
        {
            return $"{name}DbContext";
        }

        public static String ToController(this string name)
        {
            return $"{name}Controller";
        }
        public static String ToDataDto(this string name)
        {
            return $"{name}DataDto";
        }
        public static String ToDto(this string name)
        {
            return $"{name}Dto";
        }
        public static String ToHandler(this string name)
        {
            return $"{name}Handler";
        }
        public static String ToValidator(this string name)
        {
            return $"{name}Validator";
        }
        public static String ToRepository(this string name)
        {
            return $"{name}Repository";
        }
        public static String ToRepositoryInterface(this string name)
        {
            return $"{name.ToRepository().ToInterface()}";
        }
        public static String ToFactory(this string name)
        {
            return $"{name}Factory";
        }
        public static String ToChangesHandler(this string name)
        {
            return $"{name}ChangesHandler";
        }

        public static string ToContextSpecification(this string type)
        {
            return $"ContextSpecification<{type}>";
        }

        public static string ToInterface(this string type)
        {
            return $"I{type}";
        }

        public static string ToTest(this string type)
        {
            return $"{type}Test";
        }

        public static string ToIntegrationTest(this string type)
        {
            return $"{type}IntegrationTest";
        }
        public static string ToEntityConfigurationClass(this string type)
        {
            return $"{type}Configuration";
        }
        public static string ToTestDataBuilder(this string type)
        {
            return $"{type}TestDataBuilder";
        }

        public static string ToTestDataObjects(this string applicationName)
        {
            return $"{applicationName}TestDataObjects";
        }
        public static string ToBuilder(this string type)
        {
            return $"{type}Builder";
        }
    }

    public static class StringExtensions
    {
        public static String Quoted(this string s)
        {
            return $"\"{s}\"";
        }
    }
}