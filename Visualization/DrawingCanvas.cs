// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawingCanvas.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// Customized Canvas to Display AffdexSDK AI-Results
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using Affdex;
using InstantImprovement.DataControl;
using InstantImprovement.SDKControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InstantImprovement.Visualization
{
    /// <summary>
    /// Customized Canvas to Display AffdexSDK AI-Results
    /// </summary>
    public class DrawingCanvas : System.Windows.Controls.Canvas
    {
        private const int _fpRadius = 2;

        private const int _margin = 5;

        private const int _metricFontSize = 14;

        private Dictionary<string, BitmapImage> _appImgs;

        private SolidColorBrush _boundingBrush;

        private Pen _boundingPen;

        private SolidColorBrush _emojiBrush;

        private Dictionary<Affdex.Emoji, BitmapImage> _emojiImages;

        private double _maxTxtHeight;

        private double _maxTxtWidth;

        private Typeface _metricTypeFace;

        private SolidColorBrush _negMetricBrush;

        private SolidColorBrush _pointBrush;

        private SolidColorBrush _pozMetricBrush;

        private UpperCaseConverter _upperConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingCanvas"/> class.
        /// </summary>
        public DrawingCanvas()
        {
            Init();
        }

        /// <summary>
        /// Face Dictionary to store detected Faces
        /// </summary>
        public Dictionary<int, Affdex.Face> Faces { get; set; }

        public bool ShowAppearance { get; set; }

        public bool ShowEmojis { get; set; }

        public bool ShowMetrics { get; set; }

        public bool ShowPoints { get; set; }

        public double XScale { get; set; }

        public double YScale { get; set; }

        /// <summary>
        /// Restart custom DrawingCanvas: all Elements are reinitialized
        /// </summary>
        public void Restart()
        {
            Init();
        }

        /// <summary>
        /// Draws the content of a <see cref="T:System.Windows.Media.DrawingContext" /> object during the render pass of a <see cref="T:System.Windows.Controls.Panel" /> element.
        /// </summary>
        /// <param name="dc">The <see cref="T:System.Windows.Media.DrawingContext" /> object to draw.</param>
        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            //For each face
            foreach (KeyValuePair<int, Affdex.Face> pair in Faces)
            {
                Affdex.Face face = pair.Value;

                FeaturePoint[] featurePoints = face.FeaturePoints;

                //Calculate bounding box corners coordinates.
                System.Windows.Point tl = new System.Windows.Point(featurePoints.Min(r => r.X) * XScale,
                                                   featurePoints.Min(r => r.Y) * YScale);
                System.Windows.Point br = new System.Windows.Point(featurePoints.Max(r => r.X) * XScale,
                                                                   featurePoints.Max(r => r.Y) * YScale);

                System.Windows.Point bl = new System.Windows.Point(tl.X, br.Y);

                if (ShowPoints)
                    DrawPoints(featurePoints, dc, tl, br);

                if (ShowMetrics)
                    DrawMetrics(face, dc, tl, br);

                if (ShowEmojis)
                    DrawEmojis(face, dc, tl, br);

                if (ShowAppearance)
                    DrawAppearance(face, dc, tl, br);
            }
            base.OnRender(dc);
        }

        /// <summary>
        /// Concatenation of 2 Integers
        /// </summary>
        /// <param name="x">Integer 1</param>
        /// <param name="y">Integer 2</param>
        /// <returns></returns>
        private string ConcatInt(int x, int y)
        {
            return String.Format("{0}{1}", x, y);
        }

        /// <summary>
        /// Draw Appearence-Emoji on Webcam-Video for given face-parameters
        /// </summary>
        /// <param name="face">Input-Face</param>
        /// <param name="dc">Drawing Context of Webcam Video</param>
        /// <param name="tl">Point (top left)</param>
        /// <param name="br">Point (bottom right)</param>
        private void DrawAppearance(Face face, DrawingContext dc, System.Windows.Point tl, System.Windows.Point br)
        {
            BitmapImage img = _appImgs[ConcatInt((int)face.Appearance.Gender, (int)face.Appearance.Glasses)];
            double imgRatio = ((br.Y - tl.Y) * 0.3) / img.Width;
            double imgH = img.Height * imgRatio;
            dc.DrawImage(img, new System.Windows.Rect(br.X + _margin, br.Y - imgH, img.Width * imgRatio, imgH));
        }

        /// <summary>
        /// Draw Feature Emoji on Webcam-Video for given face-parameters
        /// </summary>
        /// <param name="face">Input-Face</param>
        /// <param name="dc">Drawing Context of Webcam Video</param>
        /// <param name="tl">Point (top left)</param>
        /// <param name="br">Point (bottom right)</param>
        private void DrawEmojis(Face face, DrawingContext dc, System.Windows.Point tl, System.Windows.Point br)
        {
            if (face.Emojis.dominantEmoji != Affdex.Emoji.Unknown)
            {
                BitmapImage img = _emojiImages[face.Emojis.dominantEmoji];
                double imgRatio = ((br.Y - tl.Y) * 0.3) / img.Width;
                System.Windows.Point tr = new System.Windows.Point(br.X + _margin, tl.Y);
                dc.DrawImage(img, new System.Windows.Rect(tr.X, tr.Y, img.Width * imgRatio, img.Height * imgRatio));
            }
        }

        /// <summary>
        /// Draw Feature Metrics on Webcam-Video according to given face-parameters and Classificator Selection
        /// </summary>
        /// <param name="face">Input-Face</param>
        /// <param name="dc">Drawing Context of Webcam Video</param>
        /// <param name="tl">Point (top left)</param>
        /// <param name="bl">Point (bottom left)</param>
        private void DrawMetrics(Face face, DrawingContext dc, System.Windows.Point tl, System.Windows.Point bl)
        {
            double padding = (bl.Y - tl.Y) / DataManager.FaceWatcher.EnabledClassifiers.Count;
            double startY = tl.Y - padding;
            foreach (string metric in DataManager.FaceWatcher.EnabledClassifiers)
            {
                double width = _maxTxtWidth;
                double height = _maxTxtHeight;
                float value = DataManager.ExtractFeaturePropertyValue(face, metric);

                SolidColorBrush metricBrush = value > 0 ? _pozMetricBrush : _negMetricBrush;
                value = Math.Abs(value);
                SolidColorBrush txtBrush = value > 1 ? _emojiBrush : _boundingBrush;

                double x = tl.X - width - _margin;
                double y = startY += padding;
                double valBarWidth = width * (value / 100);

                if (value > 1) dc.DrawRectangle(null, _boundingPen, new System.Windows.Rect(x, y, width, height));
                dc.DrawRectangle(metricBrush, null, new System.Windows.Rect(x, y, valBarWidth, height));

                FormattedText metricFTScaled = new FormattedText((String)_upperConverter.Convert(metric, null, null, null),
                                                        System.Globalization.CultureInfo.CurrentCulture,
                                                        System.Windows.FlowDirection.LeftToRight,
                                                        _metricTypeFace, _metricFontSize * width / _maxTxtWidth, txtBrush);

                dc.DrawText(metricFTScaled, new System.Windows.Point(x, y));
            }
        }

        /// <summary>
        /// Draw Feature Points that determine the current Classification State (Face-Emotions & -Expressions)
        /// </summary>
        /// <param name="featurePoints">Collection of Feature Points of current Face</param>
        /// <param name="dc">Drawing Context of Webcam Video</param>
        /// <param name="tl">Point (top left)</param>
        /// <param name="br">Point (bottom right)</param>
        private void DrawPoints(FeaturePoint[] featurePoints, DrawingContext dc, System.Windows.Point tl, System.Windows.Point br)
        {
            foreach (var point in featurePoints)
            {
                dc.DrawEllipse(_pointBrush, null, new System.Windows.Point(point.X * XScale, point.Y * YScale), _fpRadius, _fpRadius);
            }

            //Draw BoundingBox
            var rect = new System.Windows.Rect(tl, br);
            dc.DrawRectangle(null, _boundingPen, rect);
        }

        /// <summary>
        /// Recalculate Drawing Area as Classifiers are Updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FaceWatcher_ClassifierUpdated(object sender, EventArgs e)
        {
            Dictionary<string, FormattedText> txtArray = new Dictionary<string, FormattedText>();

            foreach (String metric in DataManager.FaceWatcher.EnabledClassifiers)
            {
                FormattedText metricFT = new FormattedText((String)_upperConverter.Convert(metric, null, null, null),
                                                        System.Globalization.CultureInfo.CurrentCulture,
                                                        System.Windows.FlowDirection.LeftToRight,
                                                        _metricTypeFace, _metricFontSize, _emojiBrush);
                txtArray.Add(metric, metricFT);
            }

            if (txtArray.Count > 0)
            {
                _maxTxtWidth = txtArray.Max(r => r.Value.Width);
                _maxTxtHeight = txtArray.Max(r => r.Value.Height);
            }
        }

        /// <summary>
        /// Initialization of all relevant Variables & Properties
        /// </summary>
        private void Init()
        {
            ShowMetrics = true;
            ShowPoints = true;
            ShowAppearance = true;
            ShowEmojis = true;

            InitializeHelperTools();

            // Subscribe Classifier Updates & Call Method to initialize
            DataManager.FaceWatcher.ClassifierUpdated += new EventHandler(FaceWatcher_ClassifierUpdated);
            FaceWatcher_ClassifierUpdated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Initialize all Helper Tools that are needed for functionality of DrawingCanvas Class
        /// </summary>
        private void InitializeHelperTools()
        {
            _boundingBrush = new SolidColorBrush(Colors.LightGray);
            _pointBrush = new SolidColorBrush(Colors.Cornsilk);
            _emojiBrush = new SolidColorBrush(Colors.Black);
            _pozMetricBrush = new SolidColorBrush(Colors.LimeGreen);
            _negMetricBrush = new SolidColorBrush(Colors.Red);
            _boundingPen = new Pen(_boundingBrush, 1);

            NameToResourceConverter conv = new NameToResourceConverter();
            _upperConverter = new UpperCaseConverter();
            _metricTypeFace = Fonts.GetTypefaces((Uri)conv.Convert("Square", null, "ttf", null)).FirstOrDefault();

            Faces = new Dictionary<int, Affdex.Face>();
            _emojiImages = new Dictionary<Affdex.Emoji, BitmapImage>();
            _appImgs = new Dictionary<string, BitmapImage>();

            var emojis = Enum.GetValues(typeof(Affdex.Emoji));
            foreach (int emojiVal in emojis)
            {
                BitmapImage img = LoadImage(emojiVal.ToString());
                _emojiImages.Add((Affdex.Emoji)emojiVal, img);
            }

            var gender = Enum.GetValues(typeof(Affdex.Gender));
            foreach (int genderVal in gender)
            {
                for (int g = 0; g <= 1; g++)
                {
                    string name = ConcatInt(genderVal, g);
                    BitmapImage img = LoadImage(name);
                    _appImgs.Add(name, img);
                }
            }
        }

        /// <summary>
        /// Load Image from resource-library
        /// </summary>
        /// <param name="name">name of image</param>
        /// <param name="extension">image-suffig (default: png)</param>
        /// <returns></returns>
        private BitmapImage LoadImage(string name, string extension = "png")
        {
            NameToResourceConverter conv = new NameToResourceConverter();
            var pngURI = conv.Convert(name, null, extension, null);
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = (Uri)pngURI;
            img.EndInit();
            return img;
        }
    }
}