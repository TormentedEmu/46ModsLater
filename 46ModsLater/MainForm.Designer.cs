namespace FortySixModsLater
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            splitContainerMain = new SplitContainer();
            tabControlMain = new TabControl();
            tabPageHome = new TabPage();
            groupModList = new GroupBox();
            listViewMods = new ListView();
            columnHeaderName = new ColumnHeader();
            columnHeaderAuthor = new ColumnHeader();
            columnHeaderVersion = new ColumnHeader();
            columnHeaderDesc = new ColumnHeader();
            columnHeaderWebsite = new ColumnHeader();
            columnHeaderDisplayName = new ColumnHeader();
            groupControls = new GroupBox();
            btnOpenGameFolder = new Button();
            btnOpenModsFolder = new Button();
            btnRefreshMods = new Button();
            btnBuild = new Button();
            tabPageSettings = new TabPage();
            groupSettings = new GroupBox();
            btnBrowseGameFolder = new Button();
            rtbGameFolder = new RichTextBox();
            labelGameFolder = new Label();
            btnBrowseModsFolder = new Button();
            rtbModsFolder = new RichTextBox();
            labelModsFolder = new Label();
            menuStripMain = new MenuStrip();
            toolStripMenuItemMain = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            aboutDescToolStripMenuItem = new ToolStripMenuItem();
            rtbLog = new RichTextBox();
            contextMenuStriprtbLog = new ContextMenuStrip(components);
            toolStripMenuItemCopy = new ToolStripMenuItem();
            folderBrowserDialogModsFolder = new FolderBrowserDialog();
            folderBrowserDialogGameFolder = new FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabPageHome.SuspendLayout();
            groupModList.SuspendLayout();
            groupControls.SuspendLayout();
            tabPageSettings.SuspendLayout();
            groupSettings.SuspendLayout();
            menuStripMain.SuspendLayout();
            contextMenuStriprtbLog.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.BackColor = SystemColors.ControlDark;
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Orientation = Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(tabControlMain);
            splitContainerMain.Panel1.Controls.Add(menuStripMain);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(rtbLog);
            splitContainerMain.Size = new Size(875, 630);
            splitContainerMain.SplitterDistance = 444;
            splitContainerMain.SplitterWidth = 10;
            splitContainerMain.TabIndex = 0;
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPageHome);
            tabControlMain.Controls.Add(tabPageSettings);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Font = new Font("Copperplate Gothic Bold", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            tabControlMain.Location = new Point(0, 24);
            tabControlMain.Margin = new Padding(10);
            tabControlMain.Multiline = true;
            tabControlMain.Name = "tabControlMain";
            tabControlMain.Padding = new Point(6, 6);
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(875, 420);
            tabControlMain.TabIndex = 0;
            // 
            // tabPageHome
            // 
            tabPageHome.AutoScroll = true;
            tabPageHome.BackColor = SystemColors.ControlDarkDark;
            tabPageHome.Controls.Add(groupModList);
            tabPageHome.Controls.Add(groupControls);
            tabPageHome.Location = new Point(4, 30);
            tabPageHome.Name = "tabPageHome";
            tabPageHome.Padding = new Padding(3);
            tabPageHome.Size = new Size(867, 386);
            tabPageHome.TabIndex = 0;
            tabPageHome.Text = "Home";
            // 
            // groupModList
            // 
            groupModList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupModList.Controls.Add(listViewMods);
            groupModList.Location = new Point(3, 102);
            groupModList.Name = "groupModList";
            groupModList.Size = new Size(861, 278);
            groupModList.TabIndex = 3;
            groupModList.TabStop = false;
            groupModList.Text = "Mods";
            // 
            // listViewMods
            // 
            listViewMods.Activation = ItemActivation.OneClick;
            listViewMods.BackColor = SystemColors.ActiveBorder;
            listViewMods.CheckBoxes = true;
            listViewMods.Columns.AddRange(new ColumnHeader[] { columnHeaderName, columnHeaderAuthor, columnHeaderVersion, columnHeaderDesc, columnHeaderWebsite, columnHeaderDisplayName });
            listViewMods.Dock = DockStyle.Fill;
            listViewMods.Font = new Font("Copperplate Gothic Light", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            listViewMods.GridLines = true;
            listViewMods.HoverSelection = true;
            listViewMods.Location = new Point(3, 18);
            listViewMods.Name = "listViewMods";
            listViewMods.ShowItemToolTips = true;
            listViewMods.Size = new Size(855, 257);
            listViewMods.Sorting = SortOrder.Ascending;
            listViewMods.TabIndex = 0;
            listViewMods.UseCompatibleStateImageBehavior = false;
            listViewMods.View = View.Details;
            listViewMods.ItemChecked += listViewMods_ItemChecked;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Tag = "Name";
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 200;
            // 
            // columnHeaderAuthor
            // 
            columnHeaderAuthor.Tag = "Author";
            columnHeaderAuthor.Text = "Author";
            columnHeaderAuthor.Width = 140;
            // 
            // columnHeaderVersion
            // 
            columnHeaderVersion.Tag = "Version";
            columnHeaderVersion.Text = "Version";
            columnHeaderVersion.Width = 80;
            // 
            // columnHeaderDesc
            // 
            columnHeaderDesc.Tag = "Desc";
            columnHeaderDesc.Text = "Description";
            columnHeaderDesc.Width = 360;
            // 
            // columnHeaderWebsite
            // 
            columnHeaderWebsite.Tag = "Website";
            columnHeaderWebsite.Text = "Website";
            columnHeaderWebsite.Width = 200;
            // 
            // columnHeaderDisplayName
            // 
            columnHeaderDisplayName.Tag = "Display";
            columnHeaderDisplayName.Text = "Display Name";
            columnHeaderDisplayName.Width = 200;
            // 
            // groupControls
            // 
            groupControls.Controls.Add(btnOpenGameFolder);
            groupControls.Controls.Add(btnOpenModsFolder);
            groupControls.Controls.Add(btnRefreshMods);
            groupControls.Controls.Add(btnBuild);
            groupControls.Dock = DockStyle.Top;
            groupControls.Location = new Point(3, 3);
            groupControls.Name = "groupControls";
            groupControls.Size = new Size(861, 86);
            groupControls.TabIndex = 2;
            groupControls.TabStop = false;
            // 
            // btnOpenGameFolder
            // 
            btnOpenGameFolder.Location = new Point(269, 24);
            btnOpenGameFolder.Name = "btnOpenGameFolder";
            btnOpenGameFolder.Size = new Size(136, 45);
            btnOpenGameFolder.TabIndex = 4;
            btnOpenGameFolder.Text = "Open Game Folder";
            btnOpenGameFolder.UseVisualStyleBackColor = true;
            btnOpenGameFolder.Click += btnOpenGameFolder_Click;
            // 
            // btnOpenModsFolder
            // 
            btnOpenModsFolder.Location = new Point(140, 24);
            btnOpenModsFolder.Name = "btnOpenModsFolder";
            btnOpenModsFolder.Size = new Size(123, 45);
            btnOpenModsFolder.TabIndex = 3;
            btnOpenModsFolder.Text = "Open Mods Folder";
            btnOpenModsFolder.UseVisualStyleBackColor = true;
            btnOpenModsFolder.Click += btnOpenModsFolder_Click;
            // 
            // btnRefreshMods
            // 
            btnRefreshMods.Location = new Point(6, 24);
            btnRefreshMods.Name = "btnRefreshMods";
            btnRefreshMods.Size = new Size(128, 45);
            btnRefreshMods.TabIndex = 2;
            btnRefreshMods.Text = "Refresh Mods";
            btnRefreshMods.UseVisualStyleBackColor = true;
            btnRefreshMods.Click += btnRefreshMods_Click;
            // 
            // btnBuild
            // 
            btnBuild.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBuild.BackColor = SystemColors.ControlDark;
            btnBuild.Font = new Font("Copperplate Gothic Bold", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            btnBuild.Location = new Point(725, 24);
            btnBuild.Name = "btnBuild";
            btnBuild.Padding = new Padding(1);
            btnBuild.Size = new Size(130, 45);
            btnBuild.TabIndex = 1;
            btnBuild.Text = "Build Mods";
            btnBuild.UseVisualStyleBackColor = false;
            btnBuild.Click += btnBuild_Click;
            // 
            // tabPageSettings
            // 
            tabPageSettings.BackColor = SystemColors.ControlDarkDark;
            tabPageSettings.Controls.Add(groupSettings);
            tabPageSettings.Location = new Point(4, 30);
            tabPageSettings.Name = "tabPageSettings";
            tabPageSettings.Padding = new Padding(3);
            tabPageSettings.Size = new Size(867, 386);
            tabPageSettings.TabIndex = 1;
            tabPageSettings.Text = "Settings";
            // 
            // groupSettings
            // 
            groupSettings.Controls.Add(btnBrowseGameFolder);
            groupSettings.Controls.Add(rtbGameFolder);
            groupSettings.Controls.Add(labelGameFolder);
            groupSettings.Controls.Add(btnBrowseModsFolder);
            groupSettings.Controls.Add(rtbModsFolder);
            groupSettings.Controls.Add(labelModsFolder);
            groupSettings.Dock = DockStyle.Fill;
            groupSettings.Location = new Point(3, 3);
            groupSettings.Name = "groupSettings";
            groupSettings.Size = new Size(861, 380);
            groupSettings.TabIndex = 0;
            groupSettings.TabStop = false;
            // 
            // btnBrowseGameFolder
            // 
            btnBrowseGameFolder.Location = new Point(742, 136);
            btnBrowseGameFolder.Name = "btnBrowseGameFolder";
            btnBrowseGameFolder.Size = new Size(113, 54);
            btnBrowseGameFolder.TabIndex = 5;
            btnBrowseGameFolder.Text = "Select";
            btnBrowseGameFolder.UseVisualStyleBackColor = true;
            btnBrowseGameFolder.Click += btnBrowseGameFolder_Click;
            // 
            // rtbGameFolder
            // 
            rtbGameFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtbGameFolder.Location = new Point(6, 136);
            rtbGameFolder.Name = "rtbGameFolder";
            rtbGameFolder.Size = new Size(730, 54);
            rtbGameFolder.TabIndex = 4;
            rtbGameFolder.Text = "C:\\MyGameFolder";
            // 
            // labelGameFolder
            // 
            labelGameFolder.AutoSize = true;
            labelGameFolder.Location = new Point(6, 115);
            labelGameFolder.Name = "labelGameFolder";
            labelGameFolder.Size = new Size(100, 15);
            labelGameFolder.TabIndex = 3;
            labelGameFolder.Text = "Game Folder";
            // 
            // btnBrowseModsFolder
            // 
            btnBrowseModsFolder.Location = new Point(742, 42);
            btnBrowseModsFolder.Name = "btnBrowseModsFolder";
            btnBrowseModsFolder.Size = new Size(113, 54);
            btnBrowseModsFolder.TabIndex = 2;
            btnBrowseModsFolder.Text = "Select";
            btnBrowseModsFolder.UseVisualStyleBackColor = true;
            btnBrowseModsFolder.Click += btnBrowseModsFolder_Click;
            // 
            // rtbModsFolder
            // 
            rtbModsFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtbModsFolder.Location = new Point(6, 42);
            rtbModsFolder.Name = "rtbModsFolder";
            rtbModsFolder.Size = new Size(730, 54);
            rtbModsFolder.TabIndex = 1;
            rtbModsFolder.Text = "C:\\MyMods";
            // 
            // labelModsFolder
            // 
            labelModsFolder.AutoSize = true;
            labelModsFolder.Location = new Point(6, 21);
            labelModsFolder.Name = "labelModsFolder";
            labelModsFolder.Size = new Size(100, 15);
            labelModsFolder.TabIndex = 0;
            labelModsFolder.Text = "Mods Folder";
            // 
            // menuStripMain
            // 
            menuStripMain.BackColor = SystemColors.ControlDarkDark;
            menuStripMain.Items.AddRange(new ToolStripItem[] { toolStripMenuItemMain, aboutToolStripMenuItem });
            menuStripMain.Location = new Point(0, 0);
            menuStripMain.Name = "menuStripMain";
            menuStripMain.Size = new Size(875, 24);
            menuStripMain.TabIndex = 1;
            menuStripMain.Text = "menuStrip1";
            // 
            // toolStripMenuItemMain
            // 
            toolStripMenuItemMain.DropDownItems.AddRange(new ToolStripItem[] { exitToolStripMenuItem });
            toolStripMenuItemMain.Name = "toolStripMenuItemMain";
            toolStripMenuItemMain.Size = new Size(37, 20);
            toolStripMenuItemMain.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(93, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutDescToolStripMenuItem });
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(52, 20);
            aboutToolStripMenuItem.Text = "About";
            // 
            // aboutDescToolStripMenuItem
            // 
            aboutDescToolStripMenuItem.Name = "aboutDescToolStripMenuItem";
            aboutDescToolStripMenuItem.Size = new Size(496, 22);
            aboutDescToolStripMenuItem.Text = "Based on SDX and DMT by Domonix and Hal9000.   Rebooted by TormentedEmu";
            // 
            // rtbLog
            // 
            rtbLog.BackColor = SystemColors.ControlDark;
            rtbLog.ContextMenuStrip = contextMenuStriprtbLog;
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Font = new Font("Copperplate Gothic Light", 9F, FontStyle.Regular, GraphicsUnit.Point);
            rtbLog.HideSelection = false;
            rtbLog.Location = new Point(0, 0);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.ShowSelectionMargin = true;
            rtbLog.Size = new Size(875, 176);
            rtbLog.TabIndex = 0;
            rtbLog.Text = "";
            // 
            // contextMenuStriprtbLog
            // 
            contextMenuStriprtbLog.Font = new Font("Copperplate Gothic Light", 9F, FontStyle.Regular, GraphicsUnit.Point);
            contextMenuStriprtbLog.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCopy });
            contextMenuStriprtbLog.Name = "contextMenuStriprtbLog";
            contextMenuStriprtbLog.Size = new Size(104, 26);
            // 
            // toolStripMenuItemCopy
            // 
            toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
            toolStripMenuItemCopy.Size = new Size(103, 22);
            toolStripMenuItemCopy.Text = "Copy";
            toolStripMenuItemCopy.Click += toolStripMenuItemCopy_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(875, 630);
            Controls.Add(splitContainerMain);
            ForeColor = SystemColors.ControlText;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripMain;
            Name = "MainForm";
            Text = "46 Mods Later...";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel1.PerformLayout();
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            tabControlMain.ResumeLayout(false);
            tabPageHome.ResumeLayout(false);
            groupModList.ResumeLayout(false);
            groupControls.ResumeLayout(false);
            tabPageSettings.ResumeLayout(false);
            groupSettings.ResumeLayout(false);
            groupSettings.PerformLayout();
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            contextMenuStriprtbLog.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainerMain;
        private TabControl tabControlMain;
        private TabPage tabPageHome;
        private TabPage tabPageSettings;
        private RichTextBox rtbLog;
        private MenuStrip menuStripMain;
        private ToolStripMenuItem toolStripMenuItemMain;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ListView listViewMods;
        private ColumnHeader columnHeaderName;
        private ColumnHeader columnHeaderDisplayName;
        private ColumnHeader columnHeaderAuthor;
        private ColumnHeader columnHeaderDesc;
        private ColumnHeader columnHeaderVersion;
        private ColumnHeader columnHeaderWebsite;
        private ToolStripMenuItem aboutDescToolStripMenuItem;
        private Button btnBuild;
        private GroupBox groupModList;
        private GroupBox groupControls;
        private GroupBox groupSettings;
        private RichTextBox rtbModsFolder;
        private Label labelModsFolder;
        private Button btnBrowseModsFolder;
        private FolderBrowserDialog folderBrowserDialogModsFolder;
        private FolderBrowserDialog folderBrowserDialogGameFolder;
        private Button btnRefreshMods;
        private Button btnOpenGameFolder;
        private Button btnOpenModsFolder;
        private Label labelGameFolder;
        private Button btnBrowseGameFolder;
        private RichTextBox rtbGameFolder;
        private ContextMenuStrip contextMenuStriprtbLog;
        private ToolStripMenuItem toolStripMenuItemCopy;
    }
}