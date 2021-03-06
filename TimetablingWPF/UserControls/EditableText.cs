﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace TimetablingWPF
{
    public class EditableText : TextBox
    {
        private string lastText;
        public new string Text
        {
            get => base.Text;
            set
            {
                lastText = value;
                base.Text = value;
            }
        }
        public EditableText()
        {
            IsKeyboardFocusedChanged += delegate (object sender, DependencyPropertyChangedEventArgs e)
            {
                if ((bool)e.NewValue)
                {
                    SelectAll();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Text))
                    {
                        Text = lastText;
                    }
                    lastText = Text;
                }
            };
            KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    Text = lastText;
                    Keyboard.ClearFocus();
                }
            };
            PreviewMouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e)
            {
                Focus();
                e.Handled = true;
            };
            BorderThickness = new Thickness(0);
            Background = GenericResources.TRANSPARENT;
            CaretBrush = GenericResources.BLACK;
            //HorizontalAlignment = HorizontalAlignment.Center;
        }
    }
}
