﻿using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Controls;
using MetroFramework.Forms;
using Minecraft_Wii_U_Mod_Injector.Helpers;
using Minecraft_Wii_U_Mod_Injector.Helpers.Files;
using Minecraft_Wii_U_Mod_Injector.Helpers.Win_Forms;
using Minecraft_Wii_U_Mod_Injector.Properties; 
//ReSharper disable UnusedMember.Local

namespace Minecraft_Wii_U_Mod_Injector.Forms.Managers
{
    public partial class CemuPckMngr : MetroForm
    {
        #region references

        private readonly MainForm _iw;
        private readonly string _cemuPckRootDir = Application.StartupPath + @"\Saved\Cemu\";

        private readonly StringBuilder _cemuGraphicPckRulesBuilder = new();
        private readonly StringBuilder _cemuGraphicPckPatchesBuilder = new();

        private enum SliderDefaults
        {
            EnchantmentLevelSlider = 0,
            PotionAmplifierSlider = 0,
            MaxStackSlider = 64,
            LureSlider = 0,
            LuckSlider = 0,
            WorldSizeSlider = 172,
            NetherSizeSlider = 176,
            BabyRateSlider = 5,
            WoolMultiplierSlider = 3,
            RequiredPlayersSlider = 2,
            RefillIntervalSlider = 30,
            RingScoreGreen = 0,
            RingScoreOrange = 0,
            RingScoreBlue = 0,
            LiquidSpreadTimeSlider = 0,
        }

        #endregion

        public CemuPckMngr(MainForm iw)
        {
            InitializeComponent();
            _iw = iw;
            StyleMngr.Style = Style = iw.StyleMngr.Style;
            StyleMngr.Theme = Theme = iw.StyleMngr.Theme;
            MainTabs.SelectedIndex = 0;
            ModificationTabs.SelectedIndex = 0;
            MinigamesTabs.SelectedIndex = 0;
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


            _cemuGraphicPckPatchesBuilder.AppendLine("[Minecraft]\nmoduleMatches = " + ModuleMatchesBox.Text);

            DiscordRp.SetPresence(_iw.IsConnected ? "Connected" : "Disconnected",
                "Cemu Graphics Pack Manager");
        }

        private void Exiting(object sender, FormClosingEventArgs e)
        {
            _cemuGraphicPckPatchesBuilder.Clear();
            _cemuGraphicPckRulesBuilder.Clear();
            DiscordRp.SetPresence(_iw.IsConnected ? "Connected" : "Disconnected",
                _iw.MainTabs.SelectedTab.Text + " tab");
            Dispose();
        }

        private void SwapTab(object sender, EventArgs e)
        {
            var tile = (MetroTile) sender;

            if((string)tile.Tag == "ModTile")
            {
                if (ModificationTabs.SelectedIndex != tile.TileCount)
                    ModificationTabs.SelectedIndex = tile.TileCount;

                EmptyTile2.Size = ModificationTabs.SelectedIndex == 3 ? new Size(337, 41) : new Size(169, 41);
                return;
            }
            if ((string)tile.Tag == "MgTile")
            {
                if (MinigamesTabs.SelectedIndex != tile.TileCount)
                    MinigamesTabs.SelectedIndex = tile.TileCount;

                EmptyTile2.Size = ModificationTabs.SelectedIndex == 3 ? new Size(337, 41) : new Size(169, 41);
                return;
            }

            if (MainTabs.SelectedIndex != tile.TileCount)
                    MainTabs.SelectedIndex = tile.TileCount;

            EmptyTile2.Visible = tile.TileCount == 1;
        }

        private void SaveBtnClicked(object sender, EventArgs e)
        {
            try
            {
                var gfxPckFolder = _cemuPckRootDir + "Minecraft Wii U Mod Injector - " + NameBox.Text;
                _cemuGraphicPckRulesBuilder.AppendLine("[Definition]\ntitleIds = " + TitleIdsBox.Text + "\nname = " +
                                                       NameBox.Text + "\npath = " + PathBox.Text + NameBox.Text + "\n" +
                                                       "description = " + DescriptionBox.Text +
                                                       " - Generated by the Minecraft: Wii U Mod Injector\nversion = 4");

                foreach (MetroTabPage page in ModificationTabs.TabPages)
                foreach (Control c in page.Controls)
                {
                    if (c is MetroCheckBox cb)
                        if (cb.Checked)
                        {
                            _cemuGraphicPckPatchesBuilder.AppendLine("#" + cb.Text);
                            if (cb.Tag.ToString().Contains("|"))
                            {
                                _cemuGraphicPckPatchesBuilder.AppendLine(cb.Tag.ToString().Replace('|', '\n'));
                                continue;
                            }

                            _cemuGraphicPckPatchesBuilder.AppendLine(cb.Tag.ToString());
                        }

                    if (c is NumericUpDown sldr)
                    {
                        if(sldr.Value != (int)Enum.Parse(typeof(SliderDefaults), sldr.Name))
                        {
                            _cemuGraphicPckPatchesBuilder.AppendLine("#" + sldr.Name);
                            if (sldr.Tag.ToString().Contains("&"))
                            {
                                var addreStrings = sldr.Tag.ToString().Split('&');
                                foreach (var adr in addreStrings)
                                    _cemuGraphicPckPatchesBuilder.AppendLine(adr + " = li r3,0x" +
                                                                             StringUtils.ToHex(sldr.Value));
                                continue;
                            }

                            _cemuGraphicPckPatchesBuilder.AppendLine(sldr.Tag + "0x" + StringUtils.ToHex(sldr.Value));
                        }
                    }

                    if (c is MetroComboBox comB)
                    {
                        if (comB == RabbitVariantBox && comB.SelectedIndex == 6)
                        {
                            _cemuGraphicPckPatchesBuilder.AppendLine("#" + comB.Name + "\n" + comB.Tag + "0x63");
                            continue;
                        }
                        if(comB.Text != string.Empty)
                        {
                            _cemuGraphicPckPatchesBuilder.AppendLine("#" + comB.Name + "\n" + comB.Tag + "0x" +
                                                                     StringUtils.ToHex(comB.SelectedIndex));
                        }
                    }
                }

                foreach (MetroTabPage page in MinigamesTabs.TabPages)
                foreach (Control c in page.Controls)
                {
                    if (c is MetroCheckBox cb)
                        if (cb.Checked)
                        {
                            _cemuGraphicPckPatchesBuilder.AppendLine("#" + cb.Text);
                            if (cb.Tag.ToString().Contains("|"))
                            {
                                _cemuGraphicPckPatchesBuilder.AppendLine(cb.Tag.ToString().Replace('|', '\n'));
                                continue;
                            }

                            _cemuGraphicPckPatchesBuilder.AppendLine(cb.Tag.ToString());
                        }

                    if (c is NumericUpDown sldrMini)
                    {
                        if (sldrMini.Value != (int) Enum.Parse(typeof(SliderDefaults), sldrMini.Name))
                        {
                            _cemuGraphicPckPatchesBuilder.AppendLine("#" + sldrMini.Name);
                            if (sldrMini.Tag.ToString().Contains("&"))
                            {
                                var addreStrings = sldrMini.Tag.ToString().Split('&');
                                foreach (var adr in addreStrings)
                                    _cemuGraphicPckPatchesBuilder.AppendLine(adr + " = li r3,0x" +
                                                                             StringUtils.ToHex(sldrMini.Value));
                                continue;
                            }

                            _cemuGraphicPckPatchesBuilder.AppendLine(sldrMini.Tag + "0x" +
                                                                     StringUtils.ToHex(sldrMini.Value));
                        }
                    }

                    if (c is MetroComboBox comB)
                    {
                        if (comB.Text != string.Empty)
                        {
                            _cemuGraphicPckPatchesBuilder.AppendLine("#" + comB.Name + "\n" + comB.Tag + "0x" +
                                                                     StringUtils.ToHex(comB.SelectedIndex));
                        }
                    }
                }

                if (!Directory.Exists(gfxPckFolder)) Directory.CreateDirectory(gfxPckFolder);

                using (var file = new StreamWriter(gfxPckFolder + @"\rules.txt"))
                {
                    file.WriteLine(_cemuGraphicPckRulesBuilder.ToString());
                }

                using (var file = new StreamWriter(gfxPckFolder + @"\patches.txt"))
                {
                    file.WriteLine(_cemuGraphicPckPatchesBuilder.ToString());
                }

                Messaging.Show("Successfully saved " + NameBox.Text + " to\n" + gfxPckFolder);

                if (Messaging.Show("Would you like to automatically export the graphics pack to Cemu?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var folderBrowser = new FolderBrowserDialog();
                    folderBrowser.SelectedPath = Settings.Default.CemuGfxLoc;

                    if (folderBrowser.ShowDialog() == DialogResult.OK)
                    {
                        Settings.Default.CemuGfxLoc = folderBrowser.SelectedPath;
                        Settings.Default.Save();
                        Miscellaneous.CopyFolder(gfxPckFolder,
                            folderBrowser.SelectedPath + @"\Minecraft Wii U Mod Injector - " + NameBox.Text);
                        Messaging.Show("Graphics Pack has been successfully installed!");
                    }
                }

                Close();
            }
            catch (Exception exception)
            {
                Exceptions.LogError(exception, "An error occurred while saving/exporting Graphics Pack", true);
            }
        }
    }
}