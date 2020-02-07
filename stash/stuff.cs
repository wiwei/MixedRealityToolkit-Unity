using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        
        public interface ITestInterface3
        {
            public void TestFunction3();
        }

        
        public class RootDangerousClass : ITestInterface3
        {
            public int unusedData2;
            public void TestFunction3() {}
        }

        public class DangerousClass : RootDangerousClass
        {
            public int unusedData1;
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

        class ClassVisitor : CSharpSyntaxRewriter
        {
            private SemanticModel model;
            public ClassVisitor(SemanticModel model)
            {
                this.model = model;
            }

            List<string> classes = new List<String>();

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
                var typeInfo = model.GetTypeInfo(node);
                var symbolInfo = model.GetSymbolInfo(node);
                return node;
            }
        }

        public class DangerInformation
        {
            public DangerInformation(bool isDangerous, string causalChain)
            {
                this.IsDangerous = isDangerous;
                this.CausalChain = causalChain;
            }

            public bool IsDangerous = false;
            public string CausalChain;
        }


        static Dictionary<string, string> classRelationships = new Dictionary<string, string>();
        static Dictionary<string, List<string>> interfaceRelationships = new Dictionary<string, List<string>>();
        static Dictionary<string, DangerInformation> classDanger = new Dictionary<string, DangerInformation>();
        static Dictionary<string, DangerInformation> interfaceDanger = new Dictionary<string, DangerInformation>();
        static ImmutableList<string> dangerousBaseClasses = ImmutableList.Create("MonoBehaviour");

        static async Task Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            var compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);

            SemanticModel model = compilation.GetSemanticModel(tree);
            NameSyntax nameSyntax = root.Usings[0].Name;

            var results = root.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>();
            foreach (ConditionalAccessExpressionSyntax result in results)
            {
                Console.WriteLine(result.GetText());
                var expression = result.Expression;
                var typeInfo = model.GetTypeInfo(expression);
                var symbolInfo = model.GetSymbolInfo(expression);
                Console.WriteLine("============================");
                Console.WriteLine(typeInfo.Type);
                Console.WriteLine(typeInfo.Type.BaseType);
                foreach (var value in typeInfo.Type.AllInterfaces)
                {
                    Console.WriteLine(value);
                }
            }

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var value in classes)
            {
                Console.WriteLine(value.BaseList);
                var symbolInfo = model.GetDeclaredSymbol(value);
                Console.WriteLine(symbolInfo.BaseType);
                Console.WriteLine(symbolInfo.AllInterfaces);
            }



            Console.WriteLine("Begin: " + DateTime.Now);
            DateTime beginTime = DateTime.Now;
            using (var workspace = MSBuildWorkspace.Create())
            {
                var solution = await workspace.OpenSolutionAsync(
                    "C:\\src\\MixedRealityToolkit-Unity\\MixedRealityToolkit-Unity.sln", new ConsoleProgressReporter());

                foreach (var project in solution.Projects)
                {
                    // Skip Unity-based projects, which we don't care to validate.
                    if (project.Name.StartsWith("Unity"))
                    {
                        continue;
                    }
                    Console.WriteLine("Analyzing: " + project.Name);
                    var compliationResult = await project.GetCompilationAsync();
                    foreach (var syntaxTree in compliationResult.SyntaxTrees)
                    {
                        var semanticModel = compliationResult.GetSemanticModel(syntaxTree);
                        var allClasses = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
                        foreach (var value in allClasses)
                        {
                            var symbolInfo = semanticModel.GetDeclaredSymbol(value);
                            UpdateClassRelationships(symbolInfo);
                        }
                    }
                }
            }
            Console.WriteLine("End: " + DateTime.Now);
            Console.WriteLine("Duration: " + (DateTime.Now - beginTime));
        }

        private static void UpdateClassRelationships(INamedTypeSymbol symbol)
        {
            // Because we recursively add base class relationships, it's possible
            // for us to have already processed this class if it's a common
            // shared class.
            if (classRelationships.ContainsKey(symbol.Name))
            {
                return;
            }

            // Another case is that the basetype itself is null, in which case
            // there's nothing to do.
            if (symbol.BaseType == null)
            {
                return;
            }

            string className = symbol.Name;
            string baseClassName = symbol.BaseType.Name;
            classRelationships.Add(className, baseClassName);
            UpdateClassRelationships(symbol.BaseType);
        }

        private static void UpdateClassDanger()
        {
            foreach (var dangerousBaseClass in dangerousBaseClasses)
            {
                classDanger.Add(dangerousBaseClass, new DangerInformation(true, dangerousBaseClass));
            }

            foreach (var className in classRelationships.Keys)
            {
                UpdateDanger(className);
            }
        }

        private static void UpdateInterfaceDanger()
        {
            foreach (var interfaceInfo in interfaceRelationships)
            {
                bool isDangerous = false;
                string causalChain = "";
                foreach (var className in interfaceInfo.Value)
                {
                    if (classDanger[className].IsDangerous)
                    {
                        isDangerous = true;
                        causalChain = interfaceInfo.Key + " -> " + classDanger[className].CausalChain;
                    }
                }

                interfaceDanger.Add(interfaceInfo.Key, new DangerInformation(isDangerous, causalChain));
            }
        }

        private static Boolean UpdateDanger(string className)
        {
            if (classDanger.ContainsKey(className))
            {
                return classDanger[className].IsDangerous;
            }

            // Otherwise, not evaluated - it's dangerous if anything
            // in its chain is dangerous. If it's at a root already
            // then it won't be dangerous
            Boolean isDangerous = false;
            string causalChain = "";
            if (classRelationships.ContainsKey(className))
            {
                isDangerous = UpdateDanger(classRelationships[className]);
                if (isDangerous)
                {
                    causalChain = className + " -> " + classDanger[classRelationships[className]].CausalChain;
                }
            }

            classDanger[className] = new DangerInformation(isDangerous, causalChain);
            return isDangerous;
        }

        private static string GenerateCausalChain(string className)
        {
            StringBuilder causalChain = new StringBuilder(className);

            string currentClassName = className;
            while (classRelationships.ContainsKey(currentClassName))
            {
                currentClassName = classRelationships[currentClassName];
                causalChain.Append(" -> ");
                causalChain.Append(currentClassName);
            }
            return causalChain.ToString();
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

        */
    }
}
