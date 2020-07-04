using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
    public partial class VideoWindow
    {
        private bool _videoWindowActive;

        public VideoWindow()
        {
            InitializeComponent();
            DataManager.CenterWindowOnScreen(this);
            InitializeFaceWatcher();
        }

        /// <summary>
        /// Initialize FaceWatcher according to VideoWindow-Purposes
        /// </summary>
        private void InitializeFaceWatcher()
        {
            const double cameraFPS = 20;
            const double processFPS = 20;
            const uint numberOfFaces = 1;
            DataManager.FaceWatcher.ConfigureDetector(cameraFPS, processFPS, numberOfFaces);
        }

        #region Properties
        private Thread ChartAnimationThread { get; set; }
        #endregion

        #region ChartControl
        /// <summary>
        /// Initialize Chart Series Collection with all available Emotions
        /// </summary>
        private void InitializeChart()
        {
            EmotionTimeline.DisableAnimations = true;
            foreach (KeyValuePair<string, RingBuffer> bufferPair in DataManager.RingBuffers)
            {
                EmotionTimeline.Series.Add(new LineSeries { Title = bufferPair.Key, PointGeometry = null, Values = bufferPair.Value.Values });
            }
        }

        /// <summary>
        /// Start Thread that continuously updates Emotion-Chart
        /// </summary>
        private void StartChartAnimationThread()
        {
            ChartAnimationThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    UpdateChart();
                }
            }));
            ChartAnimationThread.Start();
        }

        /// <summary>
        /// Push new values to Emotion-Chart and show Updates
        /// </summary>
        private void UpdateChart()
        {
            Thread.Sleep(100);
            Dispatcher.Invoke(() =>
            {
                // Get Latest Buffer-Values
                foreach (var emotionTimeline in EmotionTimeline.Series)
                {
                    if(DataManager.RingBuffers.TryGetValue(emotionTimeline.Title, out RingBuffer ringBuffer))
                    {
                        emotionTimeline.Values = ringBuffer.Values;
                    }
                }

                // Update Chart Control
                EmotionTimeline.Update(false, true);
            });
        }
        #endregion ChartControl

        #region DataControl

        /// <summary>
        /// Continuously Update Emotions on Chart Control
        /// </summary>
        private void ExtractData(object sender, ImageListenerEventArgs e)
        {
            Dictionary<int, Affdex.Face> FaceDictionary = e.Faces;

            if (FaceDictionary.Count != 0)
            {
                Emotions emotions = FaceDictionary.ElementAt(0).Value.Emotions;
                Expressions expressions = FaceDictionary.ElementAt(0).Value.Expressions;

                foreach (String classifier in DataManager.FaceWatcher.EnabledClassifiers)
                {
                    PropertyInfo expressionPropertyInfo = expressions.GetType().GetProperty(classifier);
                    if(expressionPropertyInfo != null)
                    {
                        DataManager.TrackData(classifier, (float)(expressionPropertyInfo.GetValue(expressions)));
                        break;
                    }

                    PropertyInfo emotionPropertyInfo = emotions.GetType().GetProperty(classifier);
                    if(emotionPropertyInfo != null)
                    {
                        DataManager.TrackData(classifier, (float)(emotionPropertyInfo.GetValue(emotions)));
                    }
                    
                }
            }
        }
        #endregion

        #region WindowControl

        private void VideoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeChart();
            this.ContentRendered += VideoWindow_ContentRendered;

        }

        private void VideoWindow_ContentRendered(object sender, EventArgs e)
        {
            DataManager.FaceWatcher.DetectorStarted += new EventHandler(StartVideo);
            DataManager.FaceWatcher.DetectorStopped += new EventHandler(PauseVideo);
            DataManager.FaceWatcher.ImageReceived += new EventHandler<ImageListenerEventArgs>(ExtractData);
            DataManager.FaceWatcher.StartDetector();

            _videoWindowActive = true;
            StartChartAnimationThread();

        }

        private void VideoWindow_Closing(object sender, CancelEventArgs e)
        {
            _videoWindowActive = false;
            DataManager.FaceWatcher.StopDetector();
            ChartAnimationThread.Abort();
        }

        #endregion WindowControl

        #region VideoControl
        /// <summary>
        /// Start Video Animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartVideo(object sender, EventArgs e)
        {
            VideoControl.Source = new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\videoplayback.mp4");
            while (!VideoControl.IsLoaded && !_videoWindowActive)
            {
                Thread.Sleep(5);
            }
            VideoControl.Play();
        }

        /// <summary>
        /// Pause Video Animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseVideo(object sender, EventArgs e)
        {
            if (VideoControl.CanPause)
            {
                VideoControl.Pause();
            }
        }
        #endregion VideoControl

    }
}
