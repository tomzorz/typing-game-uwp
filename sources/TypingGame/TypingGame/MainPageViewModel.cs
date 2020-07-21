using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media.DialProtocol;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Humanizer;
using PokeApiNet;

namespace TypingGame
{
    public class MainPageViewModel : ViewModelBase
    {
        public ICommand DoneCommand { get; }

        public ICommand ResetCommand { get; }

        private List<MyPokemon> _unsolved;

        public ObservableCollection<MyPokemon> SolvedPokemons { get; }

        private DateTimeOffset _startedGame;

        public MainPageViewModel()
        {
            DoneCommand = new RelayCommand(HandleDone);
            ResetCommand = new RelayCommand(HandleReset);
            SolvedPokemons = new ObservableCollection<MyPokemon>();
            Load();
            PropertyChanged += MainPageViewModel_PropertyChanged;
            _dt = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(33)
            };
            _dt.Tick += _dt_Tick;
        }

        private void _dt_Tick(object sender, object e)
        {
            var currentTime = DateTimeOffset.Now;
            var diff = currentTime - _startedGame;
            Clock = $"{diff.TotalSeconds:F2}s";
        }

        private async void MainPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // other property was updated, we don't care
            if (e.PropertyName != nameof(CurrentPokemon)) return;

            // empty pokemon name, invalid for sure
            if (string.IsNullOrWhiteSpace(CurrentPokemon)) return;

            CheckStartGame();

            // find pokemon in unsolved list by lowercase comparison
            var searchterm = CurrentPokemon.ToLowerInvariant();
            var found = _unsolved.FirstOrDefault(x => x.Name == searchterm);

            // no such pokemon, bye
            if(found == null) return;

            _unsolved.Remove(found);
            SolvedPokemons.Add(found);
            await Task.Delay(1);
            CurrentPokemon = string.Empty;
        }

        private void CheckStartGame()
        {
            if (SolvedPokemons.Count != 0) return;
            if (_startedGame > DateTimeOffset.MaxValue) return;
            _startedGame = DateTimeOffset.Now;
            _dt.Start();
        }

        private async void Load()
        {
            var client = new PokeApiClient();
            _originalPokemonList = await client.GetNamedResourcePageAsync<Pokemon>(151, 0);
            HandleReset();
        }

        private void HandleReset()
        {
            _dt.Stop();
            _startedGame = DateTimeOffset.MinValue;
            SolvedPokemons.Clear();
            _unsolved = new List<MyPokemon>(_originalPokemonList.Results.Select((x, i) => new MyPokemon
            {
                Name = x.Name,
                Number = i + 1
            }).ToList());
            Clock = "";
        }

        private async void HandleDone()
        {
            _dt.Stop();
            var currentTime = DateTimeOffset.Now;
            var diff = currentTime - _startedGame;
            var dialog = new MessageDialog($"Congratulations you've found {SolvedPokemons.Count} pokemon(s) in {diff.Humanize(3)}!");
            await dialog.ShowAsync();
            HandleReset();
        }

        private string _currentPokemon;
        private NamedApiResourceList<Pokemon> _originalPokemonList;

        public string CurrentPokemon
        {
            get { return _currentPokemon; }
            set { Set(() => CurrentPokemon, ref _currentPokemon, value); }
        }

        private string _clock;
        private DispatcherTimer _dt;

        public string Clock
        {
            get { return _clock; }
            set { Set(() => Clock, ref _clock, value); }
        }
    }
}
