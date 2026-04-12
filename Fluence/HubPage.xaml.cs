using Fluence.Common;
using Fluence.ViewModels;
using Fluence.Views;
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

        private const string BackgroundFileName = "cached_background.jpg";
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

            if (HubViewModel.IsOverviewDirty)
            {
                await this.hubViewModel.LoadOverviewAsync();
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

        private void EditProfileAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(SettingsPage), "profile"))
            {
                throw new Exception("Failed to navigate to profile form.");
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

        private double _targetOpacity = 0.6;
        private double _currentOpacity = 0.6;

        private void hub_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isRenderingHooked) return;
            _isRenderingHooked = true;

            var scrollViewer = GetScrollViewer(hub);
            if (scrollViewer != null)
            {
                _targetOpacity = 0.6;
                _currentOpacity = 0.6;
                BackgroundDimmer.Opacity = 0.6;

                scrollViewer.ViewChanged += (s, args) =>
                {
                    if (scrollViewer.ScrollableWidth > 0)
                    {
                        _targetX = -scrollViewer.HorizontalOffset * 0.5;
                        System.Diagnostics.Debug.WriteLine($"hub_Loaded: ScrollableWidth={scrollViewer.ScrollableWidth}, Offset={scrollViewer.HorizontalOffset}, TargetX={_targetX}");
                        
                        double sectionWidth = 360;
                        double progress = Math.Min(1, scrollViewer.HorizontalOffset / sectionWidth);
                        _targetOpacity = 0.6 + (0.2 * progress);
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

        private async System.Threading.Tasks.Task InitializeBackgroundAsync()
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Starting...");

            var systemVM = new SystemViewModel();

            try
            {
                bool cacheExists = false;
                try
                {
                    var cachedFile = await localFolder.GetFileAsync(BackgroundFileName);
                    var properties = await cachedFile.GetBasicPropertiesAsync();
                    using (var stream = await cachedFile.OpenReadAsync())
                    {
                        if (properties.Size > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Loading from cache...");
                            cacheExists = true;
                            BitmapImage bitmap = new BitmapImage();
                            await bitmap.SetSourceAsync(stream);
                            if (BackgroundImageBrush != null)
                            {
                                BackgroundImageBrush.ImageSource = bitmap;
                            }
                        }
                    }
                }
                catch (Exception) { }

                bool shouldFetch = false;
                if (systemVM.IsIntervalRefreshEnabled)
                {
                    double intervalMinutes = 60;
                    double parsedInterval;
                    if (double.TryParse(systemVM.RefreshIntervalMinutes, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedInterval))
                    {
                        intervalMinutes = parsedInterval;
                    }

                    if (!cacheExists || DateTime.Now >= systemVM.LastRefreshTime.AddMinutes(intervalMinutes))
                    {
                        shouldFetch = true;
                    }
                }
                else
                {
                    if (!cacheExists)
                    {
                        shouldFetch = true;
                    }
                }

                if (shouldFetch)
                {
                    try
                    {
                        string currentUrl = WallpaperUrl;
                        System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Fetching random wallpaper from " + currentUrl);
                        
                        using (var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
                        {
                            filter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
                            filter.AllowAutoRedirect = false; // disable auto-redirect to debug the link

                            using (var client = new Windows.Web.Http.HttpClient(filter))
                            {
                                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko");
                                
                                Windows.Web.Http.HttpResponseMessage response = null;
                                
                                try
                                {
                                    System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Sending initial request...");
                                    response = await client.GetAsync(new Uri(currentUrl));
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"InitializeBackgroundAsync: Initial request exception: 0x{ex.HResult:X8} - {ex.Message}");
                                    if ((uint)ex.HResult == 0x80072EFD)
                                    {
                                        System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: HTTPS connection failed. Retrying with HTTP...");
                                        currentUrl = currentUrl.Replace("https://", "http://");
                                        try { response = await client.GetAsync(new Uri(currentUrl)); }
                                        catch (Exception ex2) { System.Diagnostics.Debug.WriteLine($"InitializeBackgroundAsync: HTTP retry failed: 0x{ex2.HResult:X8}"); }
                                    }
                                }

                                if (response != null && ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399))
                                {
                                    string location = response.Headers.Location.ToString();
                                    System.Diagnostics.Debug.WriteLine($"InitializeBackgroundAsync: Redirect detected ({response.StatusCode}) to: {location}");
                                    
                                    try 
                                    {
                                        response = await client.GetAsync(new Uri(location));
                                    }
                                    catch (Exception ex3)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"InitializeBackgroundAsync: Redirect follow failed: 0x{ex3.HResult:X8}");
                                        if ((uint)ex3.HResult == 0x80072EFD)
                                        {
                                            System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Redirect HTTPS failed. Retrying redirect with HTTP...");
                                            response = await client.GetAsync(new Uri(location.Replace("https://", "http://")));
                                        }
                                    }
                                }

                                if (response == null || !response.IsSuccessStatusCode)
                                {
                                    System.Diagnostics.Debug.WriteLine($"InitializeBackgroundAsync: Fetch failed. Status: {(response != null ? (int)response.StatusCode : 0)}");
                                    return;
                                }

                                var buffer = await response.Content.ReadAsBufferAsync();
                                System.Diagnostics.Debug.WriteLine($"InitializeBackgroundAsync: Downloaded {buffer.Length} bytes");

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
                                                System.Diagnostics.Debug.WriteLine($"InitializeBackgroundAsync: New wallpaper applied. {bitmap.PixelWidth}x{bitmap.PixelHeight}");
                                                hub.UpdateLayout();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: UI set error: " + ex.Message);
                                    }
                                });

                                var newFile = await localFolder.CreateFileAsync(BackgroundFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                                using (var stream = await newFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                                {
                                    await stream.WriteAsync(buffer);
                                    await stream.FlushAsync();
                                }
                                System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Cache updated");
                                
                                systemVM.LastRefreshTime = DateTime.Now;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Fetch error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("InitializeBackgroundAsync: Fatal error: " + ex.Message);
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
