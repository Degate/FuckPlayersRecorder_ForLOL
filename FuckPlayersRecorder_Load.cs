using FuckPlayersRecorder_ForLOL;
using FuckPlayersRecorder_ForLOL.Models;
using FuckPlayersRecorder_ForLOL.Resources;
using Gma.System.MouseKeyHook;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;
using WindowsInput;
using WindowsInput.Events;
using RadioButton = System.Windows.Forms.RadioButton;

namespace FuckPlayersRecorder
{
    public partial class FuckPlayersRecorderMainForm : Form
    {
        public FuckPlayersRecorderMainForm()
        {
            InitializeComponent();
        }
        private string playerSelected;
        private GameServiceForLCU _lcuapi;
        private IKeyboardMouseEvents _keyboardMouseEvent;
        private List<RadioButton> rdbt_Players = new List<RadioButton>();
        private List<Panel> Team1Panels = new List<Panel>();
        private List<Panel> Team2Panels = new List<Panel>();
        private string logFilePath = string.Empty;
        private void btn_ClearPlayerName_Click(object sender, EventArgs e)
        {
            tbx_PlayerFullName.Text = string.Empty;
            listView1.Items.Clear();
        }
        // Fix for CS1001, CS1031, CS1003 errors in the method signature

        private void LoadXmlDocumentPath(string xmlPath)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);
                XmlElement rootElement = xmlDoc.DocumentElement;
                if (rootElement.HasAttributes)
                {
                    tbx_DocumentPath.Text = rootElement.GetAttribute("DocumentPath");
                    if (File.Exists(tbx_DocumentPath.Text))
                    {
                        debugBox.Text += ("找到历史数据库文件");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载XML文件时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FuckPlayersRecorder_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            logFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"\\[{DateTime.Now.ToString("yyyy-MM-dd")}]Debug.log";
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string documentFilePath = Path.GetDirectoryName(assemblyPath) + @"\FuckPlayersRecorder.xml";
            if (!File.Exists(documentFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("APP_Configuration");
                root.SetAttribute("DocumentPath", string.Empty);
                xmlDoc.AppendChild(root);
                xmlDoc.Save(documentFilePath);
                tbx_DocumentPath.Text = "设置数据保存路径,若指定路径不存在文件则自动创建";
                tbx_DocumentPath.ForeColor = Color.Red;
            }
            LoadXmlDocumentPath(documentFilePath);

            _lcuapi = new GameServiceForLCU();
            _lcuapi.Initialize(this);

            lbl_LCUStatus.DataBindings.Add("Text", _lcuapi, "GameStatus", true, DataSourceUpdateMode.OnPropertyChanged);
            pbx_LCUStatus.DataBindings.Add("BackColor", _lcuapi, "GameStatusColor", true, DataSourceUpdateMode.OnPropertyChanged);
            btn_ReloadGameCilent.DataBindings.Add("Enabled", _lcuapi, "LcuConnected", true, DataSourceUpdateMode.OnPropertyChanged);

            Team1Panels.AddRange(new[] { panel_TEAM1_1, panel_TEAM1_2, panel_TEAM1_3, panel_TEAM1_4, panel_TEAM1_5 });
            Team2Panels.AddRange(new[] { panel_TEAM2_1, panel_TEAM2_2, panel_TEAM2_3, panel_TEAM2_4, panel_TEAM2_5 });
            #region set rdbt_Players list
            rdbt_Players.AddRange(new[] { Teammate_1, Teammate_2, Teammate_3, Teammate_4, Teammate_5, Teammate_6, Teammate_7, Teammate_8, Teammate_9, Teammate_10 });
            #endregion
            #region set hotkey
            _keyboardMouseEvent = Hook.GlobalEvents();
            _keyboardMouseEvent.KeyDown += OnKeyDown;
            _keyboardMouseEvent.KeyUp += OnKeyUp;
            #endregion
        }

        private void btn_ChangeDocumentPath_Click(object sender, EventArgs e)
        {
            ofdg_GetDocumentPath.ShowDialog();
            if (ofdg_GetDocumentPath.FileName != string.Empty)
            {
                debugBox.Text += ofdg_GetDocumentPath.FileName + "\n";
                //try
                //{
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string documentFilePath = Path.GetDirectoryName(assemblyPath) + @"\FuckPlayersRecorder.xml";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(documentFilePath);
                XmlElement rootElement = xmlDoc.DocumentElement;
                rootElement.Attributes[0].Value = ofdg_GetDocumentPath.FileName;
                xmlDoc.Save(documentFilePath);
                LoadXmlDocumentPath(documentFilePath);
                if (!File.Exists(tbx_DocumentPath.Text))
                {
                    using (StreamWriter writer = new StreamWriter(tbx_DocumentPath.Text))
                    {
                        string thisDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        writer.WriteLine($"CreatedDate: {thisDate}");
                    }
                    tbx_DocumentPath.ForeColor = Color.Black;
                }
            }
            else
            {
                debugBox.Text += "未选择文件路径\n";
            }
        }

        private void btn_AddIn_Click(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter(tbx_DocumentPath.Text, append: true))
            {
                string thisLine = $"{playerSelected}|{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|";
                if (adbtn_ThisGameMode.Checked)
                {
                    thisLine += $"{Constant.currentMapGameModeName}|";
                }
                else if (adbtn_SelectGameMode.Checked)
                {
                    if (string.IsNullOrEmpty(cbx_SelectGameMode.Text))
                        thisLine += $"{Constant.currentQueueGameMode}|";
                    else thisLine += $"{cbx_SelectGameMode.Text}|";
                }
                foreach (RadioButton rb in rdbt_Players)
                {
                    if (rb.Checked)
                    {
                        foreach (Label label in rb.Parent.Controls.OfType<Label>())
                        {
                            if (label.Tag == "KDA")
                            {
                                thisLine += $"{label.Text}|";
                            }
                            if(label.Tag == "PlayerFullName")
                            {
                                label.ForeColor = Color.Red;
                            }
                        }
                        break;
                    }
                }
                //thisLine += $"|{numud_K.Value}/{numud_D.Value}/{numud_A.Value}|";
                if (ckb_IAFK.Checked)
                {
                    thisLine += "挂机,";
                }
                if (ckb_Profanity.Checked)
                {
                    thisLine += "脏话,";
                }
                if (ckb_Idiot.Checked)
                {
                    thisLine += "苯比,";
                }
                if (ckb_IFeeding.Checked)
                {
                    thisLine += "送人头,";
                }
                if (ckb_UselessPokebot.Checked)
                {
                    thisLine += "无用丢子,";
                }
                if (ckb_UselessLeecher.Checked)
                {
                    thisLine += "玩混子英雄,";
                }
                if (ckb_Talk2Much.Checked)
                {
                    thisLine += "事多墨迹,";
                }
                writer.WriteLine(thisLine.TrimEnd(','));
            }
            FillListViewWithPlayerData(playerSelected);
        }
        public void UpdateDebugBox(string text, bool appendToDebugBox)
        {
            while (true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true)) // 第二个参数表示追加模式
                    {
                        writer.WriteLine($"\n[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]:{text}");
                    }
                    if (appendToDebugBox)
                    {
                        debugBox.AppendText($"\n[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]:{text}");
                        debugBox.SelectionStart = debugBox.Text.Length;
                        debugBox.ScrollToCaret();
                    }
                    break; // 成功写入后跳出循环
                }
                catch
                {
                    Task.Delay(10).Wait(); // 等待1秒后重试
                }
            }
        }
        public void UpdateLCUStatus(string text)
        {
            lbl_LCUStatus.Text = text;
            //pbx_LCUStatus.BackColor = _lcuapi.GameStatusColor;
        }
        public void UpdateGameMode(string mode)
        {
            //adbtn_ThisGameMode.Text = mode;
        }
        public void UpdatePlayerInfo(List<Account> accounts1, List<Account> accounts2)
        {
            UpdateDebugBox($"UpdatePlayerInfo() => accounts1.Count: {accounts1.Count}, accounts2.Count: {accounts2.Count}", false);
            foreach (var account in accounts1)
            {
                ChangePlayerInfo(account.Puuid, 1, Math.Min(accounts1.IndexOf(account), 10), account.Champion, account.FullName, account.Kill, account.Assist, account.Death, account.Damage);
            }
            foreach (var account in accounts2)
            {
                ChangePlayerInfo(account.Puuid, 2, Math.Min(accounts2.IndexOf(account), 10), account.Champion, account.FullName, account.Kill, account.Assist, account.Death, account.Damage);
            }
            FillListViewWithPlayerData(PlayerFullName_1.Text);
            Teammate_1.Checked = true;
            rbt_Teammate_CheckedChanged(Teammate_1, null);
            foreach (Panel panel in Team1Panels)
            {
                UpdateDebugBox($"panel.name:{panel.Name},panel.Tag: {panel.Tag}", false);
            }
            foreach (Panel panel in Team2Panels)
            {
                UpdateDebugBox($"panel.name:{panel.Name},panel.Tag: {panel.Tag}", false);
            }
        }
        public void UpdatePlayerInfoGameEnd(List<Account> accounts1, List<Account> accounts2)
        {
            UpdateDebugBox($"UpdatePlayerInfoGameEnd() => accounts1.Count: {accounts1.Count}, accounts2.Count: {accounts2.Count}", false);
            foreach (var account in accounts1)
            {
                UpdateDebugBox($"UpdatePlayerInfoGameEnd() => account.Puuid: {account.Puuid}", false);
                foreach (Panel panel in Team1Panels)
                {
                    UpdateDebugBox($"foreach(Team1Panels) => panel.Tag: {panel.Tag}", false);
                    if (panel.Tag == account.Puuid)
                    {
                        foreach (Control ctrl in panel.Controls)
                        {
                            UpdateDebugBox($"foreach(Control) => Control.Tag: {ctrl.Tag}", false);
                            switch (ctrl.Tag)
                            {
                                case "KDA":
                                    ctrl.Text = $"{account.Kill}/{account.Death}/{account.Assist}";
                                    break;
                                case "Damage":
                                    ctrl.Text = account.Damage > 0 ? $"{Math.Round(account.Damage / 1000, 2)} K" : "0 K";
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
            foreach (var account in accounts2)
            {
                UpdateDebugBox($"UpdatePlayerInfoGameEnd() => account.Puuid: {account.Puuid}", false);
                foreach (Panel panel in Team2Panels)
                {
                    UpdateDebugBox($"foreach(Team2Panels) => panel.Tag: {panel.Tag}", false);
                    if (panel.Tag == account.Puuid)
                    {
                        foreach (Control ctrl in panel.Controls)
                        {
                            UpdateDebugBox($"foreach(Control) => Control.Tag: {ctrl.Tag}", false);
                            switch (ctrl.Tag)
                            {
                                case "KDA":
                                    ctrl.Text = $"{account.Kill}/{account.Death}/{account.Assist}";
                                    break;
                                case "Damage":
                                    ctrl.Text = account.Damage > 0 ? $"{Math.Round(account.Damage / 1000, 2)} K" : "0 K";
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
        }
        public void RestMainFormUI()
        {
            ResetTeammateLabels();
            adbtn_ThisGameMode.Text = "当前对局模式";
            listView1.Items.Clear();

        }
        private void ResetTeammateLabels()
        {
            for (int teamId = 1; teamId <= 2; teamId++)
            {
                for (int indexId = 0; indexId < 5; indexId++)
                {
                    ChangePlayerInfo(string.Empty, teamId, indexId, null, "默认召唤师名#12345", 0, 0, 0, 0);
                }
            }
            Teammate_1.Checked = true;
            rbt_Teammate_CheckedChanged(Teammate_1, null);
            playerSelected = null;
        }
        private List<string> GetPlayerFromDoc(string thisName)
        {
            var filteredLines = File.ReadLines(tbx_DocumentPath.Text)
                       .Where(line => line.Contains(thisName))
                       .Select(line => line.ToUpper());
            return filteredLines.ToList<string>();
        }
        private void ChangePlayerInfo(string puuid, int teamId, int indexId, Hero hero, string fullName, int kills, int assists, int deaths, double damage)
        {
            UpdateDebugBox($"ChangePlayerInfo() => puuid:[{puuid}], teamId:[{teamId}], indexId:[{indexId}], fullName:[{fullName}],kills:[{kills}],assists:[{assists}], deaths:[{deaths}], damage:[{damage}]", false);
            var thisTeamList = teamId == 1 ? Team1Panels : Team2Panels;
            if (indexId > 4 && indexId < 10)
            {
                indexId = indexId - 5;
                thisTeamList = Team2Panels;
            }
            else if (indexId > 9)
            {
                return;
            }
            UpdateDebugBox($"indexId Parse:{indexId}", false);
            var labels = thisTeamList[indexId].Controls.OfType<Label>();
            foreach (var label in labels) switch (label.Tag)
                {
                    case "PlayerFullName":
                        if (fullName == "默认召唤师名#12345" || fullName == null)
                            thisTeamList[indexId].Enabled = false;
                        else thisTeamList[indexId].Enabled = true;
                        thisTeamList[indexId].Tag = puuid; // 设置Panel的Tag为puuid
                        label.Text = fullName;
                        label.ForeColor = GetPlayerFromDoc(fullName).Count > 0 ? Color.Red : Color.Black;
                        break;
                    case "KDA":
                        label.Text = $"{kills}/{deaths}/{assists}";
                        break;
                    case "Damage":
                        label.Text = damage > 0 ? $"{Math.Round(damage / 1000, 2)} K" : "0 K";
                        break;
                }
            List<PictureBox> avatar = thisTeamList[indexId].Controls.OfType<PictureBox>().ToList();

            if (hero != null && hero.Avatar != null)
            {
                avatar[0].ImageLocation = hero.Avatar;
                avatar[0].LoadAsync();
            }
            else
            {
                // 清除图像路径和资源
                avatar[0].Image = null;
                avatar[0].ImageLocation = null;
                // 重置显示状态
                avatar[0].Invalidate();
            }

        }

        private void FillListViewWithPlayerData(string thisName)
        {
            UpdateDebugBox($"FillListViewWithPlayerData() => thisName= {thisName}", false);
            var filteredLines = File.ReadLines(tbx_DocumentPath.Text)
                       .Where(line => line.Contains(thisName))
                       .Select(line => line.ToUpper());
            if (filteredLines.Count() > 0)
            {
                listView1.Items.Clear();
                foreach (string line in filteredLines)
                {
                    string[] lineSplit = line.Split('|');
                    if (lineSplit.Count() == 5)
                    {
                        ListViewItem lvi1 = new ListViewItem(lineSplit[0]);// 玩家名称
                        lvi1.SubItems.Add(lineSplit[1]); // 时间
                        lvi1.SubItems.Add(lineSplit[2]); // 游戏模式
                        lvi1.SubItems.Add(lineSplit[3]); // 战绩
                        lvi1.SubItems.Add(lineSplit[4]); // 违规类型
                        listView1.Items.Add(lvi1);
                    }
                }
            }
            else
            {
                listView1.Items.Clear();
            }
        }
        private void btn_Inquire_Click(object sender, EventArgs e)
        {
            if (File.Exists(tbx_DocumentPath.Text) && tbx_PlayerFullName.Text != string.Empty)
            {
                // 筛选包含关键字的行
                string thisName = Regex.Replace(tbx_PlayerFullName.Text, "[\u2066-\u2069]", string.Empty); // 去除不可见Unicode字符
                thisName = Regex.Replace(thisName, @"[\r\n]+", string.Empty); // 去除换行符  
                thisName = Regex.Replace(thisName, @" #", @"#"); // 去除#前面空格
                //UpdateDebugBox($"开始以<加入了队伍聊天><离开了队伍聊天>分割<thisName>");
                string[] splitChar = new[] { "加入了队伍聊天", "离开了队伍聊天" };
                string[] thisNames = thisName.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                thisNames = thisNames.Distinct().ToArray();
                //UpdateDebugBox($"分割<thisName>结束,thisNames.Length:{thisNames.Length}");
                //GetPlayerFromDoc(name);
                for (int index = 0; index < Math.Min(5, thisNames.Length); index++)
                {
                    ChangePlayerInfo(string.Empty, 1, index, null, thisNames[index], 0, 0, 0, 0);
                }
                if (thisNames.Length > 5)
                {
                    for (int index = 5; index < thisNames.Length; index++)
                    {
                        ChangePlayerInfo(string.Empty, 2, index - 5, null, thisNames[index], 0, 0, 0, 0);
                    }
                }
                Teammate_1.Checked = true;
                rbt_Teammate_CheckedChanged(Teammate_1, null);
                playerSelected = thisNames[0];
                UpdateDebugBox($"查询到玩家数量:[{thisNames.Length}]", true);
            }
        }

        private void tbx_PlayerFullName_TextChanged(object sender, EventArgs e)
        {
            ResetTeammateLabels();
            btn_Inquire.PerformClick();
        }

        private void btn_CopyToClipboard_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                string thisFullName = listView1.Items[0].SubItems[0].Text;
                string nameText = (thisFullName.Split('#'))[0];
                string text = $"[{nameText}]共战犯[{listView1.Items.Count}]次\n  *最近在[{listView1.Items[0].SubItems[1].Text}],{listView1.Items[0].SubItems[2].Text}模式,\n  *KDA:{listView1.Items[0].SubItems[3].Text},\n  *{listView1.Items[0].SubItems[4].Text}";
                Clipboard.SetText(text);
                UpdateDebugBox($"btn_CopyToClipboard_Click: 复制战绩到剪贴板, 共{listView1.Items.Count}条记录", true);

            }
            else
            {
                Clipboard.SetText($"未查询到过往对局中与玩家<{playerSelected}>相遇并且该玩家有异常行为。");
            }
        }

        private void btn_HideDebugBox_Click(object sender, EventArgs e)
        {

            if (btn_HideDebugBox.Text == "∧")
            {
                this.Height = 706;
                btn_HideDebugBox.Text = "∨";
            }
            else
            {
                this.Height = 880;
                btn_HideDebugBox.Text = "∧";
            }
        }
        private void rbt_Teammate_CheckedChanged(object sender, EventArgs e)
        {
            //UpdateDebugBox($"in methodes:<rbt_Teammate_CheckedChanged>");
            RadioButton thisRBT = sender as RadioButton;
            if (thisRBT.Checked)
            {
                foreach (RadioButton rb in rdbt_Players)
                {
                    if (thisRBT != rb)
                    {
                        rb.Checked = false;
                    }
                }
                Panel? parentPanel = thisRBT.Parent as Panel;
                //if(parentPanel!= null)UpdateDebugBox($"parentPanel:<{parentPanel.Name}>");
                foreach (Label label in parentPanel.Controls.OfType<Label>())
                {
                    if (thisRBT.Checked && label.Tag == "PlayerFullName" && label.Text != "默认召唤师名#12345")
                    {
                        UpdateDebugBox($"Select Player: {label.Text}", true);
                        playerSelected = label.Text;
                        FillListViewWithPlayerData(playerSelected);
                    }
                }
            }
        }
        public void rbt_Teammate_CheckedChangedDefault()
        {
            rbt_Teammate_CheckedChanged(Teammate_1, null);
        }
        private async Task SendRecordInGame()
        {
            foreach (RadioButton ctrlRBT in gbx_Players.Controls)
            {
                Label label = ctrlRBT.Parent.Controls.OfType<Label>().FirstOrDefault(l => l.Tag.ToString() == "PlayerFullName");
                if (label != null && label.ForeColor == Color.Red)
                {
                    List<string> contants = GetPlayerFromDoc(label.Text);
                    if (contants.Count > 0)
                    {
                        string contant = contants[0];
                        string[] lineSplit = contant.Split('|');
                        if (lineSplit.Count() == 5)
                        {
                            string nameText = (lineSplit[0].Split('#'))[0];
                            UpdateDebugBox($"SendRecordInGame: 发送玩家战绩<{nameText}>", true);
                            string text = $"[{nameText}]共战犯[{contants.Count}]次";//最近在[{lineSplit[1]}],{lineSplit[2]}模式,KDA:{lineSplit[3]},{lineSplit[4]}
                            await InGameSendMessage(text);
                            await Task.Delay(100);
                            text = $"最近在[{lineSplit[1]}],{lineSplit[2]}模式";
                            await InGameSendMessage(text);
                            await Task.Delay(100);
                            text = $"KDA:{lineSplit[3]},{lineSplit[4].Replace(',', '+')}";
                            await InGameSendMessage(text);
                        }
                    }
                }
            }
        }
        private async Task InGameSendMessage(string message)
        {
            await Simulate.Events()
                        .Click(KeyCode.Enter).Wait(75)
                        .Click(message).Wait(75)
                        .Click(KeyCode.Enter)
                        .Invoke();
        }
        #region 热键
        private List<string> selectKey = new List<string>();

        private string ParseKey(string key)
        {
            if (key.Contains("Control"))
            {
                key = "Control";
            }
            else if (key.Contains("Menu"))
            {
                key = "Alt";
            }
            return key;
        }

        private int ComputeHash(IEnumerable<string> source)
        {
            int hash = 0;
            foreach (var item in source)
            {
                hash += item.GetHashCode();
            }

            return hash;
        }

        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (selectKey.Contains(ParseKey(e.KeyCode.ToString())))
            {
                return;
            }
            else
            {
                selectKey.Add(ParseKey(e.KeyCode.ToString()));
            }

            long keyHash = 0;
            foreach (var key in selectKey)
            {
                var newKey = ParseKey(key);
                keyHash += newKey.Trim().GetHashCode();
            }

            if (keyHash == 0)
            {
                return;
            }

            if (keyHash == "F7".GetHashCode())
            {
                SendRecordInGame();
            }
            else if (keyHash == "F8".GetHashCode())
            {
            }
            else if (keyHash == "F11".GetHashCode())
            {
            }
            else if (keyHash == ComputeHash(new string[] { "Oemtilde", "F7" }))
            {
                SendRecordInGame();
            }
            else if (keyHash == ComputeHash(new string[] { "Oemtilde", "F8" }))
            {
            }
        }

        private async void OnKeyUp(object sender, KeyEventArgs e)
        {
            var key = ParseKey(e.KeyCode.ToString());
            if (selectKey.Contains(key))
            {
                for (int i = 0; i < selectKey.Count; i++)
                {
                    if (selectKey[i] == key)
                    {
                        selectKey.Remove(selectKey[i]);
                    }
                }
            }
        }
        #endregion
        private void FuckPlayersRecorderMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _lcuapi.UnsubscribeAllEvents();
        }
        private void adbtn_URF_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void FuckPlayersRecorderMainForm_Resize(object sender, EventArgs e)
        {
            debugBox.Height = this.Height - 721;
        }
        private void btn_ReloadGameCilent_Click(object sender, EventArgs e)
        {
            _lcuapi._gameService.AutoNewGameAsync();
        }
    }
}
