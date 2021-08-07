using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Ayase.UI
{
    public class ReuseWindow : Window
    {
        public ReuseWindow() : base()
        {
            Closing += Window_Closing;
            StateChanged += Window_StateChanged;
            KeyDown += Window_KeyDown;
        }

        public void Present()
        {
            Show();
            Activate();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Hide();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }
    }
}
