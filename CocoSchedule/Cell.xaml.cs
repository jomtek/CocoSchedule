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

            MouseEnter += (object sender, MouseEventArgs e) =>
            {
                if (_showLabel1) AssociatedLabel1.Visibility = Visibility.Visible;
                if (_showLabel2) AssociatedLabel2.Visibility = Visibility.Visible;
            };

            MouseLeave += (object sender, MouseEventArgs e) =>
            {
                AssociatedLabel1.Visibility = Visibility.Hidden;
                AssociatedLabel2.Visibility = Visibility.Hidden;
            };

            Description = description;
            ConfigureDisplay(true);
        }

        public void ConfigureDisplay(bool init = false)
        {
            // When
            TimeSpan distanceFrom6AM = Description.When - new TimeSpan(6, 0, 0);
            double topMargin = Utils.TimespanToHeight(distanceFrom6AM);
            Margin = new Thickness(0, topMargin, 0, 0);

            // Duration
            Height = Utils.TimespanToHeight(Description.Duration);

            // Title and description
            TitleTB.Text = Description.TitleText;
            DescTB.Text = Description.DescriptionText;

            // Time indicators
            if (!init)
            {
                JustResized(false, true);
            }
        }

        public void ApplySchedule()
        {
            Margin = new Thickness(0, Utils.TimespanToHeight(Description.When - new TimeSpan(6, 0, 0)), 0, 0);
            Height = Utils.TimespanToHeight(Description.Duration);
            JustResized();
        }

        #region Resize
        #region Thumb
        private bool IsResizingAllowed(double newSize)
        {
            return newSize > Utils.TimespanToHeight(new TimeSpan(0, 4, 58)); // 5 minutes
        }

        public void JustResized(bool north = false, bool noLabels = false)
        {
            var startTime = Utils.HeightToTimespan(Margin.Top) + new TimeSpan(6, 0, 0);
            var endTime = Description.Duration + startTime;

            if (startTime.Minutes != 0)
            {
                AssociatedLabel1.Text = DateTime.Today.Add(startTime).ToString("hh:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
                AssociatedLabel1.Margin = new Thickness(0, Utils.TimespanToHeight(startTime - new TimeSpan(6, 0, 0)), 0, 0);
                if (!noLabels) AssociatedLabel1.Visibility = Visibility.Visible;
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
                    AssociatedLabel2.Margin = new Thickness(0, Utils.TimespanToHeight(endTime - new TimeSpan(6, 0, 0)), 0, 0);
                    if (!noLabels) AssociatedLabel2.Visibility = Visibility.Visible;
                    if (!noLabels) _showLabel2 = true;
                }
                else
                {
                    AssociatedLabel2.Visibility = Visibility.Hidden;
                    _showLabel2 = false;
                }
            }
        }

        #region South
        private void southThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            double yadjust = Height + e.VerticalChange;
            if (yadjust >= 0)
            {
                if (!IsResizingAllowed(yadjust)) return;
                
                JustResized();
                var eventArgs = new CellResizedEventArgs(false, Description.When, Description.Duration);

                Height = yadjust;
                Description.Duration = Utils.HeightToTimespan(yadjust);

                Resized?.Invoke(this, eventArgs);
            }
        }

        private void southThumb_MouseEnter(object sender, MouseEventArgs e) { Cursor = Cursors.SizeNS; }
        private void southThumb_MouseLeave(object sender, MouseEventArgs e) { Cursor = Cursors.Arrow;  }
        #endregion
        #region North
        private void northThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!IsResizingAllowed(Height - e.VerticalChange)) return;
            Height -= e.VerticalChange;
            Margin = new Thickness(Margin.Left, Margin.Top + e.VerticalChange, Margin.Right, Margin.Bottom);

            JustResized(true, false);
            var eventArgs = new CellResizedEventArgs(true, Description.When, Description.Duration);

            Description.Duration = Utils.HeightToTimespan(Height);
            Description.When = Utils.HeightToTimespan(Margin.Top) + new TimeSpan(6, 0, 0);

            Resized?.Invoke(this, eventArgs);
        }
        #endregion

        private void Thumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Description.Duration = Utils.RoundTimespanToNearest(Description.Duration, new TimeSpan(0, 5, 0));
            Description.When = Utils.RoundTimespanToNearest(Description.When, new TimeSpan(0, 5, 0));
            ApplySchedule();

            Resized?.Invoke(this, new CellResizedEventArgs(true, Description.When, Description.Duration));
        }
        #endregion

        #region Display
        private void userControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // TODO: make the limit-values non-arbitrary

            // Edit button
            if (e.NewSize.Height > EditBTN.ActualHeight + EditBTN.Margin.Bottom)
            {
                if (Utils.DetectCollisions(this, EditBTN, TitleViewbox))
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

        private void userControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_allowEditBtn) EditBTN.Visibility = Visibility.Visible;
        }

        private void userControl_MouseLeave(object sender, MouseEventArgs e)
        {
            EditBTN.Visibility = Visibility.Hidden;
        }
        #endregion
        #region Context Menu
        private void EditTaskItem_Click(object sender, RoutedEventArgs e)
        {
            ShowTaskEditor();
        }
        private void DeleteTaskItem_Click(object sender, RoutedEventArgs e)
        {

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