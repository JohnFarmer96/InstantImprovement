using Affdex;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InstantImprovement.SDKControl
{
    public sealed class FaceWatcher : Affdex.ImageListener, Affdex.ProcessStatusListener
    {
        #region Singleton
        private static FaceWatcher _instance;
        private static readonly object padlock = new object();
        public static FaceWatcher Instance
        {
            get
            {
                lock(padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new FaceWatcher();
                    }
                }

                return _instance;
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Frames per Second Settings for PC-Camera
        /// </summary>
        public double CameraFPS { get; private set; }

        /// <summary>
        /// Processed Frames per Second
        /// </summary>
        public double ProcessFPS { get; private set; }

        /// <summary>
        /// Number of Facecs to Detect
        /// </summary>
        public uint NumberOfFaces { get; private set; }

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
        public StringCollection EnabledClassifiers { get; set; } = Settings.Default.Classifiers;

        #endregion Properties

        #region CustomEvents

        public event EventHandler DetectorStarted;

        public void OnDetectorStarted()
        {
            DetectorStarted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DetectorStopped;

        public void OnDetectorStopped()
        {
            DetectorStopped?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<ImageListenerEventArgs> ImageReceived;

        public void OnImageReceived(ImageListenerEventArgs e)
        {
            ImageReceived?.Invoke(this, e);
        }

        public event EventHandler<ImageListenerEventArgs> ImageCaptured;

        public void OnImageCaptured(ImageListenerEventArgs e)
        {
            ImageCaptured?.Invoke(this, e);
        }

        #endregion CustomEvents

        #region ClassifierSelection
        /// <summary>
        /// Set the Classifiers that we are interested in tracking
        /// </summary>
        public void TurnOnSelectedClassifiers()
        {
            Detector.setDetectAllEmotions(false);
            Detector.setDetectAllExpressions(false);
            Detector.setDetectAllEmojis(true);
            Detector.setDetectGender(true);
            Detector.setDetectGlasses(true);
            foreach (String metric in EnabledClassifiers)
            {
                MethodInfo setMethodInfo = Detector.GetType().GetMethod(String.Format("setDetect{0}", (metric == "Frown") ? "LipCornerDepressor" : metric));
                setMethodInfo.Invoke(Detector, new object[] { true });
            }
        }

        public void UpdateClassifiers(HashSet<string> classifiers)
        {
            EnabledClassifiers = new StringCollection();
            foreach (String classifier in classifiers)
            {
                EnabledClassifiers.Add(classifier);
            }
        }

        #endregion ClassifierSelection

        #region DetectorControl

        /// <summary>
        /// Configure Detector by (re)setting Detector-Parameters
        /// </summary>
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
        /// Starts the camera processing.
        /// </summary>
        public void StartDetector()
        {
            try
            {              
                //Set location of the classifier data files, needed by the SDK
                Detector.setClassifierPath(FilePath.GetClassifierDataFolder());

                // Set the Classifiers that we are interested in tracking
                TurnOnSelectedClassifiers();

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
        /// Resets the camera processing.
        /// </summary>
        public void ResetDetector()
        {
                Detector.reset();
        }

        /// <summary>
        /// Stops the camera processing.
        /// </summary>
        public void StopDetector()
        {
            if ((Detector != null) && (Detector.isRunning()))
            {
                Detector.stop();
            }

            OnDetectorStopped();
        }
        #endregion DetectorControl

        #region InterfaceMethods

        /// <summary>
        /// Handles the Image results event produced by Affdex.Detector
        /// </summary>
        /// <param name="faces">The detected faces.</param>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image analyzed.</param>
        public void onImageResults(Dictionary<int, Face> faces, Frame frame)
        {
            OnImageReceived(new ImageListenerEventArgs(faces, frame));
        }

        /// <summary>
        /// Handles the Image capture from source produced by Affdex.Detector
        /// </summary>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image captured from camera.</param>
        public void onImageCapture(Frame frame)
        {
            OnImageCaptured(new ImageListenerEventArgs(null, frame));
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
        public void onProcessingFinished(){}

        #endregion InterfaceMethods
    }

    public class ImageListenerEventArgs : EventArgs
    {
        public Dictionary<int, Face> Faces { get; private set; }
        public Frame Frame { get; private set; }

        public ImageListenerEventArgs(Dictionary<int, Face> faces, Frame frame)
        {
            Faces = faces;
            Frame = frame;
        }
    }
}
