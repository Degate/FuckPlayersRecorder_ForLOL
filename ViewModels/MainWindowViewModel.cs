﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Gma.System.MouseKeyHook;
//using HandyControl.Controls;
//using HandyControl.Data;
//using FuckPlayersRecorder_ForLOL.Client;
using FuckPlayersRecorder_ForLOL.Event;
using FuckPlayersRecorder_ForLOL.Models;
using FuckPlayersRecorder_ForLOL.Resources;
using FuckPlayersRecorder_ForLOL.ViewModels;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using WindowsInput;
using WindowsInput.Events;
using WindowsInput.EventSources;
using Account = FuckPlayersRecorder_ForLOL.Models.Account;
using Teammate = FuckPlayersRecorder_ForLOL.Models.Teammate;
using FuckPlayersRecorder_ForLOL.Service.Request;
using FuckPlayersRecorder_ForLOL.Service.Account;
using FuckPlayersRecorder_ForLOL.Service.LiveGame;
using FuckPlayersRecorder_ForLOL.Service.Game;
using FuckPlayersRecorder_ForLOL.Service.Teamup;

namespace FuckPlayersRecorder_ForLOL.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly string _cmdPath = @"C:\Windows\System32\cmd.exe";
        private readonly string _excuteShell = "WMIC PROCESS WHERE name=\"LeagueClientUx.exe\" GET commandline";
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public RelayCommand ShiftSettingsPageCommand { get; set; }
        public RelayCommand ShiftMainPageCommand { get; set; }
        public RelayCommand ShiftNoticePageCommand { get; set; }
        public RelayCommand ShiftRankPageCommand { get; set; }
        public RelayCommand OpenChampionSelectToolCommand { get; set; }
        public RelayCommand ShiftTeamupPageCommand { get; set; }
        public RelayCommand OpenTeamDetailCommand { get; set; }
        public AsyncRelayCommand ResetCommandAsync { get; set; }
        public RelayCommand ExitCommand { get; set; }

        //private Page _currentPage;
        //public Page CurrentPage
        //{
        //    get => _currentPage;
        //    set => SetProperty(ref _currentPage, value);
        //}

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => SetProperty(ref _connected, value);
        }

        private string gameStatus;
        public string GameStatus
        {
            get => gameStatus;
            set => SetProperty(ref gameStatus, value);
        }

        private string unReadNotices;
        public string UnReadNotices
        {
            get => unReadNotices;
            set => SetProperty(ref unReadNotices, value);
        }

        private int _onlineCounts;
        public int OnlineCounts
        {
            get => _onlineCounts;
            set => SetProperty(ref _onlineCounts, value);
        }


        private bool _isLoop = false;
        private bool _isLoopChampionSelect = false;
        private bool _isLoopLive = false;
        private IKeyboardMouseEvents _keyboardMouseEvent;
        public List<Models.Account> Team1Accounts { get; set; } = new List<Models.Account>();
        public List<Models.Account> Team2Accounts { get; set; } = new List<Models.Account>();
        //private readonly IApplicationService _applicationService;
        private readonly IRequestService _requestService;
        private readonly IGameService _gameService;
        private readonly ITeamupService _teamupService;
        //private readonly IClientService _clientService;
        private readonly IAccountService _accountService;
        private readonly IEventService _eventService;
        //private readonly InjectDllHelper _dllHelper;
        //private readonly IniSettingsModel _iniSettingsModel;
        //private readonly IConfiguration _configuration;
        //private readonly Settings _settingsPage;
        //private readonly Teamup _teamup;
        //private readonly MainPage _mainPage;
        //private readonly RecordRank _recordRank;
        //private readonly LeagueOfLegendsBoxer.Pages.Notice _notice;
        //private readonly ChampionSelectTool _championSelectTool;
        //private readonly ILogger<MainWindowViewModel> _logger;
        //private readonly ImageManager _imageManager;
        //private readonly RuneAndItemViewModel _runeViewModel;
        private readonly ILiveGameService _livegameservice;
        //private readonly TeammateViewModel _teammateViewModel;
        //private readonly SettingsViewModel _settingsViewModel;
        //private readonly Team1V2Window _team1V2Window;
        //private readonly BlackList _blackList;
        //private readonly HtmlHelper _htmlHelper;

        //public MainWindowViewModel(IApplicationService applicationService,
        //                           IClientService clientService,
        //                           IRequestService requestService,
        //                           IEventService eventService,
        //                           ITeamupService teamupService,
        //                           IGameService gameService,
        //                           IAccountService accountService,
        //                           IniSettingsModel iniSettingsModel,
        //                           IConfiguration configuration,
        //                           Settings settingsPage,
        //                           MainPage mainPage,
        //                           RecordRank recordRank,
        //                           Teamup teamup,
        //                           ImageManager imageManager,
        //                           RuneAndItemViewModel runeViewModel,
        //                           SettingsViewModel settingsViewModel,
        //                           ChampionSelectTool championSelectTool,
        //                           ILogger<MainWindowViewModel> logger,
        //                           ILiveGameService livegameservice,
        //                           TeammateViewModel teammateViewModel,
        //                           BlackList blackList,
        //                           HtmlHelper htmlHelper,
        //                           InjectDllHelper dllHelper,
        //                           LeagueOfLegendsBoxer.Pages.Notice notice,
        //                           Team1V2Window team1V2Window)
        //{
        //    LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
        //    ShiftSettingsPageCommand = new RelayCommand(OpenSettingsPage);
        //    ShiftMainPageCommand = new RelayCommand(OpenMainPage);
        //    OpenChampionSelectToolCommand = new RelayCommand(OpenChampionSelectTool);
        //    OpenTeamDetailCommand = new RelayCommand(OpenTeamDetail);
        //    ResetCommandAsync = new AsyncRelayCommand(ResetAsync);
        //    ShiftNoticePageCommand = new RelayCommand(OpenNoticePage);
        //    ShiftTeamupPageCommand = new RelayCommand(ShiftTeamupPage);
        //    ShiftRankPageCommand = new RelayCommand(OpenRankPage);
        //    ExitCommand = new RelayCommand(() => { App.ServiceProvider.GetRequiredService<MainWindow>().Close(); Environment.Exit(0); });
        //    _applicationService = applicationService;
        //    _requestService = requestService;
        //    _clientService = clientService;
        //    _teamupService = teamupService;
        //    _accountService = accountService;
        //    _iniSettingsModel = iniSettingsModel;
        //    _configuration = configuration;
        //    _settingsPage = settingsPage;
        //    _recordRank = recordRank;
        //    _teamup = teamup;
        //    _notice = notice;
        //    _championSelectTool = championSelectTool;
        //    _mainPage = mainPage;
        //    _eventService = eventService;
        //    _settingsViewModel = settingsViewModel;
        //    _gameService = gameService;
        //    _logger = logger;
        //    _htmlHelper = htmlHelper;
        //    _blackList = blackList;
        //    _runeViewModel = runeViewModel;
        //    _imageManager = imageManager;
        //    _dllHelper = dllHelper;
            //GameStatus = "获取状态中";
            //_livegameservice = livegameservice;
            //_teammateViewModel = teammateViewModel;
            //_team1V2Window = team1V2Window;
            //_keyboardMouseEvent = Hook.GlobalEvents();
            //_keyboardMouseEvent.KeyDown += OnKeyDown;
            //_keyboardMouseEvent.KeyUp += OnKeyUp;
            //WeakReferenceMessenger.Default.Register<MainWindowViewModel, IEnumerable<Notice>>(this, (x, y) =>
            //{
            //    UnReadNotices = y.FirstOrDefault(x => x.IsMust) != null ? "必读" + y.Where(x => x.IsMust).Count() : y.Count().ToString();
            //});
        //}


        /// <summary>
        /// send 开黑信息
        /// </summary>
        /// <param name="Keyboard"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListenerTeamBuildInfo_Triggered(IKeyboardEventSource Keyboard, object sender, KeyChordEventArgs e)
        {
            var myTeam = Team1Accounts.FirstOrDefault(x => x.SummonerId == Constant.Account.SummonerId) == null ? Team2Accounts : Team1Accounts;
            var otherTeam = Team1Accounts.FirstOrDefault(x => x.SummonerId == Constant.Account.SummonerId) == null ? Team1Accounts : Team2Accounts;
            foreach (var item in myTeam.GroupBy(x => x.TeamID))
            {
                if (item.Count() >= 2)
                {
                    if (item.FirstOrDefault()?.Champion != null)
                    {
                        var sb = new StringBuilder();
                        sb.Append("我方开黑:[");
                        sb.Append(string.Join(",", item.Select(x => x.Champion?.Label)));
                        sb.Append("]");
                        await Task.Delay(300);
                        await InGameSendMessage(sb.ToString());
                    }
                    else 
                    {
                        var sb = new StringBuilder();
                        sb.Append("我方开黑:[");
                        sb.Append(string.Join(",", item.Select(x => x.SummonerInternalName)));
                        sb.Append("]");
                        await Task.Delay(300);
                        await InGameSendMessage(sb.ToString());
                    }
                }
            }

            foreach (var item in otherTeam.GroupBy(x => x.TeamID))
            {
                if (item.Count() >= 2)
                {
                    if (item.FirstOrDefault()?.Champion != null)
                    {
                        var sb = new StringBuilder();
                        sb.Append("敌方开黑:[");
                        sb.Append(string.Join(",", item.Select(x => x.Champion?.Label)));
                        sb.Append("]");
                        await Task.Delay(300);
                        await InGameSendMessage(sb.ToString());
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        sb.Append("敌方开黑:[");
                        sb.Append(string.Join(",", item.Select(x => x.SummonerInternalName)));
                        sb.Append("]");
                        await Task.Delay(300);
                        await InGameSendMessage(sb.ToString());
                    }
                }
            }
        }

        private async void SendFuckWords()
        {
            //if (_iniSettingsModel.FuckWordCollection != null && _iniSettingsModel.FuckWordCollection.Count <= 5)
            //{
            //    foreach (var str in _iniSettingsModel.FuckWordCollection)
            //    {
            //        await Task.Delay(300);
            //        await InGameSendMessage(str);
            //    }
            //}
            //else if (_iniSettingsModel.FuckWordCollection != null && _iniSettingsModel.FuckWordCollection.Count > 5)
            //{
            //    foreach (var str in _iniSettingsModel.FuckWordCollection.OrderBy(x => Guid.NewGuid()).Take(5))
            //    {
            //        await Task.Delay(300);
            //        await InGameSendMessage(str);
            //    }
            //}
        }

        private async void SendGoodWords()
        {
            //if (_iniSettingsModel.GoodWordCollection != null && _iniSettingsModel.GoodWordCollection.Count <= 5)
            //{
            //    foreach (var str in _iniSettingsModel.GoodWordCollection)
            //    {
            //        await Task.Delay(300);
            //        await InGameSendMessage(str);
            //    }
            //}
            //else if (_iniSettingsModel.GoodWordCollection != null && _iniSettingsModel.GoodWordCollection.Count > 5)
            //{
            //    foreach (var str in _iniSettingsModel.GoodWordCollection.OrderBy(x => Guid.NewGuid()).Take(5))
            //    {
            //        await Task.Delay(300);
            //        await InGameSendMessage(str);
            //    }
            //}
        }

        private async Task LoadAsync()
        {
            //await _teamupService.Initialize(_configuration.GetSection("TeamupApi").Value);
            //await CheckGameNotExistWhenStartAsync();
            await ConnnectAsync();
            await Task.Delay(1000);
            //等websocket恢复后在使用
            _eventService.Subscribe(Constant.ChampSelect, new EventHandler<EventArgument>(ChampSelect));
            _eventService.Subscribe(Constant.GameFlow, new EventHandler<EventArgument>(GameFlow));
            Connected = true;
            //await (_mainPage.DataContext as MainViewModel).LoadAsync();
            GameStatus = "获取状态中";
            //获取大乱斗buff数据
            await LoadAramBuffAsync();
            await LoopforClientStatus();
        }

        private void CheckUpdate()
        {
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe");
            var updateFile = files.FirstOrDefault(f => f.Split('\\').LastOrDefault() == "NPhoenixAutoUpdateTool.exe");
            if (updateFile != null)
            {
                Process.Start(updateFile, version);
            }
            else
            {
                Task.Run(async () =>
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var responseMessage = await client.GetAsync("http://www.dotlemon.top:5200/upload/NPhoenix/NPhoenixAutoUpdateTool.exe", HttpCompletionOption.ResponseHeadersRead, CancellationToken.None).ConfigureAwait(false);
                            responseMessage.EnsureSuccessStatusCode();
                            if (responseMessage.StatusCode == HttpStatusCode.OK)
                            {
                                var filePath = Directory.GetCurrentDirectory() + "/NPhoenixAutoUpdateTool.exe";
                                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                                {
                                    await responseMessage.Content.CopyToAsync(fs);
                                }
                                if (File.Exists(filePath))
                                {
                                    Process.Start(filePath, version);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
        }

        private async Task LoadAramBuffAsync()
        {
            //var notice = _configuration.GetSection("AramBuffLocation").Value;
            //if (string.IsNullOrEmpty(notice))
            //{
            //    Growl.WarningGlobal(new GrowlInfo()
            //    {
            //        WaitTime = 2,
            //        Message = "未能拉取大乱斗buff数据",
            //        ShowDateTime = false
            //    });

            //    return;
            //}

            //using (var client = new HttpClient())
            //{
            //    client.Timeout = TimeSpan.FromSeconds(10);
            //    try
            //    {
            //        var data = await client.GetByteArrayAsync(notice);
            //        if (data == null || data.Count() <= 0)
            //        {
            //            return;
            //        }
            //        var dataStr = Encoding.UTF8.GetString(data);
            //        Constant.AramBuffs = JsonConvert.DeserializeObject<IEnumerable<AramBuff>>(dataStr);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex.ToString());
            //        Growl.WarningGlobal(new GrowlInfo()
            //        {
            //            WaitTime = 2,
            //            Message = "拉取大乱斗buff数据错误",
            //            ShowDateTime = false
            //        });
            //    }
            //}
        }

        private async Task ResetAsync()
        {
            await LoadAsync();
        }

        private async Task LoopforClientStatus()
        {
            if (_isLoop)
                return;

            _isLoop = true;
            await Task.Yield();
            while (true)
            {
                try
                {
                    var data = await _accountService.GetLoginInfoAsync();
                    if (data == null)
                        throw new Exception("未知的登录信息");

                    await Task.Delay(3000);
                }
                catch
                {
                    //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    Connected = false;
                    //    GameStatus = "断线中...";
                    //});

                    await LoadAsync();
                    await Task.Delay(3000);
                }
            }
        }

        #region 打开各页面
        private void OpenSettingsPage()
        {
        }
        private void OpenMainPage()
        {
        }
        private void OpenNoticePage()
        {
        }
        private void OpenRankPage()
        {
        }
        private void ShiftTeamupPage()
        {
        }
        #endregion

        #region 各种事件

        #region websocket恢复后再恢复监听游戏流程代码
        private async void GameFlow(object obj, EventArgument @event)
        {
            var data = $"{@event.Data}";
            if (string.IsNullOrEmpty(data))
                return;

            if (data == "ReadyCheck" ||
                data == "ChampSelect" ||
                data == "Lobby" ||
                data == "Matchmaking" ||
                data == "None")
            {
            }
            if (data != "ChampSelect")
            {
            }
            switch (data)
            {
                case "ReadyCheck":
                    GameStatus = "找到对局";
                    break;
                case "ChampSelect":
                    GameStatus = "英雄选择中";
                    await ChampSelectAsync();
                    break;
                case "None":
                    GameStatus = "大厅中或正在创建对局";
                    break;
                case "Reconnect":
                    GameStatus = "游戏中,等待重新连接";
                    break;
                case "Lobby":
                    GameStatus = "房间中";
                    break;
                case "Matchmaking":
                    GameStatus = "匹配中";
                    break;
                case "InProgress":
                    GameStatus = "游戏中";
                    break;
                case "GameStart":
                    GameStatus = "游戏开始了";
                    Team1Accounts = new List<Account>();
                    Team2Accounts = new List<Account>();
                    await ActionWhenGameBegin();
                    break;
                case "WaitingForStats":
                    GameStatus = "等待结算界面";
                    break;
                case "PreEndOfGame":
                    break;
                case "EndOfGame":
                    GameStatus = "对局结束";
                    await ActionWhenGameEnd();
                    break;
                default:
                    GameStatus = "未知状态" + data;
                    break;
            }
        }
        #endregion

        private string _preStatus = string.Empty;

        private async void ChampSelect(object obj, EventArgument @event)
        {
            try
            {
                var gInfo = await _gameService.GetCurrentGameInfoAsync();
                var mode = JToken.Parse(gInfo)["gameData"]["queue"]["gameMode"].ToString();
                var myData = JObject.Parse(@event.Data.ToString());
                int playerCellId = int.Parse(@event.Data["localPlayerCellId"].ToString());

                if ((bool)@event.Data["allowSkinSelection"] == true)
                {
                    //await _runeViewModel.GetCurrentChampionColorSkins();
                }

                IEnumerable<Team> teams = JsonConvert.DeserializeObject<IEnumerable<Team>>(@event.Data["myTeam"].ToString());
                var me = teams.FirstOrDefault(x => x.CellId == playerCellId);
                if (me == null)
                    return;

                if (mode == "ARAM")
                {
                }
            }
            catch (Exception ex)
            {
            }
        }

        //获取游戏内实时的一些数据，目前只是获取选择的英雄和召唤师技能
        private void LoopLiveGameEventAsync()
        {
        }

        private async Task AutoAcceptAsync()
        {
            //if (_iniSettingsModel.AutoAcceptGameDelay > 0)
            //{
            //    await Task.Delay(_iniSettingsModel.AutoAcceptGameDelay * 1000);
            //}

            await _gameService.AutoAcceptGameAsync();
        }

        private async Task ChampSelectAsync()
        {
        }

        private async Task ActionWhenGameBegin()
        {
            try
            {
                var gameInformation = await _gameService.GetCurrentGameInfoAsync();
                var token = JToken.Parse(gameInformation)["gameData"];
                var t1 = token["teamOne"].ToObject<IEnumerable<Teammate>>();
                var t2 = token["teamTwo"].ToObject<IEnumerable<Teammate>>();

                if (t1.All(x => x.SummonerId == default) && t2.All(x => x.SummonerId == default))
                {
                    return;
                }

                if (!t1.All(x => string.IsNullOrEmpty(x.Puuid?.Trim())))
                {
                    Team1Accounts = (await TeamToAccountsAsync(t1))?.OrderBy(x => x.TeamID)?.ToList();
                }

                if (!t2.All(x => string.IsNullOrEmpty(x.Puuid?.Trim())))
                {
                    Team2Accounts = (await TeamToAccountsAsync(t2))?.OrderBy(x => x.TeamID)?.ToList();
                }

            }
            catch (Exception ex)
            {
            }
        }

        private async Task ActionWhenGameEnd()
        {
            var game = await _gameService.GetCurrentGameInfoAsync();
            if (Team1Accounts.Count <= 0 && Team2Accounts.Count <= 0)
                return;

            try
            {
                var gameId = JToken.Parse(game)["gameData"].Value<long>("gameId");
                var details = await _gameService.QueryGameDetailAsync(gameId);
                var detailRecordsData = JToken.Parse(details);
                var DetailRecord = detailRecordsData.ToObject<Record>();
                var myTeam = Team1Accounts.FirstOrDefault(x => x?.SummonerId == Constant.Account?.SummonerId) == null ? Team2Accounts : Team1Accounts;
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<List<Account>> TeamToAccountsAsync(IEnumerable<Teammate> teammates)
        {
            var accounts = new List<Account>();
            //var teamvm = App.ServiceProvider.GetRequiredService<TeammateViewModel>();
            //int teamId = 1;
            //List<(int, int)> teams = new List<(int, int)>();
            //foreach (var id in teammates)
            //{
            //    var account = await teamvm.GetAccountAsync(id.SummonerId);
            //    account.SummonerInternalName = id.SummonerInternalName;
            //    if (id.TeamParticipantId == null)
            //    {
            //        account.TeamID = teamId++;
            //    }
            //    else
            //    {
            //        var team = teams.FirstOrDefault(x => x.Item2 == id.TeamParticipantId);
            //        if (team == default)
            //        {
            //            account.TeamID = teamId++;
            //            teams.Add((account.TeamID, id.TeamParticipantId.Value));
            //        }
            //        else
            //        {
            //            account.TeamID = team.Item1;
            //        }
            //    }

            //    if (!string.IsNullOrEmpty(id.GameCustomization?.Perks))
            //    {
            //        account.Runes = new ObservableCollection<Rune>(JToken.Parse(id.GameCustomization?.Perks)["perkIds"].ToObject<IEnumerable<int>>()?.Select(x => Constant.Runes.FirstOrDefault(y => y.Id == x))?.ToList());
            //    }
            //    if (account != null)
            //    {
            //        accounts.Add(account);
            //    }
            //}

            return accounts;
        }
        #endregion

        private async Task<string> GetAuthenticate()
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = _cmdPath;
                p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true; //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true; //不显示程序窗口
                p.Start();
                p.StandardInput.WriteLine(_excuteShell.TrimEnd('&') + "&exit");
                p.StandardInput.AutoFlush = true;
                string output = await p.StandardOutput.ReadToEndAsync();
                p.WaitForExit();
                p.Close();

                return output;
            }
        }

        /// <summary>
        /// 首次打开不能存在lol进程
        /// </summary>
        /// <returns></returns>
        private async Task CheckGameNotExistWhenStartAsync()
        {
            var authenticate = await GetAuthenticate();
            if (!string.IsNullOrEmpty(authenticate) && authenticate.Contains("--remoting-auth-token="))
            {
                await Task.Delay(500);
                Environment.Exit(0);
            }
        }

        private async Task ConnnectAsync()
        {
            while (true)
            {
                try
                {
                    var authenticate = await GetAuthenticate();
                    if (!string.IsNullOrEmpty(authenticate) && authenticate.Contains("--remoting-auth-token="))
                    {
                        var tokenResults = authenticate.Split("--remoting-auth-token=");
                        var portResults = authenticate.Split("--app-port=");
                        var PidResults = authenticate.Split("--app-pid=");
                        var installLocations = authenticate.Split("--install-directory=");
                        Constant.Token = tokenResults[1].Substring(0, tokenResults[1].IndexOf("\""));
                        Constant.Port = int.TryParse(portResults[1].Substring(0, portResults[1].IndexOf("\"")), out var temp) ? temp : 0;
                        Constant.Pid = int.TryParse(PidResults[1].Substring(0, PidResults[1].IndexOf("\"")), out var temp1) ? temp1 : 0;
                        if (string.IsNullOrEmpty(Constant.Token) || Constant.Port == 0)
                            throw new InvalidOperationException("invalid data when try to crack.");

                        await Task.WhenAll(_requestService.Initialize(Constant.Port, Constant.Token),
                                           _eventService.Initialize(Constant.Port, Constant.Token));

                        await _eventService.ConnectAsync();
                        break;
                    }
                    else
                        throw new InvalidOperationException("can't read right token and port");
                }
                catch (Exception ex)
                {
                    await Task.Delay(2000);
                }
            }
        }

        private async Task<bool> InGameSendMessage(string message)
        {
            return await Simulate.Events()
                .Click(KeyCode.Enter).Wait(75)
                .Click(message).Wait(75)
                .Click(KeyCode.Enter)
                .Invoke();
        }

        private async Task<bool> EndofGameAutoExit()
        {
            //if (_iniSettingsModel.AutoEndGame)
            //{
            //    return await Simulate.Events()
            //        .Hold(KeyCode.Alt).Wait(75)
            //        .Click(KeyCode.F4).Wait(75)
            //        .Click(KeyCode.F4).Wait(75).Release(KeyCode.Alt).Invoke();
            //}

            return false;
        }

        private void OpenChampionSelectTool()
        {
        }

        private void OpenTeamDetail()
        {
        }

        private bool _thisGameHadOpenDialog = false;
        #region 兼容模式
        private async Task LoopGameFlow(string phase)
        {
            if (string.IsNullOrEmpty(phase) || _preStatus == phase)
                return;

            _preStatus = phase;
            if (phase == "ReadyCheck" ||
                phase == "ChampSelect" ||
                phase == "Lobby" ||
                phase == "Matchmaking" ||
                phase == "None")
            {
            }
            switch (phase)
            {
                case "ReadyCheck":
                    GameStatus = "找到对局";
                    break;
                case "ChampSelect":
                    GameStatus = "英雄选择中";
                    await ChampSelectAsync();
                    _thisGameHadOpenDialog = false;
                    break;
                case "None":
                    GameStatus = "大厅中或正在创建对局";
                    break;
                case "Reconnect":
                    GameStatus = "游戏中,等待重新连接";
                    break;
                case "Lobby":
                    GameStatus = "房间中";
                    break;
                case "Matchmaking":
                    GameStatus = "匹配中";
                    break;
                case "InProgress":
                    GameStatus = "游戏中";
                    break;
                case "GameStart":
                    GameStatus = "游戏开始了";
                    Team1Accounts = new List<Account>();
                    Team2Accounts = new List<Account>();
                    await ActionWhenGameBegin();
                    break;
                case "WaitingForStats":
                    GameStatus = "等待结算界面";
                    break;
                case "PreEndOfGame":
                    break;
                case "EndOfGame":
                    GameStatus = "对局结束";
                    await ActionWhenGameEnd();
                    break;
                default:
                    GameStatus = "未知状态" + phase;
                    break;
            }
        }

        private void LoopGameStatus()
        {
            var _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var status = await _gameService.GetCurrentGameInfoAsync();
                        if (status == null)
                        {
                            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    _championSelectTool.Hide();
                            //    _blackList.Hide();
                            //    Team1Accounts.Clear();
                            //    Team2Accounts.Clear();
                            //    _team1V2Window.Hide();
                            //    _team1V2Window.Topmost = false;
                            //});
                            GameStatus = "大厅或者游戏主界面";
                            await Task.Delay(2000);
                            continue;
                        }

                        var phase = JObject.Parse(status)["phase"]?.ToString();
                        if (!string.IsNullOrEmpty(phase))
                        {
                            await LoopGameFlow(phase);
                        }

                        await Task.Delay(1000);
                    }
                    catch
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                }
            });
        }

        private void LoopChampSelect()
        {
            var _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var session = await _gameService.GetGameSessionAsync();
                        if (string.IsNullOrEmpty(session))
                        {
                            await Task.Delay(200);
                            continue;
                        }

                        var token = JToken.Parse(session);
                        if (token.Value<int>("httpStatus") == 404)
                        {
                            await Task.Delay(200);
                            continue;
                        }

                        await LoopChampSelect(token);
                        await Task.Delay(200);
                    }
                    catch
                    {
                        await Task.Delay(200);
                        continue;
                    }
                }
            });
        }

        private async Task LoopChampSelect(JToken data)
        {
            try
            {
                var gInfo = await _gameService.GetCurrentGameInfoAsync();
                var mode = JToken.Parse(gInfo)["gameData"]["queue"]["gameMode"].ToString();
                int playerCellId = int.Parse(data["localPlayerCellId"].ToString());
                IEnumerable<Team> teams = JsonConvert.DeserializeObject<IEnumerable<Team>>(data["myTeam"].ToString());
                var me = teams.FirstOrDefault(x => x.CellId == playerCellId);
                if (me == null)
                    return;

                if (mode == "ARAM")
                {
                    //await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    //{
                    //    if (me.ChampionId != default)
                    //        await _runeViewModel.LoadChampInfoAsync(me.ChampionId, true);
                    //});

                    //if (_iniSettingsModel.AutoLockHeroInAram) //秒抢大乱斗英雄
                    //{
                    //    var session = await _gameService.GetGameSessionAsync();
                    //    var token = JToken.Parse(session);
                    //    BenchChampion[] champs = token["benchChampions"]?.ToObject<BenchChampion[]>();
                    //    var loc = _iniSettingsModel.LockHerosInAram.IndexOf(me.ChampionId);
                    //    loc = loc == -1 ? _iniSettingsModel.LockHerosInAram.Count : loc;
                    //    if (loc != 0)
                    //    {
                    //        var heros = _iniSettingsModel.LockHerosInAram.Take(loc);
                    //        var swapHeros = new List<int>();
                    //        foreach (var item in heros)
                    //        {
                    //            if (champs.Select(x => x.ChampionId).ToList().Contains(item))
                    //            {
                    //                swapHeros.Add(item);
                    //            }
                    //        }

                    //        for (var index = swapHeros.Count - 1; index >= 0; index--)
                    //        {
                    //            await _gameService.BenchSwapChampionsAsync(swapHeros[index]);
                    //        }
                    //    }
                    //}
                    //{
                    //    var session = await _gameService.GetGameSessionAsync();
                    //    var token = JToken.Parse(session);
                    //    BenchChampion[] champs = token["benchChampions"]?.ToObject<BenchChampion[]>();
                    //    BenchChampion[] chooseChamps = token["myTeam"]?.ToObject<BenchChampion[]>();
                    //    WeakReferenceMessenger.Default.Send(new AramChooseHeroModel(chooseChamps.Select(x => x.ChampionId).ToList(), champs.Select(x => x.ChampionId).ToList()));
                    //}
                }
                else
                {
                    //foreach (var action in data["actions"])
                    //{
                    //    foreach (var actionItem in action)
                    //    {
                    //        if (int.Parse(actionItem["actorCellId"].ToString()) == playerCellId)
                    //        {
                    //            if (actionItem.Value<string>("type") == "pick")
                    //            {
                    //                foreach (var teamPlayer in data["myTeam"])
                    //                {
                    //                    if (teamPlayer.Value<int>("cellId") == playerCellId)
                    //                    {
                    //                        int champ = teamPlayer.Value<int>("championId");
                    //                        if (int.Parse((string)actionItem["championId"]) != 0 && champ != 0)
                    //                        {
                    //                            await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    //                            {
                    //                                await _runeViewModel.LoadChampInfoAsync(champ, false);
                    //                            });
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
