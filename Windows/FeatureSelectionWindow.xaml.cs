using InstantImprovement.DataControl;
using InstantImprovement.SDKControl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InstantImprovement.Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FeatureSelectionWindow : Window
    {
        const String SELECTMSG = "Please select 6 Emotions or Expressions to track.";
        const String ACTIVEMSG = "{0} Metrics chosen, please select {1} more.";
        const String DONEMSG = "{0} Metrics chosen.";
        const String DESELECTMSG = "Reached max number of metrics selected.";

        public FeatureSelectionWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            Classifiers = new HashSet<string>();
            foreach (String classifier in DataManager.FaceWatcher.EnabledClassifiers)
            {
                Classifiers.Add(classifier);
                Border border = getBorder(classifier);
                border.BorderBrush = Brushes.Green;
            }
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }


        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)((StackPanel)sender).Parent;
            if (isStackPanelSelected(border)) 
            {
                border.BorderBrush = Brushes.White;
                Classifiers.Remove(((StackPanel)sender).Name);
            }
            else {
                int classifersCount = Classifiers.Count;
                if ( classifersCount < 6)
                {
                    border.BorderBrush = Brushes.Green;
                    Classifiers.Add(((StackPanel)sender).Name);
                    if (classifersCount + 1 == 6) txtBlkInfo.Text = String.Format(DONEMSG, classifersCount + 1);
                    else txtBlkInfo.Text = String.Format(ACTIVEMSG, classifersCount+1, 6 - classifersCount);
                }
                else
                {
                    border.BorderBrush = Brushes.Red;
                    txtBlkInfo.Text = DESELECTMSG;
                }
                
            }

        }

        private Border getBorder(String name)
        {
            Border border = null;
            foreach (StackPanel panel in theGrid.Children.OfType<StackPanel>())
            {
                var tempBorder = panel.Children.OfType<Border>().FirstOrDefault();
                var stackPanel = (StackPanel)tempBorder.Child;
                if (stackPanel.Name == name) border = tempBorder;
            }
            return border;
        }

        private bool isStackPanelSelected(Border border)
        {
            return (border.BorderBrush == Brushes.Green);
        }

        public HashSet<String> Classifiers {get;set;}

        public String ActiveClassifierList 
        { 
            get
            {
                String classifiers = "";
                foreach(String key in Classifiers) 
                    classifiers += String.Format("{0},", key);
                return String.Format("{0} active classifiers: {1}", Classifiers.Count, classifiers);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int selectedClassifiersCount = Classifiers.Count;
            if ( selectedClassifiersCount < 6)
            {
                int missing = 6 - selectedClassifiersCount;
                List<StackPanel> panels = theGrid.Children.OfType<StackPanel>().ToList();
                int index = 0;
                while (selectedClassifiersCount < 6)
                {
                    var tempBorder = panels[index].Children.OfType<Border>().FirstOrDefault();
                    var stackPanel = (StackPanel)tempBorder.Child;
                    if (Classifiers.Add(stackPanel.Name)) selectedClassifiersCount++;
                    index++;
                }
            }

            DataManager.FaceWatcher.UpdateClassifiers(Classifiers);
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (StackPanel panel in theGrid.Children.OfType<StackPanel>())
            {
                Border border = panel.Children.OfType<Border>().FirstOrDefault();
                border.BorderBrush = Brushes.White;
                txtBlkInfo.Text = SELECTMSG;
            }
            Classifiers.Clear();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
