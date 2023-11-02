using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine.SceneManagement;

// 'derived' mod manager
public static class ModManagerEx
{
    public static bool IsMMInit = false;
    public static bool EarlyModsLoaded = false;

    // our 'override' method
    public static void LoadMods()
    {
        Scene scene = SceneManager.GetActiveScene();
        Log.Out($"[ModManagerEX] LoadMods start.  Active Scene is '{scene.name}'");

        InitModManager();

        if (scene.name.Equals("SceneSplash") && !EarlyModsLoaded)
        {
            EarlyModsLoaded = true;
            LoadEarlyMods();
        }
        else
        {
            if (GameManager.Instance == null && GameManager.IsDedicatedServer && !EarlyModsLoaded)
            {
                // We technically could load early mods on a dedicated server but its not
                // very useful as we're already in the SceneGame and awake has been called on most everything(it would be early by a fraction of a second)
                // Our LoadMods is called twice due to the Platform not being init on a dedicated server
                // where the SceneSplash isn't loaded
                EarlyModsLoaded = true;
                Log.Out("[ModManagerEX] Delaying mod loader for dedicated server.");
                return;
            }

            LoadNormalMods();
        }

        InitModAPI();

        Log.Out("[ModManagerEX] LoadMods done");
    }

    static void InitModManager()
    {
        if (!IsMMInit)
        {
            IsMMInit = true;
            ModManager.initModManager();
        }
    }

    static void LoadEarlyMods()
    {
        Log.Out($"[ModManagerEX] LoadEarlyMods start: 'SceneSplash' - SplashScreenScript::Awake");
        LoadEarlyModsFromFolder(ModManager.ModsBasePathLegacy);
        Log.Out("[ModManagerEX] LoadEarlyMods complete.");
    }

    static void LoadNormalMods()
    {
        Log.Out($"[ModManagerEX] LoadNormalMods start: 'SceneGame' - GameManager::Awake");
        bool loadModsBase = ModManager.loadModsFromFolder(ModManager.ModsBasePath);
        bool loadModsLegacy = GameIO.PathsEquals(ModManager.ModsBasePath, ModManager.ModsBasePathLegacy, _ignoreCase: true) || ModManager.loadModsFromFolder(ModManager.ModsBasePathLegacy);

        if (!loadModsBase && !loadModsLegacy)
        {
            Log.Out("[ModManagerEX] No mods folder found.");
        }

        Log.Out($"[ModManagerEX] LoadNormalMods done");
    }

    static void InitModAPI()
    {
        Log.Out("[ModManagerEX] Attempting to initialize mod api code");

        foreach (KeyValuePair<string, Mod> mod in ModManager.loadedMods.dict)
        {
            if (mod.Value.IsInit)
            {
                Log.Out($"[ModManagerEX] Ignoring {mod.Value.Name}, loaded mod early.");
                continue;
            }

            mod.Value.IsInit = true;
            mod.Value.InitModCode();
        }

        Log.Out("[ModManagerEX] Mods initialized.");
    }

    private static bool LoadEarlyModsFromFolder(string folder)
    {
        if (!SdDirectory.Exists(folder))
        {
            Log.Out($"Directory doesn't exist: {folder}");
            return false;
        }

        Log.Out($"[ModManagerEx] Start early loading from: {folder}");
        string[] dirs = SdDirectory.GetDirectories(folder);
        Array.Sort(dirs);

        foreach (string path in dirs)
        {
            try
            {
                Mod mod = LoadEarlyModFromFolder(path);
                if (mod != null)
                {
                    ModManager.loadedMods.Add(mod.Name, mod);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[ModManagerEx] Failed loading early mod from folder: {Path.GetFileName(path)}");
                Log.Exception(ex);
            }
        }

        return true;
    }

    public static Mod LoadEarlyModFromFolder(string path_)
    {
        string modInfoPath = Path.Combine(path_, "ModInfo.xml");
        string fileName = Path.GetFileName(path_);

        if (!SdFile.Exists(modInfoPath))
        {
            Log.Warning($"[ModManagerEx] Ignoring folder '{fileName}' - does not contain a ModInfo.xml.");
            return null;
        }

        XDocument xDoc = XDocument.Load(modInfoPath);
        XElement eleRoot = xDoc.Root;

        if (eleRoot == null)
        {
            Log.Error($"[ModManagerEx] Ignoring '{modInfoPath}' - the ModInfo.xml does not have root element '<xml>'.");
            return null;
        }

        string isLoadEarly = GetElementAttribute(eleRoot, "LoadEarly");
        if (string.IsNullOrEmpty(isLoadEarly))
        {
            return null;
        }

        if (isLoadEarly.ToLower().Equals("false"))
        {
            Log.Error($"[ModManagerEx] Ignoring '{modInfoPath}' - LoadEarly: {isLoadEarly}");
            return null;
        }

        Log.Out($"[ModManagerEx] Loading early mod: {fileName}  - LoadEarly: {isLoadEarly}");

        Mod earlyMod = ParseModInfo(path_, fileName, eleRoot);
        if (earlyMod == null)
        {
            Log.Error($"[ModManagerEx] Failed to parse {modInfoPath}");
            return null;
        }

        if (ModManager.ModLoaded(earlyMod.Name))
        {
            Log.Warning($"[ModManagerEx] Mod '{earlyMod.Name}' already loaded. Ignoring.");
            return null;
        }

        if (!earlyMod.LoadAssemblies())
        {
            return null;
        }

        earlyMod.DetectContents();

        Log.Out($"[ModManagerEx] Loaded Early Mod: {earlyMod.Name} ({earlyMod.VersionString})");

        return earlyMod;
    }

    private static Mod ParseModInfo(string modPath, string folderName, XElement eleRoot)
    {
        var modName = GetElementAttribute(eleRoot, "Name");
        if (string.IsNullOrEmpty(modName))
            return null;

        var modDisplayName = GetElementAttribute(eleRoot, "DisplayName");
        if (string.IsNullOrEmpty(modDisplayName))
            modDisplayName = modName;

        var modDescription = GetElementAttribute(eleRoot, "Description");
        if (string.IsNullOrEmpty(modDescription))
            modDescription = "empty";

        var modAuthor = GetElementAttribute(eleRoot, "Author");
        if (string.IsNullOrEmpty(modAuthor))
            modAuthor = "unknown author";

        var modVersionString = GetElementAttribute(eleRoot, "Version");
        if (string.IsNullOrEmpty(modVersionString))
            modVersionString = "0";

        Version modVersion = null;
        Version.TryParse(modVersionString, out modVersion);

        var modWebsite = GetElementAttribute(eleRoot, "Website");
        if (string.IsNullOrEmpty(modWebsite))
            modWebsite = "";

        return new Mod
        {
            Path = modPath,
            FolderName = folderName,
            Name = modName,
            DisplayName = modDisplayName,
            Description = modDescription,
            Author = modAuthor,
            Version = modVersion,
            VersionString = modVersionString,
            Website = modWebsite
        };
    }

    private static string GetElementAttribute(XElement ele, string val)
    {
        string attribVal = string.Empty;
        var key = ele.Element(val);
        if (key != null && key.FirstAttribute != null)
        {
            attribVal = key.FirstAttribute.Value;
        }
        return attribVal;
    }
}