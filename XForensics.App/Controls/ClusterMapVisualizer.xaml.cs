using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XForensics.App.Controls;

public partial class ClusterMapVisualizer : UserControl
{
    public event EventHandler<int>? ClusterClicked;

    public ClusterMapVisualizer()
    {
        InitializeComponent();
        Loaded += (_, _) => RenderDemoClusterMap();
    }

    public void RenderDemoClusterMap()
    {
        ClusterGrid.Children.Clear();

        const int totalBlocks = 44 * 9; // 396 blocks

        for (var i = 0; i < totalBlocks; i++)
        {
            var row = i / 44;
            var col = i % 44;

            string resourceKey;
            string state;

            // Placement matching screenshot features
            if ((row == 1 && (col == 15 || col == 22)) ||
                (row == 2 && (col == 8 || col == 31)) ||
                (row == 3 && (col == 19 || col == 20 || col == 33)) ||
                (row == 4 && (col == 2 || col == 14 || col == 25 || col == 35)) ||
                (row == 5 && (col == 20 || col == 21 || col == 28)) ||
                (row == 7 && (col == 3 || col == 17)))
            {
                resourceKey = "ClusterDeletedBrush";
                state = "Deleted";
            }
            else if ((row == 2 && col == 18) ||
                     (row == 4 && (col == 7 || col == 28)) ||
                     (row == 5 && col == 33) ||
                     (row == 6 && (col == 5 || col == 15)) ||
                     (row == 7 && col == 24))
            {
                resourceKey = "ClusterBadBrush";
                state = "Bad";
            }
            else if (row >= 5 && ((col >= 25 && col <= 38) || (row >= 6 && col >= 10 && col <= 22) || (row == 8 && col >= 30)))
            {
                resourceKey = "ClusterFreeBrush";
                state = "Free";
            }
            else if (row == 8 && (col == 41 || col == 42 || col == 43))
            {
                resourceKey = "ClusterFreeBrush";
                state = "Free";
            }
            else
            {
                resourceKey = "ClusterUsedBrush";
                state = "Used";
            }

            var clusterIndex = i * 64;
            var border = new Border
            {
                CornerRadius = new CornerRadius(1.5),
                Margin = new Thickness(1.2),
                ToolTip = $"Cluster #{clusterIndex:N0} - {state} (0x{clusterIndex * 4096:X8})",
                Tag = clusterIndex,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Bind background dynamically so it responds to theme switching
            border.SetResourceReference(Border.BackgroundProperty, resourceKey);

            var captured = clusterIndex;
            border.MouseEnter += (s, _) =>
            {
                if (s is Border b)
                {
                    b.BorderThickness = new Thickness(1);
                    b.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
                }
            };
            border.MouseLeave += (s, _) =>
            {
                if (s is Border b)
                {
                    b.BorderThickness = new Thickness(0);
                }
            };
            border.MouseLeftButtonDown += (_, _) => ClusterClicked?.Invoke(this, captured);

            ClusterGrid.Children.Add(border);
        }
    }
}
