// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using InstantImprovement.SDKControl;
using InstantImprovement.DataControl;

namespace InstantImprovement.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initialize <see cref="MainWindow"/> to use functionality
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataManager.CenterWindowOnScreen(this);
        }

        /// <summary>
        /// Once a face's feature points get displayed, the number of successive captures that occur without
        /// the points getting redrawn in the OnResults callback.
        /// </summary>
        private int DrawSkipCount { get; set; }

        /// <summary>
        /// Determine if Appearance-Emoji is shown
        /// </summary>
        private bool ShowAppearance { get; set; }

        /// <summary>
        /// Determine whether Featuer-Emojis are shown
        /// </summary>
        private bool ShowEmojis { get; set; }

        /// <summary>
        /// Determine whether Face-Points are shown
        /// </summary>
        private bool ShowFacePoints { get; set; }

        /// <summary>
        /// Determine whether Face-Metrics is shown
        /// </summary>
        private bool ShowMetrics { get; set; }

        /// <summary>
        /// Get stored Settings and apply them on MainWindow that is currently rendered
        /// </summary>
        private void ApplyInitialSettings()
        {
            ShowEmojis = canvas.ShowEmojis = InstantImprovement.Settings.Default.ShowEmojis;
            ShowAppearance = canvas.ShowAppearance = InstantImprovement.Settings.Default.ShowAppearance;
            ShowFacePoints = canvas.ShowPoints = InstantImprovement.Settings.Default.ShowPoints;
            ShowMetrics = canvas.ShowMetrics = InstantImprovement.Settings.Default.ShowMetrics;
            ChangeButtonStyle(FeatureEmojis, InstantImprovement.Settings.Default.ShowEmojis);
            ChangeButtonStyle(UserEmoji, InstantImprovement.Settings.Default.ShowAppearance);
            ChangeButtonStyle(FacePoints, InstantImprovement.Settings.Default.ShowPoints);
            ChangeButtonStyle(Features, InstantImprovement.Settings.Default.ShowMetrics);
        }

        /// <summary>
        /// Changes the button style based on the specified flag.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="isOn">if set to <c>true</c> [is on].</param>
        private void ChangeButtonStyle(Button button, bool isOn)
        {
            Style style;
            // No need to change button-settings
            //String buttonText = String.Empty;

            if (isOn)
            {
                style = this.FindResource("PointsOnButtonStyle") as Style;
                //buttonText = "Hide " + button.Name;
            }
            else
            {
                style = this.FindResource("CustomButtonStyle") as Style;
                //buttonText = "Show " + button.Name;
            }
            button.Style = style;
            //button.Content = buttonText;
        }

        /// <summary>
        /// Constructs the bitmap image from byte array.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        private BitmapSource ConstructImage(byte[] imageData, int width, int height)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    return imageSrc;
                }
            }
            catch (Exception ex)
            {
                DataManager.ShowExceptionAndShutDown(ex);
            }

            return null;
        }

        /// <summary>
        /// Draws the image captured from the camera.
        /// </summary>
        /// <param name="image">The image captured.</param>
        private void DrawCapturedImage(object sender, FaceWatcherEventArgs e)
        {
            // Update the Image control from the UI thread
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    Affdex.Frame image = e.Frame;
                    // Update the Image control from the UI thread
                    //cameraDisplay.Source = rtb;
                    cameraDisplay.Source = ConstructImage(image.getBGRByteArray(), image.getWidth(), image.getHeight());

                    // Allow N successive OnCapture callbacks before the FacePoint drawing canvas gets cleared.
                    if (++DrawSkipCount > 4)
                    {
                        canvas.Faces = new Dictionary<int, Affdex.Face>();
                        canvas.InvalidateVisual();
                        DrawSkipCount = 0;
                    }

                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    DataManager.ShowExceptionAndShutDown(ex);
                }
            }));
        }

        /// <summary>
        /// Draws the facial analysis captured by Affdex.Detector.
        /// </summary>
        /// <param name="image">The image analyzed.</param>
        /// <param name="faces">The faces found in the image analyzed.</param>
        private void DrawData(object sender, FaceWatcherEventArgs e)
        {
            try
            {
                Affdex.Frame image = e.Frame;
                Dictionary<int, Affdex.Face> faces = e.Faces;

                // Plot Face Points
                if (faces != null)
                {
                    var result = this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ((DataManager.FaceWatcher.Detector != null) && (DataManager.FaceWatcher.Detector.isRunning()))
                        {
                            canvas.Faces = faces;
                            canvas.Width = cameraDisplay.ActualWidth;
                            canvas.Height = cameraDisplay.ActualHeight;
                            canvas.XScale = canvas.Width / image.getWidth();
                            canvas.YScale = canvas.Height / image.getHeight();
                            canvas.InvalidateVisual();
                            DrawSkipCount = 0;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                DataManager.ShowExceptionAndShutDown(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnExit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Click event of the Points control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FacePoints_Click(object sender, RoutedEventArgs e)
        {
            ShowFacePoints = !ShowFacePoints;
            canvas.ShowPoints = ShowFacePoints;
            ChangeButtonStyle((Button)sender, ShowFacePoints);
        }

        /// <summary>
        /// Handles the Click event of the Emojis control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FeatureEmojis_Click(object sender, RoutedEventArgs e)
        {
            ShowEmojis = !ShowEmojis;
            canvas.ShowEmojis = ShowEmojis;
            ChangeButtonStyle((Button)sender, ShowEmojis);
        }

        /// <summary>
        /// Handles the Click event of the Metrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Features_Click(object sender, RoutedEventArgs e)
        {
            ShowMetrics = !ShowMetrics;
            canvas.ShowMetrics = ShowMetrics;
            ChangeButtonStyle((Button)sender, ShowMetrics);
        }

        /// <summary>
        /// Handles the Click event of the btnChooseWin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FeatureSelection_Click(object sender, RoutedEventArgs e)
        {
            DataManager.FaceWatcher.StopDetector();
            FeatureSelectionWindow w = new FeatureSelectionWindow();
            w.ShowDialog();
            DataManager.FaceWatcher.ConfigureDetector();
            DataManager.FaceWatcher.StartDetector();
        }

        /// <summary>
        /// Initializes all relevant Event for Buttons on MainWindow
        /// </summary>
        private void InitializeButtonEvents()
        {
            StartCamera.Click += StartCamera_Click;
            StopCamera.Click += StopCamera_Click;
            FacePoints.Click += FacePoints_Click;
            FeatureEmojis.Click += FeatureEmojis_Click;
            UserEmoji.Click += UserEmoji_Click;
            Features.Click += Features_Click;
            ResetCamera.Click += ResetCamera_Click;
            Exit.Click += Exit_Click;
            AppShot.Click += TakeScreenshot_Click;
            LaunchVideo.Click += LaunchDemonstration_Click;
        }

        /// <summary>
        /// Launch Demonstration-Video-Window to analze Users-Face while watching a Video.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchDemonstration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettings();
                DataManager.FaceWatcher.StopDetector();
                ResetDisplayArea();
                VideoWindow vidWin = new VideoWindow();
                vidWin.ShowDialog();
                DataManager.FaceWatcher.ConfigureDetector();
                DataManager.FaceWatcher.StartDetector();
            }
            catch (Exception ex)
            {
                DataManager.ShowExceptionAndShutDown(new Exception(ex.StackTrace));
            }

        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataManager.FaceWatcher.StopDetector();
            SaveSettings();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Once the window las been loaded and the content rendered, the camera
        /// can be initialized and started. This sequence allows for the underlying controls
        /// and watermark logo to be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            // Listen to Processing-Events
            DataManager.FaceWatcher.DetectorStarted += new EventHandler(UCameraLayout);
            DataManager.FaceWatcher.DetectorStopped += new EventHandler(UIInitLayout);
            DataManager.FaceWatcher.ImageReceived += new EventHandler<FaceWatcherEventArgs>(DrawData);
            DataManager.FaceWatcher.ImageCaptured += new EventHandler<FaceWatcherEventArgs>(DrawCapturedImage);

            //canvas.MetricNames = InstantImprovement.Settings.Default.Classifiers; -- might be obsolete
            DataManager.FaceWatcher.ConfigureDetector();
            DataManager.FaceWatcher.StartDetector();
        }

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Render Initial Layout - StartCamera is not visible on Start
            UIInitLayout(this, EventArgs.Empty);
            StartCamera.IsEnabled = false;

            // Initialize Button Click Handlers
            InitializeButtonEvents();

            // Apply Settings
            ApplyInitialSettings();

            this.ContentRendered += MainWindow_ContentRendered;
        }
        /// <summary>
        /// Handles the Click event of the btnResetCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ResetCamera_Click(object sender, RoutedEventArgs e)
        {
            DataManager.FaceWatcher.ResetDetector();
        }

        /// <summary>
        /// Resets the display area.
        /// </summary>
        private void ResetDisplayArea()
        {
            try
            {
                UIInitLayout(this, EventArgs.Empty);
                canvas.Restart();
            }
            catch (Exception ex)
            {
                DataManager.ShowExceptionAndShutDown(ex);
            }
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        void SaveSettings()
        {
            InstantImprovement.Settings.Default.ShowPoints = ShowFacePoints;
            InstantImprovement.Settings.Default.ShowAppearance = ShowAppearance;
            InstantImprovement.Settings.Default.ShowEmojis = ShowEmojis;
            InstantImprovement.Settings.Default.ShowMetrics = ShowMetrics;
            InstantImprovement.Settings.Default.Classifiers = DataManager.FaceWatcher.EnabledClassifiers;
            InstantImprovement.Settings.Default.Save();
        }

        /// <summary>
        /// Handles the Click event of the btnStartCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StartCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataManager.FaceWatcher.ConfigureDetector();
                DataManager.FaceWatcher.StartDetector();
            }
            catch (Exception ex)
            {
                DataManager.ShowExceptionAndShutDown(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStopCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StopCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataManager.FaceWatcher.StopDetector();
                ResetDisplayArea();
            }
            catch (Exception ex)
            {
                DataManager.ShowExceptionAndShutDown(ex);
            }

        }

        /// <summary>
        /// Take a shot of the current canvas and save it to a png file on disk
        /// </summary>
        /// <param name="fileName">The file name of the png file to save it in</param>
        private void TakeScreenShot(String fileName)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
            double dpi = 96d;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height,
                                                                       dpi, dpi, PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(this);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            renderBitmap.Render(dv);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(fileName))
            {
                encoder.Save(file);
            }

            appShotLocLabel.Content = String.Format("Screenshot saved to: {0}", fileName);
            ((System.Windows.Media.Animation.Storyboard)FindResource("autoFade")).Begin(appShotLocLabel);
        }

        /// <summary>
        /// Handles the Click eents of the Take Screenshot control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TakeScreenshot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                String fileName = String.Format("InstantImprovement ScreenShot {0:MMMM dd yyyy h mm ss}.png", DateTime.Now);
                fileName = System.IO.Path.Combine(picturesFolder, fileName);
                this.TakeScreenShot(fileName);
            }
            catch (Exception ex)
            {
                DataManager.ShowExceptionAndShutDown(ex);
            }
        }

        /// <summary>
        /// Adapt UI-Layout for Use of Camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UCameraLayout(object sender, EventArgs e)
        {
            // Hide the logo, show the camera feed and the data canvas
            logoBackground.Visibility = Visibility.Hidden;
            affdexLabel.Visibility = Visibility.Hidden;
            cornerLogo.Visibility = Visibility.Visible;
            canvas.Visibility = Visibility.Visible;
            cameraDisplay.Visibility = Visibility.Visible;
            LaunchVideo.Visibility = Visibility.Visible;

            // Adapt Button Layout
            StartCamera.IsEnabled = false;
            ResetCamera.IsEnabled =
            FacePoints.IsEnabled =
            Features.IsEnabled =
            UserEmoji.IsEnabled =
            FeatureEmojis.IsEnabled =
            StopCamera.IsEnabled =
            AppShot.IsEnabled =
            Exit.IsEnabled = true;
        }

        /// <summary>
        /// Adapt UI-Layout for use of setup-manu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIInitLayout(object sender, EventArgs e)
        {
            // Show the logo
            logoBackground.Visibility = Visibility.Visible;
            affdexLabel.Visibility = Visibility.Visible;
            cornerLogo.Visibility = Visibility.Hidden;
            canvas.Visibility = Visibility.Hidden;
            cameraDisplay.Visibility = Visibility.Hidden;
            LaunchVideo.Visibility = Visibility.Hidden;

            // Enable/Disable buttons on start
            StopCamera.IsEnabled =
            Exit.IsEnabled = 
            StartCamera.IsEnabled = true;
            ResetCamera.IsEnabled =
            FacePoints.IsEnabled =
            Features.IsEnabled =
            UserEmoji.IsEnabled =
            FeatureEmojis.IsEnabled =
            ResetCamera.IsEnabled =
            AppShot.IsEnabled =
            StopCamera.IsEnabled = false;
        }
        /// <summary>
        /// Handles the Click event of the Appearance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UserEmoji_Click(object sender, RoutedEventArgs e)
        {
            ShowAppearance = !ShowAppearance;
            canvas.ShowAppearance = ShowAppearance;
            ChangeButtonStyle((Button)sender, ShowAppearance);
        }

        /// <summary>
        /// Open Repository-Link
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepoLink_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/JohnFarmer96/InstantImprovement");
            }
            catch
            {

            }
        }
    }
}
