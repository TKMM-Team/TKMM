using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Tkmm.Core;

namespace Tkmm.Views.Common;

public partial class MergingModal : UserControl
{
    private static readonly DialogHost _host = new() {
        Content = new MergingModal()
    };
    
    public MergingModal()
    {
        InitializeComponent();
    }
    
    public static void ShowModal(CancellationToken cancellationToken)
    {
        if (!Config.Shared.ShowTriviaPopup) {
            return;
        }
        
        if (OverlayLayer.GetOverlayLayer(App.XamlRoot) is not { } overlayLayer) {
            return;
        }

        overlayLayer.Children.Add(_host);

        _ = Task.Run(() => {
            while (!cancellationToken.IsCancellationRequested) {
            }

            Dispatcher.UIThread.Invoke(() => {
                overlayLayer.Children.Remove(_host);
            });
        }, cancellationToken);
    }
}