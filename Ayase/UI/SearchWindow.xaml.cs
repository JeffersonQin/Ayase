using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
    /// SearchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearchWindow : MaskWindow
    {
        private bool ShiftModifier = false;

        public SearchWindow()
        {
            InitializeComponent();
        }

        private void MaskWindow_Activated(object sender, EventArgs e)
        {
            QueryTextBox.Focus();
        }

        private void QueryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if (!ShiftModifier) WindowManager.SetFocusNext();
                else WindowManager.SetFocusPrevious();
            }
            else if (e.Key == Key.Escape)
                new Thread(() => { WindowManager.EndProcess(); }).Start();
            else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                ShiftModifier = true;
        }

        private void QueryTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                ShiftModifier = false;
        }

        public void QueryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            WindowManager.candidateIndexes.Clear();
            string query = QueryTextBox.Text.ToLower();
            for (int i = 0; i < WindowManager.UIElementCount; i++)
                if (WindowManager.MatchStrings.TryGetValue(i, out List<string> names))
                {
                    bool flagContains = false;
                    foreach (var name in names)
                        if (name.Contains(query))
                        {
                            flagContains = true;
                            break;
                        }
                    if (flagContains)
                    {
                        WindowManager.candidateIndexes.Add(i);
                        WindowManager.notationLabels[i].SetStatus(NotationLabelStatus.Candidate);
                    } else WindowManager.notationLabels[i].SetStatus(NotationLabelStatus.Other);
                }
            WindowManager.candidateIndexes.Sort();
            if (WindowManager.candidateIndexes.Contains(WindowManager.focusIndex))
                WindowManager.SetFocus(WindowManager.focusIndex);
            else if (WindowManager.candidateIndexes.Count > 0)
                WindowManager.SetFocus(WindowManager.candidateIndexes[0]);
            else
                WindowManager.SetFocus(-1);
        }
    }
}
