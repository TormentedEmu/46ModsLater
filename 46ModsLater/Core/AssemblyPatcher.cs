using NLog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace FortySixModsLater
{
    internal class AssemblyPatcher
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public string GameTitle { get; set; } = "AwesomeGame";
        public string GameManagedPath { get; set; } = string.Empty;

        public AssemblyPatcher()
        {
        }

        public void Patch(string gamePath)
        {
            if (!Directory.Exists(gamePath))
            {
                _log.Error($"Directory does not exist: {gamePath}");
                return;
            }

            GameTitle = Path.GetFileName(gamePath);
            _log.Info($"Attempting to patch game: {GameTitle}");

            if (string.IsNullOrEmpty(GameManagedPath) || !FindManagedFolder(gamePath))
            {
                _log.Error($"Failed to find Data/Managed folder within: {gamePath}");
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
                string backupFolder = Path.Combine(Application.StartupPath, "BackupFiles");
                Directory.CreateDirectory(backupFolder);
                backupFolder = Path.Combine(backupFolder, "Assembly-CSharp.dll");
                if (!File.Exists(backupFolder))
                    File.Copy(assemblyCSharp, backupFolder);

                string renamedFile = Path.Combine(GameManagedPath, "Assembly-CSharp.dll.ORIGINAL");
                if (!File.Exists(renamedFile))
                    File.Copy(assemblyCSharp, renamedFile);

                ModuleDefinition gameModule = ReadModuleDefinition(backupFolder);
                var modMgr = gameModule.Types.First(t => t.Name == "ModManager");
                var loadMods = modMgr.Methods.First(m => m.Name == "LoadMods");
                var platMgr = gameModule.Types.First(t => t.Name == "PlatformManager");
                var pmInit = platMgr.Methods.First(m => m.Name == "Init");
                var methodIL = pmInit.Body.GetILProcessor();
                var lastInst = methodIL.Body.Instructions.Last();

                for (int i = methodIL.Body.Instructions.Count - 1; i >= 0; i--)
                {
                    var instr = methodIL.Body.Instructions[i];
                    if (instr.OpCode == OpCodes.Ret && instr.Previous.OpCode == OpCodes.Ldc_I4_1)
                    {
                        lastInst = instr;
                        break;
                    }
                }

                methodIL.InsertBefore(lastInst, methodIL.Create(OpCodes.Callvirt, loadMods));
                string tempFolder = Path.Combine(Application.StartupPath, "Temp");
                Directory.CreateDirectory(tempFolder);
                tempFolder = Path.Combine(tempFolder, "Assembly-CSharp.dll");
                gameModule.Write(tempFolder, new WriterParameters());
                success = true;
            }
            catch (Exception ex)
            {
                _log.Error($"Caught exception: {ex.Message}");
                success = false;
            }
            finally
            {
                _log.Info($"Patched game assembly successfully: {success}");
            }
        }

        private bool FindManagedFolder(string gamePath)
        {
            string dataFolder = Directory.GetDirectories(gamePath).FirstOrDefault(dir => dir.EndsWith("_Data"), null);
            if (dataFolder == null)
            {
                _log.Error("Did not find the _game_Data folder");
                return false;
            }

            if (!Directory.Exists(dataFolder))
            {
                _log.Error($"_Data directory does not exist: {gamePath}");
                return false;
            }

            string managedFolder = Path.Combine(dataFolder, "Managed");
            if (!Directory.Exists(managedFolder))
            {
                _log.Error($"Managed directory does not exist: {managedFolder}");
                return false;
            }

            GameManagedPath = managedFolder;
            _log.Info($"GameManagedPath: {GameManagedPath}");
            return true;
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
            string asmFile = Path.Combine(gameLocation, filename);

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
