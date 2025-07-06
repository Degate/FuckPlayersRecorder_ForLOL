using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using FuckPlayersRecorder_ForLOL.Service.Teamup;
using FuckPlayersRecorder_ForLOL.Models.V2;
using System;
using System.Threading.Tasks;

namespace LeagueOfLegendsBoxer.ViewModels
{
    public class PostDetailWindowViewModel : ObservableObject
    {
        private readonly ITeamupService _teamupService;

        private Post _post;
        public Post Post 
        {
            get => _post;
            set => SetProperty(ref _post, value);
        }

        public AsyncRelayCommand GoodCommandAsync { get; set; }


        internal async Task LoadPostDetailAsync(Post post)
        {
            Post = post;

            await Task.CompletedTask;
        }

        private async Task GoodAsync() 
        {
            try
            {
                var result = await _teamupService.GoodAsync(Post.Id);
                Post.HadGood = result.Item1;
                Post.GoodCount = result.Item2;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
