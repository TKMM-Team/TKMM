using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
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

        if (EnsureConfiguration(out string? reason)) {
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
        
        await ShowFailureDialog(actionName, reason);
        
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
    protected virtual bool EnsureConfiguration([MaybeNullWhen(true)] out string reason)
    {
        if (TKMM.TryGetTkRom() is not { } tkRom) {
            // TODO: Set reason to a translated message
            reason = "Failed to retrieve TotK rom. Please review your TotK configuration.";
            TkLog.Instance.LogCritical("[ROM Check] Failed to retrieve TotK rom.");
            return false;
        }
        
        TkLog.Instance.LogInformation("[ROM Check] Good | {Version}", tkRom.GameVersion);
        reason = null;
        return true;
    }
}