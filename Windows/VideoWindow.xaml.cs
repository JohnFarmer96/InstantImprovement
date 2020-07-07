// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VideoWindow.xaml.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// Interaction logic for VideoWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using InstantImprovement.DataControl;
using InstantImprovement.SDKControl;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace InstantImprovement.Windows
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow
    {
        /// <summary>
        /// Indicate if Window is Active
        /// </summary>
        private bool _videoWindowActive;

        /// <summary>
        /// Initialization of <see cref="VideoWindow"/>
        /// </summary>
        public VideoWindow()
        {
            InitializeComponent();
            DataManager.CenterWindowOnScreen(this);
            DataManager.InitializeRingBufferDictionary();
            InitializeFaceWatcher();
        }

        /// <summary>
        /// Thread that updates live-chart on UI
        /// </summary>
        private Thread ChartAnimationThread { get; set; }

        /// <summary>
        /// Handle Export-Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportResults_Clicked(object sender, EventArgs e)
        {
            VideoControl.Stop();
            DataManager.FaceWatcher.StopDetector();
            ChartAnimationThread.Abort();

            var exportThread = new Thread(new ThreadStart(DataManager.ExportResultsToExcelSheet));
            exportThread.Start();

            this.Close();
        }

        /// <summary>
        /// Continuously Update Emotions on Chart Control
        /// </summary>
        private void ExtractData(object sender, FaceWatcherEventArgs e)
        {
            Dictionary<int, Affdex.Face> FaceDictionary = e.Faces;

            if (FaceDictionary.Count != 0)
            {
                foreach (string classifier in DataManager.FaceWatcher.EnabledClassifiers)
                {
                    float value = DataManager.ExtractFeaturePropertyValue(FaceDictionary.ElementAt(0).Value, classifier);
                    DataManager.TrackData(classifier, value);
                }
            }
        }

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
        /// Initialize FaceWatcher according to VideoWindow-Purposes
        /// </summary>
        private void InitializeFaceWatcher()
        {
            const double cameraFPS = 20;
            const double processFPS = 20;
            const uint numberOfFaces = 1;
            DataManager.FaceWatcher.ConfigureDetector(cameraFPS, processFPS, numberOfFaces);
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
                    if (DataManager.RingBuffers.TryGetValue(emotionTimeline.Title, out RingBuffer ringBuffer))
                    {
                        emotionTimeline.Values = ringBuffer.Values;
                    }
                }

                // Update Chart Control
                EmotionTimeline.Update(false, true);
            });
        }

        /// <summary>
        /// Handle Window-Closing Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoWindow_Closing(object sender, CancelEventArgs e)
        {
            _videoWindowActive = false;
            DataManager.FaceWatcher.StopDetector();
            ChartAnimationThread.Abort();
        }

        /// <summary>
        /// Handle Window-Content-Rendered Eventf
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoWindow_ContentRendered(object sender, EventArgs e)
        {
            DataManager.FaceWatcher.DetectorStarted += new EventHandler(StartVideo);
            DataManager.FaceWatcher.DetectorStopped += new EventHandler(PauseVideo);
            DataManager.FaceWatcher.ImageReceived += new EventHandler<FaceWatcherEventArgs>(ExtractData);
            DataManager.FaceWatcher.StartDetector();

            _videoWindowActive = true;
            StartChartAnimationThread();
        }

        /// <summary>
        /// Handle Window-Loaded Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeChart();
            this.ContentRendered += VideoWindow_ContentRendered;
        }
    }
}