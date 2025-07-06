﻿using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using FuckPlayersRecorder_ForLOL.Resources;
using System;
using System.Windows;
using FuckPlayersRecorder_ForLOL.Service.Teamup;

namespace FuckPlayersRecorder_ForLOL.Models
{
    public class ChatMessage
    {
        public bool IsLoadData { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Desc { get; set; }
        public string ServerArea { get; set; }
        public string Rank_SOLO_5x5 { get; set; }
        public string Rank_FLEX_SR { get; set; }
        public string Message { get; set; }
        public bool IsSender { get; set; }
        public bool IsAdministrator { get; set; }
        public bool CurrentIsAdministrator { get; set; }
        public string Role { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateTimeText => ConvertDateTimeToText(CreateTime);
        public RelayCommand CopyCurrentUserNameCommand { get; set; }
        public AsyncRelayCommand DenySendMessageCommandAsync { get; set; }
        public AsyncRelayCommand OpenRecordByIdCommandAsync { get; set; }

        public ChatMessage()
        {
            CopyCurrentUserNameCommand = new RelayCommand(() =>
            {
                Clipboard.SetText(UserName);
            });

            DenySendMessageCommandAsync = new AsyncRelayCommand(async () =>
            {
            });

            OpenRecordByIdCommandAsync = new AsyncRelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(ServerArea) || string.IsNullOrEmpty(Constant.Account.ServerArea) || ServerArea != Constant.Account.ServerArea) 
                {

                    return;
                }

                //var summonerAnalyse = App.ServiceProvider.GetRequiredService<SummonerAnalyse>();
                //var summonerAnalyseViewModel = App.ServiceProvider.GetRequiredService<SummonerAnalyseViewModel>();
                //await summonerAnalyseViewModel.LoadPageAsync(UserId);
                //summonerAnalyse.Show();
            });
        }

        //TODO 统一方法
        public string ConvertDateTimeToText(DateTime dateTime)
        {
            if (dateTime.Year == DateTime.Now.Year && dateTime.DayOfYear == DateTime.Now.DayOfYear)
            {
                //今天
                return dateTime.ToString("t");
            }
            else if (dateTime.Year == DateTime.Now.Year)
            {
                //今年
                return dateTime.ToString("MM-dd HH:mm");
            }
            else
            {
                return dateTime.ToString("yyyy/MM/dd HH:mm");
            }
        }
    }
}
