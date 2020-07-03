using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Affdex;
using InstantImprovement.DataControl;
using InstantImprovement.SDKControl;
using LiveCharts;
using LiveCharts.Wpf;

namespace InstantImprovement.Windows
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window, Affdex.ImageListener, Affdex.ProcessStatusListener
    {
        public VideoWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            StartBackgroundProcessing();
        }

        private Thread ChartControlThread { get; set; }
        public CameraDetector Detector { get; private set; }

        private readonly RingBuffer _anger = new RingBuffer(60);
        private readonly RingBuffer _contempt = new RingBuffer(60);
        private readonly RingBuffer _disgust = new RingBuffer(60);
        private readonly RingBuffer _engagement = new RingBuffer(60);
        private readonly RingBuffer _fear = new RingBuffer(60);
        private readonly RingBuffer _joy = new RingBuffer(60);
        private readonly RingBuffer _sadness = new RingBuffer(60);
        private readonly RingBuffer _surprise = new RingBuffer(60);
        private readonly RingBuffer _valence = new RingBuffer(60);

        #region ChartControl
        /// <summary>
        /// Initialize Chart Series Collection with all available Emotions
        /// </summary>
        private void InitializeChart()
        {
            EmotionTimeline.DisableAnimations = true;
            EmotionTimeline.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Anger",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Contempt",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Disgust",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Engagement",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Fear",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Joy",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Sadness",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Surprise",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                },
                new LineSeries
                {
                    Title = "Valence",
                    PointGeometry = null,
                    Values = new ChartValues<float>()
                }
            };
        }

        /// <summary>
        /// Start Thread that continuously updates Main Chart
        /// </summary>
        private void UpdateChart()
        {
            Console.WriteLine("Chart Update Requested");
            Thread.Sleep(500);
            Dispatcher.Invoke(() =>
            {
                // Get Latest Buffer-Values
                EmotionTimeline.Series[0].Values = _anger.Values;
                EmotionTimeline.Series[1].Values = _contempt.Values;
                EmotionTimeline.Series[2].Values = _disgust.Values;
                EmotionTimeline.Series[3].Values = _engagement.Values;
                EmotionTimeline.Series[4].Values = _fear.Values;
                EmotionTimeline.Series[5].Values = _joy.Values;
                EmotionTimeline.Series[6].Values = _sadness.Values;
                EmotionTimeline.Series[7].Values = _surprise.Values;
                EmotionTimeline.Series[8].Values = _valence.Values;

                // Update Chart Control
                EmotionTimeline.Update(false, true);
                Console.WriteLine("Chart Update Done");
            });
        }
        #endregion ChartControl

        #region DataControl

        /// <summary>
        /// Continuously Update Emotions on Chart Control
        /// </summary>
        private void ExtractData(Dictionary<int, Affdex.Face> FaceDictionary)
        {
            if (FaceDictionary.Count != 0)
            {
                var temp = FaceDictionary.ElementAt(0).Value.Emotions;
                Console.WriteLine(temp.Anger);
                Console.WriteLine(temp.Contempt);
                Console.WriteLine(temp.Disgust);
                Console.WriteLine(temp.Engagement);
                Console.WriteLine(temp.Fear);
                Console.WriteLine(temp.Joy);
                Console.WriteLine(temp.Sadness);
                Console.WriteLine(temp.Surprise);
                Console.WriteLine(temp.Valence);

                _anger.Add(temp.Anger);
                _contempt.Add(temp.Contempt);
                _disgust.Add(temp.Disgust);
                _engagement.Add(temp.Engagement);
                _fear.Add(temp.Fear);
                _joy.Add(temp.Joy);
                _sadness.Add(temp.Sadness);
                _surprise.Add(temp.Surprise);
                _valence.Add(temp.Valence);

                //Dispatcher.Invoke(() =>
                //{
                //    // Get Latest Buffer-Values
                //    //EmotionTimeline.Series[0].Values.Add(temp.Anger); // = _anger.Values;
                //    //EmotionTimeline.Series[1].Values.Add(temp.Contempt); //= _contempt.Values;
                //    //EmotionTimeline.Series[2].Values.Add(temp.Disgust); // = _disgust.Values;
                //    //EmotionTimeline.Series[3].Values.Add(temp.Engagement); // = _engagement.Values;
                //    //EmotionTimeline.Series[4].Values.Add(temp.Fear); // = _fear.Values;
                //    //EmotionTimeline.Series[5].Values.Add(temp.Sadness); // = _joy.Values;
                //    //EmotionTimeline.Series[6].Values.Add(temp.Surprise); // = _sadness.Values;
                //    //EmotionTimeline.Series[7].Values.Add(temp.Surprise); //= _surprise.Values;
                //    //EmotionTimeline.Series[8].Values.Add(temp.Valence); // = _valence.Values;

                //    EmotionTimeline.Series[0].Values = _anger.Values;
                //    EmotionTimeline.Series[1].Values = _contempt.Values;
                //    EmotionTimeline.Series[2].Values = _disgust.Values;
                //    EmotionTimeline.Series[3].Values = _engagement.Values;
                //    EmotionTimeline.Series[4].Values = _fear.Values;
                //    EmotionTimeline.Series[5].Values = _joy.Values;
                //    EmotionTimeline.Series[6].Values = _sadness.Values;
                //    EmotionTimeline.Series[7].Values = _surprise.Values;
                //    EmotionTimeline.Series[8].Values = _valence.Values;
                //    EmotionTimeline.Update(false, true);
                //    Console.WriteLine("Chart Update Done");
                //});
            }
        }
        #endregion

        #region CameraControl
        /// <summary>
        /// Starts the camera processing.
        /// </summary>
        private void StartBackgroundProcessing()
        {
            try
            {
                // Instantiate CameraDetector using default camera ID
                const int cameraId = 0;
                const int numberOfFaces = 1;
                const int cameraFPS = 5;
                const int processFPS = 5;
                Detector = new Affdex.CameraDetector(cameraId, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);

                //Set location of the classifier data files, needed by the SDK
                Detector.setClassifierPath(FilePath.GetClassifierDataFolder());

                // Set the Classifiers that we are interested in tracking
                Detector.setDetectAllEmotions(true);
                Detector.setDetectAllExpressions(true);
                Detector.setDetectAllEmojis(false);
                Detector.setDetectGender(true);
                Detector.setDetectGlasses(true);

                Detector.setImageListener(this);
                Detector.setProcessStatusListener(this);

                Detector.start();
            }
            catch (Affdex.AffdexException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    // If this is a camera failure, then reset the application to allow the user to turn on/enable camera
                    if (ex.Message.Equals("Unable to open webcam."))
                    {
                        MessageBoxResult result = MessageBox.Show(ex.Message,
                                                                "InstantImprovement Error",
                                                                MessageBoxButton.OK,
                                                                MessageBoxImage.Error);
                        StopBackgroundProcessing();
                        return;
                    }
                }

                String message = String.IsNullOrEmpty(ex.Message) ? "InstantImprovement error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "InstantImprovement error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Stops the camera processing.
        /// </summary>
        private void StopBackgroundProcessing()
        {
            try
            {
                if ((Detector != null) && (Detector.isRunning()))
                {
                    Detector.stop();
                    Detector.Dispose();
                    Detector = null;
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "InstantImprovement error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Displays a alert with exception details
        /// </summary>
        /// <param name="exceptionMessage"> contains the exception details.</param>
        private void ShowExceptionAndShutDown(String exceptionMessage)
        {
            MessageBoxResult result = MessageBox.Show(exceptionMessage,
                                                        "InstantImprovement Error",
                                                        MessageBoxButton.OK,
                                                        MessageBoxImage.Error);
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                StopBackgroundProcessing();
            }));
        }


        #endregion CameraControl

        #region WindowControl

        /// <summary>
        /// Center the main window on the screen
        /// </summary>
        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VideoControl.Source = new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\videoplayback.mp4");
            InitializeChart();
            this.ContentRendered += Window_ContentRendered;

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            while (!VideoControl.IsLoaded)
            {
                Thread.Sleep(5);
            }
            VideoControl.Play();

            ChartControlThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    UpdateChart();
                }
            }));
            ChartControlThread.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ChartControlThread.Abort();
            StopBackgroundProcessing();
        }

        #endregion WindowControl

        #region InterfaceControl

        public void onImageResults(Dictionary<int, Face> faces, Affdex.Frame frame)
        {
            ExtractData(faces);
        }

        public void onImageCapture(Affdex.Frame frame){}

        public void onProcessingException(AffdexException ex)
        {
            String message = String.IsNullOrEmpty(ex.Message) ? "InstantImprovement error encountered." : ex.Message;
            ShowExceptionAndShutDown(message);
        }

        public void onProcessingFinished(){}

        #endregion InterfaceControl
    }
}
