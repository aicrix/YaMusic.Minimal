using ReactiveUI;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using YandexMusicResolver;
using YandexMusicResolver.Config;
using System.Linq;
using System.Collections.ObjectModel;
using YandexMusicResolver.AudioItems;
using DynamicData;
using YandexMusicResolver.Loaders;
using System.Reactive;
using NAudio.Wave;
using System.Threading;

#nullable disable

namespace YaMusic.Minimal.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private YandexMusicMainResolver musicMainResolver;
        private int trackIndex;
        private string trackUrl;

        private bool isManuallyStopped;
        private bool isMinimalModeVisible = true;
        private bool isStandardModeVisible;
        private bool isFullModeVisible;
        private bool isPlaying;
        private bool isShuffleOn;

        private double windowWidth = 48;
        private double windowHeight = 48;

        private Random random = new Random();

        public ObservableCollection<TrackViewModel> Tracks { get; set; } = new();

        public WaveOutEvent outputDevice;

        public ReactiveCommand<Unit, Unit> DoubleTapCommand
        {
            get
            {
                return ReactiveCommand.Create(() =>
                {
                    IsMinimalModeVisible = false;
                    IsStandardModeVisible = true;
                    WindowHeight = 240;
                });
            }
        }

        public ReactiveCommand<Unit, Unit> ExpandCommand
        {
            get
            {
                return ReactiveCommand.Create(() =>
                {
                    WindowWidth = IsFullModeVisible ? 48 : 294;
                    IsFullModeVisible = !IsFullModeVisible;
                });
            }
        }

        public ReactiveCommand<string, Unit> PlayPauseMusicCommand => ReactiveCommand.CreateFromTask<string>(ControlMusic);

        public ReactiveCommand<Unit, Unit> ShuffleCommand => ReactiveCommand.Create(() =>
        {
            IsShuffleOn = !IsShuffleOn;
        });

        public bool IsMinimalModeVisible
        {
            get => isMinimalModeVisible;
            set => this.RaiseAndSetIfChanged(ref isMinimalModeVisible, value);
        }

        public bool IsStandardModeVisible
        {
            get => isStandardModeVisible;
            set => this.RaiseAndSetIfChanged(ref isStandardModeVisible, value);
        }

        public bool IsFullModeVisible
        {
            get => isFullModeVisible;
            set => this.RaiseAndSetIfChanged(ref isFullModeVisible, value);
        }

        public bool IsPlaying
        {
            get => isPlaying;
            set => this.RaiseAndSetIfChanged(ref isPlaying, value);
        }

        public bool IsShuffleOn
        {
            get => isShuffleOn;
            set => this.RaiseAndSetIfChanged(ref isShuffleOn, value);
        }

        public double WindowWidth
        {
            get => windowWidth;
            set => this.RaiseAndSetIfChanged(ref windowWidth, value);
        }

        public double WindowHeight
        {
            get => windowHeight;
            set => this.RaiseAndSetIfChanged(ref windowHeight, value);
        }

        public MainWindowViewModel()
        {
            InitializeYandexMusic();
        }

        private async void InitializeYandexMusic()
        {
            var httpClient = new HttpClient();
            var authService = new YandexMusicAuthService(httpClient);
            var credentialProvider = new YandexCredentialsProvider(authService, Secrets.Login, Secrets.Password, Secrets.Token);
            musicMainResolver = new YandexMusicMainResolver(credentialProvider, httpClient);

            var playlist = await musicMainResolver.PlaylistLoader.LoadPlaylist("yamusic-my2023", "5089719");
            var tracks = await playlist.LoadDataAsync();

            Tracks.AddRange(tracks.Select(x => new TrackViewModel(x)));

            outputDevice = new();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
            trackUrl = await musicMainResolver.DirectUrlLoader.GetDirectUrl(Tracks[trackIndex].Id);
            outputDevice.Init(new MediaFoundationReader(trackUrl));
        }

        private async void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!isManuallyStopped)
            {
                trackIndex = IsShuffleOn ? random.Next(Tracks.Count) : trackIndex + 1;
                if (Tracks.Count(x => x.IsPlaying) > 0)
                {
                    Tracks.First(x => x.IsPlaying).IsPlaying = false;
                }
                Tracks[trackIndex].IsPlaying = true;
                trackUrl = await musicMainResolver.DirectUrlLoader.GetDirectUrl(Tracks[trackIndex].Id);
                outputDevice.Stop();
                outputDevice.Init(new MediaFoundationReader(trackUrl));
                outputDevice.Play();
            }
            else isManuallyStopped = false;
        }

        private async Task ControlMusic(string commandType)
        {
            switch (commandType)
            {
                case "play_pause":
                    IsPlaying = !IsPlaying;
                    if (Tracks.Count(x => x.IsPlaying) < 1)
                    {
                        Tracks[trackIndex].IsPlaying = true;
                    }
                    if (outputDevice.PlaybackState == PlaybackState.Playing) outputDevice.Pause();
                    else outputDevice.Play();
                    break;
                case "prev":
                    IsPlaying = true;
                    trackIndex--;
                    if (Tracks.Count(x => x.IsPlaying) > 0)
                    {
                        Tracks.First(x => x.IsPlaying).IsPlaying = false;
                    }
                    Tracks[trackIndex].IsPlaying = true;
                    trackUrl = await musicMainResolver.DirectUrlLoader.GetDirectUrl(Tracks[trackIndex].Id);
                    isManuallyStopped = true;
                    outputDevice.Stop();
                    outputDevice.Init(new MediaFoundationReader(trackUrl));
                    outputDevice.Play();
                    break;
                case "next":
                    IsPlaying = true;
                    trackIndex = IsShuffleOn ? random.Next(Tracks.Count) : trackIndex + 1;
                    trackUrl = await musicMainResolver.DirectUrlLoader.GetDirectUrl(Tracks[trackIndex].Id);
                    if (Tracks.Count(x => x.IsPlaying) > 0)
                    {
                        Tracks.First(x => x.IsPlaying).IsPlaying = false;
                    }
                    Tracks[trackIndex].IsPlaying = true;
                    isManuallyStopped = true;
                    outputDevice.Stop();
                    outputDevice.Init(new MediaFoundationReader(trackUrl));
                    outputDevice.Play();
                    break;
            }
        }
    }
}
