﻿using CommunityToolkit.Mvvm.ComponentModel;
using FuckPlayersRecorder_ForLOL.Resources;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace FuckPlayersRecorder_ForLOL.Models
{
    public class Record : ObservableObject
    {
        [JsonPropertyName("gameId")]
        public long GameId { get; set; }
        [JsonPropertyName("gameCreation")]
        public long GameCreation { get; set; }
        public string GameCreationString => TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(GameCreation).Year == DateTime.Now.Year ?
                                            TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(GameCreation).ToString("MM-dd") : TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(GameCreation).ToString("d");
        public string GameCreationTimeString => TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(GameCreation).ToString("t");
        [JsonPropertyName("gameDuration")]
        public int GameDuration { get; set; }
        [JsonPropertyName("gameMode")]
        public string GameMode { get; set; }
        public int GameMinutes => GameDuration / 60;
        [JsonPropertyName("queueId")]
        public int QueueId { get; set; }
        public string CnQueue => QueueId switch
        {
            420 => "单双排",
            430 => "匹配",
            440 => "灵活排位",
            450 => "大乱斗",
            _ => "其他"
        } + $"{Participants.FirstOrDefault()?.GetDesc()}";

        [JsonPropertyName("participantIdentities")]
        public ParticipantIdentity[] ParticipantIdentities { get; set; }

        [JsonPropertyName("participants")]
        public Participant[] Participants { get; set; }

        private double team1GoldEarned;
        public double Team1GoldEarned
        {
            get => team1GoldEarned;
            set
            {
                SetProperty(ref team1GoldEarned, value);
                Team1GoldEarnedString = (value / 1000.0).ToString("0.0");
            }
        }

        private double team2GoldEarned;
        public double Team2GoldEarned
        {
            get => team2GoldEarned;
            set
            {
                SetProperty(ref team2GoldEarned, value);
                Team2GoldEarnedString = (value / 1000.0).ToString("0.0");
            }
        }

        private string team1GoldEarnedString;

        public string Team1GoldEarnedString
        {
            get => team1GoldEarnedString;
            set => SetProperty(ref team1GoldEarnedString, value);
        }


        private string team2GoldEarnedString;

        public string Team2GoldEarnedString
        {
            get => team2GoldEarnedString;
            set => SetProperty(ref team2GoldEarnedString, value);
        }

        private double team1Kills;
        public double Team1Kills
        {
            get => team1Kills;
            set => SetProperty(ref team1Kills, value);
        }
        private double team2Kills;
        public double Team2Kills
        {
            get => team2Kills;
            set => SetProperty(ref team2Kills, value);
        }

        public double GetScore()
        {
            if (Participants == null || Participants.FirstOrDefault() == null || Participants.FirstOrDefault().Stats == null)
                return 110;

            return Participants.FirstOrDefault().GetScore();
        }

        public double? GetKDA()
        {
            if (Participants == null || Participants.FirstOrDefault() == null || Participants.FirstOrDefault().Stats == null
                || Participants.FirstOrDefault().Stats.Kills + Participants.FirstOrDefault().Stats.Deaths + Participants.FirstOrDefault().Stats.Assists == 0)
                return null;

            return Participants.FirstOrDefault().GetKDA();
        }

        public bool IsSurrender()
        {
            if (Participants == null || Participants.FirstOrDefault() == null || Participants.FirstOrDefault().Stats == null)
                return false;

            return Participants.FirstOrDefault().Stats.CausedEarlySurrender;
        }

        private ObservableCollection<Tuple<ParticipantIdentity, Participant>> _leftParticipants = new ObservableCollection<Tuple<ParticipantIdentity, Participant>>();
        public ObservableCollection<Tuple<ParticipantIdentity, Participant>> LeftParticipants
        {
            get => _leftParticipants;
            set => SetProperty(ref _leftParticipants, value);
        }

        private ObservableCollection<Tuple<ParticipantIdentity, Participant>> _rightParticipants = new ObservableCollection<Tuple<ParticipantIdentity, Participant>>();
        public ObservableCollection<Tuple<ParticipantIdentity, Participant>> RightParticipants
        {
            get => _rightParticipants;
            set => SetProperty(ref _rightParticipants, value);
        }

        private Record _detailRecord;
        public Record DetailRecord
        {
            get => _detailRecord;
            set => SetProperty(ref _detailRecord, value);
        }

        private bool _isMvp;
        public bool IsMvp
        {
            get => _isMvp;
            set => SetProperty(ref _isMvp, value);
        }

        private bool _isSvp;
        public bool IsSvp
        {
            get => _isSvp;
            set => SetProperty(ref _isSvp, value);
        }

        private bool _isLeftWin;
        public bool IsLeftWin
        {
            get => _isLeftWin;
            set => SetProperty(ref _isLeftWin, value);
        }
    }

    public class ParticipantIdentity : ObservableObject
    {
        [JsonPropertyName("player")]
        public Player Player { get; set; }
        public bool IsCurrentUser { get; set; }

        private bool _isMvp;
        public bool IsMvp
        {
            get => _isMvp;
            set => SetProperty(ref _isMvp, value);
        }

        private bool _isSvp;
        public bool IsSvp
        {
            get => _isSvp;
            set => SetProperty(ref _isSvp, value);
        }

        public double Score { get; set; }

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
    }

    public class Player
    {
        [JsonPropertyName("summonerName")]
        public string SummonerName { get; set; }
        [JsonPropertyName("gameName")]
        public string GameName { get; set; }
        [JsonPropertyName("summonerId")]
        public long SummonerId { get; set; }
        public string DisplayName => string.IsNullOrEmpty(GameName) ? SummonerName : GameName;
    }

    public class Participant
    {

        [JsonPropertyName("championId")]
        public int ChampionId { get; set; }
        public Hero Hero => Constant.Heroes.FirstOrDefault(x => x.ChampId == ChampionId);
        public string ChampionImage => string.Format(Constant.HeroAvatar, Hero?.Alias);
        public string ChampionName => $"{Hero?.Label}";
        [JsonPropertyName("stats")]
        public Stats Stats { get; set; }
        [JsonPropertyName("timeline")]
        public Timeline Timeline { get; set; }
        [JsonPropertyName("spell1Id")]
        public int Spell1Id { get; set; }
        [JsonPropertyName("teamId")]
        public int TeamId { get; set; }
        public string Spell1Image => Spell1Id switch
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
        [JsonPropertyName("spell2Id")]
        public int Spell2Id { get; set; }
        public string Spell2Image => Spell2Id switch
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

        public string GetDesc()
        {
            if (Stats.GameEndedInSurrender && !Stats.Win)
                return "(投降)";

            if (Stats.GameEndedInEarlySurrender)
                return "(重开)";

            return string.Empty;
        }

        public double GetScore()
        {
            var stats = Stats;
            double score = 100;
            if (stats.FirstBloodKill)
                score += 10;
            if (stats.FirstBloodAssist)
                score += 5;
            if (stats.CausedEarlySurrender)
                score -= 10;
            if (stats.Win)
                score += 5;
            else
                score -= 5;

            score += stats.DoubleKills * 2;
            score += stats.TripleKills * 5;
            score += stats.QuadraKills * 10;
            score += stats.PentaKills * 15;
            score += stats.Kills - stats.Deaths + (stats.Assists * 0.5);
            return score;
        }

        /// <summary>
        /// (k + A)/D
        /// </summary>
        /// <returns></returns>
        public double GetKDA()
        {
            if (Stats.Deaths == 0)
                return 5;

            return (double)(Stats.Kills + Stats.Assists) / Stats.Deaths;
        }
    }

    public class Stats
    {
        public string DamageConvert { get; set; }

        [JsonPropertyName("assists")]
        public int Assists { get; set; }
        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }
        [JsonPropertyName("kills")]
        public int Kills { get; set; }
        [JsonPropertyName("win")]
        public bool Win { get; set; }
        [JsonPropertyName("firstBloodKill")]
        public bool FirstBloodKill { get; set; }
        [JsonPropertyName("firstBloodAssist")]
        public bool FirstBloodAssist { get; set; }
        [JsonPropertyName("causedEarlySurrender")]
        public bool CausedEarlySurrender { get; set; }
        [JsonPropertyName("gameEndedInEarlySurrender")]
        public bool GameEndedInEarlySurrender { get; set; }
        [JsonPropertyName("gameEndedInSurrender")]
        public bool GameEndedInSurrender { get; set; }
        [JsonPropertyName("doubleKills")]
        public int DoubleKills { get; set; }
        [JsonPropertyName("tripleKills")]
        public int TripleKills { get; set; }
        [JsonPropertyName("champLevel")]
        public int ChampLevel { get; set; }
        [JsonPropertyName("quadraKills")]
        public int QuadraKills { get; set; }
        [JsonPropertyName("pentaKills")]
        public int PentaKills { get; set; }
        public string KDA => $"{Kills}-{Deaths}-{Assists}";
        [JsonPropertyName("totalMinionsKilled")]
        public int TotalMinionsKilled { get; set; }
        [JsonPropertyName("goldEarned")]
        public int GoldEarned { get; set; }
        [JsonPropertyName("totalDamageDealt")]
        public int TotalDamageDealt { get; set; }
        [JsonPropertyName("largestKillingSpree")]
        public int LargestKillingSpree { get; set; }
        [JsonPropertyName("largestMultiKill")]
        public int LargestMultiKill { get; set; }
        [JsonPropertyName("turretKills")]
        public int TurretKills { get; set; }
        [JsonPropertyName("wardsKilled")]
        public int WardsKilled { get; set; }
        [JsonPropertyName("totalHeal")]
        public int TotalHeal { get; set; }
        [JsonPropertyName("totalDamageTaken")]
        public int TotalDamageTaken { get; set; }
        [JsonPropertyName("totalDamageDealtToChampions")]
        public int TotalDamageDealtToChampions { get; set; }
        [JsonPropertyName("physicalDamageDealtToChampions")]
        public int PhysicalDamageDealtToChampions { get; set; }
        [JsonPropertyName("magicDamageDealtToChampions")]
        public int MagicDamageDealtToChampions { get; set; }
        [JsonPropertyName("item0")]
        public int Item0 { get; set; }
        [JsonPropertyName("item1")]
        public string Item0Image => Item0 switch
        {
            7013 => "https://game.gtimg.cn/images/lol/act/img/item/3802.png",
            0 => "https://wegame.gtimg.com/g.26-r.c2d3c/helper/lol/assis/images/resources/items/0.png",
            _ => $"https://game.gtimg.cn/images/lol/act/img/item/{Item0}.png"
        };
        public int Item1 { get; set; }
        public string Item1Image => Item1 switch
        {
            7013 => "https://game.gtimg.cn/images/lol/act/img/item/3802.png",
            0 => "https://wegame.gtimg.com/g.26-r.c2d3c/helper/lol/assis/images/resources/items/0.png",
            _ => $"https://game.gtimg.cn/images/lol/act/img/item/{Item1}.png"
        };
        [JsonPropertyName("item2")]
        public int Item2 { get; set; }
        public string Item2Image => Item2 switch
        {
            7013 => "https://game.gtimg.cn/images/lol/act/img/item/3802.png",
            0 => "https://wegame.gtimg.com/g.26-r.c2d3c/helper/lol/assis/images/resources/items/0.png",
            _ => $"https://game.gtimg.cn/images/lol/act/img/item/{Item2}.png"
        };
        [JsonPropertyName("item3")]
        public int Item3 { get; set; }
        public string Item3Image => Item3 switch
        {
            7013 => "https://game.gtimg.cn/images/lol/act/img/item/3802.png",
            0 => "https://wegame.gtimg.com/g.26-r.c2d3c/helper/lol/assis/images/resources/items/0.png",
            _ => $"https://game.gtimg.cn/images/lol/act/img/item/{Item3}.png"
        };
        [JsonPropertyName("item4")]
        public int Item4 { get; set; }
        public string Item4Image => Item4 switch
        {
            7013 => "https://game.gtimg.cn/images/lol/act/img/item/3802.png",
            0 => "https://wegame.gtimg.com/g.26-r.c2d3c/helper/lol/assis/images/resources/items/0.png",
            _ => $"https://game.gtimg.cn/images/lol/act/img/item/{Item4}.png"
        };
        [JsonPropertyName("item5")]
        public int Item5 { get; set; }
        public string Item5Image => Item5 switch
        {
            7013 => "https://game.gtimg.cn/images/lol/act/img/item/3802.png",
            0 => "https://wegame.gtimg.com/g.26-r.c2d3c/helper/lol/assis/images/resources/items/0.png",
            _ => $"https://game.gtimg.cn/images/lol/act/img/item/{Item5}.png"
        };
        [JsonPropertyName("item6")]
        public int Item6 { get; set; }
        public string Item6Image => Item6 switch
        {
            7013 => "https://game.gtimg.cn/images/lol/act/img/item/3802.png",
            0 => "https://wegame.gtimg.com/g.26-r.c2d3c/helper/lol/assis/images/resources/items/0.png",
            _ => $"https://game.gtimg.cn/images/lol/act/img/item/{Item6}.png"
        };
        public Item Item0Model => Constant.Items.FirstOrDefault(x => x.Id == Item0);
        public Item Item1Model => Constant.Items.FirstOrDefault(x => x.Id == Item1);
        public Item Item2Model => Constant.Items.FirstOrDefault(x => x.Id == Item2);
        public Item Item3Model => Constant.Items.FirstOrDefault(x => x.Id == Item3);
        public Item Item4Model => Constant.Items.FirstOrDefault(x => x.Id == Item4);
        public Item Item5Model => Constant.Items.FirstOrDefault(x => x.Id == Item5);
        public Item Item6Model => Constant.Items.FirstOrDefault(x => x.Id == Item6);
        public double BarWidth { get; set; }
        public bool AboveGod => LargestKillingSpree >= 8;
        public bool Penta => PentaKills >= 1;
        public bool Qua => QuadraKills >= 1;

        public bool MaxMoney { get; set; }
        public bool MaxDamage { get; set; }

    }

    public class Timeline
    {
        [JsonPropertyName("lane")]
        public string Lane { get; set; }
        public string CnLane => Lane switch
        {
            "MIDDLE" => "中单",
            "JUNGLE" => "打野",
            "BOTTOM" => "下路",
            "SUPPORT" => "辅助",
            "TOP" => "上路",
            _ => "未知"
        };
    }
}
