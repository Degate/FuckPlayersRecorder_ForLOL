using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using FuckPlayersRecorder_ForLOL.Service.Account;
//using LeagueOfLegendsBoxer.Application.Game;
using LeagueOfLegendsBoxer.Models;
//using FuckPlayersRecorder_ForLOL.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FuckPlayersRecorder_ForLOL.Service.Game;
using FuckPlayersRecorder_ForLOL.Models;

namespace FuckPlayersRecorder_ForLOL.ViewModels
{
    public class SummonerAnalyseViewModel : ObservableObject
    {

        private string _searchName;
        public string SearchName
        {
            get => _searchName;
            set => SetProperty(ref _searchName, value);
        }

        public AsyncRelayCommand SearchRecordByNameAsyncCommand { get; set; }

        private readonly IAccountService _accountService;
        private readonly IGameService _gameService;

        public void LoadPageByAccount(Account account)
        {

        }

        public async Task LoadPageAsync(long summonerId)
        {
            var infromation = await _accountService.GetSummonerInformationAsync(summonerId);
            var account = JsonConvert.DeserializeObject<Account>(infromation);
            var rankData = JToken.Parse(await _accountService.GetSummonerRankInformationAsync(account.Puuid));
            account.Rank = rankData["queueMap"].ToObject<Rank>();
            var recordsData = JToken.Parse(await _gameService.GetRecordsByPage(id: account.Puuid));
            account.Records = new ObservableCollection<Record>(recordsData["games"]["games"].ToObject<IEnumerable<Record>>().OrderByDescending(x => x.GameCreation));

        }

        private async Task SearchRecordByNameAsync()
        {
            if (string.IsNullOrEmpty(SearchName.Trim()))
                return;

            try
            {
                var data = await _accountService.GetSummonerInformationAsync(SearchName.Trim());
                if (data == null)
                {
                }
                else
                {
                    var id = JToken.Parse(data)["accountId"].ToObject<long>();
                    await LoadPageAsync(id);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
