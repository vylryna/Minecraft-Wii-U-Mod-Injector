﻿using System;
using System.IO;
using System.Windows.Forms;
using MetroFramework.Forms;
using Minecraft_Wii_U_Mod_Injector.Helpers;
using MetroFramework.Controls;
using System.Text;
using Minecraft_Wii_U_Mod_Injector.Helpers.Files;
using Minecraft_Wii_U_Mod_Injector.Properties;
using Minecraft_Wii_U_Mod_Injector.Helpers.Win_Forms;
using Application = System.Windows.Forms.Application;

namespace Minecraft_Wii_U_Mod_Injector.Forms
{
    public partial class CemuPckMngr : MetroForm
    {
        #region references

        private readonly MainForm _iw;
        private readonly string _cemuPckRootDir = Application.StartupPath + @"\Saved\Cemu\";

        private readonly StringBuilder _cemuGraphicPckRulesBuilder = new();
        private readonly StringBuilder _cemuGraphicPckPatchesBuilder = new();

        #endregion

        public CemuPckMngr(MainForm iw)
        {
            InitializeComponent();
            _iw = iw;
            StyleMngr.Style = Style = iw.StyleMngr.Style;
            StyleMngr.Theme = Theme = iw.StyleMngr.Theme;
        }

        private void Init(object sender, EventArgs e)
        {
            if (!Directory.Exists(_cemuPckRootDir))
                Directory.CreateDirectory(_cemuPckRootDir);

            if (!Settings.Default.SeenCemuMngr)
            {
                Messaging.Show(
                    "Welcome to the Cemu Graphics Pack Manager!\nHere you are able to make Cemu graphics packs so you can use mods on Cemu. For now" +
                    " slider mods aren't supported but may in a future update.\nAll created graphics pack NEED to go into the /graphicsPack/ folder in your Cemu" +
                    " installation folder.");

                Settings.Default.SeenCemuMngr = true;
                Settings.Default.Save();
                Settings.Default.Upgrade();
            }


            _cemuGraphicPckPatchesBuilder.AppendLine("[Minecraft]\nmoduleMatches = " + moduleMatchesBox.Text);

            DiscordRp.SetPresence(_iw.IsConnected ? "Connected" : "Disconnected",
                "Cemu Graphics Pack Manager");
        }

        private void Exiting(object sender, FormClosingEventArgs e)
        {
            _cemuGraphicPckPatchesBuilder.Clear();
            _cemuGraphicPckRulesBuilder.Clear();
            DiscordRp.SetPresence(_iw.IsConnected ? "Connected" : "Disconnected", _iw.MainTabs.SelectedTab.Text + " tab");
            Dispose();
        }

        private void SwapTab(object sender, EventArgs e)
        {
            var tile = (MetroTile) sender;

            if (MainTabs.SelectedIndex != tile.TileCount)
                MainTabs.SelectedIndex = tile.TileCount;
        }

        private void AddToList(object sender, EventArgs e)
        {
            var ctrl = (Control) sender;

            if (ctrl is MetroCheckBox cb)
            {
                _cemuGraphicPckPatchesBuilder.AppendLine("#" + cb.Text);
                if (cb.Checked)
                {
                    if (cb.Tag.ToString().Contains("|"))
                    {
                        _cemuGraphicPckPatchesBuilder.AppendLine(cb.Tag.ToString().Replace('|', '\n'));
                        return;
                    }

                    _cemuGraphicPckPatchesBuilder.AppendLine(cb.Tag.ToString());
                }
            }
        }

        private void SaveBtnClicked(object sender, EventArgs e)
        {
            try
            {
                var gfxPckFolder = _cemuPckRootDir + "Minecraft Wii U Mod Injector - " + nameBox.Text;

                _cemuGraphicPckRulesBuilder.AppendLine("[Definition]\ntitleIds = " + titleIdsBox.Text + "\nname = " +
                                                       nameBox.Text + "\npath = " + pathBox.Text + nameBox.Text + "\n" +
                                                       "description = " + descriptionBox.Text + "\nversion = 4");

                if (!Directory.Exists(gfxPckFolder)) Directory.CreateDirectory(gfxPckFolder);

                using (var file = new StreamWriter(gfxPckFolder + @"\rules.txt"))
                {
                    file.WriteLine(_cemuGraphicPckRulesBuilder.ToString());
                }

                using (var file = new StreamWriter(gfxPckFolder + @"\patches.txt"))
                {
                    file.WriteLine(_cemuGraphicPckPatchesBuilder.ToString());
                }

                Messaging.Show("Successfully saved " + nameBox.Text + " to\n" + gfxPckFolder);

                if (Messaging.Show("Would you like to automatically export the graphics pack to Cemu?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var folderBrowser = new FolderBrowserDialog();

                    folderBrowser.SelectedPath = Settings.Default.CemuGfxLoc;

                    if (folderBrowser.ShowDialog() == DialogResult.OK)
                    {
                        Settings.Default.CemuGfxLoc = folderBrowser.SelectedPath;
                        Settings.Default.Save();

                        Miscellaneous.CopyFolder(gfxPckFolder,
                            folderBrowser.SelectedPath + @"\Minecraft Wii U Mod Injector - " + nameBox.Text);

                        Messaging.Show("Graphics Pack has been successfully installed!");
                    }
                }
            }
            catch (Exception exception)
            {
                Exceptions.LogError(exception, "An error occurred while saving Graphics Pack.", true);
            }
        }
    }
}