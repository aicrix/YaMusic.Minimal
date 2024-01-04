using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YandexMusicResolver.AudioItems;

namespace YaMusic.Minimal.ViewModels
{
    public class TrackViewModel : ViewModelBase
    {
        private readonly YandexMusicTrack _track;
        private bool isPlaying;

        public string Title => _track.Title;
        public string Author => _track.Author;
        public string? ArtworkUrl => _track.ArtworkUrl;
        public long Id => _track.Id;

        public bool IsPlaying
        {
            get => isPlaying;
            set => this.RaiseAndSetIfChanged(ref isPlaying, value);
        }


        public TrackViewModel(YandexMusicTrack track)
        {
            _track = track;
        }
    }
}
