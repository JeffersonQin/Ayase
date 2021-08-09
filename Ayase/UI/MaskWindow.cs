using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace Ayase.UI
{
    public class MaskWindow : ReuseWindow
    {
        public MaskWindow() : base()
        {
            WindowStyle = System.Windows.WindowStyle.None;
            AllowsTransparency = true;
            ShowInTaskbar = false;
            ResizeMode = System.Windows.ResizeMode.NoResize;
            Title = "MaskWindow";
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
            MouseDown += Window_MouseDown;
            KeyDown += Window_KeyDown;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowManager.FocusSearch();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                new Thread(() =>
                {
                    WindowManager.EndProcess();
                }).Start();
            }
        }
    }
}
