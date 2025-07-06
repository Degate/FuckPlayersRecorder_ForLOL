﻿using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;

namespace FuckPlayersRecorder_ForLOL.Models
{
    public class Rank
    {
        //单双排
        public RANKED_SOLO_5x5 RANKED_SOLO_5x5 { get; set; }
        //灵活
        public RANKED_FLEX_SR RANKED_FLEX_SR { get; set; }
        //云顶
        public RANKED_TFT RANKED_TFT { get; set; }
    }

    public class RankedEntry
    {
        [JsonPropertyName("division")]
        public string Division { get; set; }
        [JsonPropertyName("leaguePoints")]
        public int LeaguePoints { get; set; }
        [JsonPropertyName("losses")]
        public int Losses { get; set; }
        [JsonPropertyName("wins")]
        public int Wins { get; set; }
        [JsonPropertyName("tier")]
        public string Tier { get; set; }
        public string CnTier => Tier.ToUpper() switch
        {
            "CHALLENGER" => "王者",
            "GRANDMASTER" => "宗师",
            "MASTER" => "大师",
            "DIAMOND" => "钻石",
            "EMERALD" => "翡翠",
            "PLATINUM" => "铂金",
            "GOLD" => "黄金",
            "SILVER" => "白银",
            "BRONZE" => "青铜",
            "IRON" => "黑铁",
            _ => "未定"
        };

        public string PreCnTier => PreviousSeasonEndTier.ToUpper() switch
        {
            "CHALLENGER" => "王者",
            "GRANDMASTER" => "宗师",
            "MASTER" => "大师",
            "DIAMOND" => "钻石",
            "EMERALD" => "翡翠",
            "PLATINUM" => "铂金",
            "GOLD" => "黄金",
            "SILVER" => "白银",
            "BRONZE" => "青铜",
            "IRON" => "黑铁",
            _ => "未定"
        };

        public string TierIcon => string.IsNullOrEmpty(Tier) ? $"../Resources/RankLevels/none.png"
                : $"../Resources/RankLevels/{Tier.ToLower()}.png";
        public string PreTierIcon => string.IsNullOrEmpty(PreviousSeasonEndTier) ? $"../Resources/RankLevels/none.png"
                : $"../Resources/RankLevels/{PreviousSeasonEndTier.ToLower()}.png";

        [JsonPropertyName("previousSeasonEndDivision")]
        public string PreviousSeasonEndDivision { get; set; }

        [JsonPropertyName("previousSeasonEndTier")]
        public string PreviousSeasonEndTier { get; set; }


        public string Desc => Wins + Losses <= 0 ? "暂无" : $"胜: {Wins}\t 负: {Losses}\t 胜率: {(Wins * 100.0 / (Wins + Losses)).ToString("0.00")}%";
        public string ShortDesc => Wins + Losses <= 0 ? "暂无" : $"胜: {Wins} 负: {Losses}";
        public string WinRate => Wins + Losses <= 0 ? "暂无" : $"胜率: {(Wins * 100.0 / (Wins + Losses)).ToString("0.00")}%";
    }

    public class RANKED_FLEX_SR : RankedEntry
    {

    }

    public class RANKED_SOLO_5x5 : RankedEntry
    {

    }

    public class RANKED_TFT : RankedEntry
    {

    }
}
