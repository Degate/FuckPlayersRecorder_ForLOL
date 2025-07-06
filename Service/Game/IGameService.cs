﻿using FuckPlayersRecorder_ForLOL.Service.Request;

namespace FuckPlayersRecorder_ForLOL.Service.Game
{
    public interface IGameService
    {
        Task Initialize(IRequestService requestService);
        Task AutoAcceptGameAsync();
        Task AutoNewGameAsync();
        Task<string> GetGameSessionAsync();
        Task AutoLockHeroAsync(int actionID, int champID, string type);
        Task AutoDisableHeroAsync();
        Task<string> GetChatConversation();
        Task<string> GetChatConversationMessages(string chatId);
        Task<string> GetChampRankAsync(string lane, int tier, int time);
        Task<string> GetChampRestraintDataAsync(int champID);
        Task SendMessageAsync(string chatId,string message);
        Task<string> QueryGameDetailAsync(long gameId);
        Task<string> QuerySummonerSuperChampDataAsync(long summonerId);
        Task<string> test(long id);
        Task<string> GetCurrentGameInfoAsync();
        Task<string> GetCurrentChampionInfoAsync();
        Task<string> GetCurrentChampionColorSkinsAsync();
        Task<string> GetPickableChampionsAsync();
        Task BenchSwapChampionsAsync(int champID);
        Task<string> GetSkinsByHeroId(int id);
        Task<byte[]> GetResourceByUrl(string url);
        Task<string> SetSkinAsync(dynamic body);
        Task<string> SetIconAsync(dynamic body);
        Task<string> SendGameChatMessageAsync(string message);
        Task<string> GetItems();
        Task<string> GetIcons();
        Task<string> GetSpells();
        Task<string> GetRecordsByPage(int pageStart = 0 , int pageEnd = 20, string id = null);
        Task UseColorSkinAsync(dynamic body);
    }
}
