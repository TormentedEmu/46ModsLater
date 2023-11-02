using System.Xml.Linq;

namespace FortySixModsLater
{
    internal class ModInfo
    {
        public static readonly ModInfo DefaultModInfo = new ModInfo()
        {
            Name = "AwesomeMod",
            DisplayName = "Awesome Mod",
            Description = "A totally awesome mod!",
            Version = "v1.0.0",
            Author = "TotallyRadAuthor",
            Website = "https://www.google.com",
            IsEnabled = true,
            IsEarlyLoad = false,
        };

        public string ModPath { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Website { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsEarlyLoad { get; set; } = false;

        public ModInfo()
        {
        }

        public ModInfo(string path)
        {
            ModPath = Path.GetDirectoryName(path);

            XDocument xDoc = XDocument.Load(path);
            XElement root = xDoc.Root;

            Name = GetElementAttribute(root, "Name");
            if (string.IsNullOrEmpty(Name)) Name = DefaultModInfo.Name;

            DisplayName = GetElementAttribute(root, "DisplayName");
            if (string.IsNullOrEmpty(DisplayName)) DisplayName = DefaultModInfo.DisplayName;

            Description = GetElementAttribute(root, "Description");
            if (string.IsNullOrEmpty(Description)) Description = DefaultModInfo.Description;

            Version = GetElementAttribute(root, "Version");
            if (string.IsNullOrEmpty(Version)) Version = DefaultModInfo.Version;

            Author = GetElementAttribute(root, "Author");
            if (string.IsNullOrEmpty(Author)) Author = DefaultModInfo.Author;

            Website = GetElementAttribute(root, "Website");
            if (string.IsNullOrEmpty(Website)) Website = DefaultModInfo.Website;

            IsEnabled = bool.TryParse(GetElementAttribute(root, "IsEnabled"), out bool enabled) == true ? enabled : true;
            IsEarlyLoad = bool.TryParse(GetElementAttribute(root, "IsEarlyLoad"), out bool isEarlyLoad) == true ? isEarlyLoad : false;
        }

        private string GetElementAttribute(XElement ele, string val)
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
}
