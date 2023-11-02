using LiveCharts;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts.Defaults;
using System.Threading;

namespace Doctor
{
    /// <summary>
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }

        private StepLineSeriesBuilder stepLineSerieHeart;
        private StepLineSeriesBuilder stepLineSerieKm;

        private int cHeart = 1;
        private int cKm = 1;

        public ChartWindow(ClientData data)
        {
            InitializeComponent();

            stepLineSerieHeart = new StepLineSeriesBuilder("Heartrate");
            stepLineSerieKm = new StepLineSeriesBuilder("Kilometer");
            stepLineSerieHeart.GetStepLineSerie().PointGeometry = null;
            stepLineSerieKm.GetStepLineSerie().PointGeometry = null;

            bool fsSpeed = true;
            bool fsHeart = true;

            List<uint> speedList = new List<uint>();
            List<uint> heartbeatList = new List<uint>();

            foreach (var item in data.SpeedData)
            {
                speedList.Add(item.speed);
            }

            foreach (var item in data.HeartBeatData)
            {
                heartbeatList.Add(item.heartBeat);
            }

            SeriesCollection = new SeriesCollection { stepLineSerieKm.GetStepLineSerie(), stepLineSerieHeart.GetStepLineSerie() };

            data.SpeedData.CollectionChanged += (s, e) =>
            {
                if (e.NewItems == null) return;

                if (fsSpeed)
                {
                    foreach (var speed in speedList)
                    {
                        var newPoint = new ObservablePoint
                        {
                            X = cKm,
                            Y = speed
                        };

                        stepLineSerieKm.GetStepLineSerie().Values.Add(newPoint);
                        cKm++;
                    }

                    fsSpeed = false;
                }

                Task.Run(() =>
                {
                    // Separate thread to handle SpeedData changes
                    foreach (SpeedData item in e.NewItems)
                    {
                        var newPoint = new ObservablePoint
                        {
                            X = Interlocked.Increment(ref cKm), // Thread-safe increment
                            Y = item.speed
                        };

                        Dispatcher.Invoke(() => // Update UI on the main thread
                        {
                            stepLineSerieKm.GetStepLineSerie().Values.Add(newPoint);
                        });
                    }
                });
            };

            data.HeartBeatData.CollectionChanged += (s, e) =>
            {
                if (e.NewItems == null) return;

                if (fsHeart)
                {
                    foreach (var heart in heartbeatList)
                    {
                        var newPoint = new ObservablePoint
                        {
                            X = cKm,
                            Y = heart
                        };

                        stepLineSerieHeart.GetStepLineSerie().Values.Add(newPoint);
                        cHeart++;
                    }

                    fsHeart = false;
                }

                Task.Run(() =>
                {
                    // Separate thread to handle HeartBeatData changes
                    foreach (HeartBeatData item in e.NewItems)
                    {
                        var newPoint = new ObservablePoint
                        {
                            X = Interlocked.Increment(ref cHeart), // Thread-safe increment
                            Y = item.heartBeat
                        };

                        Dispatcher.Invoke(() => // Update UI on the main thread
                        {
                            stepLineSerieHeart.GetStepLineSerie().Values.Add(newPoint);
                        });
                    }
                });
            };

            // Set DataContext
            DataContext = this;
        }
    }
}
