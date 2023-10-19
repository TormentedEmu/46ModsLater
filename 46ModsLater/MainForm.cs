using NLog;
using NLog.Targets;
using System.Diagnostics;

namespace FortySixModsLater
{
    public partial class MainForm : Form
    {
        private static Logger _log = null;

        private Settings _Settings = new Settings();
        private AssemblyPatcher _AssemblyPatcher = new AssemblyPatcher();
        private ModsManager _ModsManager;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (_log == null)
                _log = LogManager.GetCurrentClassLogger();

            _log.Info("Welcome to 46 Mods Later!");
            _log.Info("Please check the settings page to choose the mods and game folders.");

            InitFormSettings();
            BuildModsList();
            LogManager.ReconfigExistingLoggers();
        }

        private void InitFormSettings()
        {
            _Settings.Init(Application.StartupPath);
            _AssemblyPatcher.GameManagedPath = _Settings.GameManagedPath;
            rtbModsFolder.Text = _Settings.ModsPath;
            rtbGameFolder.Text = _Settings.GamePath;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            btnBuild.Enabled = false;
            btnBuild.BackColor = Color.Red;
            btnBuild.Text = "Building...";

            var listViewItems = listViewMods.CheckedItems;

            if (listViewItems != null || listViewItems.Count > 0)
            {
                _log.Info($"Building {listViewItems.Count} selected Mods");
                // gather mods to build
                List<string> modsToBuild = new List<string>();
                foreach (ListViewItem modItem in listViewItems)
                {
                    if (modItem.Checked) { modsToBuild.Add(modItem.Text); }
                }

                var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                Task.Factory.StartNew(() => PatchCompile(modsToBuild)).ContinueWith(code =>
                {
                    btnBuild.Enabled = true;
                    btnBuild.BackColor = SystemColors.ControlDark;
                    btnBuild.Text = "Build Mods";
                }, uiScheduler);
            }
            else
            {
                _log.Info($"No mods selected to build: {listViewItems}");
            }
        }

        private void btnBrowseModsFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialogModsFolder = new FolderBrowserDialog();
            if (folderBrowserDialogModsFolder.ShowDialog() == DialogResult.OK)
            {
                rtbModsFolder.Text = folderBrowserDialogModsFolder.SelectedPath;
                _Settings.ModsPath = folderBrowserDialogModsFolder.SelectedPath;
                _log.Info($"Setting the Mods folder to: {folderBrowserDialogModsFolder.SelectedPath}");
            }
        }

        private void btnRefreshMods_Click(object sender, EventArgs e)
        {
            btnRefreshMods.Enabled = false;
            _log.Info("Refreshing the Mods list.");

            RefreshModsList();
            btnRefreshMods.Enabled = true;
        }

        private void btnBrowseGameFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialogGameFolder = new FolderBrowserDialog();
            if (folderBrowserDialogGameFolder.ShowDialog() == DialogResult.OK)
            {
                rtbGameFolder.Text = folderBrowserDialogGameFolder.SelectedPath;
                _Settings.GamePath = folderBrowserDialogGameFolder.SelectedPath;
                _log.Info($"Setting the Game folder to: {folderBrowserDialogGameFolder.SelectedPath}");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _log.Info("Shutting down and saving settings.");
            _Settings.Save();
        }

        private void btnOpenModsFolder_Click(object sender, EventArgs e)
        {
            string modsFolder = _Settings.ModsPath;

            if (Directory.Exists(modsFolder))
            {
                Process.Start("explorer.exe", modsFolder);
            }
        }

        private void btnOpenGameFolder_Click(object sender, EventArgs e)
        {
            string gameFolder = _Settings.GamePath;

            if (Directory.Exists(gameFolder))
            {
                Process.Start("explorer.exe", gameFolder);
            }
        }

        private void RefreshModsList()
        {
            _log.Info("Refreshing the Mods List");
            listViewMods.Items.Clear();

            if (!Directory.Exists(_Settings.ModsPath))
            {
                _log.Info($"Invalid directory: {_Settings.ModsPath}");
                return;
            }

            BuildModsList();
        }

        private void BuildModsList()
        {
            _ModsManager = new ModsManager(_Settings.ModsPath, _Settings.GameManagedPath);

            foreach (var mod in _ModsManager.Mods)
            {
                string key = mod.Key;
                ModInfo info = mod.Value;
                bool enabled = true;
                if (!_Settings.Mods.TryGetValue(key, out enabled))
                    _Settings.Mods.Add(key, enabled);

                ListViewItem item = new ListViewItem(key);
                item.Name = key;
                item.Checked = _Settings[key];
                item.SubItems.Add(info.Author);
                item.SubItems.Add(info.Version);
                item.SubItems.Add(info.Description);
                item.SubItems.Add(info.Website);
                item.SubItems.Add(info.DisplayName);
                item.ToolTipText = info.Name + " - " + info.Website;

                listViewMods.Items.Add(item);
            }
        }

        private void PatchCompile(List<string> modsToBuild)
        {
            // initial patch assembly
            Stopwatch sw = Stopwatch.StartNew();

            // patch our game with an earlier mod loader
            _AssemblyPatcher.Patch(_Settings.GamePath);

            _log.Info($"Including {modsToBuild.Count} in build.");

            // compile patch scripts, run patchers
            _ModsManager.GameManagedPath = _AssemblyPatcher.GameManagedPath;
            _ModsManager.RunPatchScripts(modsToBuild);

            // compile mod scripts
            _ModsManager.CompileHarmonyMods(modsToBuild);

            // copy mod folders to game directory
            _ModsManager.CopyModsToGame(modsToBuild, _Settings.GamePath);

            sw.Stop();
            _log.Info($"Build process completed in {sw.Elapsed.Milliseconds}ms");
        }

        private void listViewMods_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            _Settings[e.Item.Name] = e.Item.Checked;
        }

        private void toolStripMenuItemCopy_Click(object sender, EventArgs e)
        {
            rtbLog.Copy();
        }
    }
}