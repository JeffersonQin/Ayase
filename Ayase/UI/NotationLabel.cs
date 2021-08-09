using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ayase.UI
{
    public class NotationLabel : Label
    {
        private NotationLabelStatus Status;

        public NotationLabel(double x, double y, double w, double h, String Name) : base()
        {
            SetStatus(NotationLabelStatus.Candidate);
            VerticalAlignment = VerticalAlignment.Center;
            VerticalContentAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            HorizontalContentAlignment = HorizontalAlignment.Center;
            BorderBrush = new SolidColorBrush(Colors.Red);
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            BorderThickness = new Thickness(1);
            Width = w;
            Height = h;
            SetValue(Canvas.LeftProperty, x);
            SetValue(Canvas.TopProperty, y);
            TextBlock tb = new TextBlock();
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Text = Name;
            Content = tb;
        }

        public NotationLabelStatus GetStatus() { return Status; }

        public void SetStatus(NotationLabelStatus Status)
        {
            this.Status = Status;
            switch (Status)
            {
                case NotationLabelStatus.Candidate:
                    Foreground = new SolidColorBrush(Colors.Red);
                    Background = new SolidColorBrush(Color.FromArgb(100, 0, 77, 230));
                    break;
                case NotationLabelStatus.Focus:
                    Foreground = new SolidColorBrush(Colors.Red);
                    Background = new SolidColorBrush(Color.FromArgb(100, 255, 226, 104));
                    break;
                case NotationLabelStatus.Other:
                    Foreground = new SolidColorBrush(Colors.White);
                    Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    break;
            }
        }
    }

    public enum NotationLabelStatus
    {
        Candidate,
        Focus,
        Other
    }
}
