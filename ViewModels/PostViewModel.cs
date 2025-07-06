using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using FuckPlayersRecorder_ForLOL.Service.Teamup;
using FuckPlayersRecorder_ForLOL.Service.Teamup.Dtos;
//using FuckPlayersRecorder_ForLOL.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessageBox = HandyControl.Controls.MessageBox;

namespace LeagueOfLegendsBoxer.ViewModels
{
    public class PostViewModel : ObservableObject
    {
        private string _imageUri1;
        private string _imageUri2;
        private string _imageUri3;
        public List<ImageSelector> ImageSelectors { get; set; }

        private Dictionary<string, string> _postCategories;
        public Dictionary<string, string> PostCategories
        {
            get { return _postCategories; }
            set => SetProperty(ref _postCategories, value);
        }

        private KeyValuePair<string, string> _postCategory;
        public KeyValuePair<string, string> PostCategory 
        {
            get { return _postCategory; }
            set=>SetProperty(ref _postCategory, value);
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set => SetProperty(ref _title, value);
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set => SetProperty(ref _content, value);
        }

        private readonly ITeamupService _teamupService;

        public AsyncRelayCommand PostCommandAsync { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public async Task PostAsync()
        {

                var result = await _teamupService.CreateOrUpdatePostAsync(new PostCreateOrUpdateDto()
                {
                    Id = null,
                    Title = Title,
                    Content = Content,
                    Image_1 = _imageUri1,
                    Image_2 = _imageUri2,
                    Image_3 = _imageUri3,
                    PostCategory = (PostCategory)int.Parse(PostCategory.Key)
                });

            
        }

    }
}
