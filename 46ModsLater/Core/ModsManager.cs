using NLog;
using System.IO;

namespace FortySixModsLater
{
    internal class ModsManager
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        private RoslynCompiler Rosy;

        public string ModsPath { get; set; } = string.Empty;
        public string GameManagedPath {  get; set; } = string.Empty;

        public Dictionary<string, ModInfo> Mods { get; set; } = new Dictionary<string, ModInfo>();

        public ModsManager(string modsFolderPath, string gameManagedPath)
        {
            ModsPath = modsFolderPath;
            GameManagedPath = gameManagedPath;
            Rosy = new RoslynCompiler();
            Init();
        }

        public bool Init()
        {
            if (string.IsNullOrEmpty(ModsPath) || !Directory.Exists(ModsPath))
            {
                _log.Error($"ModsPath is null or directory doesn't exist: '{ModsPath}'");
                return false;
            }

            var folders = Directory.GetDirectories(ModsPath);
            foreach (var folder in folders)
            {
                var modInfoXml = Path.Combine(folder, "ModInfo.xml");

                if (!File.Exists(modInfoXml))
                    continue;

                ModInfo modInfo = new ModInfo(modInfoXml);
                if (!Mods.ContainsKey(modInfo.Name))
                {
                    if (!Mods.TryAdd(modInfo.Name, modInfo))
                    {
                        _log.Error($"Failed to add mod: {modInfo.Name}");
                    }
                }
            }

            _log.Info($"Found {Mods.Count} Mods");

            return true;
        }

        public void RunPatchScripts(List<string> modsToInclude)
        {
            List<string> psMods = new List<string>();

            foreach (var mod in modsToInclude)
            {
                ModInfo modInfo = null;
                if (Mods.TryGetValue(mod, out modInfo))
                {
                    // get directory PatchScripts
                    var path = modInfo.ModPath;
                    var patchScriptsPath = Path.Combine(path, "PatchScripts");
                    if (Directory.Exists(path) && Directory.Exists(patchScriptsPath))
                    {
                        psMods.Add(patchScriptsPath);
                    }
                }
            }

            if (psMods.Count > 0)
            {
                Rosy.Patch("PatchScripts", GameManagedPath, psMods, psMods);
            }
        }

        public void CompileHarmonyMods(List<string> modsToInclude)
        {
            foreach (var mod in modsToInclude)
            {
                ModInfo modInfo = null;
                if (Mods.TryGetValue(mod, out modInfo))
                {
                    // get directories Harmony and Scripts
                    List<string> scriptDirs = new List<string>();
                    var path = modInfo.ModPath;
                    if (!Directory.Exists(path))
                    {
                        _log.Error($"Failed to find mod folder: {path}");
                        continue;
                    }

                    var harmonyPath = Path.Combine(path, "Harmony");
                    if (Directory.Exists(harmonyPath))
                    {
                        scriptDirs.Add(harmonyPath);
                    }

                    var scriptsPath = Path.Combine(path, "Scripts");
                    if (Directory.Exists(scriptsPath))
                    {
                        scriptDirs.Add(scriptsPath);
                    }

                    if (scriptDirs.Count > 0)
                    {
                        Rosy.Create(modInfo.Name, GameManagedPath, path, scriptDirs, scriptDirs);
                    }
                }
            }
        }

        public void CopyModsToGame(List<string> modsToBuild, string gamePath)
        {
            foreach(var mod in modsToBuild)
            {
                try
                {
                    string sourceDir = Mods[mod].ModPath;
                    string destDir = Path.Combine(gamePath, "Mods");
                    Directory.CreateDirectory(destDir);
                    destDir = Path.Combine(destDir, Mods[mod].Name);
                    Directory.CreateDirectory(destDir);
                    CopyDirectory(sourceDir, destDir, true);
                }
                catch (Exception ex)
                {
                    _log.Error($"Caught error while copying mods: {ex}");
                }
            }

            _log.Info("Copy mod folders completed.");
        }
         
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            if (!Directory.Exists(sourceDir))
                return;

            try
            {
                var dir = new DirectoryInfo(sourceDir);
                DirectoryInfo[] dirs = dir.GetDirectories();
                Directory.CreateDirectory(destinationDir);

                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath, true);
                }

                if (recursive)
                {
                    foreach (DirectoryInfo subDir in dirs)
                    {
                        string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                        CopyDirectory(subDir.FullName, newDestinationDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"CopyDirectory - Caught exception: {ex}");
            }
        }

        public static bool GetAllCSFiles(string path, out List<string> csFiles)
        {
            csFiles = new List<string>();

            if (!Directory.Exists(path))
                return false;

            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            bool success = true;

            try
            {
                while (queue.Count > 0)
                {
                    path = queue.Dequeue();

                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }

                    string[] files = Directory.GetFiles(path, "*.cs");

                    if (files != null && files.Length > 0)
                        csFiles.AddRange(files);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"GetAllCSFiles - Caught exception: {ex}");
                success = false;
            }

            return success;
        }
    }
}
