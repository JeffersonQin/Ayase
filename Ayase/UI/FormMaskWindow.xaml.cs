using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ayase.UI
{
    /// <summary>
    /// FormMaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FormMaskWindow : MaskWindow
    {
        public FormMaskWindow()
        {
            InitializeComponent();
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private void MaskWindow_Activated(object sender, EventArgs e)
        {
            WindowManager.FocusSearch();
        }
    }
}
