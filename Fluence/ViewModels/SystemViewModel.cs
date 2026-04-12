using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Fluence.ViewModels
{
    public class SystemViewModel : INotifyPropertyChanged
    {
        private const string IsIntervalRefreshEnabledKey = "IsIntervalRefreshEnabled";
        private const string RefreshIntervalMinutesKey = "RefreshIntervalMinutes";
        private const string LastRefreshTimeKey = "LastRefreshTime";

        private bool _isIntervalRefreshEnabled;
        private string _refreshIntervalMinutes;
        private DateTime _lastRefreshTime;

        public event PropertyChangedEventHandler PropertyChanged;

        public SystemViewModel()
        {
            LoadSettings();
        }

        public bool IsIntervalRefreshEnabled
        {
            get { return _isIntervalRefreshEnabled; }
            set
            {
                if (_isIntervalRefreshEnabled != value)
                {
                    _isIntervalRefreshEnabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IntervalInputVisibility");
                }
            }
        }

        public string RefreshIntervalMinutes
        {
            get { return _refreshIntervalMinutes; }
            set
            {
                if (_refreshIntervalMinutes != value)
                {
                    _refreshIntervalMinutes = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastRefreshTime
        {
            get { return _lastRefreshTime; }
            set
            {
                _lastRefreshTime = value;
                SaveLastRefreshTime();
            }
        }

        public Windows.UI.Xaml.Visibility IntervalInputVisibility
        {
            get { return _isIntervalRefreshEnabled ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed; }
        }

        private void LoadSettings()
        {
            var settings = ApplicationData.Current.LocalSettings;

            if (settings.Values.ContainsKey(IsIntervalRefreshEnabledKey))
            {
                _isIntervalRefreshEnabled = (bool)settings.Values[IsIntervalRefreshEnabledKey];
            }
            else
            {
                _isIntervalRefreshEnabled = true;
            }

            _refreshIntervalMinutes = settings.Values.ContainsKey(RefreshIntervalMinutesKey) ? settings.Values[RefreshIntervalMinutesKey].ToString() : "60";

            if (settings.Values.ContainsKey(LastRefreshTimeKey))
            {
                _lastRefreshTime = DateTime.FromFileTime((long)settings.Values[LastRefreshTimeKey]);
            }
            else
            {
                _lastRefreshTime = DateTime.MinValue;
            }
        }

        public void SaveSettings()
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[IsIntervalRefreshEnabledKey] = _isIntervalRefreshEnabled;
            settings.Values[RefreshIntervalMinutesKey] = _refreshIntervalMinutes;
        }

        private void SaveLastRefreshTime()
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[LastRefreshTimeKey] = _lastRefreshTime.ToFileTime();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
