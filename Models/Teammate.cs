using System.Text.Json.Serialization;

namespace FuckPlayersRecorder_ForLOL.Models
{
    public class Teammate
    {
        
        [JsonPropertyName("summonerId")]public long SummonerId { get; set; }
        [JsonPropertyName("summonerName")] public string SummonerName { get; set; }
        [JsonPropertyName("puuid")]public string Puuid { get; set; }
        [JsonPropertyName("teamParticipantId")] public int TeamParticipantId { get; set; }
        public GameCustomization GameCustomization { get; set; }
        public string SummonerInternalName { get; set; }
        [JsonPropertyName("championId")] public int ChampionId { get; set; }
    }

    public class GameCustomization 
    {
        public string Perks { get; set; }
    }

    public class PlayerChampionSelection 
    {
        public string SummonerInternalName { get; set; }
        public double Spell1Id { get; set; }
        public double Spell2Id { get; set; }
    }
}
