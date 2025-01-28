using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Core.Providers;
using TkSharp.Core;

namespace Tkmm.Actions;

public abstract class GuardedActionGroup<TSingleton> where TSingleton : GuardedActionGroup<TSingleton>, new()
{
    protected abstract string ActionGroupName { get; }

    public static TSingleton Instance { get; } = new();

    /// <summary>
    /// Run pre-action checks to ensure the target action can run.
    /// </summary>
    protected async ValueTask<bool> CanActionRun([CallerMemberName] string? actionName = null, bool showError = true)
    {
        TkLog.Instance.LogInformation(
            "Validating {ActionName} from {ActionGroupName}.", actionName, ActionGroupName);
        
        LogInfo();

        if (EnsureConfiguration()) {
            TkLog.Instance.LogInformation(
                "Executing {ActionName} from {ActionGroupName}.", actionName, ActionGroupName);
            
            return true;
        }


        if (showError is false) {
            return false;
        }
        
        TkLog.Instance.LogCritical(
            "Failed to run {ActionName} from {ActionGroupName}, the configuration was invalid.",
            actionName, ActionGroupName);
        
        await ShowFailureDialog(actionName,
            "The application configuration is invalid, the invoked action cannot run.");
        
        return false;
    }

    /// <summary>
    /// Show a dialog with an error message.
    /// </summary>
    protected async ValueTask ShowFailureDialog(string? actionName, string errorMessage)
    {
        ContentDialog dialog = new() {
            Title = $"Failed to run {actionName}",
            Content = new TextBlock {
                Text = errorMessage,
                TextWrapping = TextWrapping.WrapWithOverflow
            },
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "OK"
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// Log information about the configured application for end-user debugging.
    /// </summary>
    public static void LogInfo()
    {
        TkLog.Instance.LogInformation(
            "Game Language: {GameLanguage}.", TKMM.Config.GameLanguage);
    }

    /// <summary>
    /// Ensure the application is configured correctly.
    /// </summary>
    protected virtual bool EnsureConfiguration()
    {
        return TkRomProvider.CanProvideRom();
    }
}