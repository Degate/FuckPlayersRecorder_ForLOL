using CommunityToolkit.Mvvm.ComponentModel;
using FuckPlayersRecorder;
using FuckPlayersRecorder_ForLOL.Event;
using FuckPlayersRecorder_ForLOL.Models;
using FuckPlayersRecorder_ForLOL.Service.Account;
using FuckPlayersRecorder_ForLOL.Service.Client;
using FuckPlayersRecorder_ForLOL.Service.Game;
using FuckPlayersRecorder_ForLOL.Service.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using Constant = FuckPlayersRecorder_ForLOL.Resources.Constant;

namespace FuckPlayersRecorder_ForLOL
{
    public class GameServiceForLCU : ObservableObject
    {
        private readonly string _cmdPath = @"C:\Windows\System32\cmd.exe";
        private readonly string _excuteShell = "WMIC PROCESS WHERE name=\"LeagueClientUx.exe\" GET commandline";
        private IRequestService _requestService = new DefaultRequestService();
        public IGameService _gameService = new DefaultGameService();
        private IAccountService _accountService = new DefaultAccountService();
        private IClientService _clientService;
        private FuckPlayersRecorderMainForm _mainForm;
        private IEventService _eventService = new DefaultEventService();
        private string  _gameStatus;
        private bool hasUpdateGameMode = true;
        private Account loginAccount = new Account();

        public List<Account> Team1Accounts { get; set; } = new List<Account>();
        public List<Account> Team2Accounts { get; set; } = new List<Account>();
        private bool lcuConnected;
        public bool LcuConnected
        {
            get => lcuConnected;
            set => SetProperty(ref lcuConnected, value);
        }
        public string GameStatus
        {
            get => _gameStatus;
            set => SetProperty(ref _gameStatus, value);
        }
        private Color _gameStatusColor;
        public Color GameStatusColor
        {
            get => _gameStatus switch
            {
                "连接成功" => Color.Green, // Green
                "断开连接" => Color.Red, // Red
                "尝试连接" => Color.Orange, // Orange
                _ => Color.Green // Green for unknown status
            };
            set => SetProperty(ref _gameStatusColor, value);
        }

        public void Initialize()
        {
            GameStatus = "未启动LCU API";
        }
        public async void Initialize(FuckPlayersRecorderMainForm _form)
        {
            Initialize();
            _mainForm = _form;
            await LUC_API_StatusInfo();
        }

        private bool IsLeagueClientUxRunning()
        {
            Process[] processesForLUx = Process.GetProcessesByName("LeagueClientUx");
            if (processesForLUx.Length == 0)
            {
                _mainForm.UpdateDebugBox($"LCU进程不存在，等待登录LOL客户端", false);
                GameStatus = "断开连接";
                LcuConnected = false;
            }
            return processesForLUx.Length > 0;
        }

        private async Task<string> LoadLCU_CMD()
        {
            Process getLCUPortTokenCMD = new Process();
            getLCUPortTokenCMD.StartInfo.FileName = _cmdPath;
            getLCUPortTokenCMD.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
            getLCUPortTokenCMD.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
            getLCUPortTokenCMD.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
            getLCUPortTokenCMD.StartInfo.RedirectStandardError = true; //重定向标准错误输出
            getLCUPortTokenCMD.StartInfo.CreateNoWindow = true; //不显示程序窗口
            getLCUPortTokenCMD.Start();
            getLCUPortTokenCMD.StandardInput.WriteLine(_excuteShell.TrimEnd('&') + "&exit");
            getLCUPortTokenCMD.StandardInput.AutoFlush = true;
            string output = await getLCUPortTokenCMD.StandardOutput.ReadToEndAsync();
            getLCUPortTokenCMD.WaitForExit();
            getLCUPortTokenCMD.Close();

            return output;
        }
        private async Task LUC_API_StatusInfo()
        {
            while (true)
            {
                if (IsLeagueClientUxRunning()&& !LcuConnected)
                {
                    string output = await LoadLCU_CMD();
                    if (!string.IsNullOrEmpty(output) && output.Contains("--remoting-auth-token="))
                    {
                        _mainForm.UpdateDebugBox("捕获到命令行信息", false);
                        var tokenResults = output.Split("--remoting-auth-token=");
                        var portResults = output.Split("--app-port=");
                        var PidResults = output.Split("--app-pid=");
                        var installLocations = output.Split("--install-directory=");
                        Constant.Token = tokenResults[1].Substring(0, tokenResults[1].IndexOf("\""));
                        Constant.Port = int.TryParse(portResults[1].Substring(0, portResults[1].IndexOf("\"")), out var temp) ? temp : 0;
                        Constant.Pid = int.TryParse(PidResults[1].Substring(0, PidResults[1].IndexOf("\"")), out var temp1) ? temp1 : 0;
                        if (!string.IsNullOrEmpty(Constant.Token) || Constant.Port != 0)
                        {
                            _mainForm.UpdateDebugBox($"捕获到端口秘钥信息：Token:{Constant.Token},Port:{Constant.Port}", true);
                            _requestService = new DefaultRequestService();
                            await _requestService.Initialize(Constant.Port, Constant.Token);
                            _mainForm.UpdateDebugBox($"建立请求服务", false);
                            await _accountService.Initialize(_requestService);
                            _mainForm.UpdateDebugBox($"建立账号服务", false);
                            string thisAccountInfo;
                            while (true)
                            {
                                try
                                {
                                    thisAccountInfo = await _accountService.GetUserAccountInformationAsync();
                                    if (string.IsNullOrEmpty(JToken.Parse(thisAccountInfo)["gameName"].ToString()))
                                        continue;
                                    else
                                    {
                                        _mainForm.UpdateDebugBox($"thisAccountInfo:\n{thisAccountInfo}",false);
                                        break; // 成功获取账号信息后跳出循环
                                    }
                                        
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(2000);
                                    continue;
                                }
                            }
                            await _gameService.Initialize(_requestService);
                            _mainForm.UpdateDebugBox($"建立游戏服务", true);
                            await _eventService.Initialize(Constant.Port, Constant.Token);
                            _mainForm.UpdateDebugBox($"建立事件服务", true);
                            _clientService = new DefaultClientService(_requestService);
                            _mainForm.UpdateDebugBox($"建立客户端服务", true);
                            await _eventService.ConnectAsync();
                            _eventService.Subscribe(Constant.ChampSelect, new EventHandler<EventArgument>(ChampSelect));
                            _mainForm.UpdateDebugBox($"订阅英雄选择事件", true);
                            _eventService.Subscribe(Constant.GameFlow, new EventHandler<EventArgument>(GameFlow));
                            _mainForm.UpdateDebugBox($"订阅客户端状态变更事件", true);
                            await LoadHerosConfig();//加载英雄数据
                            _mainForm.UpdateDebugBox($"加载游戏端英雄数据", true);
                            loginAccount.AccountId = (long)JToken.Parse(thisAccountInfo)["accountId"];
                            loginAccount.Puuid = (string?)JToken.Parse(thisAccountInfo)["puuid"];
                            loginAccount.SummonerId = (long)JToken.Parse(thisAccountInfo)["summonerId"];
                            _mainForm.UpdateDebugBox($"当前账号信息：\n  完整账号名称：{JToken.Parse(thisAccountInfo)["gameName"].ToString()}#{JToken.Parse(thisAccountInfo)["tagLine"].ToString()}\n" +
                                                        $"  summonerId：{JToken.Parse(thisAccountInfo)["summonerId"].ToString()}\n" +
                                                        $"  puuid：{JToken.Parse(thisAccountInfo)["puuid"].ToString()}", true);
                            GameStatus = "连接成功";
                            await UpdateUIInfoAsync(_mainForm.RestMainFormUI);
                            Team1Accounts.Clear();
                            Team2Accounts.Clear();
                            LcuConnected = true;
                        }
                        else
                        {
                            GameStatus = "断开连接";
                            _mainForm.UpdateDebugBox($"{GameStatus}-- output has no port and token", true);
                            LcuConnected = true;
                        }
                    }
                    else
                    {
                        LcuConnected = false;
                        GameStatus = "尝试连接";
                        _mainForm.UpdateDebugBox($"{GameStatus}-- LOL UX Precess exist but no output", true);
                    }
                }
                await Task.Delay(2000);
            }
        }
        private static string GetRandomPUUID()
        {
            var random = new Random();
            string chars = "abcdefghijklmnopqrstuvwxyzQWRTYUIOPASDFGHJKLZXCVBNM0123456789";
            StringBuilder output = new StringBuilder();
            for (int i = 1; i <= 12; i++)
            {
                output.Append(chars[random.Next(0, chars.Length)]);
            }
            return output.ToString();
        }
        #region async methods

        public void UnsubscribeAllEvents()
        {
            _eventService.UnsubscribeAll();
        }
        private async Task UpdateDebugBoxAsync(string text, bool appendToDebugBox)
        {
            await UpdateUIInfoAsync(() => _mainForm.UpdateDebugBox(text, appendToDebugBox));
        }
        private void UpdateGameStatusAsync(string text)
        {
            _mainForm.BeginInvoke((MethodInvoker)delegate
            {
                _mainForm.UpdateLCUStatus(text);
            });
        }
        private Task UpdateUIInfoAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            _mainForm.Invoke((MethodInvoker)(() =>
             {
                 try
                 {
                     action();
                     tcs.SetResult(true); // 标记完成
                 }
                 catch (Exception ex)
                 {
                     tcs.SetException(ex); // 传递异常
                 }
             }));
            return tcs.Task;
        }
        private void UptateGameModeAsync(string mode)
        {
            _mainForm.BeginInvoke((MethodInvoker)delegate
            {
                _mainForm.UpdateGameMode(mode);
            });
        }
        #endregion
        #region 各种事件

        private async void GameFlow(object obj, EventArgument @event)
        {
            var data = $"{@event.Data}";
            if (string.IsNullOrEmpty(data))
                return;
            await UpdateDebugBoxAsync($"客户端状态变更:{data}", false);
            switch (data)
            {
                case "None":
                    GameStatus = "主页";
                    UpdateGameStatusAsync(GameStatus);
                    break;
                case "Lobby":
                    GameStatus = "房间中";
                    UpdateGameStatusAsync(GameStatus);
                    break;
                case "Matchmaking":
                    GameStatus = "匹配中";
                    UpdateGameStatusAsync(GameStatus);
                    break;
                case "ReadyCheck":
                    GameStatus = "找到对局";
                    try { await AutoAcceptAsync(); } catch { }
                    break;
                case "ChampSelect":
                    GameStatus = "英雄选择中";
                    UpdateGameStatusAsync(GameStatus);
                    await ChampSelectAsync();//写读队友列表功能--还没写--官方已不公开聊天相关API
                    break;
                case "GameStart":
                    GameStatus = "游戏开始";
                    UpdateGameStatusAsync(GameStatus);
                    break;

                case "InProgress":
                    GameStatus = "对局中";
                    UpdateGameStatusAsync(GameStatus);
                    await UpdateUIInfoAsync(_mainForm.RestMainFormUI);
                    try { await GetAccountsFromGameInfo_ARAM(); } 
                        catch (Exception ex) 
                        { _mainForm.UpdateDebugBox($"GetAccountsFromGameInfo_ARAM() Error:{ex.Message}", true); }                    
                    //LoopLiveGameEventAsync();
                    break;
                case "WaitingForStats":
                    GameStatus = "等待结算";
                    UpdateGameStatusAsync(GameStatus);
                    break;
                case "PreEndOfGame":
                    break;
                case "EndOfGame":
                    GameStatus = "对局结束";
                    await ActionWhenGameEnd();
                    UpdateGameStatusAsync(GameStatus);
                    break;
                case "Reconnect":
                    break;
                default:
                    GameStatus = "未知状态";
                    UpdateGameStatusAsync(GameStatus);
                    await UpdateDebugBoxAsync($"未知状态:\n{data}", true);
                    break;
            }
        }

        private async void ChampSelect(object obj, EventArgument @event)
        {
            //UpdateDebugBoxAsync("英雄选择界面");
            //    try
            //    {
            //        var gInfo = await _gameService.GetCurrentGameInfoAsync();
            //        var mode = JToken.Parse(gInfo)["gameData"]["queue"]["gameMode"].ToString();
            //        var myData = JObject.Parse(@event.Data.ToString());
            //        int playerCellId = int.Parse(@event.Data["localPlayerCellId"].ToString());

            //        if ((bool)@event.Data["allowSkinSelection"] == true)
            //        {
            //            await _runeViewModel.GetCurrentChampionColorSkins();
            //        }

            //        IEnumerable<Team> teams = JsonConvert.DeserializeObject<IEnumerable<Team>>(@event.Data["myTeam"].ToString());
            //        var me = teams.FirstOrDefault(x => x.CellId == playerCellId);
            //        if (me == null)
            //            return;

            //        if (mode == "ARAM")
            //        {
            //            await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            //            {
            //                if (me.ChampionId != default)
            //                    await _runeViewModel.LoadChampInfoAsync(me.ChampionId, true);
            //            });

            //            if (_iniSettingsModel.AutoLockHeroInAram) //秒抢大乱斗英雄
            //            {
            //                var session = await _gameService.GetGameSessionAsync();
            //                var token = JToken.Parse(session);
            //                BenchChampion[] champs = token["benchChampions"]?.ToObject<BenchChampion[]>();
            //                var loc = _iniSettingsModel.LockHerosInAram.IndexOf(me.ChampionId);
            //                loc = loc == -1 ? _iniSettingsModel.LockHerosInAram.Count : loc;
            //                if (loc != 0)
            //                {
            //                    var heros = _iniSettingsModel.LockHerosInAram.Take(loc);
            //                    var swapHeros = new List<int>();
            //                    foreach (var item in heros)
            //                    {
            //                        if (champs.Select(x => x.ChampionId).ToList().Contains(item))
            //                        {
            //                            swapHeros.Add(item);
            //                        }
            //                    }

            //                    for (var index = swapHeros.Count - 1; index >= 0; index--)
            //                    {
            //                        await _gameService.BenchSwapChampionsAsync(swapHeros[index]);
            //                    }
            //                }
            //            }
            //            {
            //                var session = await _gameService.GetGameSessionAsync();
            //                var token = JToken.Parse(session);
            //                BenchChampion[] champs = token["benchChampions"]?.ToObject<BenchChampion[]>();
            //                BenchChampion[] chooseChamps = token["myTeam"]?.ToObject<BenchChampion[]>();
            //                WeakReferenceMessenger.Default.Send(new AramChooseHeroModel(chooseChamps.Select(x => x.ChampionId).ToList(), champs.Select(x => x.ChampionId).ToList()));
            //            }
            //        }
            //        else
            //        {
            //            foreach (var action in @event.Data["actions"])
            //            {
            //                foreach (var actionItem in action)
            //                {
            //                    if (int.Parse(actionItem["actorCellId"].ToString()) == playerCellId)
            //                    {
            //                        if (actionItem["type"] == "pick")
            //                        {
            //                            foreach (var teamPlayer in myData["myTeam"])
            //                            {
            //                                if (teamPlayer["cellId"] == playerCellId)
            //                                {
            //                                    int champ = teamPlayer["championId"];
            //                                    if (int.Parse((string)actionItem["championId"]) != 0 && champ != 0)
            //                                    {
            //                                        await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            //                                        {
            //                                            await _runeViewModel.LoadChampInfoAsync(champ, false);
            //                                        });
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex.ToString());
            //    }
        }

        //获取游戏内实时的一些数据，目前只是获取选择的英雄和召唤师技能
        private void LoopLiveGameEventAsync()
        {
           // UpdateDebugBoxAsync("trig LoopLiveGameEventAsync", true);
            //var item1 = false;
            //var item2 = false;
            //var _ = Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        try
            //        {
            //            var gInfo = await _gameService.GetCurrentGameInfoAsync();
            //            if (Team1Accounts.Count <= 0 && Team2Accounts.Count <= 0)
            //            {
            //                await ActionWhenGameBegin();
            //            }
            //            else if (Team1Accounts.All(x => x?.Champion == null) && Team2Accounts.All(x => x?.Champion == null))
            //            {
            //                var teams1 = await _livegameservice.GetPlayersAsync(100);
            //                var teams2 = await _livegameservice.GetPlayersAsync(200);
            //                if (!string.IsNullOrEmpty(teams1) && !string.IsNullOrEmpty(teams2))
            //                {
            //                    var token1 = JArray.Parse(teams1);
            //                    var token2 = JArray.Parse(teams2);

            //                    foreach (var item in token1)
            //                    {
            //                        var name = item["summonerName"].ToObject<string>();
            //                        var account = (Team1Accounts.Concat(Team2Accounts)).FirstOrDefault(x => x?.DisplayName == name);
            //                        if (account == null)
            //                            continue;

            //                        var championName = item["championName"].ToObject<string>();
            //                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //                        {
            //                            account.Champion = Constant.Heroes.FirstOrDefault(x => x.Label == championName);
            //                        });
            //                        item1 = true;
            //                    }

            //                    foreach (var item in token2)
            //                    {
            //                        var name = item["summonerName"].ToObject<string>();
            //                        var account = (Team1Accounts.Concat(Team2Accounts)).FirstOrDefault(x => x?.DisplayName == name);
            //                        if (account == null)
            //                            continue;
            //                        var championName = item["championName"].ToObject<string>();
            //                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //                        {
            //                            account.Champion = Constant.Heroes.FirstOrDefault(x => x.Label == championName);
            //                        });

            //                        item2 = true;
            //                    }
            //                }
            //            }

            //            if (item2 && item1)
            //            {
            //                break;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            await Task.Delay(5000);
            //            break;
            //        }
            //    }
            //});
        }
        #endregion
        private async Task AutoAcceptAsync()
        {
            await UpdateDebugBoxAsync("trig AutoAcceptAsync", true);
            var gameInformation = await _gameService.GetCurrentGameInfoAsync();
            await UpdateDebugBoxAsync($"GameStatus:[{GameStatus}], GameID:{Constant.gameId.ToString()}, gameInformation:\n{gameInformation}", false);
            if(_mainForm.cbx_AutoAcceptMatch.Checked)
            {
                await UpdateDebugBoxAsync("5秒后自动接受对局", true);
                await Task.Delay(5000);
                await _gameService.AutoAcceptGameAsync();
            }
        }
        private async Task GetPreteams(List<Account> accounts)
        {
            string teamMark = accounts.Any(account => account.Puuid == loginAccount.Puuid) ? "我方" : "敌方";
            var TeamParticipantIdlist = accounts.Where(x => x.TeamParticipantId != null && x.TeamParticipantId != 0)
                                                .Select(x => x.TeamParticipantId)
                                                .Distinct().ToList();
            string text = $"[{teamMark}] => ";
            if(TeamParticipantIdlist.Count == 5)
            {
                await UpdateUIInfoAsync(() => _mainForm.UpdateDebugBox($"{teamMark}无开黑小队", true));
            }
            foreach (var teamParticipantId in TeamParticipantIdlist)
            {
                var thisPreTeam = accounts.Where(x => x.TeamParticipantId == teamParticipantId).ToList();
                if (thisPreTeam.Count > 1)
                {
                    text += $"开黑小队:";
                    foreach(Account account in accounts)
                    {
                        if (account.TeamParticipantId == teamParticipantId)
                            text += $"GameName:[{account.GameName}#{account.TagLine}]+";
                    }
                    text = text.TrimEnd('+');
                }
                else break;
                await UpdateUIInfoAsync(() => _mainForm.UpdateDebugBox(text, true));
            }
        }
        private async Task GetAccountsFromGameInfo_ARAM()
        {
            await UpdateDebugBoxAsync("trig GetAccountsFromCurrentGameInfo", false);
            Team1Accounts.Clear();
            Team2Accounts.Clear();
            var gameInformation = await _gameService.GetCurrentGameInfoAsync();
            //game data
            var gameData = JToken.Parse(gameInformation)["gameData"];
            var gameId = gameData["gameId"] != null ? long.Parse(gameData["gameId"].ToString()):0;
            Constant.gameId = gameId;
            await UpdateDebugBoxAsync($"GameStatus:[{GameStatus}], GameID:{Constant.gameId.ToString()}, gameInformation:\n{gameInformation}", false);

            try
            {
                //teams
                var t1 = gameData["teamOne"].ToObject<IEnumerable<Teammate>>();
                var t2 = gameData["teamTwo"].ToObject<IEnumerable<Teammate>>();

                await UpdateDebugBoxAsync($"GetAccountsFromGameInfo_ARAM() => teamOne.count:[{t1.ToArray().Length}], teamTwo.count:[{t2.ToArray().Length}]", false);
                foreach (Teammate teammate in t1)
                {
                    Account thisPlyaerAccount = new Account();
                    await UpdateDebugBoxAsync($"foreach(t1) => teammate.ChampionId:[{teammate.ChampionId}], teammate.Puuid:[{teammate.Puuid}], teammate.SummonerName:[{teammate.SummonerName}], teammate.SummonerId:[{teammate.SummonerId}]", false);
                    if (teammate.SummonerId == 0)
                    {
                        thisPlyaerAccount.SummonerName = Constant.Heroes.FirstOrDefault(x => x.ChampId == teammate.ChampionId).Alias;
                        thisPlyaerAccount.Puuid = GetRandomPUUID();
                        thisPlyaerAccount.FullName = Constant.Heroes.FirstOrDefault(x => x.ChampId == teammate.ChampionId).Name;
                    }
                    else
                    {
                        var thisPlayerInfo = await _accountService.GetSummonerInformationAsync(teammate.SummonerId);
                        //UpdateDebugBoxAsync($"thisPlayerInfo:{thisPlayerInfo}");
                        thisPlyaerAccount = thisPlayerInfo == null ? new Account() : JsonConvert.DeserializeObject<Account>(thisPlayerInfo);
                    }
                    thisPlyaerAccount.Champion = Constant.Heroes.FirstOrDefault(x => x.ChampId == teammate.ChampionId);
                    string text = $"result => Team one : Hero:{thisPlyaerAccount.Champion.Name}, FullName:{thisPlyaerAccount.FullName},Puuid:{thisPlyaerAccount.Puuid},TeamParticipantId:{thisPlyaerAccount.TeamParticipantId}";
                    Team1Accounts.Add(thisPlyaerAccount);
                    await UpdateDebugBoxAsync(text, false);
                }
                await GetPreteams(Team1Accounts);
                await UpdateDebugBoxAsync($"teamTwo.count:{t2.ToArray().Length}", false);
                foreach (Teammate teammate in t2)
                {
                    Account thisPlyaerAccount = new Account();
                    await UpdateDebugBoxAsync($"foreach(t2) => teammate.ChampionId:[{teammate.ChampionId}], teammate.Puuid:[{teammate.Puuid}], teammate.SummonerName:[{teammate.SummonerName}], teammate.SummonerId:[{teammate.SummonerId}]", false);
                    if (teammate.SummonerId == 0)
                    {
                        thisPlyaerAccount.SummonerName = Constant.Heroes.FirstOrDefault(x => x.ChampId == teammate.ChampionId).Alias;
                        thisPlyaerAccount.Puuid = GetRandomPUUID();
                        thisPlyaerAccount.FullName = Constant.Heroes.FirstOrDefault(x => x.ChampId == teammate.ChampionId).Name;
                    }
                    else
                    {
                        var thisPlayerInfo = await _accountService.GetSummonerInformationAsync(teammate.SummonerId);
                        //UpdateDebugBoxAsync($"thisPlayerInfo:{thisPlayerInfo}");
                        thisPlyaerAccount = thisPlayerInfo == null ? new Account() : JsonConvert.DeserializeObject<Account>(thisPlayerInfo);
                    }
                    thisPlyaerAccount.Champion = Constant.Heroes.FirstOrDefault(x => x.ChampId == teammate.ChampionId);
                    string text = $"result => Team two : Hero:{thisPlyaerAccount.Champion.Name}, FullName:{thisPlyaerAccount.FullName},Puuid:{thisPlyaerAccount.Puuid},TeamParticipantId:{thisPlyaerAccount.TeamParticipantId}";
                    Team2Accounts.Add(thisPlyaerAccount);
                    await UpdateDebugBoxAsync(text, false);
                }
                await GetPreteams(Team2Accounts);
                await UpdateDebugBoxAsync($"TeamOne.count:[{Team1Accounts.Count}], TeamTwo.count:{Team2Accounts.Count}", false);

                await UpdateUIInfoAsync(() => _mainForm.UpdatePlayerInfo(Team1Accounts, Team2Accounts));

            }catch(Exception ex)
            {
                await UpdateDebugBoxAsync($"GetAccountsFromGameInfo_ARAM() error:{ex.Message}", true);
            }
        }
        private async Task ChampSelectAsync()
        {
            try
            {
                if (hasUpdateGameMode)
                {
                    await UpdateDebugBoxAsync("trig ChampSelectAsync", false);
                    var gameInformation = await _gameService.GetCurrentGameInfoAsync();
                    //game data
                    var gameData = JToken.Parse(gameInformation)["gameData"];
                    var gameId = long.Parse(gameData["gameId"].ToString());
                    Constant.gameId = gameId;
                    //map queue
                    var queue = gameData["queue"];
                    Constant.currentQueueId = int.Parse(queue["id"].ToString());
                    Constant.currentQueueMapId = int.Parse(queue["mapId"].ToString());
                    Constant.currentQueueName = queue["name"].ToString();
                    Constant.currentQueueGameMode = queue["gameMode"].ToString();
                    Constant.currentQueueShortName = queue["shortName"].ToString();
                    await UpdateDebugBoxAsync($"Current Queue => \n   Id:{Constant.currentQueueId}, " +
                                                            $"\n   MapId:{Constant.currentQueueMapId}," +
                                                            $"\n   Name:{Constant.currentQueueName}," +
                                                            $"\n   GameMode:{Constant.currentQueueGameMode}," +
                                                            $"\n    ShortName:{Constant.currentQueueShortName}", false);

                    var map = JToken.Parse(gameInformation)["map"];
                    Constant.currentMapId = int.Parse(map["id"].ToString());
                    Constant.currentMapmapStringId = map["mapStringId"].ToString();
                    Constant.currentMapGameMode = map["gameMode"].ToString();
                    Constant.currentMapGameModeName = map["gameModeName"].ToString();
                    await UpdateDebugBoxAsync($"Current map => \n   Id:{Constant.currentMapId}, " +
                                                    $"\n   mapStringId:{Constant.currentMapmapStringId}," +
                                                    $"\n   GameMode:{Constant.currentMapGameMode}," +
                                                    $"\n   GameModeName:{Constant.currentMapGameModeName}", true);
                    UptateGameModeAsync(Constant.currentMapGameModeName);
                    hasUpdateGameMode = false;
                }
            }
            catch { }
        }
        private async Task LoadHerosConfig()
        {
            using (var client = new HttpClient())
            {
                var heros = await client.GetStringAsync("https://game.gtimg.cn/images/lol/act/img/js/heroList/hero_list.js");
                Constant.Heroes = JToken.Parse(heros)["hero"].ToObject<IEnumerable<Hero>>();
            }
        }
        private async Task ActionWhenGameEnd()
        {
            await UpdateDebugBoxAsync($"对局结束：gameId:{Constant.gameId}", true);
            hasUpdateGameMode = true;
            if (Team1Accounts.Count == 0 && Team2Accounts.Count == 0)// && Constant.currentMapGameMode == "ARAM"
            {
                await GetAccountsFromGameInfo_ARAM();
            }

            var gameDetails = JToken.Parse(await _gameService.QueryGameDetailAsync(Constant.gameId));
            await UpdateDebugBoxAsync($"GameStatus:[{GameStatus}], GameID:{Constant.gameId.ToString()}, gameDetails:\n{gameDetails.ToString()}", false);

            if (!string.IsNullOrEmpty(gameDetails.ToString()))
            {
                List<Account> teamAccountsAll = new List<Account>();
                teamAccountsAll.AddRange(Team1Accounts);
                teamAccountsAll.AddRange(Team2Accounts);
                var participantIdentities = gameDetails["participantIdentities"];
                await UpdateDebugBoxAsync($"participantIdentities:\n{participantIdentities}", false);
                foreach (Account account in teamAccountsAll)
                {
                    await UpdateDebugBoxAsync($"foreach(teamAccountsAll) => account.Puuid:{account.Puuid}", false);
                    int participantId = 0;
                    if (participantIdentities != null && account.Puuid != null)
                    {
                        if (account.Puuid.Length == 12)
                        {
                            await UpdateDebugBoxAsync($"this account is a bot:{account.SummonerName}", false);
                            foreach (var participantIdentitie in participantIdentities)
                            {
                                await UpdateDebugBoxAsync($"foreach(participantIdentities) => participantIdentitie[\"player\"][\"summonerName\"]:{participantIdentitie["player"]["summonerName"].ToString()}", false);
                                if (participantIdentitie["player"]["summonerName"].ToString() == account.Champion.Alias)
                                {
                                    participantId = participantIdentitie["participantId"] == null ? 0 : (int)participantIdentitie["participantId"];
                                    await UpdateDebugBoxAsync($"matched participantIdentitie => \n    participantIdentitie.player.summonerName:{participantIdentitie["player"]["summonerName"].ToString()}\n" +
                                                                                                    $"  this account.puuid:{account.Puuid}" +
                                                                                                    $"  this participantId:{participantId}", false);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            await UpdateDebugBoxAsync($"this account is real player => Puuid:{account.Puuid}, full name:{account.FullName}", false);
                            foreach (var participantIdentitie in participantIdentities)
                            {
                                if (participantIdentitie["player"]["puuid"].ToString() == account.Puuid)
                                {
                                    participantId = participantIdentitie["participantId"] == null ? 0 : (int)participantIdentitie["participantId"];
                                    await UpdateDebugBoxAsync($"matched participantIdentitie => \n    participantIdentitie.player.puuid:{participantIdentitie["player"]["puuid"].ToString()}\n" +
                                                                                                $"  this account.fullName:{account.FullName}" +
                                                                                                $"  this account.puuid:{account.Puuid}" +
                                                                                                $"  this participantId:{participantId}", false);
                                    break;
                                }
                                else
                                {
                                    await UpdateDebugBoxAsync("account.Puuid = null", false);
                                    continue;
                                }
                            }
                        }
                        await UpdateDebugBoxAsync($"participantId = {participantId}", false);
                        if (participantId != 0)
                        {
                            account.Kill = (int)gameDetails["participants"]
                                                .First(p => (int)p["participantId"] == participantId)?["stats"]["kills"];
                            account.Assist = (int)gameDetails["participants"]
                                                .First(p => (int)p["participantId"] == participantId)?["stats"]["assists"];
                            account.Death = (int)gameDetails["participants"]
                                                .First(p => (int)p["participantId"] == participantId)?["stats"]["deaths"];
                            account.Damage = (double)gameDetails["participants"]
                                                .First(p => (int)p["participantId"] == participantId)?["stats"]["totalDamageDealtToChampions"];
                            await UpdateDebugBoxAsync($"{account.FullName} KDA:{account.Kill}/{account.Death}/{account.Assist},TotalDamage:{account.Damage}", false);
                        }
                    }
                    await UpdateDebugBoxAsync($"foreach (teamAccountsAll) end of item, going next", false);
                }
            }
            await UpdateUIInfoAsync(() => _mainForm.UpdatePlayerInfoGameEnd(Team1Accounts, Team2Accounts));
        }
    }
}
