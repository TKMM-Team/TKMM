using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

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
        _ = Dispatcher.UIThread.InvokeAsync(async () => {
            if (OverlayLayer.GetOverlayLayer(App.XamlRoot) is not { } overlayLayer) {
                return;
            }

            overlayLayer.Children.Add(_host);

            await Task.Run(() => {
                while (!cancellationToken.IsCancellationRequested) {
                }
            }, cancellationToken);
            
            overlayLayer.Children.Remove(_host);
        });
    }
}