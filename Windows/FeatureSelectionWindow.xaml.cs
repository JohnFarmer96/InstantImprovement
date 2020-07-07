// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FeatureSelectionWindow.xaml.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// Interaction logic for FeatureSelectionWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using InstantImprovement.DataControl;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace InstantImprovement.Windows
{
    /// <summary>
    /// Interaction logic for FeatureSelectionWindow.xaml
    /// </summary>
    public partial class FeatureSelectionWindow : Window
    {
        private const String ACTIVEMSG = "{0} Metrics chosen, please select {1} more.";
        private const String DESELECTMSG = "Reached max number of metrics selected.";
        private const String DONEMSG = "{0} Metrics chosen.";
        private const String SELECTMSG = "Please select 6 Emotions or Expressions to track.";
        public FeatureSelectionWindow()
        {
            InitializeComponent();
            SyncFeatureButton();
            DataManager.CenterWindowOnScreen(this);
        }

        /// <summary>
        /// Handle Button-ClearAll Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (StackPanel panel in theGrid.Children.OfType<StackPanel>())
            {
                Border border = panel.Children.OfType<Border>().FirstOrDefault();
                border.BorderBrush = Brushes.White;
                txtBlkInfo.Text = SELECTMSG;
            }
            DataManager.FaceWatcher.EnabledClassifiers.Clear();
        }

        /// <summary>
        /// Handle Button-OK Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            SyncFeatureButton();
            DataManager.FaceWatcher.OnClassifierUpdated();
            this.Close();
        }

        /// <summary>
        /// Handle Feature-StackPanel Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool error = false;
            StackPanel stackPanel = (StackPanel)sender;

            if (DataManager.FaceWatcher.EnabledClassifiers.Contains(stackPanel.Name))
            {
                DataManager.FaceWatcher.RemoveClassifier(stackPanel.Name);
                SyncFeatureButton();
            }
            else
            {
                try
                {
                    DataManager.FaceWatcher.AddClassifier(stackPanel.Name);
                    SyncFeatureButton();
                }
                catch (Exception)
                {
                    Border border = (Border)stackPanel.Parent;
                    border.BorderBrush = Brushes.Red;
                    error = true;
                }
            }

            UpdateStatusMessage(error);
        }

        /// <summary>
        /// Sync Feature-StackPanel-Border with selected Features
        /// </summary>
        private void SyncFeatureButton()
        {
            foreach (StackPanel panel in theGrid.Children.OfType<StackPanel>())
            {
                var tempBorder = panel.Children.OfType<Border>().FirstOrDefault();
                tempBorder.BorderBrush = Brushes.White;

                StackPanel stackPanel = (StackPanel)tempBorder.Child;
                if (DataManager.FaceWatcher.EnabledClassifiers.Contains(stackPanel.Name))
                {
                    tempBorder.BorderBrush = Brushes.Green;
                }
            }
        }

        /// <summary>
        /// Update Status Banner
        /// </summary>
        /// <param name="error"></param>
        private void UpdateStatusMessage(bool error)
        {
            if (error)
            {
                txtBlkInfo.Text = DESELECTMSG;
                return;
            }

            switch (DataManager.FaceWatcher.EnabledClassifiers.Count)
            {
                case (DataManager.ClassifierCapacity):
                    txtBlkInfo.Text = String.Format(DONEMSG, DataManager.ClassifierCapacity);
                    break;

                case (0):
                    txtBlkInfo.Text = SELECTMSG;
                    break;

                default:
                    txtBlkInfo.Text = String.Format(ACTIVEMSG, DataManager.FaceWatcher.EnabledClassifiers.Count, DataManager.ClassifierCapacity - DataManager.FaceWatcher.EnabledClassifiers.Count); ;
                    break;
            }
        }
    }
}