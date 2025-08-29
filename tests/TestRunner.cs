using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Xunit;

namespace NovaLang.Tests
{
    public class TestRunner
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== NovaLang Test Suite ===");
            Console.WriteLine();

            // Load all test assemblies and run them
            var assembly = Assembly.GetExecutingAssembly();
            var testTypes = assembly.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(typeof(FactAttribute), false).Length > 0))
                .ToList();

            Console.WriteLine($"Found {testTypes.Count} test classes:");
            foreach (var testType in testTypes)
            {
                Console.WriteLine($"  - {testType.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("To run the tests, use:");
            Console.WriteLine("  dotnet test");
            Console.WriteLine();
            Console.WriteLine("For more detailed output:");
            Console.WriteLine("  dotnet test --verbosity normal");
            Console.WriteLine();
            Console.WriteLine("To run specific test categories:");
            Console.WriteLine("  dotnet test --filter \"FullyQualifiedName~Unit\"");
            Console.WriteLine("  dotnet test --filter \"FullyQualifiedName~Integration\"");
        }
    }
}
