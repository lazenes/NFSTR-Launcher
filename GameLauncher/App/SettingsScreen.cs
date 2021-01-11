﻿using GameLauncher.App.Classes;
using GameLauncher.App.Classes.InsiderKit;
using GameLauncher.App.Classes.LauncherCore.ModNet;
using GameLauncher.App.Classes.Logger;
using GameLauncher.App.Classes.SystemPlatform.Windows;
using GameLauncher.Resources;
using GameLauncherReborn;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using WindowsFirewallHelper;

namespace GameLauncher.App
{
    public partial class SettingsScreen : Form
    {
        /*******************************/
        /* Global Functions             /
        /*******************************/

        private IniFile _settingFile = new IniFile("Settings.ini");
        private string _userSettings = Environment.GetEnvironmentVariable("AppData") + "/Need for Speed World/Settings/UserSettings.xml";

        private int _lastSelectedCdnId;
        private bool _disableProxy;
        private bool _disableDiscordRPC;
        private bool _restartRequired;
        private string _newLauncherPath;
        private string _newGameFilesPath;

        public string ServerIP = String.Empty;
        public string ServerName = String.Empty;

        List<CDNObject> finalCDNItems = new List<CDNObject>();

        public SettingsScreen(string serverIP, string serverName)
        {
            ServerIP = serverIP;
            ServerName = serverName;

            InitializeComponent();
            SetVisuals();
        }

        private void SetVisuals()
        {
            /*******************************/
            /* Set Background Image         /
            /*******************************/

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Theme\\Settings\\Settings.png"))
            {
                BackgroundImage = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "\\Theme\\Settings\\Settings.png");
            }
            else
            {
                BackgroundImage = Properties.Resources.secondarybackground;
            }

            /*******************************/
            /* Set Hardcoded Text           /
            /*******************************/

            SettingsCDNCurrent.Text = _settingFile.Read("CDN");
            SettingsLauncherVersion.Text = "Version: v" + Application.ProductVersion;

            /*******************************/
            /* Set Font                     /
            /*******************************/

            FontFamily DejaVuSans = FontWrapper.Instance.GetFontFamily("DejaVuSans.ttf");
            FontFamily DejaVuSansBold = FontWrapper.Instance.GetFontFamily("DejaVuSans-Bold.ttf");

            SettingsAboutButton.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsGamePathText.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsGameFiles.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsCDNText.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsCDNPick.Font = new Font(DejaVuSans, 8f, FontStyle.Regular);
            SettingsLanguageText.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsLanguage.Font = new Font(DejaVuSans, 8f, FontStyle.Regular);
            SettingsClearCrashLogsButton.Font = new Font(DejaVuSansBold, 8f, FontStyle.Bold);
            SettingsClearCommunicationLogButton.Font = new Font(DejaVuSansBold, 8f, FontStyle.Bold);
            SettingsClearServerModCacheButton.Font = new Font(DejaVuSansBold, 8f, FontStyle.Bold);
            SettingsWordFilterCheck.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsProxyCheckbox.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsDiscordRPCCheckbox.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsGameFilesCurrentText.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsGameFilesCurrent.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsCDNCurrentText.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsCDNCurrent.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsLauncherPathText.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsLauncherPathCurrent.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsNetworkText.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsMainSrvText.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsMainCDNText.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsBkupSrvText.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsBkupCDNText.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsVFilesButton.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsLauncherVersion.Font = new Font(DejaVuSans, 9f, FontStyle.Regular);
            SettingsSave.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);
            SettingsCancel.Font = new Font(DejaVuSansBold, 9f, FontStyle.Bold);

            /********************************/
            /* Events                        /
            /********************************/
            SettingsCDNPick.DrawItem += new DrawItemEventHandler(SettingsCDNPick_DrawItem);
            /*
            SettingsSave.MouseEnter += new EventHandler(Greenbutton_hover_MouseEnter);
            SettingsSave.MouseLeave += new EventHandler(Greenbutton_MouseLeave);
            SettingsSave.MouseUp += new MouseEventHandler(Greenbutton_hover_MouseUp);
            SettingsSave.MouseDown += new MouseEventHandler(Greenbutton_click_MouseDown);

            SettingsCancel.MouseEnter += new EventHandler(Graybutton_hover_MouseEnter);
            SettingsCancel.MouseLeave += new EventHandler(Graybutton_MouseLeave);
            SettingsCancel.MouseUp += new MouseEventHandler(Graybutton_hover_MouseUp);
            SettingsCancel.MouseDown += new MouseEventHandler(Graybutton_click_MouseDown);
            */
        }

        /********************************/
        /* Draw Events                   /
        /********************************/

        private void SettingsCDNPick_DrawItem(object sender, DrawItemEventArgs e)
        {
            var font = (sender as ComboBox).Font;
            Brush backgroundColor;
            Brush textColor;
            Brush customTextColor = new SolidBrush(Color.FromArgb(178, 210, 255));
            Brush customBGColor = new SolidBrush(Color.FromArgb(44, 58, 76));

            var cdnListText = "";

            if (sender is ComboBox cb)
            {
                if (cb.Items[e.Index] is CDNObject si)
                {
                    cdnListText = si.Name;
                }
            }

            if (cdnListText.StartsWith("<GROUP>"))
            {
                font = new Font(font, FontStyle.Bold);
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                e.Graphics.DrawString(cdnListText.Replace("<GROUP>", string.Empty), font, Brushes.Black, e.Bounds);
            }
            else
            {
                font = new Font(font, FontStyle.Bold);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && e.State != DrawItemState.ComboBoxEdit)
                {
                    backgroundColor = SystemBrushes.Highlight;
                    textColor = SystemBrushes.HighlightText;
                }
                else
                {
                    backgroundColor = customBGColor;
                    textColor = customTextColor;
                }

                e.Graphics.FillRectangle(backgroundColor, e.Bounds);
                e.Graphics.DrawString(cdnListText, font, textColor, e.Bounds);
            }
        }

        private void SettingsScreen_Load(object sender, EventArgs e)
        {
            /********************************/
            /* Get CDN List                  /
            /********************************/

            CDNListUpdater.UpdateCDNList();

            Log.Info("WELCOME: Setting CDN list");
            List<CDNObject> finalCDNItems = CDNListUpdater.GetCDNList();

            SettingsCDNPick.DisplayMember = "Name";
            SettingsCDNPick.DataSource = finalCDNItems;

            /*******************************/
            /* Folder Locations             /
            /*******************************/

            _newGameFilesPath = Path.GetFullPath(_settingFile.Read("InstallationDirectory"));
            _newLauncherPath = AppDomain.CurrentDomain.BaseDirectory;

            SettingsProxyCheckbox.Checked = _disableProxy;
            SettingsDiscordRPCCheckbox.Checked = _disableDiscordRPC;

            SettingsLanguage.DisplayMember = "Text";
            SettingsLanguage.ValueMember = "Value";

            var languages = new[]
            {
                new { Text = "English", Value = "EN" },
                new { Text = "Deutsch", Value = "DE" },
                new { Text = "Español", Value = "ES" },
                new { Text = "Français", Value = "FR" },
                new { Text = "Polski", Value = "PL" },
                new { Text = "Русский", Value = "RU" },
                new { Text = "Português (Brasil)", Value = "PT" },
                new { Text = "繁體中文", Value = "TC" },
                new { Text = "简体中文", Value = "SC" },
                new { Text = "ภาษาไทย", Value = "TH" },
                new { Text = "Türkçe", Value = "TR" },
            };

            SettingsLanguage.DataSource = languages;

            /*******************************/
            /* Read Settings.ini            /
            /*******************************/

            if (_settingFile.KeyExists("Language"))
            {
                SettingsLanguage.SelectedValue = _settingFile.Read("Language");
            }

            _disableProxy = (_settingFile.KeyExists("DisableProxy") && _settingFile.Read("DisableProxy") == "1");
            _disableDiscordRPC = (_settingFile.KeyExists("DisableRPC") && _settingFile.Read("DisableRPC") == "1");

            if (File.Exists(_settingFile.Read("InstallationDirectory") + "/profwords") || File.Exists(_settingFile.Read("InstallationDirectory") + "/profwords_dis"))
            {
                try
                {
                    SettingsWordFilterCheck.Checked = !File.Exists(_settingFile.Read("InstallationDirectory") + "/profwords");
                }
                catch
                {
                    SettingsWordFilterCheck.Checked = false;
                }
            }
            else
            {
                SettingsWordFilterCheck.Enabled = false;
            }

            /*******************************/
            /* Enable/Disable Visuals       /
            /*******************************/

            if (File.Exists(_settingFile.Read("InstallationDirectory") + "/NFSWO_COMMUNICATION_LOG.txt"))
            {
                SettingsClearCommunicationLogButton.Enabled = true;
            }

            if (Directory.Exists(_settingFile.Read("InstallationDirectory") + "/.data"))
            {
                SettingsClearServerModCacheButton.Enabled = true;
            }

            var crashLogFilesDirectory = new DirectoryInfo(_settingFile.Read("InstallationDirectory"));

            if (crashLogFilesDirectory.EnumerateFiles("SBRCrashDump_CL0*.dmp").Count() != 0)
            {
                SettingsClearCrashLogsButton.Enabled = true;
            }

            try
            {
                string SavedCDN = _settingFile.Read("CDN");
                char[] charsToTrim = { '/', 'n', 'f', 's', 'w' };
                string FinalCDNURL = SavedCDN.TrimEnd(charsToTrim);

                if (EnableInsider.ShouldIBeAnInsider() == true)
                {
                    Log.Info("SETTINGS VERIFYHASH: Checking Characters in URL");
                    Log.Info("SETTINGS VERIFYHASH: Trimed end of URL -> " + FinalCDNURL);
                    SettingsVFilesButton.Enabled = true;
                }
                else
                {

                    switch (APIStatusChecker.CheckStatus(FinalCDNURL + "/unpacked/checksums.dat"))
                    {
                        case API.Online:
                            SettingsVFilesButton.Enabled = true;
                            break;
                        default:
                            SettingsVFilesButton.Enabled = false;
                            break;
                    }
                }
            }
            catch { }

            /********************************/
            /* CDN, APIs, & Restore Last CDN /
            /********************************/

            RememberLastCDN();
            IsCDNDownGame();
            PingAPIStatus();
        }

        /*******************************/
        /* On Button/Dropdown Functions /
        /*******************************/

        /* Settings Save */
        private void SettingsSave_Click(object sender, EventArgs e)
        {
            //TODO null check
            _settingFile.Write("Language", SettingsLanguage.SelectedValue.ToString());

            if (WindowsProductVersion.GetWindowsNumber() >= 10.0 && (_settingFile.Read("InstallationDirectory") != _newGameFilesPath) && !DetectLinux.LinuxDetected())
            {
                WindowsDefenderGameFilesDirctoryChange();
            }
            else if (_settingFile.Read("InstallationDirectory") != _newGameFilesPath)
            {
                CheckGameFilesDirectoryPrevention();

                if (!DetectLinux.LinuxDetected())
                {
                    //Remove current Firewall for the Game Files 
                    string CurrentGameFilesExePath = Path.Combine(_settingFile.Read("InstallationDirectory") + "\\nfsw.exe");

                    if (File.Exists(CurrentGameFilesExePath) && FirewallHelper.RuleExist("SBRW - Game") == true)
                    {
                        bool removeFirewallRule = true;
                        bool firstTimeRun = true;

                        string nameOfGame = "SBRW - Game";
                        string localOfGame = CurrentGameFilesExePath;

                        string groupKeyGame = "Need for Speed: World";
                        string descriptionGame = groupKeyGame;

                        //Inbound & Outbound
                        FirewallHelper.DoesRulesExist(removeFirewallRule, firstTimeRun, nameOfGame, localOfGame, groupKeyGame, descriptionGame, FirewallProtocol.Any);
                    }
                }

                _settingFile.Write("InstallationDirectory", _newGameFilesPath);

                //Clean Mods Files from New Dirctory (If it has .links in directory)
                var linksPath = Path.Combine(_settingFile.Read("InstallationDirectory"), ".links");
                ModNetLinksCleanup.CleanLinks(linksPath);

                _restartRequired = true;
            }

            Console.WriteLine(_settingFile.Read("CDN"));
            Console.WriteLine(((CDNObject)SettingsCDNPick.SelectedItem).Url);

            if (_settingFile.Read("CDN") != ((CDNObject)SettingsCDNPick.SelectedItem).Url)
            {
                SettingsCDNCurrentText.Text = "CHANGED CDN";
                SettingsCDNCurrent.Text = ((CDNObject)SettingsCDNPick.SelectedItem).Url;
                _settingFile.Write("CDN", ((CDNObject)SettingsCDNPick.SelectedItem).Url);
                _restartRequired = true;
            }

            String disableProxy = (SettingsProxyCheckbox.Checked == true) ? "1" : "0";
            if (_settingFile.Read("DisableProxy") != disableProxy)
            {
                _settingFile.Write("DisableProxy", (SettingsProxyCheckbox.Checked == true) ? "1" : "0");
                _restartRequired = true;
            }

            String disableRPC = (SettingsDiscordRPCCheckbox.Checked == true) ? "1" : "0";
            if (_settingFile.Read("DisableRPC") != disableRPC)
            {
                _settingFile.Write("DisableRPC", (SettingsDiscordRPCCheckbox.Checked == true) ? "1" : "0");
                _restartRequired = true;
            }

            if (_restartRequired)
            {
                MessageBox.Show(null, "In order to see settings changes, you need to restart launcher manually.", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            //Actually lets check those 2 files
            if (File.Exists(_settingFile.Read("InstallationDirectory") + "/profwords") && File.Exists(_settingFile.Read("InstallationDirectory") + "/profwords_dis"))
            {
                File.Delete(_settingFile.Read("InstallationDirectory") + "/profwords_dis");
            }

            //Delete/Enable profwords filter here
            if (SettingsWordFilterCheck.Checked)
            {
                if (File.Exists(_settingFile.Read("InstallationDirectory") + "/profwords")) File.Move(_settingFile.Read("InstallationDirectory") + "/profwords", _settingFile.Read("InstallationDirectory") + "/profwords_dis");
            }
            else
            {
                if (File.Exists(_settingFile.Read("InstallationDirectory") + "/profwords_dis")) File.Move(_settingFile.Read("InstallationDirectory") + "/profwords_dis", _settingFile.Read("InstallationDirectory") + "/profwords");
            }

            var userSettingsXml = new XmlDocument();
            try
            {
                if (File.Exists(_userSettings))
                {
                    try
                    {
                        userSettingsXml.Load(_userSettings);
                        var language = userSettingsXml.SelectSingleNode("Settings/UI/Language");
                        language.InnerText = SettingsLanguage.SelectedValue.ToString();
                    }
                    catch
                    {
                        File.Delete(_userSettings);

                        var setting = userSettingsXml.AppendChild(userSettingsXml.CreateElement("Settings"));
                        var ui = setting.AppendChild(userSettingsXml.CreateElement("UI"));

                        var persistentValue = setting.AppendChild(userSettingsXml.CreateElement("PersistentValue"));
                        var chat = persistentValue.AppendChild(userSettingsXml.CreateElement("Chat"));
                        chat.InnerXml = "<DefaultChatGroup Type=\"string\">" + Self.currentLanguage + "</DefaultChatGroup>";
                        ui.InnerXml = "<Language Type=\"string\">" + SettingsLanguage.SelectedValue + "</Language>";

                        var directoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(_userSettings));
                    }
                }
                else
                {
                    try
                    {
                        var setting = userSettingsXml.AppendChild(userSettingsXml.CreateElement("Settings"));
                        var ui = setting.AppendChild(userSettingsXml.CreateElement("UI"));

                        var persistentValue = setting.AppendChild(userSettingsXml.CreateElement("PersistentValue"));
                        var chat = persistentValue.AppendChild(userSettingsXml.CreateElement("Chat"));
                        chat.InnerXml = "<DefaultChatGroup Type=\"string\">" + Self.currentLanguage + "</DefaultChatGroup>";
                        ui.InnerXml = "<Language Type=\"string\">" + SettingsLanguage.SelectedValue + "</Language>";

                        var directoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(_userSettings));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(null, "There was an error saving your settings to actual file. Restoring default.\n" + ex.Message, "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        File.Delete(_userSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, "There was an error saving your settings to actual file. Restoring default.\n" + ex.Message, "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(_userSettings);
            }
            userSettingsXml.Save(_userSettings);

            DialogResult = DialogResult.OK;
            Close();
        }

        /* Settings Cancel */
        private void SettingsCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /* Settings Verify Hash */
        private void SettingsVFilesButton_Click(object sender, EventArgs e)
        {
            new VerifyHash().ShowDialog();
        }

        /* Settings Clear ModNet Cache */
        private void SettingsClearServerModCacheButton_Click(object sender, EventArgs e)
        {
            Directory.Delete(_settingFile.Read("InstallationDirectory") + "/.data", true);
            Directory.Delete(_settingFile.Read("InstallationDirectory") + "/MODS", true);
            Log.Warning("LAUNCHER: User Confirmed to Delete Server Mods Cache");
            MessageBox.Show(null, "Deleted Server Mods Cache", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SettingsClearServerModCacheButton.Enabled = false;
        }

        /* Settings Clear Communication Logs */
        private void SettingsClearCommunicationLogButton_Click(object sender, EventArgs e)
        {
            File.Delete(_settingFile.Read("InstallationDirectory") + "/NFSWO_COMMUNICATION_LOG.txt");
            MessageBox.Show(null, "Deleted NFSWO Communication Log", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SettingsClearCommunicationLogButton.Enabled = false;
        }

        /* Settings Clear Game Crash Logs */
        private void SettingsClearCrashLogsButton_Click(object sender, EventArgs e)
        {
            var crashLogFilesDirectory = new DirectoryInfo(_settingFile.Read("InstallationDirectory"));

            foreach (var file in crashLogFilesDirectory.EnumerateFiles("SBRCrashDump_CL0*.dmp"))
            {
                file.Delete();
            }

            foreach (var file in crashLogFilesDirectory.EnumerateFiles("SBRCrashDump_CL0*.txt"))
            {
                file.Delete();
            }

            foreach (var file in crashLogFilesDirectory.EnumerateFiles("NFSCrashDump_CL0*.dmp"))
            {
                file.Delete();
            }

            MessageBox.Show(null, "Deleted Crash Logs", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SettingsClearCrashLogsButton.Enabled = false;
        }

        /* Settings Change Game Files Location */
        private void SettingsGameFiles_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog changeGameFilesPath = new System.Windows.Forms.OpenFileDialog
            {
                InitialDirectory = "C:\\",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select the location to Find or Download nfsw.exe",
                FileName = "Select Game Files Folder"
            };
            if (changeGameFilesPath.ShowDialog() == DialogResult.OK)
            {
                _newGameFilesPath = Path.GetDirectoryName(changeGameFilesPath.FileName);
                SettingsGameFilesCurrentText.Text = "NEW DIRECTORY";
                SettingsGameFilesCurrent.Text = _newGameFilesPath;
            }
        }

        /* Settings Open Current CDN in Browser */
        private void SettingsCDNCurrent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(_settingFile.Read("CDN"));
        }

        /* Settings Open Current Launcher Path in Explorer */
        private void SettingsLauncherPathCurrent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(_newLauncherPath);
        }

        /* Settings Open Current Game Files Path in Explorer */
        private void SettingsGameFilesCurrent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(_newGameFilesPath);
        }

        /* Settings Open About Dialog */
        private void SettingsAboutButton_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        private void SettingsLauncherVersion_Click(object sender, EventArgs e)
        {
            new DebugWindow(ServerIP, ServerName).ShowDialog();
        }

        /* Settings CDN Dropdown Menu Index */
        private void SettingsCDNPick_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((CDNObject)SettingsCDNPick.SelectedItem).IsSpecial && ((CDNObject)SettingsCDNPick.SelectedItem).Url == null)
            {
                SettingsCDNPick.SelectedIndex = _lastSelectedCdnId;
                return;
            }
            else if (((CDNObject)SettingsCDNPick.SelectedItem).Url != null)
            {
                IsChangedCDNDown();
            }
            else
            {
                SettingsCDNText.Text = "CDN:";
                SettingsCDNText.ForeColor = Color.FromArgb(224, 224, 224);
            }

            _lastSelectedCdnId = SettingsCDNPick.SelectedIndex;
        }

        /*******************************/
        /* Callback Functions           /
        /*******************************/

        private void IsChangedCDNDown()
        {
            if (!string.IsNullOrEmpty(((CDNObject)SettingsCDNPick.SelectedItem).Url))
            {
                SettingsCDNText.Text = "CDN: PINGING";
                SettingsCDNText.ForeColor = Color.FromArgb(66, 179, 189);
                Log.Info("SETTINGS PINGING CHANGED CDN: Checking Changed CDN from Drop Down List");

                switch (APIStatusChecker.CheckStatus(((CDNObject)SettingsCDNPick.SelectedItem).Url + "/index.xml"))
                {
                    case API.Online:
                        SettingsCDNText.Text = "CDN: ONLINE";
                        SettingsCDNText.ForeColor = Color.FromArgb(159, 193, 32);
                        Log.UrlCall("SETTINGS PINGING CHANGED CDN: " + ((CDNObject)SettingsCDNPick.SelectedItem).Url + " Is Online!");
                        break;
                    default:
                        SettingsCDNText.Text = "CDN: OFFLINE";
                        SettingsCDNText.ForeColor = Color.FromArgb(254, 0, 0);
                        Log.UrlCall("SETTINGS PINGING CHANGED CDN: " + ((CDNObject)SettingsCDNPick.SelectedItem).Url + " Is Offline!");
                        break;
                }
            }
            else
            {
                Log.Error("SETTINGS PINGING CHANGED CDN: '((CDNObject)SettingsCDNPick.SelectedItem).Url)' has an Empty CDN URL");
            }
        }

        private void WindowsDefenderGameFilesDirctoryChange()
        {
            //Check if New Game! Files is not in Banned Folder Locations
            CheckGameFilesDirectoryPrevention();

            //Remove current Exclusion and Add new location for Exclusion
            using (PowerShell ps = PowerShell.Create())
            {
                Log.Warning("WINDOWS DEFENDER: Removing OLD Game Files Directory: " + _settingFile.Read("InstallationDirectory"));
                ps.AddScript($"Remove-MpPreference -ExclusionPath \"{_settingFile.Read("InstallationDirectory")}\"");
                Log.Core("WINDOWS DEFENDER: Excluding NEW Game Files Directory: " + _newGameFilesPath);
                ps.AddScript($"Add-MpPreference -ExclusionPath \"{_newGameFilesPath}\"");
                var result = ps.Invoke();
            }

            //Remove current Firewall for the Game Files 
            string CurrentGameFilesExePath = Path.Combine(_settingFile.Read("InstallationDirectory") + "\\nfsw.exe");

            if (File.Exists(CurrentGameFilesExePath) && FirewallHelper.RuleExist("SBRW - Game") == true)
            {
                bool removeFirewallRule = true;
                bool firstTimeRun = true;

                string nameOfGame = "SBRW - Game";
                string localOfGame = CurrentGameFilesExePath;

                string groupKeyGame = "Need for Speed: World";
                string descriptionGame = groupKeyGame;

                //Inbound & Outbound
                FirewallHelper.DoesRulesExist(removeFirewallRule, firstTimeRun, nameOfGame, localOfGame, groupKeyGame, descriptionGame, FirewallProtocol.Any);
            }

            _settingFile.Write("InstallationDirectory", _newGameFilesPath);

            //Clean Mods Files from New Dirctory (If it has .links in directory)
            var linksPath = Path.Combine(_settingFile.Read("InstallationDirectory"), ".links");
            ModNetLinksCleanup.CleanLinks(linksPath);

            _restartRequired = true;
        }

        private void CheckGameFilesDirectoryPrevention()
        {
            if (!DetectLinux.LinuxDetected())
            {
                switch (Self.CheckFolder(_settingFile.Read("InstallationDirectory")))
                {
                    case FolderType.IsSameAsLauncherFolder:
                        Directory.CreateDirectory("Game Files");
                        Log.Error("LAUNCHER: Installing NFSW in same directory where the launcher resides is disadvised.");
                        MessageBox.Show(null, string.Format("Installing NFSW in same directory where the launcher resides is disadvised. Instead, we will install it at {0}.", AppDomain.CurrentDomain.BaseDirectory + "Game Files"), "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _settingFile.Write("InstallationDirectory", AppDomain.CurrentDomain.BaseDirectory + "\\Game Files");
                        break;
                    case FolderType.IsTempFolder:
                        Directory.CreateDirectory("Game Files");
                        Log.Error("LAUNCHER: (╯°□°）╯︵ ┻━┻ Installing NFSW in the Temp Folder is disadvised!");
                        MessageBox.Show(null, string.Format("(╯°□°）╯︵ ┻━┻\n\nInstalling NFSW in the Temp Folder is disadvised! Instead, we will install it at {0}.", AppDomain.CurrentDomain.BaseDirectory + "\\Game Files" + "\n\n┬─┬ ノ( ゜-゜ノ)"), "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _settingFile.Write("InstallationDirectory", AppDomain.CurrentDomain.BaseDirectory + "\\Game Files");
                        break;
                    case FolderType.IsProgramFilesFolder:
                    case FolderType.IsUsersFolders:
                    case FolderType.IsWindowsFolder:
                        Directory.CreateDirectory("Game Files");
                        Log.Error("LAUNCHER: Installing NFSW in a Special Directory is disadvised.");
                        MessageBox.Show(null, string.Format("Installing NFSW in a Special Directory is disadvised. Instead, we will install it at {0}.", AppDomain.CurrentDomain.BaseDirectory + "\\Game Files"), "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _settingFile.Write("InstallationDirectory", AppDomain.CurrentDomain.BaseDirectory + "\\Game Files");
                        break;
                }
            }
        }

        //DavidCarbon
        private async void PingAPIStatus()
        {
            ClearColoredPingStatus();
            Log.Api("SETTINGS PINGING API: Checking APIs");

            bool WUGG = true;

            await Task.Delay(1000);
            switch (APIStatusChecker.CheckStatus(Self.mainserver + "/serverlist.json"))
            {
                case API.Online:
                    SettingsMainSrvText.Text = "[API] United: ONLINE";
                    SettingsMainSrvText.ForeColor = Color.FromArgb(159, 193, 32);
                    WUGG = true;
                    break;
                case API.Offline:
                    SettingsMainSrvText.Text = "[API] United: OFFLINE";
                    SettingsMainSrvText.ForeColor = Color.FromArgb(254, 0, 0);
                    WUGG = false;
                    break;
                default:
                    SettingsMainSrvText.Text = "[API] United: ERROR";
                    SettingsMainSrvText.ForeColor = Color.FromArgb(254, 0, 0);
                    WUGG = false;
                    break;
            }

            bool DC = true;

            if (WUGG == false)
            {
                SettingsMainCDNText.Visible = true;
                await Task.Delay(1500);
                switch (APIStatusChecker.CheckStatus(Self.staticapiserver + "/serverlist.json"))
                {
                    case API.Online:
                        SettingsMainCDNText.Text = "[API] Carbon: ONLINE";
                        SettingsMainCDNText.ForeColor = Color.FromArgb(159, 193, 32);
                        DC = true;
                        break;
                    case API.Offline:
                        SettingsMainCDNText.Text = "[API] Carbon: OFFLINE";
                        SettingsMainCDNText.ForeColor = Color.FromArgb(254, 0, 0);
                        DC = false;
                        break;
                    default:
                        SettingsMainCDNText.Text = "[API] Carbon: ERROR";
                        SettingsMainCDNText.ForeColor = Color.FromArgb(254, 0, 0);
                        DC = false;
                        break;
                }
            }

            bool DC2 = true;

            if (DC == false)
            {
                SettingsBkupSrvText.Visible = true;
                await Task.Delay(2000);
                switch (APIStatusChecker.CheckStatus(Self.secondstaticapiserver + "/serverlist.json"))
                {
                    case API.Online:
                        SettingsBkupSrvText.Text = "[API] Carbon (2nd): ONLINE";
                        SettingsBkupSrvText.ForeColor = Color.FromArgb(159, 193, 32);
                        DC2 = true;
                        break;
                    case API.Offline:
                        SettingsBkupSrvText.Text = "[API] Carbon (2nd): OFFLINE";
                        SettingsBkupSrvText.ForeColor = Color.FromArgb(254, 0, 0);
                        DC2 = false;
                        break;
                    default:
                        SettingsBkupSrvText.Text = "[API] Carbon (2nd): ERROR";
                        SettingsBkupSrvText.ForeColor = Color.FromArgb(254, 0, 0);
                        DC2 = false;
                        break;
                }
            }

            if (DC2 == false)
            {
                SettingsBkupCDNText.Visible = true;
                await Task.Delay(2500);
                switch (APIStatusChecker.CheckStatus(Self.woplserver + "/serverlist.json"))
                {
                    case API.Online:
                        SettingsBkupCDNText.Text = "[API] WOPL: ONLINE";
                        SettingsBkupCDNText.ForeColor = Color.FromArgb(159, 193, 32);
                        break;
                    case API.Offline:
                        SettingsBkupCDNText.Text = "[API] WOPL: OFFLINE";
                        SettingsBkupCDNText.ForeColor = Color.FromArgb(254, 0, 0);
                        break;
                    default:
                        SettingsBkupCDNText.Text = "[API] WOPL: ERROR";
                        SettingsBkupCDNText.ForeColor = Color.FromArgb(254, 0, 0);
                        break;
                }
            }
        }

        private void ClearColoredPingStatus()
        {
            SettingsMainCDNText.Visible = false;
            SettingsBkupSrvText.Visible = false;
            SettingsBkupCDNText.Visible = false;
            //Reset Connection Status Labels - DavidCarbon
            SettingsMainSrvText.Text = "[API] United: PINGING";
            SettingsMainSrvText.ForeColor = Color.FromArgb(66, 179, 189);
            SettingsMainCDNText.Text = "[API] Carbon: PINGING";
            SettingsMainCDNText.ForeColor = Color.FromArgb(66, 179, 189);
            SettingsBkupSrvText.Text = "[API] Carbon (2nd): PINGING";
            SettingsBkupSrvText.ForeColor = Color.FromArgb(66, 179, 189);
            SettingsBkupCDNText.Text = "[API] WOPL: PINGING";
            SettingsBkupCDNText.ForeColor = Color.FromArgb(66, 179, 189);
        }

        private void RememberLastCDN()
        {
            /* Last Selected CDN */
            Log.Core("SETTINGS CDNLIST: Checking...");
            Log.Core("SETTINGS CDNLIST: Setting first server in list");
            Log.Core("SETTINGS CDNLIST: Checking if server is set on INI File");

            if (_settingFile.KeyExists("CDN"))
            {
                string SavedCDN = _settingFile.Read("CDN");
                char[] charsToTrim = { '/' };
                string FinalCDNURL = SavedCDN.TrimEnd(charsToTrim);

                Log.Core("SETTINGS CDNLIST: Found something!");
                Log.Core("SETTINGS CDNLIST: Checking if CDN exists on our database");

                if (finalCDNItems.FindIndex(i => string.Equals(i.Url, FinalCDNURL)) != 0)
                {
                    Log.Core("SETTINGS CDNLIST: CDN found! Checking ID");
                    var index = finalCDNItems.FindIndex(i => string.Equals(i.Url, FinalCDNURL));

                    Log.Core("SETTINGS CDNLIST: ID is " + index);
                    if (index >= 0)
                    {
                        Log.Core("SETTINGS CDNLIST: ID set correctly");
                        SettingsCDNPick.SelectedIndex = index;
                    }
                    else if (index < 0)
                    {
                        Log.Warning("SETTINGS CDNLIST: Old CDN URL Standard Detected!");
                        SettingsCDNPick.SelectedIndex = 1;
                        Log.Warning("SETTINGS CDNLIST: Displaying First CDN in List!");
                    }
                }
                else
                {
                    Log.Warning("SETTINGS CDNLIST: Unable to find anything, assuming default");
                    SettingsCDNPick.SelectedIndex = 1;
                    Log.Warning("SETTINGS CDNLIST: Unknown entry value is " + FinalCDNURL);
                }
                Log.Core("SETTINGS CDNLIST: All done");
            }
        }

        //CDN Display Playing Game! - DavidCarbon
        private async void IsCDNDownGame()
        {
            if (!string.IsNullOrEmpty(_settingFile.Read("CDN")))
            {
                SettingsCDNCurrent.LinkColor = Color.FromArgb(66, 179, 189);
                Log.Info("SETTINGS PINGING CDN: Checking Current CDN from Settings.ini");
                await Task.Delay(500);

                switch (APIStatusChecker.CheckStatus(_settingFile.Read("CDN") + "/index.xml"))
                {
                    case API.Online:
                        SettingsCDNCurrent.LinkColor = Color.FromArgb(159, 193, 32);
                        Log.UrlCall("SETTINGS PINGING CDN: " + _settingFile.Read("CDN") + " Is Online!");
                        break;
                    default:
                        SettingsCDNCurrent.LinkColor = Color.FromArgb(254, 0, 0);
                        Log.UrlCall("SETTINGS PINGING CDN: " + _settingFile.Read("CDN") + " Is Offline!");
                        break;
                }
            }
            else
            {
                Log.Error("SETTINGS PINGING CDN: Settings.ini has an Empty CDN URL");
            }
        }
    }
}
