using NLog;
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
            var listViewItems = listViewMods.CheckedItems;

            if (listViewItems != null || listViewItems.Count > 0)
            {
                // gather mods to build
                List<string> modsToBuild = new List<string>();
                foreach (ListViewItem modItem in listViewItems)
                {
                    if (modItem.Checked) { modsToBuild.Add(modItem.Text); }
                }

                if (modsToBuild.Count == 0)
                {
                    _log.Info("No mods currently selected to build.");
                    return;
                }

                if (!Utils.CheckPath(_Settings.GamePath))
                {
                    _log.Error($"Game folder is invalid: {_Settings.GamePath}");
                    return;
                }

                if (!Utils.CheckPath(_Settings.GameManagedPath))
                {
                    _log.Error($"Game Managed folder is invalid: {_Settings.GameManagedPath}");
                    return;
                }

                btnBuild.Enabled = false;
                btnBuild.BackColor = Color.Red;
                btnBuild.Text = "Building...";
                _log.Info($"Building {listViewItems.Count} selected Mods");
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
                _log.Info($"The Mods list is currently empty: {listViewItems}");
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
                _Settings.GameManagedPath = Utils.FindManagedFolder(_Settings.GamePath);

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

            if (!Utils.CheckPath(modsFolder))
            {
                _log.Error($"Mods folder is invalid: '{modsFolder}'.");
                return;
            }

            Process.Start("explorer.exe", modsFolder);
        }

        private void btnOpenGameFolder_Click(object sender, EventArgs e)
        {
            string gameFolder = _Settings.GamePath;

            if (!Utils.CheckPath(gameFolder))
            {
                _log.Error($"Game folder is invalid: '{gameFolder}'.");
                return;
            }

            Process.Start("explorer.exe", gameFolder);
        }

        private void RefreshModsList()
        {
            _log.Info("Refreshing the Mods List");
            listViewMods.Items.Clear();

            if (!Utils.CheckPath(_Settings.ModsPath))
            {
                _log.Info($"Mods folder is invalid: '{_Settings.ModsPath}'");
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

            // patch our game with an earlier mod loader - ModManagerEX
            _AssemblyPatcher.Patch(_Settings.GamePath);

            _log.Info($"Including {modsToBuild.Count} in build.");

            // compile patch scripts, run patchers
            _ModsManager.GameManagedPath = _AssemblyPatcher.GameManagedPath;
            _ModsManager.RunPatchScripts(modsToBuild, _AssemblyPatcher.GameMainModule);

            // compile mod scripts
            _ModsManager.CompileHarmonyMods(modsToBuild, _AssemblyPatcher.GameMainModule);

            // copy mod folders to game directory
            _ModsManager.CopyModsToGame(modsToBuild, _Settings.GamePath);

            _AssemblyPatcher.WriteFinalOutput();

            sw.Stop();
            _log.Info($"Build process completed in {sw.ElapsedMilliseconds}ms \n********************************************************************************\n");
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