using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Mono.Cecil;
using NLog;
using System.Reflection;

namespace FortySixModsLater
{
    public class RoslynCompiler
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public CSharpCompilationOptions CompileOptions { get; set; }
        public string GameManagedPath { get; set; }

        public RoslynCompiler()
        {
        }

        public void Init(string gameManagedPath, List<string> resolveDirs)
        {
            GameManagedPath = gameManagedPath;
            CompileOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Release).WithAllowUnsafe(true);
        }

        public void Patch(string assemblyName, string gameManagedPath, ModuleDefinition gameModuleDef, List<string> patchDirs, List<string> resolveDirs)
        {
            _log.Info("Beginning PatchScripts patch");

            Init(gameManagedPath, resolveDirs);

            bool success = false;

            try
            {
                var trees = new List<SyntaxTree>();

                foreach (var folder in patchDirs)
                {
                    List<string> patchScripts = null;

                    if (!Utils.GetAllCSFiles(folder, out patchScripts))
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
                var dlls = Directory.GetFiles(GameManagedPath, "*.dll").Where(dll => dll.Contains("Unity.Burst.Cecil") == false && dll.Contains("Assembly-CSharp.dll") == false && dll.Contains("ModManagerEx.dll") == false);

                foreach (var dll in dlls)
                {
                    MetadataReference mdRef = AssemblyMetadata.CreateFromFile(dll).GetReference();
                    mdRefs.Add(mdRef);
                }

                mdRefs.Add(AssemblyMetadata.CreateFromFile(Application.StartupPath + "46ModsLater.dll").GetReference());
                byte[] patchedDllBytes = File.ReadAllBytes(Path.Combine(Application.StartupPath, "Temp", "Patched-Assembly-CSharp.dll"));
                mdRefs.Add(AssemblyMetadata.CreateFromImage(patchedDllBytes).GetReference());

                _log.Info("Compiling PatchScripts.dll");
                var compilation = CSharpCompilation.Create("PatchScripts", trees, mdRefs, CompileOptions);

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

                if (resultStream == null || patchBytes == null || patchBytes.Length == 0)
                {
                    throw new Exception("Failed to compile the PatchScripts.");
                }

                foreach (var d in resultStream.Diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        _log.Error($"ERROR: {d}");
                    }
                    else
                    {
                        if (d.DefaultSeverity == DiagnosticSeverity.Warning && d.ToString().Contains("Assuming assembly reference 'mscorlib"))
                            continue;

                        _log.Info($"Warning: {d}");
                    }
                }

                if (!resultStream.Success)
                {
                    throw new Exception("Failed to build PatchScripts.dll due to compiler errors.");
                }

                // Load the resulting assembly into this domain. 
                Assembly psAssembly = Assembly.Load(patchBytes);
                var iPatchType = typeof(IPatcherMod);
                var exportedTypes = psAssembly.ExportedTypes;
                var iPatchTypes = exportedTypes.Where(t => iPatchType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                using (var psModule = Utils.ReadModuleDefinition(patchBytes, GameManagedPath))
                {
                    foreach (var i in iPatchTypes)
                    {
                        IPatcherMod mod = Activator.CreateInstance(i) as IPatcherMod;
                        if (mod != null)
                        {
                            _log.Info($"Patching game module with mod: {mod}");
                            mod.Patch(gameModuleDef);
                            mod.Link(gameModuleDef, psModule);
                        }
                    }
                }

                // write patched assembly to game managed folder
                string modPatchedPath = Path.Combine(Application.StartupPath, "Temp", "Patched-Assembly-CSharp.dll");
                gameModuleDef.Write(modPatchedPath);
                success = true;
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

        public void Create(string assemblyName, string gameManagedPath, ModuleDefinition gameModuleDef, string modPath, List<string> resolveDirs, List<string> modDirs)
        {
            _log.Info("Create Harmony mods start");

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

                        if (!Utils.GetAllCSFiles(folder, out scripts))
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
                var dlls = Directory.GetFiles(GameManagedPath, "*.dll").Where(dll => dll.Contains("Unity.Burst.Cecil") == false && dll.Contains("Assembly-CSharp.dll") == false && dll.Contains("ModManagerEx.dll") == false);

                foreach (var dll in dlls)
                {
                    MetadataReference mdRef = AssemblyMetadata.CreateFromFile(dll).GetReference();
                    mdRefs.Add(mdRef);
                }

                byte[] patchedDllBytes = File.ReadAllBytes(Path.Combine(Application.StartupPath, "Temp", "Patched-Assembly-CSharp.dll"));
                mdRefs.Add(AssemblyMetadata.CreateFromImage(patchedDllBytes).GetReference());

                var compilation = CSharpCompilation.Create(assemblyName, trees, mdRefs, CompileOptions);
                string outHarmonyPath = Path.Combine(modPath, assemblyName + ".dll");
                EmitResult emitResult = compilation.Emit(outHarmonyPath);

                if (emitResult == null)
                {
                    throw new Exception($"Failed to compile the Harmony Mod: {assemblyName}");
                }

                foreach (var d in emitResult.Diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        _log.Error($"ERROR: {d}");
                    }
                    else
                    {
                        if (d.DefaultSeverity == DiagnosticSeverity.Warning && d.ToString().Contains("Assuming assembly reference 'mscorlib"))
                            continue;

                        _log.Info($"Warning: {d}");
                    }
                }

                if (!emitResult.Success)
                {
                    throw new Exception($"Failed to compile {assemblyName}.dll due to compiler errors.");
                }

                success = true;
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
    }
}
