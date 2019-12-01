﻿using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for CheckboxDialog.xaml
    /// </summary>
    public partial class ImportDialog : Window
    {
        public ImportDialog(Window owner)
        {
            Owner = owner;
            InitializeComponent();
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public List<bool> Result
        {
            get
            {
                List<bool> result = new List<bool>();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(spCheckboxes); i++)
                {
                    Visual visual = (Visual)VisualTreeHelper.GetChild(spCheckboxes, i);

                    if (visual is Separator)
                    {
                        continue;
                    }

                    result.Add((bool)visual.GetValue(CheckBox.IsCheckedProperty));
                }
                return result;
            }
        }
    }

    public class InverseBool : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Cast<bool>().All(x => !x);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
