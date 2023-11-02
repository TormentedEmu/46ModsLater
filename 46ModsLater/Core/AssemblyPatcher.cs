using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NLog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace FortySixModsLater
{
    public class AssemblyPatcher
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public ModuleDefinition GameMainModule { get; set; }

        public string GameTitle { get; set; } = "AwesomeGame";
        public string GameManagedPath { get; set; } = string.Empty;

        public AssemblyPatcher()
        {
        }

        public void Patch(string gamePath)
        {
            if (!Utils.CheckPath(gamePath))
            {
                _log.Error($"Directory does not exist: {gamePath}");
                return;
            }

            GameTitle = Path.GetFileName(gamePath);
            _log.Info($"Attempting to patch game: {GameTitle}");
            GameManagedPath = Utils.FindManagedFolder(gamePath);
            if (string.IsNullOrEmpty(GameManagedPath))
            {
                _log.Error($"Failed to find the game_Data/Managed folder within: {gamePath}");
                return;
            }

            string assemblyCSharp = Path.Combine(GameManagedPath, "Assembly-CSharp.dll");
            if (!File.Exists(assemblyCSharp))
            {
                _log.Error($"Assembly-CSharp.dll does not exist in the directory: {GameManagedPath}");
                return;
            }

            _log.Info($"Found file: {assemblyCSharp}");

            bool success = false;

            try
            {
                // Make two backups of the original assembly.  First in our local folder
                string backupFolder = Path.Combine(Application.StartupPath, "BackupFiles");
                Directory.CreateDirectory(backupFolder);
                backupFolder = Path.Combine(backupFolder, "Assembly-CSharp.dll");
                if (!File.Exists(backupFolder))
                    File.Copy(assemblyCSharp, backupFolder);

                // Second copy in the game's managed folder
                string renamedFile = Path.Combine(GameManagedPath, "Assembly-CSharp.dll.ORIGINAL");
                if (!File.Exists(renamedFile))
                    File.Copy(assemblyCSharp, renamedFile);

                // ** Modifying our favorite game **

                // Read from our backup assembly so we don't lock the game's file
                GameMainModule = Utils.ReadModuleDefinition(backupFolder, GameManagedPath);

                // Implement an optional earlier mod loader in the splash scene
                // grab all the types and methods
                var modMgr = GameMainModule.Types.First(t => t.Name == "ModManager");
                var loadMods = modMgr.Methods.First(m => m.Name == "LoadMods");
                var loadModsFromFolder = modMgr.Methods.First(m => m.Name == "loadModsFromFolder");
                TypeReference boolTypeRef = loadModsFromFolder.ReturnType;
                var modClass = GameMainModule.Types.First(t => t.Name == "Mod");
                // add a new field to track a mod's init state
                FieldDefinition fdBool = new FieldDefinition("IsInit", Mono.Cecil.FieldAttributes.Public, boolTypeRef) { Constant = false };
                modClass.Fields.Add(fdBool);
                // grab the method where we'll inject our early loader into
                var platMgr = GameMainModule.Types.First(t => t.Name == "PlatformManager");
                var pmInit = platMgr.Methods.First(m => m.Name == "Init");
                var methodIL = pmInit.Body.GetILProcessor();
                var lastInst = methodIL.Body.Instructions.Last();

                // start at the end of the method and work back to find a nice place
                for (int i = methodIL.Body.Instructions.Count - 1; i >= 0; i--)
                {
                    var instr = methodIL.Body.Instructions[i];
                    // place the new loader just before the end after all the platform init code has completed successfully and is returning true
                    if (instr.OpCode == OpCodes.Ret && instr.Previous.OpCode == OpCodes.Ldc_I4_1)
                    {
                        lastInst = instr;
                        break;
                    }
                }

                methodIL.InsertBefore(lastInst, methodIL.Create(OpCodes.Callvirt, loadMods));

                // publicize some types and methods so we can access them from our override method
                Utils.SetAllMethodsToPublic(modMgr);
                Utils.SetAllFieldsToPublic(modMgr);
                Utils.SetAllNestedTypesToPublic(modMgr);

                Utils.SetAllMethodsToPublic(modClass);
                Utils.SetAllFieldsToPublic(modClass);

                // write the current state of our assembly to disk in a temp folder
                string tempFolder = Path.Combine(Application.StartupPath, "Temp");
                Directory.CreateDirectory(tempFolder);
                tempFolder = Path.Combine(tempFolder, "Patched-Assembly-CSharp.dll");
                GameMainModule.Write(tempFolder);

                // read our example patch script for our mod loader code
                _log.Info("Reading initial PatchScript: ModManagerEx.cs");
                var mmExSource = File.ReadAllText(Path.Combine(Application.StartupPath, "PatchScripts", "ModManagerEx.cs"));

                // grab a reference to all the assemblies that are needed in our mod loader
                List<MetadataReference> mdRefs = new List<MetadataReference>();
                // for the initial patch scripts, we want to skip adding Burst.Cecil because it conflicts with Mono.Cecil
                var dlls = Directory.GetFiles(GameManagedPath, "*.dll").Where(dll => dll.Contains("Unity.Burst.Cecil") == false && dll.Contains("Assembly-CSharp.dll") == false && dll.Contains("ModManagerEx.dll") == false);

                foreach (var dll in dlls)
                {
                    mdRefs.Add(AssemblyMetadata.CreateFromFile(dll).GetReference());
                }

                // include a ref to our dll
                mdRefs.Add(AssemblyMetadata.CreateFromFile(Application.StartupPath + "46ModsLater.dll").GetReference());
                byte[] patchedDllBytes = File.ReadAllBytes(Path.Combine(Application.StartupPath, "Temp", "Patched-Assembly-CSharp.dll"));
                mdRefs.Add(AssemblyMetadata.CreateFromImage(patchedDllBytes).GetReference());

                _log.Info("Compiling PatchScripts/ModManagerEx.cs");
                var modMgrExCompile = CSharpCompilation.Create("ModManagerEx", new List<SyntaxTree>() { SyntaxFactory.ParseSyntaxTree(mmExSource) }, mdRefs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                string mmDll = Path.Combine(Application.StartupPath, "Temp", "ModManagerEx.dll");
                // write the compilation to disk
                EmitResult emitResult = modMgrExCompile.Emit(mmDll);

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
                    success = false;
                    throw new Exception("Failed to compile ModManagerEx.dll due to compiler errors.");
                }

                using (ModuleDefinition tmpModule = Utils.ReadModuleDefinition(mmDll, GameManagedPath))
                {
                    // grab a ref to our new mod loader
                    var modMgrEx = tmpModule.Types.First(t => t.Name == "ModManagerEx");
                    var loadModsEx = modMgrEx.Methods.First(t => t.Name == "LoadMods");
                    // import the ref into our game's assembly
                    var importedMethod = GameMainModule.ImportReference(loadModsEx);
                    // grab the game's original method
                    var loadModsOrig = modMgr.Methods.First(t => t.Name == "LoadMods");
                    var instructions = loadModsOrig.Body.Instructions;
                    var first = instructions.First();
                    var ilProc = loadModsOrig.Body.GetILProcessor();
                    // shim our mod loader and return to end the original method
                    ilProc.InsertBefore(first, ilProc.Create(OpCodes.Call, importedMethod));
                    ilProc.InsertBefore(first, ilProc.Create(OpCodes.Ret));
                }

                // write the current state of our patched game's assembly
                string tempOutPath = Path.Combine(Application.StartupPath, "Temp", "Patched-Assembly-CSharp.dll");
                GameMainModule.Write(tempOutPath);

                // copy our compiled mod loader to the game's managed folder
                File.Copy(mmDll, Path.Combine(GameManagedPath, "ModManagerEx.dll"), true);
                success = true;
            }
            catch (Exception ex)
            {
                _log.Error($"AssemblyPatcher::Patch - Caught exception: {ex.Message}");
                success = false;
            }
            finally
            {
                _log.Info($"Patched game assembly successfully: {success}");
            }
        }

        public void WriteFinalOutput()
        {
            // write the final state of our patched game assembly to the temp folder
            string finalOutput = Path.Combine(Application.StartupPath, "Temp", "Patched-Assembly-CSharp.dll");
            GameMainModule.Write(finalOutput);

            // copy the final assembly to the game's managed folder and overwrite the file
            string managedPath = Path.Combine(GameManagedPath, "Assembly-CSharp.dll");
            File.Copy(finalOutput, managedPath, true);
            // finally dispose of the ModuleDefinition
            GameMainModule.Dispose();
        }
    }
}
