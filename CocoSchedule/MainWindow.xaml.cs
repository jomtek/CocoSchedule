using CocoSchedule.Data;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using System.IO;

namespace CocoSchedule
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Other
        private Dictionary<DayOfWeek, Grid> _daysGrids;
        private DateTime _firstDayShown;

        // UI
        private DispatcherTimer _uiTimer;
        private double _lastClickedY = 0;

        public MainWindow()
        {
            InitializeComponent();
            Init_UITimer();

            _daysGrids = new Dictionary<DayOfWeek, Grid>()
            {
                { DayOfWeek.Monday,    MondayGrid    },
                { DayOfWeek.Tuesday,   TuesdayGrid   },
                { DayOfWeek.Wednesday, WednesdayGrid },
                { DayOfWeek.Thursday,  ThursdayGrid  },
                { DayOfWeek.Friday,    FridayGrid    },
                { DayOfWeek.Saturday,  SaturdayGrid  },
                { DayOfWeek.Sunday,    SundayGrid    }
            };

            Utils.General.RearrangeDays((int)DateTime.Now.DayOfWeek, ref TopGrid);
            Utils.General.RearrangeDays((int)DateTime.Now.DayOfWeek, ref DaysGrid);

            _firstDayShown = Utils.General.GetActualDay();
            RefreshDaysAnnotations();
        }

        private void Init_UITimer()
        {
            _uiTimer = new DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromSeconds(2);
            _uiTimer.Tick += (object sender, EventArgs e) => { UpdateTimeLine(); };
            _uiTimer.Start();
        }

        private void RefreshDaysAnnotations()
        {
            int counter = 0;
            foreach (TextBlock dayTB in TopGrid.Children)
            {
                dayTB.Text = _firstDayShown.AddDays(counter).ToString("ddd MM/dd");
                
                // Highlight day annotation if actual day
                if (counter == 0 && _firstDayShown == Utils.General.GetActualDay())
                {
                    dayTB.Background = Brushes.Yellow;
                    dayTB.ToolTip = "Today";
                }
                else
                {
                    dayTB.Background = Brushes.Transparent;
                    dayTB.ToolTip = null;
                }

                counter++;
            }
        }

        #region Timeline
        private void UpdateTimeLine()
        {
            TimeLinePath.Margin = new Thickness(25, GuessTimeLineY(), 0, 0);
        }

        private double GuessTimeLineY()
        {
            var margin = 30;

            if (DateTime.Now.TimeOfDay > new TimeSpan(5, 0, 0))
            {
                var secondsSince = (int)DateTime.Now.Subtract(new TimeSpan(5, 0, 0)).TimeOfDay.TotalSeconds;
                return margin + (secondsSince * GlobalInfo.HourHeight) / 3600d;
            }
            else
            {
                return margin;
            }
        }
        #endregion

        #region UI Reactivity
        private void GuessHourHeight()
        {
            GlobalInfo.HourHeight = DaysGrid.ActualHeight / 20d;
        }

        private void RefreshAll(bool init = false)
        {
            GuessHourHeight();

            ForAllCells((Cell c) => c.ConfigureDisplay(init));
            UpdateTimeLine();

            TimeIndicationPath1.Visibility = Visibility.Hidden;
            TimeIndicationPath2.Visibility = Visibility.Hidden;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshAll();

        }
        #endregion

        #region UX Help
        #region Time Indicators
        private double ComputeTimeIndicatorsWidth(Cell cell)
        {
            Point relativeLocation = cell.TranslatePoint(new Point(0, 0), TimeTableGrid);
            return relativeLocation.X - 20 /* margin */ + MondayGrid.ActualWidth;
        }

        private void MoveTimeIndicators(Cell cell, bool north = false)
        {
            TimeIndicationPath1.Visibility = Visibility.Visible;
            TimeIndicationPath2.Visibility = Visibility.Visible;

            TimeIndicationPath1.Margin = new Thickness(
                TimeIndicationPath1.Margin.Left,
                cell.Margin.Top + 30 - 3,
                0,
                0
            );

            if (!north)
            {
                TimeIndicationPath2.Margin = new Thickness(
                    TimeIndicationPath2.Margin.Left,
                    cell.Margin.Top + 30 + cell.ActualHeight,
                    0,
                    0
                );
            }

            TimeIndicationPath1.Width = ComputeTimeIndicatorsWidth(cell);
            TimeIndicationPath2.Width = ComputeTimeIndicatorsWidth(cell);
        }
        #endregion

        #region Zoom
        private async void TableZoomPlus_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TimeTableGrid.Height = TimeTableGrid.ActualHeight * 1.25;
            await Task.Delay(10);
            RefreshAll();
        }

        private async void TableZoomMinus_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TimeTableGrid.Height = TimeTableGrid.ActualHeight * (1 / 1.25);
            await Task.Delay(10);
            RefreshAll();
        }
        #endregion

        #region Cells
        private bool IsMouseOverAnyCell() // TODO: is there any more concise way of doing this ?
        {
            foreach (KeyValuePair<DayOfWeek, Grid> entry in _daysGrids)
            {
                foreach (Cell cell in entry.Value.Children)
                {
                    if (cell.IsMouseOver) return true;
                }
            }

            return false;
        }

        private void ForAllCells(Action<Cell> f)
        {
            foreach (KeyValuePair<DayOfWeek, Grid> entry in _daysGrids)
            {
                foreach (Cell cell in entry.Value.Children)
                {
                    f(cell);
                }
            }
        }
        #endregion
        #endregion

        #region Context Menu
        private void DayGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOverAnyCell())
            {
                var cmnu = FindResource("CMenu") as ContextMenu;
                cmnu.IsOpen = true;
            }
        }
        private void AddTaskCMItem_Click(object sender, RoutedEventArgs e)
        {
            Cell c;

            foreach (KeyValuePair<DayOfWeek, Grid> entry in _daysGrids)
            {
                var grid = entry.Value;

                if (grid.IsMouseOver)
                {
                    // TODO: avoid repeating code pattern for task information window dialog
                    
                    // Placeholder arguments for constructor...
                    var cellDescription = new TaskDescription(entry.Key, new TimeSpan(), new TimeSpan(), null, null, (TaskColor)1);

                    var taskInfoWindow = new Forms.TaskInformationWindow(ref cellDescription) { Owner = this };
                    taskInfoWindow.ShowDialog();

                    // Dialog canceled
                    if (taskInfoWindow.DialogResult == false)
                    {
                        return;
                    }

                    var ts = Utils.General.HeightToTimespan(_lastClickedY) + new TimeSpan(5, 0, 0);
                    var when = Utils.General.RoundTimespanToNearest(ts, new TimeSpan(0, 10, 0));
                    var duration = new TimeSpan(1, 0, 0);

                    var candidateTime = when + duration;

                    // Reverse iteration through the tasks of the day
                    for (int i = grid.Children.Count - 1; i >= 0; i--)
                    {
                        var cell = (Cell)grid.Children[i];
                        if (Utils.General.CheckTimespanOverlap(when, when + duration, cell.Description.When, cell.Description.When + cell.Description.Duration))
                        {
                            duration = cell.Description.When - when;
                            if (duration < new TimeSpan(0, 4, 55))
                            {
                                return;
                            }
                            
                            break;
                        }
                    }

                    cellDescription.When = when;
                    cellDescription.Duration = duration;

                    c = new Cell(cellDescription);
                    grid.Children.Add(c);

                    InitCell(c);
                }
            }

            RefreshAll();
        }

        private void AddDeadlineCMItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region Other
        private void TableGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastClickedY = e.GetPosition(TableGrid).Y;
        }
        #endregion
        #endregion

        #region Temp
        private void InitCell(Cell c)
        {
            c.HorizontalAlignment = HorizontalAlignment.Stretch;
            c.Width = Double.NaN;

            var lbl1 = new TextBlock()
            {
                Text = "0 AM",
                Margin = new Thickness(0, 0, 15, 0),
                FontSize = 15,
                Opacity = 0.75,
                Visibility = Visibility.Hidden,
            };

            var lbl2 = new TextBlock()
            {
                Text = lbl1.Text,
                Margin = lbl1.Margin,
                FontSize = lbl1.FontSize,
                Opacity = lbl1.Opacity,
                Visibility = lbl1.Visibility,
            };

            c.AssociatedLabel1 = lbl1;
            c.AssociatedLabel2 = lbl2;
            OtherTimeAnnotationsGrid.Children.Add(lbl1);
            OtherTimeAnnotationsGrid.Children.Add(lbl2);

            c.MouseEnter += (object sender_, MouseEventArgs __) =>
            {
                MoveTimeIndicators(c);
            };

            c.Resized += (object sender_, CellResizedEventArgs e) =>
            {
                foreach (KeyValuePair<DayOfWeek, Grid> entry in _daysGrids)
                {
                    if (c.Description.Day == entry.Key)
                    {
                        bool collision = new Func<bool>(() =>
                        {
                            foreach (Cell candidateCell in entry.Value.Children)
                            {
                                if (candidateCell.Description.When == c.Description.When) continue; // Ignore self

                                if (Utils.General.CheckTimespanOverlap(
                                    c.Description.When, c.Description.When + c.Description.Duration,
                                    candidateCell.Description.When, candidateCell.Description.When + candidateCell.Description.Duration))
                                {
                                    return true;
                                }
                            }

                            return false;
                        })();

                        if (collision)
                        {
                            c.Description.When = e.InitialWhen;
                            c.Description.Duration = e.InitialDuration;
                            c.ApplySchedule();

                            return;
                        }
                    }
                }

                MoveTimeIndicators(c, e.North);
            };

            c.MouseLeave += (object sender_, MouseEventArgs __) =>
            {
                var cell = (Cell)sender_;

                TimeIndicationPath1.Visibility = Visibility.Hidden;
                TimeIndicationPath2.Visibility = Visibility.Hidden;
            };

            c.ConfigureDisplay(true);
            c.JustResized(false, false, true);
        }
        #endregion

        #region Tool Menu
        #region Effects
        private void LastWeekBTN_MouseEnter(object sender, MouseEventArgs e)
        {
            LastWeekBTN.Background = Brushes.SkyBlue;
        }

        private void LastWeekBTN_MouseLeave(object sender, MouseEventArgs e)
        {
            LastWeekBTN.Background = Brushes.Transparent;
        }

        private void NextWeekBTN_MouseEnter(object sender, MouseEventArgs e)
        {
            NextWeekBTN.Background = Brushes.SkyBlue;
        }

        private void NextWeekBTN_MouseLeave(object sender, MouseEventArgs e)
        {
            NextWeekBTN.Background = Brushes.Transparent;
        }
        #endregion

        #region Navigate
        private void SaveActuallyShownDays()
        {
            var currentIterDay = _firstDayShown;
            foreach (Grid dayGrid in DaysGrid.Children)
            {
                var tasks = new List<TaskDescription>();
                foreach (Cell cell in dayGrid.Children)
                {
                    tasks.Add(cell.Description);
                }

                if (GlobalInfo.Calendar.ContainsKey(currentIterDay))
                {
                    GlobalInfo.Calendar.Remove(currentIterDay);
                }

                GlobalInfo.Calendar.Add(currentIterDay, tasks);

                currentIterDay = currentIterDay.AddDays(1);
            }
        }

        private void FillTableWithCalendarInfo()
        {
            var currentIterDay = _firstDayShown;
            foreach (Grid dayGrid in DaysGrid.Children)
            {
                dayGrid.Children.Clear();

                if (GlobalInfo.Calendar.ContainsKey(currentIterDay))
                {
                    List<TaskDescription> tasks = GlobalInfo.Calendar[currentIterDay];
                    foreach (TaskDescription task in tasks)
                    {
                        var cell = new Cell(task);
                        InitCell(cell);

                        dayGrid.Children.Add(cell);
                    }
                }
                else
                {
                    // Add empty day to the calendar with current iteration day
                    GlobalInfo.Calendar.Add(currentIterDay, new List<TaskDescription>());
                }

                currentIterDay = currentIterDay.AddDays(1);
            }
        }

        private void LastWeekBTN_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveActuallyShownDays();
            _firstDayShown = _firstDayShown.AddDays(-7);

            FillTableWithCalendarInfo();

            RefreshDaysAnnotations();
        }

        private void NextWeekBTN_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveActuallyShownDays();
            _firstDayShown = _firstDayShown.AddDays(7);

            FillTableWithCalendarInfo();

            RefreshDaysAnnotations();
        }
        #endregion

        #endregion

        #region Window events
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (File.Exists("calendar.ser"))
            {
                GlobalInfo.Calendar =
                    Utils.Serialization.Deserialize<Dictionary<DateTime, List<TaskDescription>>>("calendar.ser");
            }
            else
            {
                MessageBox.Show(
                    "Welcome and thank you for choosing CocoSchedule !\n\n" +
                    "Find this project on Github.\nMade with love by Jomtek.",
                    "CocoSchedule",
                    MessageBoxButton.OK, MessageBoxImage.Information
                );
            }

            FillTableWithCalendarInfo();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveActuallyShownDays();
            Utils.Serialization.Serialize(GlobalInfo.Calendar, "calendar.ser");
        }
        #endregion

        private async void MainSV_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
            {
                TimeTableGrid.Height = TimeTableGrid.ActualHeight * 1.25;
            }
            else if (e.Delta < 0)
            {
                TimeTableGrid.Height = TimeTableGrid.ActualHeight * (1 / 1.25);
            }

            await Task.Delay(10);
            RefreshAll();
        }
    }
}