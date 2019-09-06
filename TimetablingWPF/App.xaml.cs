﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Humanizer;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Input;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Properties["APPLICATION_NAME"] = "Timetabler";
            Properties["RecentFiles"] = new Queue<string>(6);
            Properties["Teachers"] = new ObservableCollection<Teacher>();
            Properties["Rooms"] = new ObservableCollection<Room>();
            Properties["Classes"] = new ObservableCollection<Class>();
            Properties["Subjects"] = new ObservableCollection<Subject>();
            Properties["Groups"] = new ObservableCollection<Subject>();
            Properties["Structure"] = new TimetableStructure(2, new List<TimetableStructurePeriod>()
            {
                new TimetableStructurePeriod("1", true),
                new TimetableStructurePeriod("2", true),
                new TimetableStructurePeriod("Brk", false),
                new TimetableStructurePeriod("3", true),
                new TimetableStructurePeriod("4", true),
                new TimetableStructurePeriod("Lch", false),
                new TimetableStructurePeriod("5", true)
            });

        }

    }

    internal class FileFunctions
    {
        public static void LoadFile(string fpath)
        {
            Uri URI = new Uri(fpath);
            using (FileStream fs = new FileStream(URI.LocalPath, FileMode.Open))
            {
                
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    IDictionary<object, object> dict = (Dictionary<object, object>)formatter.Deserialize(fs);
                    Application.Current.Properties.Clear();
                    foreach (KeyValuePair<object, object> entry in dict)
                    {
                        Application.Current.Properties[entry.Key] = entry.Value;
                    }
                }
                catch (Exception e)
                {
                    Utility.ShowErrorBox(e.Message);
                    throw e;
                }
            }
        }
        public static void SaveFile(string fpath)
        {
            Uri URI = new Uri(fpath);
            using (FileStream fs = new FileStream(URI.LocalPath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, Application.Current.Properties);
                }
                catch (Exception e)
                {
                    Utility.ShowErrorBox(e.Message);
                    throw e;
                }
            }
        }
    }

    internal class Utility
    {
        public static string Pluralize(int number, string word)
        {
            return number == 1 ? word : word.Pluralize();
        }
        public static Border setInternalBorder(FrameworkElement element)
        {
            return new Border()
            {
                Child = element,
                Style = (Style)Application.Current.Resources["GridLineInternal"]
            };
        }
        public static StackPanel verticalMenuItem(object @object, MouseButtonEventHandler @event = null)
        {
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Tag = @object, Margin = new Thickness(0, 0, 0, 5) };
            TextBlock tb = new TextBlock()
            {
                Style = (Style)Application.Current.Resources["DialogText"],
                Text = @object.ToString(),
                Margin = new Thickness(0, 0, 5, 0),
                Height = 22,
                Padding = new Thickness(1)                
            };
            Image img = new Image()
            {
                Source = (ImageSource)Application.Current.Resources["CrossIcon"],
                Height = 22,
                Tag = sp,
                Cursor = Cursors.Hand
            };
            if (@event != null)
            {
                img.MouseDown += @event;
            }
            sp.Children.Add(tb);
            sp.Children.Add(img);
            return sp;
        }
        public static void ShowErrorBox(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}