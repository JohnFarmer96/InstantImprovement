using InstantImprovement.SDKControl;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace InstantImprovement.DataControl
{
    public static class DataManager
    {
        const int RingBufferCapacity = 60;

        /// <summary>
        /// Assistant that manages User-Analysis according to functionality of AffdexSDK
        /// </summary>
        public static FaceWatcher FaceWatcher { get { return FaceWatcher.Instance; } }

        public static Dictionary<string, RingBuffer> RingBuffers { get; set; } = InitializeRingBufferDictionary();

        private static Dictionary<string, RingBuffer> InitializeRingBufferDictionary()
        {
            Dictionary<string, RingBuffer> ringBuffers = new Dictionary<string, RingBuffer>();
            foreach (String item in FaceWatcher.EnabledClassifiers)
            {
                ringBuffers.Add(item, new RingBuffer(RingBufferCapacity));
            }

            return ringBuffers;
        }

        public static void TrackData(String classifier, float newData)
        {
            if(!RingBuffers.ContainsKey(classifier))
            {
                RingBuffers.Add(classifier, new RingBuffer(RingBufferCapacity));
            }
            RingBuffers.TryGetValue(classifier, out var ringBuffer);
            ringBuffer.Add(newData);
        }

        /// <summary>
        /// Center UI-Window on Screen
        /// </summary>
        /// <param name="window">Window that needs to be centered</param>
        public static void CenterWindowOnScreen(System.Windows.Window window)
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = window.Width;
            double windowHeight = window.Height;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
            window.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        /// <summary>
        /// Displays a alert with exception details
        /// </summary>
        /// <param name="exceptionMessage"> contains the exception details.</param>
        public static void ShowExceptionAndShutDown(String exceptionMessage)
        {
            MessageBox.Show(exceptionMessage, "InstantImprovement Error", MessageBoxButton.OK, MessageBoxImage.Error);
            FaceWatcher.StopDetector();
        }

    }
}
