using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Mono.Cecil;
using NLog;
using System.Reflection;

namespace FortySixModsLater
{
    internal class RoslynCompiler
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public CSharpCompilationOptions CompileOptions { get; set; }
        public string GameManagedPath { get; set; }

        public RoslynCompiler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void Init(string gameManagedPath, List<string> resolveDirs)
        {
            GameManagedPath = gameManagedPath;
            CompileOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Release).WithAllowUnsafe(true);
        }


        public void Patch(string assemblyName, string gameManagedPath, List<string> patchDirs, List<string> resolveDirs)
        {
            Init(gameManagedPath, resolveDirs);

            bool success = false;
            try
            {
                var trees = new List<SyntaxTree>();

                foreach (var folder in patchDirs)
                {
                    List<string> patchScripts = null;

                    if (!ModsManager.GetAllCSFiles(folder, out patchScripts))
                        continue;

                    foreach (var patch in patchScripts)
                    {
                        _log.Info($"Reading patch script: {patch}");
                        var sourceCode = File.ReadAllText(patch);
                        var tree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode)).WithFilePath(patch);
                        trees.Add(tree);
                    }
                }

                List<MetadataReference> mdRefs = new List<MetadataReference>();
                // for the initial patch scripts, we want to skip adding Burst.Cecil because it conflicts with Mono.Cecil
                var dlls = Directory.GetFiles(GameManagedPath, "*.dll").Where(dll => dll.Contains("Unity.Burst.Cecil") == false && dll.Contains("Assembly-CSharp.dll") == false);

                foreach (var dll in dlls)
                {
                    MetadataReference mdRef = AssemblyMetadata.CreateFromFile(dll).GetReference();
                    mdRefs.Add(mdRef);
                }

                mdRefs.Add(AssemblyMetadata.CreateFromFile(Application.StartupPath + "46ModsLater.dll").GetReference());

                var compilation = CSharpCompilation.Create("PatcherScripts", trees, mdRefs, CompileOptions);

                byte[] patchBytes = Array.Empty<byte>();
                EmitResult resultStream = null;

                using (var memStream = new MemoryStream())
                {
                    resultStream = compilation.Emit(memStream);

                    if (resultStream.Success)
                    {
                        patchBytes = memStream.ToArray();
                    }
                }

                if (resultStream == null)
                {
                    throw new Exception("Failed to compile the PatchScripts.");
                }

                if (patchBytes == null || patchBytes.Length == 0)
                {
                    throw new Exception("Failed to convert the PatchScripts into a byte array.");
                }

                // Load the resulting assembly into this domain. 
                Assembly psAssembly = Assembly.Load(patchBytes);

                if (resultStream.Success)
                {
                    _log.Info("Success building PatchScripts.dll");
                    success = true;
                }

                foreach (var d in resultStream.Diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        _log.Error(d.ToString());
                    }
                    else
                    {
                        if (d.DefaultSeverity == DiagnosticSeverity.Warning && d.ToString().Contains("Assuming assembly reference 'mscorlib"))
                            continue;

                        _log.Info(d.ToString());
                    }
                }

                // load temp assembly
                string outPatchedAssembly = Path.Combine(Application.StartupPath, "Temp", "Assembly-CSharp.dll");
                ModuleDefinition gameModule = ReadModuleDefinition(outPatchedAssembly);
                var iPatchType = typeof(IPatcherMod);
                var exportedTypes = psAssembly.ExportedTypes;
                var iPatchTypes = exportedTypes.Where(t => iPatchType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var i in iPatchTypes)
                {
                    IPatcherMod mod = Activator.CreateInstance(i) as IPatcherMod;
                    if (mod != null)
                    {
                        _log.Info($"Patching game module with mod: {mod}");
                        mod.Patch(gameModule);
                    }
                }

                // write patched assembly to game managed folder
                string patchedOutput = Path.Combine(GameManagedPath, "Assembly-CSharp.dll");
                gameModule.Write(patchedOutput);
            }
            catch (Exception ex)
            {
                _log.Error($"Caught exception in Patch: {ex}");
                success = false;
            }
            finally
            {
                _log.Info($"Patched optional 'PatchScripts' successfully: {success}");
            }
        }

        public void Create(string assemblyName, string gameManagedPath, string modPath, List<string> resolveDirs, List<string> modDirs)
        {
            Init(gameManagedPath, resolveDirs);

            bool success = false;
            try
            {
                var trees = new List<SyntaxTree>();

                foreach (var folder in modDirs)
                {
                    _log.Info($"Searching folder: {folder}");

                    if (Directory.Exists(folder))
                    {
                        List<string> scripts = null;

                        if (!ModsManager.GetAllCSFiles(folder, out scripts))
                            continue;

                        foreach (var script in scripts)
                        {
                            _log.Info($"Reading cs script: {script}");
                            var sourceCode = File.ReadAllText(script);
                            var tree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode)).WithFilePath(script);
                            trees.Add(tree);
                        }
                    }
                }

                List<MetadataReference> mdRefs = new List<MetadataReference>();
                var dlls = Directory.GetFiles(GameManagedPath, "*.dll").Where(dll => dll.Contains("Unity.Burst.Cecil") == false);

                foreach (var dll in dlls)
                {
                    MetadataReference mdRef = AssemblyMetadata.CreateFromFile(dll).GetReference();
                    mdRefs.Add(mdRef);
                }

                var compilation = CSharpCompilation.Create(assemblyName, trees, mdRefs, CompileOptions);
                string outHarmonyPath = Path.Combine(modPath, assemblyName + ".dll");
                EmitResult emitResult = compilation.Emit(outHarmonyPath);

                if (emitResult == null)
                {
                    throw new Exception($"Failed to compile the Harmony Mod: {assemblyName}");
                }

                if (emitResult.Success)
                {
                    success = true;
                }

                foreach (var d in emitResult.Diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        _log.Error(d.ToString());
                    }
                    else
                    {
                        if (d.DefaultSeverity == DiagnosticSeverity.Warning && d.ToString().Contains("Assuming assembly reference 'mscorlib"))
                            continue;

                        _log.Info(d.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Caught exception in Create: {ex}");
                success = false;
            }
            finally
            {
                _log.Info($"Created Mod: {assemblyName}.dll successfully: {success}");
            }
        }

        // Original code from DMT - Hal9000
        private ModuleDefinition ReadModuleDefinition(string path)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(GameManagedPath);

            ModuleDefinition module = ModuleDefinition.ReadModule(path, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver, });
            return module;
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string filename = args.Name.Split(',')[0] + ".dll".ToLower();

            var gameLocation = Path.GetDirectoryName(GameManagedPath);
            string asmFile = Path.Combine(gameLocation,  filename);

            try
            {
                return Assembly.LoadFrom(asmFile);
            }
            catch (Exception ex)
            {
                _log.Error($"AssemblyResolve caught exception: {ex}");
                return null;
            }
        }
    }
}
