using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Mapping;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ArcGISPro2GoogleEarthPro
{
    public partial class PageArc2Earth : Page
    {
        private A2E a2e = A2E.GetInstance();

        // Timer to poll for map view extent changes.
        private DispatcherTimer _viewUpdateTimer;
        // Keep track of the previous extent so we only update on change.
        private Envelope _previousExtent;

        public PageArc2Earth()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = a2e;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (MapView.Active == null)
            {
                MessageBox.Show("No active map view detected! Please open a map before starting synchronization.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Update the button colors.
            StartButton.Background = new SolidColorBrush(Color.FromRgb(0, 100, 0)); // Dark Green
            StopButton.Background = new SolidColorBrush(Color.FromRgb(255, 160, 160)); // Light Red
            a2e.WatchingYou = "Whatca looking at?";

            // Check if Google Earth is running.
            var processExists = Process.GetProcessesByName("googleearth").Any();
            if (!processExists)
            {
                try
                {
                    Process.Start(@"C:\Program Files\Google\Google Earth Pro\client\googleearth.exe");
                    //Delay 5 seconds
                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open Google Earth: {ex.Message}");
                    return;
                }
            }
            // Start polling the active map view.
            StartPolling();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the button colors.
            StopButton.Background = new SolidColorBrush(Color.FromRgb(139, 0, 0)); // Dark Red
            StartButton.Background = new SolidColorBrush(Color.FromRgb(144, 238, 144)); // Light Green
            a2e.WatchingYou = "Coffeee break...";

            // Stop polling for view updates.
            StopPolling();
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWin = new AboutWindow();
            aboutWin.ShowDialog(); // Open the About Window
        }
        /// <summary>
        /// Starts a DispatcherTimer to poll for extent changes every 500ms.
        /// </summary>
        private void StartPolling()
        {
            if (_viewUpdateTimer == null)
            {
                _viewUpdateTimer = new DispatcherTimer();
                _viewUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
                _viewUpdateTimer.Tick += ViewUpdateTimer_Tick;
            }
            _viewUpdateTimer.Start();
        }

        /// <summary>
        /// Stops the DispatcherTimer and resets the previously observed extent.
        /// </summary>
        private void StopPolling()
        {
            _viewUpdateTimer?.Stop();
            _previousExtent = null;
        }

        /// <summary>
        /// Checks if the map extent has changed. If so, updates Google Earth's view.
        /// </summary>
        private void ViewUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (MapView.Active == null)
                return;

            Envelope currentExtent = MapView.Active.Extent;

            if (_previousExtent == null || !AreEnvelopesEqual(_previousExtent, currentExtent))
            {
                _previousExtent = currentExtent;
                SyncGoogleEarthView(currentExtent);
            }
        }

        /// <summary>
        /// Updates Google Earth's view by creating a KML file based on the current ArcGIS Pro extent.
        /// The KML file is stored in a folder called "A2E" within the user's Documents folder.
        /// This version projects the center point to WGS84 (decimal degrees) and uses the active view’s scale as the range.
        /// </summary>
        private void SyncGoogleEarthView(Envelope extent)
        {
            if (extent == null) return;

            // Create the center point using the MapPointBuilder with the map's native spatial reference.

            MapPoint centerPoint = MapPointBuilder.CreateMapPoint(
                MapView.Active.Camera.X,
                MapView.Active.Camera.Y,
                MapView.Active.Map.SpatialReference);

            // Project the center to WGS84 so the coordinates are in decimal degrees.
            MapPoint wgs84Center = GeometryEngine.Instance.Project(centerPoint, SpatialReferences.WGS84) as MapPoint;
            double centerLon = wgs84Center.X;
            double centerLat = wgs84Center.Y;

            // Use the current scale of the active view as the range.


            //double currentRange = (((MapView.Active.Camera.Scale / 12) * .0254) * MapView.Active.Camera.ViewportHeight);
            double currentRange = MapView.Active.Camera.ViewportHeight;


            // Build the KML snippet using the projected center and dynamic range.
            string kml = $@"<kml xmlns=""http://www.opengis.net/kml/2.2"">
  <Placemark>
    <name>Current View</name>
    <LookAt>
      <longitude>{centerLon}</longitude>
      <latitude>{centerLat}</latitude>
      <altitude>0</altitude>
      <range>{currentRange}</range>
      <tilt>0</tilt>
      <heading>0</heading>
    </LookAt>
  </Placemark>
</kml>";

            // Determine the path to the Documents\A2E folder.
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = System.IO.Path.Combine(documentsPath, "A2E");

            // Create the directory if it doesn't already exist.
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            // Define the path to the KML file.
            string kmlPath = System.IO.Path.Combine(folderPath, "update_view.kml");

            try
            {
                // Write the KML file.
                System.IO.File.WriteAllText(kmlPath, kml);

                // Use ProcessStartInfo with UseShellExecute = true so that the file opens in its default application (Google Earth).
                ProcessStartInfo startInfo = new ProcessStartInfo(kmlPath)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update Google Earth view: " + ex.Message);
            }
        }

        /// <summary>
        /// Determines whether two envelopes are effectively equal by comparing their coordinates with a small tolerance.
        /// </summary>
        private bool AreEnvelopesEqual(Envelope env1, Envelope env2)
        {
            if (env1 == null || env2 == null)
                return false;

            double tolerance = 0.000000001; // Updated tolerance value.
            return (Math.Abs(env1.XMin - env2.XMin) < tolerance &&
                    Math.Abs(env1.YMin - env2.YMin) < tolerance &&
                    Math.Abs(env1.XMax - env2.XMax) < tolerance &&
                    Math.Abs(env1.YMax - env2.YMax) < tolerance);
        }
    }

    public class A2E : INotifyPropertyChanged
    {
        private static A2E Instance = new A2E();
        private A2E() { }
        public static A2E GetInstance() { if (Instance == null) { Instance = new A2E(); } return Instance; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        string _WatchingYou = "Coffeee break...";

        public string WatchingYou
        {
            get { return _WatchingYou; }
            set { _WatchingYou = value; NotifyPropertyChanged(); }
        }
    }
}
