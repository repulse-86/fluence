using Fluence.Common;
using Fluence.ViewModels;
using Fluence.Views;
using Fluence.Services;
using System;
using System.IO;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;

namespace Fluence
{
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private readonly CategoryViewModel categoryViewModel = new CategoryViewModel();
        private readonly HubViewModel hubViewModel = new HubViewModel();
        private double _targetX = 0;
        private double _currentX = 0;
        private bool _isRenderingHooked = false;

        private const string WallpaperUrl = "https://picsum.photos/1920/1080";

        public HubPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.DefaultViewModel["CategoryViewModel"] = this.categoryViewModel;
            this.DefaultViewModel["HubViewModel"] = this.hubViewModel;

            Window.Current.VisibilityChanged += Window_VisibilityChanged;
        }

        private void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible && !hubViewModel.IsActive)
            {
                System.Diagnostics.Debug.WriteLine("HubPage: Window visible and Hub inactive. Triggering reload.");
                NavigationHelper_LoadState(this, null);
            }
        }

        public NavigationHelper NavigationHelper => this.navigationHelper;
        public ObservableDictionary DefaultViewModel => this.defaultViewModel;

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("HubPage: NavigationHelper_LoadState - Entering");

            if (HubViewModel.IsDirty)
            {
                HubViewModel.IsOverviewDirty = true;
                HubViewModel.IsHistoryDirty = true;
                HubViewModel.IsReportsDirty = true;
                HubViewModel.IsDirty = false;
            }

            if (HubViewModel.IsOverviewDirty || hubViewModel.Wallets == null || hubViewModel.Wallets.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("HubPage: NavigationHelper_LoadState - Triggering LoadOverviewAsync");
                await hubViewModel.LoadOverviewAsync();
                HubViewModel.IsOverviewDirty = false;
            }

            // wait 2 seconds before loading History and Reports
            System.Diagnostics.Debug.WriteLine("HubPage: NavigationHelper_LoadState - Waiting 1.5 seconds...");
            await System.Threading.Tasks.Task.Delay(1500);

            // check if we are still on the page before loading
            var currentFrame = Window.Current.Content as Frame;
            if (currentFrame == null || currentFrame.Content != this)
            {
                System.Diagnostics.Debug.WriteLine("HubPage: NavigationHelper_LoadState - Aborting load, user navigated away");
                return;
            }

            // set IsActive to true to uncollapse the sections first, so LoadHistoryAsync can populate UI
            this.hubViewModel.IsActive = true;

            // always reload History and Reports because they are unloaded in OnNavigatedFrom
            await this.hubViewModel.LoadHistoryAsync();
            await this.hubViewModel.LoadReportAsync();

            hub.UpdateLayout();

            HubViewModel.IsHistoryDirty = false;
            HubViewModel.IsReportsDirty = false;

            await InitializeBackgroundAsync();
            System.Diagnostics.Debug.WriteLine("HubPage: NavigationHelper_LoadState - Completed");
        }
        
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

        private void AddCategoryAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SettingsPage), "category"))
            {
                throw new Exception("Failed to navigate to category form.");
            }
        }

        private void QuickAddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(QuickAddPage)))
            {
                throw new Exception("Failed to navigate to quick add page.");
            }
        }

        private void AddWalletAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SettingsPage), "wallet"))
            {
                throw new Exception("Failed to navigate to wallet management.");
            }
        }

        private void SettingsAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SettingsPage), "system"))
            {
                throw new Exception("Failed to navigate to system settings.");
            }
        }

        private void AboutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(AboutPage)))
            {
                throw new Exception("Failed to navigate to about page.");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) => this.navigationHelper.OnNavigatedTo(e);
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("HubPage: OnNavigatedFrom - Unloading content");
            this.hubViewModel.IsActive = false;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private double _targetOpacity = AppConstants.DefaultOpacity;
        private double _currentOpacity = AppConstants.DefaultOpacity;

        private void hub_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isRenderingHooked) return;
            _isRenderingHooked = true;

            var scrollViewer = GetScrollViewer(hub);
            if (scrollViewer != null)
            {
                _targetOpacity = AppConstants.DefaultOpacity;
                _currentOpacity = AppConstants.DefaultOpacity;
                BackgroundDimmer.Opacity = AppConstants.DefaultOpacity;

                scrollViewer.ViewChanged += (s, args) =>
                {
                    if (scrollViewer.ScrollableWidth > 0)
                    {
                        _targetX = -scrollViewer.HorizontalOffset * 0.5;
                        System.Diagnostics.Debug.WriteLine($"hub_Loaded: ScrollableWidth={scrollViewer.ScrollableWidth}, Offset={scrollViewer.HorizontalOffset}, TargetX={_targetX}");
                        
                        double sectionWidth = 360;
                        double progress = Math.Min(1, scrollViewer.HorizontalOffset / sectionWidth);
                        _targetOpacity = AppConstants.DefaultOpacity + (0.2 * progress);
                    }
                };

                CompositionTarget.Rendering += (s, a) =>
                {
                    double xDiff = _targetX - _currentX;
                    if (Math.Abs(xDiff) > 0.0001)
                    {
                        _currentX += xDiff * 0.3;
                        BackgroundTransform.TranslateX = _currentX;
                    }

                    double oDiff = _targetOpacity - _currentOpacity;
                    if (Math.Abs(oDiff) > 0.0001)
                    {
                        _currentOpacity += oDiff * 0.3;
                        BackgroundDimmer.Opacity = _currentOpacity;
                    }
                };
            }
        }

        private static readonly System.Threading.SemaphoreSlim _backgroundLock = new System.Threading.SemaphoreSlim(1, 1);

        private async Task InitializeBackgroundAsync()
        {
            if (!await _backgroundLock.WaitAsync(0)) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Starting...");
                var systemVM = new SystemViewModel();

                bool shouldFetch = false;
                if (BackgroundImageBrush == null || BackgroundImageBrush.ImageSource == null)
                {
                    shouldFetch = true;
                }
                else if (systemVM.IsIntervalRefreshEnabled)
                {
                    double intervalMinutes = 60;
                    double parsedInterval;
                    if (double.TryParse(systemVM.RefreshIntervalMinutes, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedInterval))
                    {
                        intervalMinutes = parsedInterval;
                    }

                    if (DateTime.Now >= systemVM.LastRefreshTime.AddMinutes(intervalMinutes))
                    {
                        shouldFetch = true;
                    }
                }

                if (shouldFetch)
                {
                    try
                    {
                        string currentUrl = WallpaperUrl;
                        System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Fetching wallpaper into RAM from " + currentUrl);
                        
                        using (var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
                        {
                            filter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
                            filter.AllowAutoRedirect = false; 

                            using (var client = new Windows.Web.Http.HttpClient(filter))
                            {
                                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko");
                                
                                Windows.Web.Http.HttpResponseMessage response = null;
                                try
                                {
                                    response = await client.GetAsync(new Uri(currentUrl));
                                }
                                catch (Exception ex) when ((uint)ex.HResult == 0x80072EFD)
                                {
                                    System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: HTTPS failed, retrying HTTP...");
                                    currentUrl = currentUrl.Replace("https://", "http://");
                                    response = await client.GetAsync(new Uri(currentUrl));
                                }

                                // Handle redirects manually to be safe with HTTP/HTTPS transitions
                                if (response != null && ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399))
                                {
                                    string location = response.Headers.Location.ToString();
                                    try 
                                    {
                                        response = await client.GetAsync(new Uri(location));
                                    }
                                    catch (Exception)
                                    {
                                        response = await client.GetAsync(new Uri(location.Replace("https://", "http://")));
                                    }
                                }

                                if (response != null && response.IsSuccessStatusCode)
                                {
                                    var buffer = await response.Content.ReadAsBufferAsync();
                                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                    {
                                        try
                                        {
                                            using (var stream = new InMemoryRandomAccessStream())
                                            {
                                                await stream.WriteAsync(buffer);
                                                stream.Seek(0);
                                                BitmapImage bitmap = new BitmapImage();
                                                await bitmap.SetSourceAsync(stream);
                                                if (BackgroundImageBrush != null)
                                                {
                                                    BackgroundImageBrush.ImageSource = bitmap;
                                                    hub.UpdateLayout();
                                                }
                                            }
                                        }
                                        catch { }
                                    });
                                    systemVM.LastRefreshTime = DateTime.Now;
                                }
                                else
                                {
                                    throw new Exception("Fetch failed");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Fetch error: " + ex.Message);
                        if (BackgroundImageBrush != null)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                BackgroundImageBrush.ImageSource = null;
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Fatal error: " + ex.Message);
            }
            finally
            {
                _backgroundLock.Release();
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            ScrollViewer viewer = depObj as ScrollViewer;
            if (viewer != null) return viewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                ScrollViewer result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
