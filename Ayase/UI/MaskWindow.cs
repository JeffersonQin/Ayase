using System;
using System.Collections.Generic;
using System.Text;
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
            Background = new SolidColorBrush(Color.FromArgb(82, 0, 0, 0));
            MouseDown += Window_MouseDown;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
