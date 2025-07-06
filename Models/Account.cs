﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuckPlayersRecorder_ForLOL.Resources;
using FuckPlayersRecorder_ForLOL.Service.LiveGame;
using FuckPlayersRecorder_ForLOL.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace FuckPlayersRecorder_ForLOL.Models
{
    public class Account : ObservableObject
    {
        [JsonPropertyName("displayName")]public string DisplayName { get; set; }
        private long _summonerId;
        [JsonPropertyName("summonerId")]public long SummonerId
        {
            get { return _summonerId; }
            set
            {
                _summonerId = value;
                IsCurrentUser = Constant.Account?.SummonerId == value;
            }
        }
        [JsonPropertyName("summonerName")]public string SummonerName { get; set; }
        [JsonPropertyName("tagLine")]public string TagLine { get; set; }
        [JsonPropertyName("gameName")]public string GameName { get; set; }        
        [JsonPropertyName("accountId")]public long AccountId { get; set; }
        [JsonPropertyName("summonerLevel")]public int SummonerLevel { get; set; }
        [JsonPropertyName("puuid")]public string Puuid { get; set; }
        [JsonPropertyName("xpSinceLastLevel")]public int XpSinceLastLevel { get; set; }
        [JsonPropertyName("xpUntilNextLevel")]public int XpUntilNextLevel { get; set; }
        [JsonPropertyName("profileIconId")]public int ProfileIconId { get; set; }
        private string _fullName;
        public string FullName
        {
            get => string.IsNullOrEmpty(_fullName)? $"{GameName}#{TagLine}":_fullName;
            set => SetProperty(ref _fullName, value);
        }
        public string Avatar => string.Format(Constant.Avatar, ProfileIconId);
        public string XpTip => $"经验值:{XpSinceLastLevel}/{XpUntilNextLevel}";

        private int _teamParticipantId = 0;
        public int TeamParticipantId
        {
            get => _teamParticipantId;
            set => SetProperty(ref _teamParticipantId, value);
        }
        private Rank _rank;
        public Rank Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value);
        }

        private ObservableCollection<Record> _records;
        public ObservableCollection<Record> Records
        {
            get => _records;
            set => SetProperty(ref _records, value);
        }

        private ObservableCollection<Record> _currentModeRecord;
        public ObservableCollection<Record> CurrentModeRecord
        {
            get => _currentModeRecord;
            set
            {
                SetProperty(ref _currentModeRecord, value);
                KDA = GetKDA();
                SurRate = GetSurrenderRate();
            }
        }

        private string _serverArea;
        public string ServerArea
        {
            get => _serverArea;
            set => SetProperty(ref _serverArea, value);
        }

        public int TeamID { get; set; }

        private string _kda;
        public string KDA
        {
            get => _kda;
            set => SetProperty(ref _kda, value);
        }
        private int _kill = 0;
        public  int Kill
        {
            get => _kill;
            set => SetProperty(ref _kill, value);
        }
        private int _death = 0;

        public int Death
        {
            get => _death;
            set => SetProperty(ref _death, value);
        }
        private int _assist = 0;
        public int Assist
        {
            get => _assist;
            set => SetProperty(ref _assist, value);
        }
        private double _damage = 0;
        public double Damage
        {
            get => _damage;
            set => SetProperty(ref _damage, value);
        }
        private string _surRate ;
        public string SurRate
        {
            get => _surRate;
            set => SetProperty(ref _surRate, value);
        }


        public string GetKDA()
        {
            if (CurrentModeRecord == null || CurrentModeRecord.Count <= 4)
                return "未知";

            return CurrentModeRecord.Where(x => x.GetKDA() != null).Average(x => x.GetKDA().Value).ToString("0.00");
        }

        public string GetSurrenderRate()
        {
            if (CurrentModeRecord == null || CurrentModeRecord.Count <= 4)
                return "未知";

            return $"{(CurrentModeRecord.Where(x => x.Participants.FirstOrDefault().Stats.GameEndedInSurrender && !x.Participants.FirstOrDefault().Stats.Win).Count() * 100.0 / CurrentModeRecord.Count()).ToString("0.00")}%";
        }

        //live
        private Hero _champion;
        public Hero Champion
        {
            get => _champion;
            set => SetProperty(ref _champion, value);
        }

        #region 排行
        private string _mvpRank;
        public string MvpRank
        {
            get => _mvpRank;
            set => SetProperty(ref _mvpRank, value);
        }

        private string _xiaguKill;
        public string XiaguKill
        {
            get => _xiaguKill;
            set => SetProperty(ref _xiaguKill, value);
        }

        private string _aramKill;
        public string AramKill
        {
            get => _aramKill;
            set => SetProperty(ref _aramKill, value);
        }
        #endregion


        private ObservableCollection<Champ> _champs;
        public ObservableCollection<Champ> Champs
        {
            get => _champs;
            set => SetProperty(ref _champs, value);
        }

        //blacklist
        private bool _isOpenBlack;
        public bool IsOpenBlack
        {
            get => _isOpenBlack;
            set => SetProperty(ref _isOpenBlack, value);
        }

        public string _reason;
        public string Reason
        {
            get { return _reason; }
            set { SetProperty(ref _reason, value); }
        }

        private bool _isCurrentUser;
        public bool IsCurrentUser
        {
            get { return _isCurrentUser; }
            set { SetProperty(ref _isCurrentUser, value); }
        }

        private bool _isInBlackList;
        public bool IsInBlackList
        {
            get { return _isInBlackList; }
            set { SetProperty(ref _isInBlackList, value); }
        }

        private string _blackInfo;
        public string BlackInfo
        {
            get { return _blackInfo; }
            set { SetProperty(ref _blackInfo, value); }
        }

        private string _winRate;
        public string WinRate
        {
            get { return _winRate; }
            set { SetProperty(ref _winRate, value); }
        }

        public double WinRateValue;

        public string SummonerInternalName { get; set; }

        private double _spell1Id;
        public double Spell1Id
        {
            get { return _spell1Id; }
            set
            {
                SetProperty(ref _spell1Id, value);
                Spell1Image = value switch
                {
                    4 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_flash.png",
                    14 => "https://game.gtimg.cn/images/lol/act/img/spell/SummonerIgnite.png",
                    11 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_smite.png",
                    6 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_haste.png",
                    12 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_teleport.png",
                    21 => "https://game.gtimg.cn/images/lol/act/img/spell/SummonerBarrier.png",
                    3 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_exhaust.png",
                    1 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_boost.png",
                    7 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_heal.png",
                    32 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_Mark.png",
                    _ => "https://game.gtimg.cn/images/lol/act/img/spell/SummonerMana.png"
                };
            }
        }
        private double _spell2Id;
        public double Spell2Id
        {
            get { return _spell2Id; }
            set
            {
                SetProperty(ref _spell2Id, value);
                Spell2Image = value switch
                {
                    4 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_flash.png",
                    14 => "https://game.gtimg.cn/images/lol/act/img/spell/SummonerIgnite.png",
                    11 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_smite.png",
                    6 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_haste.png",
                    12 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_teleport.png",
                    21 => "https://game.gtimg.cn/images/lol/act/img/spell/SummonerBarrier.png",
                    3 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_exhaust.png",
                    1 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_boost.png",
                    7 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_heal.png",
                    32 => "https://game.gtimg.cn/images/lol/act/img/spell/Summoner_Mark.png",
                    _ => "https://game.gtimg.cn/images/lol/act/img/spell/SummonerMana.png"
                };
            }
        }

        public string _spell1Image;
        public string Spell1Image
        {
            get { return _spell1Image; }
            set { SetProperty(ref _spell1Image, value); }
        }

        public string _spell2Image;
        public string Spell2Image
        {
            get { return _spell2Image; }
            set { SetProperty(ref _spell2Image, value); }
        }

        public bool IsAdministrator { get; set; }

        public Account ShallowCopy()
        {
            return this.MemberwiseClone() as Account;
        }

        public int _spell1Time;
        public int Spell1Time
        {
            get { return _spell1Time; }
            set { SetProperty(ref _spell1Time, value); }
        }

        public int _spell2Time;
        public int Spell2Time
        {
            get { return _spell2Time; }
            set { SetProperty(ref _spell2Time, value); }
        }

        public bool _spell1Start;
        public bool Spell1Start
        {
            get { return _spell1Start; }
            set { SetProperty(ref _spell1Start, value); }
        }

        public bool _spell2Start;
        public bool Spell2Start
        {
            get { return _spell2Start; }
            set { SetProperty(ref _spell2Start, value); }
        }

        public bool _isAram;
        public bool _already14Minute;
        private CancellationTokenSource _cancelTokenSource1;
        private CancellationTokenSource _cancelTokenSource2;
        public AsyncRelayCommand StartIntoCDTimeCommand1 { get; set; }
        public AsyncRelayCommand StartIntoCDTimeCommand2 { get; set; }
        public RelayCommand ResetSpell1Command { get; set; }
        public RelayCommand ResetSpell2Command { get; set; }

        public Account()
        {
            StartIntoCDTimeCommand1 = new AsyncRelayCommand(StartIntoCDTime1);
            StartIntoCDTimeCommand2 = new AsyncRelayCommand(StartIntoCDTime2);
            ResetSpell1Command = new RelayCommand(ResetSpell1);
            ResetSpell2Command = new RelayCommand(ResetSpell2);
        }

        private async Task StartIntoCDTime1()
        {
            if (Spell1Start)
                return;

            Spell1Time = await GetTimeBySpellAndItems(Spell1Id);
            _cancelTokenSource1 = new CancellationTokenSource();
            var _ = Task.Run(async () =>
            {
                while (Spell1Time > 0 && !_cancelTokenSource1.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    Spell1Time--;
                }
                Spell1Start = false;
            }, _cancelTokenSource1.Token);
            Spell1Start = true;
        }

        private async Task StartIntoCDTime2()
        {
            if (Spell2Start)
                return;

            Spell2Time = await GetTimeBySpellAndItems(Spell2Id);
            _cancelTokenSource2 = new CancellationTokenSource();
            var _ = Task.Run(async () =>
            {
                while (Spell2Time > 0 && !_cancelTokenSource2.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    Spell2Time--;
                }
                Spell2Start = false;
            }, _cancelTokenSource2.Token);
            Spell2Start = true;
        }

        private void ResetSpell1()
        {
            _cancelTokenSource1?.Cancel();
        }
        private void ResetSpell2()
        {
            _cancelTokenSource2?.Cancel();
        }

        public async Task<int> GetTimeBySpellAndItems(double spell)
        {
            return 500;
        }
    }
}
