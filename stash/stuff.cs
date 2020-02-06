using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;


namespace CodeAnalysisApp1
{
    class Program
    {
        const string programText = @"
            using System;
            using System.Collections.Generic;
            using System.Text;
 
            namespace HelloWorld
            {
                
        public interface ITestInterface1
        {
            public void TestFunction1();
        }

        public interface ITestInterface2
        {
            public void TestFunction2();
        }

        public class DangerousClass
        {
            public int unusedData;
        }

        public class TestClass1 : ITestInterface1
        {
            public void TestFunction1()
            {
            }
        }

        public class TestClass2 : ITestInterface2
        {
            public void TestFunction2()
            {
            }
        }

        public class DangerousClassExtension : DangerousClass, ITestInterface1
        {
            public void TestFunction1()
            {
            }
        }

        public void Test()
        {
                TestClass1 testClass1 = new TestClass1();
                TestClass2 testClass2 = new TestClass2();
                DangerousClassExtension dangerous = new DangerousClassExtension();

                testClass1?.TestFunction1();
                testClass2?.TestFunction2();
                dangerous?.TestFunction1();

                TestClass1 other = testClass1 == null ? testClass1 : null;
        }

                class Program
                {
                    static void Main(string[] args)
                    {
                        Test();
                    }
                }
            }";

        static void Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            var compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);

            SemanticModel model = compilation.GetSemanticModel(tree);
            NameSyntax nameSyntax = root.Usings[0].Name;

            var results = root.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>();
            foreach (ConditionalAccessExpressionSyntax result in results) {
                Console.WriteLine(result.GetText());
                var expression = result.Expression;
                Console.WriteLine(expression.());
                Console.WriteLine(result.Expression);
            }

             
            SymbolInfo nameInfo = model.GetSymbolInfo(nameSyntax);
            Console.WriteLine(nameInfo.Symbol);
        }

        public interface ITestInterface1
        {
            public void TestFunction1();
        }

        public interface ITestInterface2
        {
            public void TestFunction2();
        }

        public class DangerousClass
        {
            public int unusedData;
        }

        public class TestClass1 : ITestInterface1
        {
            public void TestFunction1()
            {
            }
        }

        public class TestClass2 : ITestInterface2
        {
            public void TestFunction2()
            {
            }
        }

        public class DangerousClassExtension : DangerousClass, ITestInterface1
        {
            public void TestFunction1()
            {
            }
        }

        public void Test()
        {
                TestClass1 testClass1 = new TestClass1();
                TestClass2 testClass2 = new TestClass2();
                DangerousClassExtension dangerous = new DangerousClassExtension();

                testClass1?.TestFunction1();
                testClass2?.TestFunction2();
                dangerous?.TestFunction1();
        }

        /*
        static async Task Main(string[] args)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                var solutionPath = args[0];
                Console.WriteLine($"Loading solution '{solutionPath}'");

                // Attach progress reporter so we print projects as they are loaded.
                var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                Console.WriteLine($"Finished loading solution '{solutionPath}'");

                // TODO: Do analysis on the projects in the loaded solution
            }
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
        */
    }
}
