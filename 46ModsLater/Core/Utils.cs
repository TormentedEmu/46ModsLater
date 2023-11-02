using Mono.Cecil;
using NLog;

namespace FortySixModsLater
{
    public static class Utils
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public static void SetAllMethodsToPublic(TypeDefinition typeDef)
        {
            foreach (var m in typeDef.Methods)
            {
                SetMethodToPublic(m);
            }
        }

        public static void SetMethodToPublic(MethodDefinition meth)
        {
            meth.IsPrivate = false;
            meth.IsPublic = true;
        }

        public static void SetAllFieldsToPublic(TypeDefinition typeDef)
        {
            foreach (var f in typeDef.Fields)
            {
                SetFieldToPublic(f);
            }
        }

        public static void SetFieldToPublic(FieldDefinition field)
        {
            field.IsPrivate = false;
            field.IsPublic = true;
        }

        public static void SetAllNestedTypesToPublic(TypeDefinition typeDef)
        {
            foreach (var nType in typeDef.NestedTypes)
            {
                nType.IsNestedPrivate = false;
                nType.IsPublic = true;
                SetAllFieldsToPublic(nType);
                SetAllMethodsToPublic(nType);
            }
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

        public static bool CheckPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            return true;
        }

        public static string FindManagedFolder(string basePath)
        {
            string ret = string.Empty;

            if (!CheckPath(basePath))
                return ret;

            string dataFolder = Directory.GetDirectories(basePath).FirstOrDefault(dir => dir.EndsWith("_Data"));
            if (dataFolder == null || dataFolder.Length == 0)
            {
                _log.Error("Did not find the _game_Data folder");
            }
            else
            {
                _log.Info($"Data directory found: {dataFolder}");
                string managedFolder = Path.Combine(dataFolder, "Managed");

                if (Directory.Exists(managedFolder))
                {
                    ret = managedFolder;
                    _log.Info($"Managed directory found: {managedFolder}");
                }
            }

            return ret;
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

        public static ModuleDefinition ReadModuleDefinition(byte[] codeBytes, string searchDir)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(searchDir);

            using (var memStream = new MemoryStream(codeBytes))
            {
                return ModuleDefinition.ReadModule(memStream, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver, ReadWrite = true });
            }
        }

        // Based on code from DMT - Hal9000 - Thank you <3
        public static ModuleDefinition ReadModuleDefinition(string filePath, string searchDir)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(searchDir);

            return ModuleDefinition.ReadModule(new MemoryStream(File.ReadAllBytes(filePath)), new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver, ReadWrite = true });
        }
    }
}

