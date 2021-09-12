using CocoSchedule.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CocoSchedule
{
    public class CellResizedEventArgs
    {
        public bool North { get; private set; }
        public TimeSpan InitialWhen { get; private set; }
        public TimeSpan InitialDuration { get; private set; }

        public CellResizedEventArgs(bool north, TimeSpan iWhen, TimeSpan iDuration)
        {
            North = north;
            InitialWhen = iWhen;
            InitialDuration = iDuration;
        }
    }

    /// <summary>
    /// Logique d'interaction pour Cell.xaml
    /// </summary>
    public partial class Cell : UserControl
    {
        public TextBlock AssociatedLabel1 { get; set; }
        public TextBlock AssociatedLabel2 { get; set; }

        public TaskDescription Description { get; set; }

        private bool _showLabel1 = false;
        private bool _showLabel2 = false;
        private bool _allowEditBtn = false;

        #region Events
        [Category("CocoSchedule")]
        public event EventHandler<CellResizedEventArgs> Resized;
        #endregion

        public Cell(TaskDescription description)
        {
            InitializeComponent();

            Description = description;
            ConfigureDisplay(true);
        }

        #region Labels
        public void UpdateLabels(bool north = false, bool noLabels = false, bool hideLabels = false)
        {
            var startTime = Utils.General.HeightToTimespan(Margin.Top) + new TimeSpan(5, 0, 0);
            var endTime = Description.Duration + startTime;

            if (startTime.Minutes != 0)
            {
                AssociatedLabel1.Text = DateTime.Today.Add(startTime).ToString("hh:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
                AssociatedLabel1.Margin = new Thickness(0, Utils.General.TimespanToHeight(startTime - new TimeSpan(5, 0, 0)), 0, 0);
                if (!noLabels && !hideLabels) AssociatedLabel1.Visibility = Visibility.Visible;
                if (!noLabels) _showLabel1 = true;
            }
            else
            {
                AssociatedLabel1.Visibility = Visibility.Hidden;
                _showLabel1 = false;
            }

            if (!north)
            {
                if (endTime.Minutes != 0)
                {
                    AssociatedLabel2.Text = DateTime.Today.Add(endTime).ToString("hh:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
                    AssociatedLabel2.Margin = new Thickness(0, Utils.General.TimespanToHeight(endTime - new TimeSpan(5, 0, 0)), 0, 0);
                    if (!noLabels && !hideLabels) AssociatedLabel2.Visibility = Visibility.Visible;
                    if (!noLabels) _showLabel2 = true;
                }
                else
                {
                    AssociatedLabel2.Visibility = Visibility.Hidden;
                    _showLabel2 = false;
                }
            }
        }
        #endregion

        public void ConfigureDisplay(bool init = false)
        {
            // When
            TimeSpan distanceFrom5AM = Description.When - new TimeSpan(5, 0, 0);
            double topMargin = Utils.General.TimespanToHeight(distanceFrom5AM);
            Margin = new Thickness(0, topMargin, 0, 0);

            // Duration
            Height = Utils.General.TimespanToHeight(Description.Duration);

            // Title and description
            TitleTB.Text = Description.TitleText;
            DescTB.Text = Description.DescriptionText;
            BorderComponent.Background = (Brush)App.Current.Resources[GlobalInfo.TaskColorResKeys[Description.Color]];

            // Time indicators
            if (!init)
            {
                UpdateLabels(false, true);
            }
        }

        public void ApplySchedule()
        {
            Margin = new Thickness(0, Utils.General.TimespanToHeight(Description.When - new TimeSpan(5, 0, 0)), 0, 0);
            Height = Utils.General.TimespanToHeight(Description.Duration);
            UpdateLabels();
        }

        #region Resize
        #region Thumb
        private bool IsResizingAllowed(double newSize)
        {
            return newSize > Utils.General.TimespanToHeight(new TimeSpan(0, 4, 58)); // 5 minutes
        }

        #region South
        private void SouthThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            double yadjust = Height + e.VerticalChange;
            if (yadjust >= 0)
            {
                if (!IsResizingAllowed(yadjust))
                    return;

                var desiredDuration = Utils.General.HeightToTimespan(yadjust);
                if (((MainWindow)App.Current.MainWindow).CheckLogic(this, Description.When, desiredDuration))
                {
                    UpdateLabels();
                    var eventArgs = new CellResizedEventArgs(false, Description.When, Description.Duration);

                    Height = yadjust;
                    Description.Duration = desiredDuration;

                    Resized?.Invoke(this, eventArgs);
                }
            }
        }
        #endregion

        #region North
        private void NorthThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!IsResizingAllowed(Height - e.VerticalChange))
                return;

            var desiredMargin = new Thickness(Margin.Left, Margin.Top + e.VerticalChange, Margin.Right, Margin.Bottom);
            var desiredTime = Utils.General.HeightToTimespan(Margin.Top) + new TimeSpan(5, 0, 0);
            var desiredHeight = Height - e.VerticalChange;
            var desiredDuration = Utils.General.HeightToTimespan(desiredHeight);

            if (((MainWindow)App.Current.MainWindow).CheckLogic(this, desiredTime, desiredDuration))
            {
                Margin = desiredMargin;
                Height = desiredHeight;
                Description.When = desiredTime;
                Description.Duration = desiredDuration;

                UpdateLabels(true, false);

                var eventArgs = new CellResizedEventArgs(true, Description.When, Description.Duration);
                Resized?.Invoke(this, eventArgs);
            }
        }
        #endregion

        private void Thumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Description.Duration = Utils.General.RoundTimespanToNearest(Description.Duration, new TimeSpan(0, 5, 0));
            Description.When = Utils.General.RoundTimespanToNearest(Description.When, new TimeSpan(0, 5, 0));
            ApplySchedule();

            Resized?.Invoke(this, new CellResizedEventArgs(true, Description.When, Description.Duration));
        }
        #endregion

        #region Display
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Edit button
            if (e.NewSize.Height > EditBTN.ActualHeight + EditBTN.Margin.Bottom)
            {
                if (Utils.General.DetectCollisions(this, EditBTN, TitleViewbox))
                {
                    _allowEditBtn = false;
                    EditBTN.Visibility = Visibility.Hidden; // Hide directly on resize
                }
                else
                {
                    _allowEditBtn = true;
                }
            }
            else
            {
                _allowEditBtn = false;
                EditBTN.Visibility = Visibility.Hidden; // Hide directly on resize
            }

            // Description
            if (e.NewSize.Height > 50)
            {
                DescGrid.Visibility = Visibility.Visible;
            }
            else
            {
                DescGrid.Visibility = Visibility.Hidden;
            }
        }
        #endregion
        #endregion

        #region Drag
        private bool _dragging = false;
        private double _relativeCursorPosY;

        private void DragGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _relativeCursorPosY = e.GetPosition(Application.Current.MainWindow).Y;
            _dragging = true;
        }
        
        private void DragGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _relativeCursorPosY = 0;
            _dragging = false;

            // Apply new schedule
            Description.When = Utils.General.RoundTimespanToNearest(Description.When, new TimeSpan(0, 5, 0));
            ApplySchedule();
        }

        private void DragGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                double deltaDirection =
                    _relativeCursorPosY - e.GetPosition(Application.Current.MainWindow).Y;
                
                var desiredMargin = new Thickness(Margin.Left, Margin.Top - deltaDirection, Margin.Right, Margin.Bottom);
                var desiredTime = Utils.General.HeightToTimespan(Margin.Top) + new TimeSpan(5, 0, 0);

                if (((MainWindow)App.Current.MainWindow).CheckLogic(this, desiredTime, Description.Duration))
                {
                    Margin = desiredMargin;
                    Description.When = desiredTime;
                    UpdateLabels();

                    Resized?.Invoke(this, new CellResizedEventArgs(deltaDirection > 0, Description.When, Description.Duration));
                }

                _relativeCursorPosY = e.GetPosition(Application.Current.MainWindow).Y;
            }
        }
        #endregion

        #region Menu
        #region Effects
        private void EditBTN_MouseEnter(object sender, MouseEventArgs e)
        {
            EditBtnPath.Fill = Brushes.DimGray;
        }
        private void EditBTN_MouseLeave(object sender, MouseEventArgs e)
        {
            EditBtnPath.Fill = Brushes.Black;
        }
        private void EditBTN_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowTaskEditor();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_allowEditBtn) EditBTN.Visibility = Visibility.Visible;
            if (_showLabel1) AssociatedLabel1.Visibility = Visibility.Visible;
            if (_showLabel2) AssociatedLabel2.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            EditBTN.Visibility = Visibility.Hidden;
            AssociatedLabel1.Visibility = Visibility.Hidden;
            AssociatedLabel2.Visibility = Visibility.Hidden;

            if (_dragging)
            {
                DragGrid_MouseUp(null, null);
            }
        }
        #endregion

        #region Context Menu
        private void EditTaskItem_Click(object sender, RoutedEventArgs e)
        {
            ShowTaskEditor();
        }
        private void DeleteTaskItem_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.75;

            var dialog = MessageBox.Show(
                "Are you sure to delete this task ?",
                "CocoSchedule - Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialog == MessageBoxResult.Yes)
            {
                var parentDayGrid = (Grid)Parent;
                parentDayGrid.Children.Remove(this);
            }
            else
            {
                Opacity = 1;
            }
        }
        #endregion
        #endregion

        #region Config
        private void ShowTaskEditor()
        {
            // https://github.com/dotnet/roslyn/issues/51515

            var tempDescription = Description;

            var taskInfoWindow = new Forms.TaskInformationWindow(ref tempDescription) { Owner = App.Current.MainWindow };
            taskInfoWindow.ShowDialog();

            if (taskInfoWindow.DialogResult == false)
            {
                return;
            }

            Description = tempDescription;
            ConfigureDisplay();
        }

        #endregion
    }
}