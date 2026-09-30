using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XForensics.App.Controls;

public partial class ScanLogPanel : UserControl
{
    public event EventHandler<string>? TabChanged;

    public ScanLogPanel()
    {
        InitializeComponent();
    }

    public void AppendLog(string message, bool isSuccess = false, bool isError = false)
    {
        Dispatcher.Invoke(() =>
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            var tb = new TextBlock
            {
                Text = $"[{time}] {message}",
                FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                FontSize = 11,
                Margin = new Thickness(0, 1.5, 0, 1.5)
            };

            if (isSuccess)
            {
                tb.SetResourceReference(TextBlock.ForegroundProperty, "SuccessBrush");
                tb.FontWeight = FontWeights.SemiBold;
            }
            else if (isError)
            {
                tb.SetResourceReference(TextBlock.ForegroundProperty, "DangerBrush");
                tb.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                tb.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            }

            LogContainer.Children.Add(tb);
            LogScrollViewer.ScrollToEnd();
        });
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        LogContainer.Children.Clear();
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Content is string tabName)
        {
            TabChanged?.Invoke(this, tabName);
        }
    }
}
