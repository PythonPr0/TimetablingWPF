﻿using Humanizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for TeacherTab.xaml
    /// </summary>
    public partial class TeacherTab : Page
    {
        public TeacherTab(Teacher teacher = null)
        {
            InitializeComponent();
            tbTitle.Text = "Create a new Teacher";
            txName.Text = teacher?.Name;
            UnavailablePeriods = teacher?.UnavailablePeriods ?? new ObservableCollection<TimetableSlot>();
            Subjects = teacher?.Subjects ?? new ObservableCollection<Subject>();
            Assignments = teacher?.Assignments ?? new ObservableCollection<Assignment>();
            cmbxSubjects.ItemsSource = (IEnumerable<Subject>)Application.Current.Properties["Subjects"];
            cmbxAssignmentSubject.ItemsSource = Subjects;
            cmbxAssignmentClass.ItemsSource = (IEnumerable<Class>)Application.Current.Properties["Classes"];
            cmbxAssignmentSubject.comboBox.SelectionChanged += CmbxAssignmentsSubjectsSelectionChanged;

            TimetableStructure structure = (TimetableStructure)Application.Current.Properties["Structure"];
            string[] days = new string[5] { "Mon", "Tue", "Wed", "Thu", "Fri" };
            for (int week = 0; week < structure.WeeksPerCycle; week++)
            {
                Grid gridWeek = new Grid()
                {
                    Width = 200
                };
                gridWeek.ColumnDefinitions.Add(new ColumnDefinition());
                gridWeek.RowDefinitions.Add(new RowDefinition());

                gridWeek.Children.Add(Utility.setInternalBorder(new TextBlock()
                {
                    Text = Convert.ToString((char)('A' + week)),
                    Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                    Padding = new Thickness(2),
                    TextAlignment = TextAlignment.Center
                })
                );


                for (int day = 0; day < 5; day++)
                {
                    ColumnDefinition columnDay = new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                    };
                    gridWeek.ColumnDefinitions.Add(columnDay);

                    TextBlock dayHeading = new TextBlock()
                    {
                        Text = days[day],
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                        Padding = new Thickness(2),
                        TextAlignment = TextAlignment.Center
                    };
                    Border dayBorder = Utility.setInternalBorder(dayHeading);
                    Grid.SetColumn(dayBorder, day + 1);
                    gridWeek.Children.Add(dayBorder);
                }

                for (int periodCount=0; periodCount<structure.Structure.Count; periodCount++)
                {
                    TimetableStructurePeriod period = structure.Structure[periodCount];
                    RowDefinition rowPeriod = new RowDefinition()
                    {
                        //Height = new GridLength(1, GridUnitType.Star)
                    };
                    gridWeek.RowDefinitions.Add(rowPeriod);

                    TextBlock periodHeading = new TextBlock()
                    {
                        Text = period.Name,
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                        Padding = new Thickness(2)
                    };
                    Border periodBorder = Utility.setInternalBorder(periodHeading);
                    Grid.SetRow(periodBorder, periodCount + 1);
                    gridWeek.Children.Add(periodBorder);

                    for (int day = 0; day < 5; day++)
                    {
                        bool schedulable = period.IsSchedulable;
                        Rectangle rect = new Rectangle()
                        {
                            Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(schedulable ? "#00FF00" : "#A8A8A8"),
                            Tag = schedulable ? Tuple.Create(new TimetableSlot(week, day, periodCount), true) : null
                        };
                        if (schedulable)
                        {
                            rect.MouseLeftButtonDown += ToggleSlot;
                        }
                        Border rectBorder = Utility.setInternalBorder(rect);
                        Grid.SetColumn(rectBorder, day + 1);
                        Grid.SetRow(rectBorder, periodCount + 1);
                        gridWeek.Children.Add(rectBorder);
                    }
                }
                gridWeek.MouseRightButtonDown += ToggleAll;
                spPeriods.Children.Add(new Border()
                {
                    Child = gridWeek,
                    Style = (Style)Application.Current.Resources["GridLineExternal"],
                    Margin = new Thickness(0, 5, 10, 5)
                });
            }
        }

        private void SubjectButtonClick(object sender, RoutedEventArgs e)
        {
            
            Subject subject = (Subject)cmbxSubjects.SelectedItem;
            if (subject == null)
            {
                if (string.IsNullOrEmpty(cmbxSubjects.Text))
                {
                    return;
                }
                subject = new Subject(cmbxSubjects.Text);
            }
            else
            {
                if (Subjects.Contains(subject))
                {
                    return;
                }
            }
            AddSubject(subject);
        }

        private void AssignmentButtonClick(object sender, RoutedEventArgs e)
        {
            Class @class = (Class)cmbxAssignmentClass.SelectedItem;
            int? periods = iupdown.Value;
            if (@class == null || periods == null)
            {
                return;
            }
            Assignment assignment = new Assignment(@class, (int)periods);
            AddAssignment(assignment);
        }

        private void AddSubject(Subject subject)
        {
            Subjects.Add(subject);
            
            spSubjects.Children.Add(Utility.verticalMenuItem(subject, RemoveSubject));
        }

        private void RemoveSubject(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Subject subject = (Subject)sp.Tag;
            Subjects.Remove(subject);
            spSubjects.Children.Remove(sp);
        }

        private void AddAssignment(Assignment assignment)
        {
            Assignments.Add(assignment);

            spAssignments.Children.Add(Utility.verticalMenuItem(assignment, RemoveAssignment));
        }

        private void RemoveAssignment(object sender, RoutedEventArgs e)
        {
            StackPanel sp = (StackPanel)((FrameworkElement)sender).Tag;
            Assignment assignment = (Assignment)sp.Tag;
            Assignments.Remove(assignment);
            spAssignments.Children.Remove(sp);
        }

        private ObservableCollection<TimetableSlot> UnavailablePeriods;
        private ObservableCollection<Subject> Subjects;
        private ObservableCollection<Assignment> Assignments;
        public MainPage MainPage = (MainPage)Application.Current.MainWindow.Content;
        private Dictionary<Class, int> OldAssignments = new Dictionary<Class, int>();

        private void ToggleAll(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)sender;
            foreach (Border border in grid.Children)
            {
                if (border.Child is Rectangle rect && rect.Tag != null)
                {
                    ToggleSlot(rect, null);
                }
            }

        }


        private void ToggleSlot(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;
            Tuple<TimetableSlot, bool> tag = (Tuple<TimetableSlot, bool>)rect.Tag;
            rect.Tag = Tuple.Create(tag.Item1, !tag.Item2);
            rect.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(tag.Item2 ? "#FF0000" : "#00FF00");
            if (tag.Item2)
            {
                UnavailablePeriods.Add(tag.Item1);
                return;
            }
            UnavailablePeriods.Remove(tag.Item1);
        }

        private void CmbxSubjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SubjectButtonClick(null, null);
            }
            if (e.Key == Key.Escape)
            {
                cmbxSubjects.SelectedItem = null;
                Keyboard.ClearFocus();
            }
        }

        private void CmbxAssignmentsSubjectsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbx = (ComboBox)sender;
            Subject subject = (Subject)cmbx.SelectedItem;
            IEnumerable<Class> all_classes = (IEnumerable<Class>)Application.Current.Properties["Classes"];
            if (subject != null)
            {
                cmbxAssignmentClass.ItemsSource = all_classes;
                return;
            }
            IEnumerable<Class> classes = from @class in all_classes where @class.Subject == subject select @class;
            cmbxAssignmentClass.ItemsSource = classes;
        }
    }
}
