﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuckPlayersRecorder_ForLOL;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FuckPlayersRecorder_ForLOL.Models
{
    public class Skin : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SplashPath { get; set; }
        public string Description { get; set; }
        public IEnumerable<Skin> Chromas { get; set; } = new List<Skin>();

        public AsyncRelayCommand UseSkinCommandAsync { get; set; }
        public Skin()
        {
            UseSkinCommandAsync = new AsyncRelayCommand(UseSkinAsync);
        }

        private async Task UseSkinAsync() 
        {
        }
    }
}
