// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataManager.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// Customized Canvas to Display AffdexSDK AI-Results
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Affdex;
using InstantImprovement.SDKControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace InstantImprovement.DataControl
{
    public static class DataManager
    {
        public const int ClassifierCapacity = 6;
        public const int RingBufferCapacity = 60;

        /// <summary>
        /// Assistant that manages User-Analysis according to functionality of AffdexSDK
        /// </summary>
        public static FaceWatcher FaceWatcher { get { return FaceWatcher.Instance; } }

        /// <summary>
        /// RingBuffer-Dictionary that contains all relevant Emotions & Expressions
        /// </summary>
        public static Dictionary<string, RingBuffer> RingBuffers { get; set; } = new Dictionary<string, RingBuffer>();

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
        /// Export Video-Analysis Results to Excel-Sheet
        /// </summary>
        public static void ExportResultsToExcelSheet()
        {
            if (!XLSExporter.IsExcelInstalled())
            {
                ShowExceptionAndShutDown(new Exception("Excel is not installed. Please install Excel to execute this function."));
            }

            XLSExporter.InitializeExcel("AnalysisResult", false);

            foreach (KeyValuePair<string, RingBuffer> ringBuffer in RingBuffers)
            {
                ringBuffer.Value.ExportData(ringBuffer.Key);
            }

            //XLSExporter.GenerateOverviewChart("AnalysisResult", FaceWatcher.EnabledClassifiers);
            XLSExporter.ShowResults();
        }

        /// <summary>
        /// Extract Value (in %) of certain feature.
        /// </summary>
        /// <param name="face">Desired Face</param>
        /// <param name="metric">Desired Feature</param>
        /// <returns></returns>
        public static float ExtractFeaturePropertyValue(Face face, string metric)
        {
            PropertyInfo info;
            if ((info = face.Expressions.GetType().GetProperty(NameMappings(metric))) != null)
            {
                return (float)info.GetValue(face.Expressions, null);
            }
            else if ((info = face.Emotions.GetType().GetProperty(NameMappings(metric))) != null)
            {
                return (float)info.GetValue(face.Emotions, null);
            }
            else if ((info = face.Emotions.GetType().GetProperty(NameMappings(metric))) != null)
            {
                return (float)info.GetValue(face.Emojis, null);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Initialize Dictionary of RingBuffers
        /// </summary>
        public static void InitializeRingBufferDictionary()
        {
            foreach (String item in FaceWatcher.EnabledClassifiers)
            {
                if(!RingBuffers.ContainsKey(item))
                {
                    RingBuffers.Add(item, new RingBuffer(RingBufferCapacity));
                }
            }
        }

        /// <summary>
        /// Local Name Mappings for Feature-Classifiers
        /// </summary>
        /// <param name="classifierName"></param>
        /// <returns></returns>
        public static String NameMappings(String classifierName)
        {
            if (classifierName == "Frown")
            {
                return "LipCornerDepressor";
            }
            return classifierName;
        }

        /// <summary>
        /// Displays a alert with exception details
        /// </summary>
        /// <param name="exception"> contains the exception</param>
        public static void ShowExceptionAndShutDown(Exception exception)
        {
            String exceptionMessage = String.Format("InstantImprovement Error Encountered, details={0}", exception.Message);
            MessageBox.Show(exceptionMessage, "InstantImprovement Error", MessageBoxButton.OK, MessageBoxImage.Error);
            FaceWatcher.StopDetector();
        }

        /// <summary>
        /// Store Data into corresponding <see cref="RingBuffer"/>-Class
        /// </summary>
        /// <param name="classifier">Desired Classifier</param>
        /// <param name="newData">Classifier Value</param>
        public static void TrackData(String classifier, float newData)
        {
            if (!RingBuffers.ContainsKey(classifier))
            {
                RingBuffers.Add(classifier, new RingBuffer(RingBufferCapacity));
            }
            RingBuffers.TryGetValue(classifier, out var ringBuffer);
            ringBuffer.Add(newData);
        }
    }
}