using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Affdex;
using LiveCharts;
using LiveCharts.Wpf;

namespace InstantImprovement
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window
    {
        public VideoWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            InitializeChart();

            MainWindow = mainWindow;
            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public MainWindow MainWindow { get; private set; }
        private Thread T { get; set; }

        /// <summary>
        /// Initialize Chart Series Collection with all available Emotions
        /// </summary>
        private void InitializeChart()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Anger",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Contempt",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Disgust",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Engagement",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Fear",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Joy",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Sadness",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Surprise",
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Valence",
                    PointGeometry = null
                }
            };
        }


        /// <summary>
        /// Continuously Update Emotions on Chart Control
        /// </summary>
        private void UpdateChart()
        {
            Task.Delay(100);
            var temp = MainWindow.Emotions;
            Console.WriteLine(MainWindow.Detector.isRunning());
            Console.WriteLine(temp.Anger);
            Console.WriteLine(temp.Contempt);
            Console.WriteLine(temp.Disgust);
            Console.WriteLine(temp.Engagement);
            Console.WriteLine(temp.Fear);
            Console.WriteLine(temp.Joy);
            Console.WriteLine(temp.Sadness);
            Console.WriteLine(temp.Surprise);
            Console.WriteLine(temp.Valence);
            //Dispatcher.Invoke(() =>
            //{
            //    SeriesCollection[0].Values.Add(MainWindow.Emotions.Anger);
            //    SeriesCollection[1].Values.Add(MainWindow.Emotions.Contempt);
            //    SeriesCollection[2].Values.Add(MainWindow.Emotions.Disgust);
            //    SeriesCollection[3].Values.Add(MainWindow.Emotions.Engagement);
            //    SeriesCollection[4].Values.Add(MainWindow.Emotions.Fear);
            //    SeriesCollection[5].Values.Add(MainWindow.Emotions.Joy);
            //    SeriesCollection[6].Values.Add(MainWindow.Emotions.Sadness);
            //    SeriesCollection[7].Values.Add(MainWindow.Emotions.Surprise);
            //    SeriesCollection[8].Values.Add(MainWindow.Emotions.Valence);
            //});
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            T = new Thread( () =>
            {
                while(true)
                {
                    UpdateChart();
                }
            });
            T.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            T.Abort();
        }
    }
}
