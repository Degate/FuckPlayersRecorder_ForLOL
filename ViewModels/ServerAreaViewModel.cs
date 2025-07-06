using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using FuckPlayersRecorder_ForLOL.Service.Teamup;
using FuckPlayersRecorder_ForLOL.Service.Teamup.Dtos;
using FuckPlayersRecorder_ForLOL.Models;
using FuckPlayersRecorder_ForLOL.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LeagueOfLegendsBoxer.ViewModels
{
    public class ServerAreaViewModel : ObservableObject
    {
        private string _name;
        public string Name 
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private ServerArea _serverArea;
        public ServerArea ServerArea 
        {
            get { return _serverArea; }
            set { SetProperty(ref _serverArea, value); }
        }

        private ObservableCollection<ServerArea> _serverAreas;
        public ObservableCollection<ServerArea> ServerAreas 
        {
            get { return _serverAreas; }
            set { SetProperty(ref _serverAreas, value); }
        }

        public AsyncRelayCommand SaveCommandAsync { get; set; }
        public RelayCommand LoadCommand { get; set; }


        private async Task SaveAsync() 
        {
            if (ServerArea == null)
                return;
        }

        private void Load() 
        {
            Name = Constant.Account.DisplayName;
            ServerArea = string.IsNullOrEmpty(Constant.Account?.ServerArea) ? null : ServerAreas.FirstOrDefault(x => x.Label == Constant.Account?.ServerArea);
        }
    }
}
