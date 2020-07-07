// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceWatcher.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// FaceWatcher ovserves webcam and extracts desired features
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Affdex;
using InstantImprovement.DataControl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;

namespace InstantImprovement.SDKControl
{
    /// <summary>
    /// FaceWatcher ovserves webcam and extracts desired features
    /// </summary>
    public sealed class FaceWatcher : Affdex.ImageListener, Affdex.ProcessStatusListener
    {
        /// <summary>
        /// Singleton Thread-Assistant
        /// </summary>
        private static readonly object padlock = new object();

        /// <summary>
        /// Singleton Placeholder
        /// </summary>
        private static FaceWatcher _instance;

        /// <summary>
        /// Classificator-Collection
        /// </summary>
        private StringCollection _enabledClassifiers = Settings.Default.Classifiers;

        /// <summary>
        /// Handles Feature-Classifier Updates
        /// </summary>
        public event EventHandler ClassifierUpdated;

        /// <summary>
        /// Handles Start of Detector
        /// </summary>
        public event EventHandler DetectorStarted;

        /// <summary>
        /// Handles Stop of Detector
        /// </summary>
        public event EventHandler DetectorStopped;

        /// <summary>
        /// Handles capture of Image
        /// </summary>
        public event EventHandler<FaceWatcherEventArgs> ImageCaptured;

        /// <summary>
        /// Handles reception of Image
        /// </summary>
        public event EventHandler<FaceWatcherEventArgs> ImageReceived;

        /// <summary>
        /// Singleton Implementation
        /// </summary>
        public static FaceWatcher Instance
        {
            get
            {
                lock (padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new FaceWatcher();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Frames per Second Settings for PC-Camera
        /// </summary>
        public double CameraFPS { get; private set; }

        /// <summary>
        /// Camera-ID to address desired Camera (Default = 0)
        /// </summary>
        public int CameraID { get; private set; }

        /// <summary>
        /// Affdex Detector
        /// </summary>
        public Affdex.Detector Detector { get; private set; }

        /// <summary>
        /// Collection of strings represent the name of the active selected metrics;
        /// </summary>
        public StringCollection EnabledClassifiers
        {
            get
            {
                return _enabledClassifiers;
            }
            set
            {
                _enabledClassifiers = value;
                OnClassifierUpdated();
            }
        }

        /// <summary>
        /// Number of Facecs to Detect
        /// </summary>
        public uint NumberOfFaces { get; private set; }

        /// <summary>
        /// Processed Frames per Second
        /// </summary>
        public double ProcessFPS { get; private set; }

        /// <summary>
        /// Set the Classifiers that we are interested in tracking
        /// </summary>
        public void ActivateSelectedClassifiers()
        {
            bool wasRunning = false;
            if (Detector.isRunning())
            {
                Detector.stop();
                wasRunning = true;
            }

            //Actual Settings Adaption
            Detector.setDetectAllEmotions(false);
            Detector.setDetectAllExpressions(false);
            Detector.setDetectAllEmojis(true);
            Detector.setDetectGender(true);
            Detector.setDetectGlasses(true);
            foreach (String metric in EnabledClassifiers)
            {
                MethodInfo setMethodInfo = Detector.GetType().GetMethod(String.Format("setDetect{0}", DataManager.NameMappings(metric)));
                setMethodInfo.Invoke(Detector, new object[] { true });
            }

            if (wasRunning)
            {
                Detector.start();
            }
        }

        /// <summary>
        /// Add a Classifier to Classifier-Collection
        /// </summary>
        /// <param name="classifier">New Classifier</param>
        public void AddClassifier(string classifier)
        {
            if (!EnabledClassifiers.Contains(classifier))
            {
                if (EnabledClassifiers.Count < DataManager.ClassifierCapacity)
                {
                    EnabledClassifiers.Add(classifier);
                }
                else 
                {
                    throw new Exception("Classifier is full");
                }
            }
        }

        /// <summary>
        /// Configure Detector by (re)setting Detector-Parameters.
        /// </summary>
        /// <remarks>
        /// Needs to be called before Detector can be started by <see cref="StartDetector()"/>
        /// </remarks>
        /// <param name="cameraFPS">Frames Per Second of Camera (Default: 15)</param>
        /// <param name="processFPS">Frames Per Second that get Processed (Default: 15)</param>
        /// <param name="numberOfFaces">Max number of Faces that should be detected (Default: 10)</param>
        /// <param name="cameraID">ID of camera that Detector should use (Default = 0)</param>
        public void ConfigureDetector(double cameraFPS = 15, double processFPS = 15, uint numberOfFaces = 10, int cameraID = 0)
        {
            if (Detector != null)
            {
                Detector.Dispose();
            }

            CameraID = cameraID;
            CameraFPS = cameraFPS;
            ProcessFPS = processFPS;
            NumberOfFaces = numberOfFaces;
            Detector = new Affdex.CameraDetector(CameraID, CameraFPS, ProcessFPS, NumberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);
        }

        /// <summary>
        /// Handle Classifier Update
        /// </summary>
        public void OnClassifierUpdated()
        {
            if (Detector.isRunning())
                ActivateSelectedClassifiers();
            ClassifierUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handle Detector Start
        /// </summary>
        public void OnDetectorStarted()
        {
            DetectorStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handle Detector Stop
        /// </summary>
        public void OnDetectorStopped()
        {
            DetectorStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the Image-Capture from source (released by Affdex.Detector)
        /// </summary>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image captured from camera.</param>
        public void onImageCapture(Frame frame)
        {
            OnImageCaptured(new FaceWatcherEventArgs(null, frame));
        }

        /// <summary>
        /// Handle Image-Capture
        /// </summary>
        /// <param name="e"></param>
        public void OnImageCaptured(FaceWatcherEventArgs e)
        {
            ImageCaptured?.Invoke(this, e);
        }

        /// <summary>
        /// Handle Image-Reception
        /// </summary>
        /// <param name="e"></param>
        public void OnImageReceived(FaceWatcherEventArgs e)
        {
            ImageReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Handles the Image results event produced by Affdex.Detector
        /// </summary>
        /// <param name="faces">The detected faces.</param>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image analyzed.</param>
        public void onImageResults(Dictionary<int, Face> faces, Frame frame)
        {
            OnImageReceived(new FaceWatcherEventArgs(faces, frame));
        }

        /// <summary>
        /// Handles occurence of exception produced by Affdex.Detector
        /// </summary>
        /// <param name="ex">The <see cref="Affdex.AffdexException"/> instance containing the exception details.</param>
        public void onProcessingException(AffdexException ex)
        {
            throw ex;
        }

        /// <summary>
        /// Only Needed to meet requirements of ProcessStatusListener-Interface - therefore Empty
        /// </summary>
        public void onProcessingFinished() { }

        /// <summary>
        /// Remove Classifier from Classifier-Collection
        /// </summary>
        /// <param name="classifier"></param>
        public void RemoveClassifier(string classifier)
        {
            EnabledClassifiers.Remove(classifier);
        }

        /// <summary>
        /// Resets the camera processing.
        /// </summary>
        public void ResetDetector()
        {
            Detector.reset();
        }

        /// <summary>
        /// Starts the camera processing.
        /// </summary>
        /// <remarks>
        /// Requires <see cref="ConfigureDetector(double, double, uint, int)"/> to be called in order to reconfigure Detector
        /// </remarks>
        public void StartDetector()
        {
            try
            {
                //Set location of the classifier data files, needed by the SDK
                Detector.setClassifierPath(AppDomain.CurrentDomain.BaseDirectory + "\\data");

                // Set the Classifiers that we are interested in tracking
                ActivateSelectedClassifiers();

                // Connect required Interfaces
                Detector.setImageListener(this);
                Detector.setProcessStatusListener(this);

                //Start Process
                Detector.start();

                OnDetectorStarted();
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
                        StopDetector();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Stops the FaceDetection if Detector was active
        /// </summary>
        /// <returns>True if Detector was active, False if Detector was inactive</returns>
        public bool StopDetector()
        {
            if ((Detector != null) && (Detector.isRunning()))
            {
                Detector.stop();
                OnDetectorStopped();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}