using Tkmm.Core.Helpers;

namespace Tkmm.Core.Services;

public static class NxServices
{
#if TARGET_NX
    private const string CONFIG_FOLDER = "/storage/.cache/services";
    private const string SSH_CONFIG_PATH = "/storage/.cache/services/sshd.conf";
    private const string SMB_DISABLED_PATH = "/storage/.cache/services/samba.disabled";
#else
    private const string CONFIG_FOLDER = "/home/archleaders/services";
    private const string SSH_CONFIG_PATH = "/home/archleaders/services/sshd.conf";
    private const string SMB_DISABLED_PATH = "/home/archleaders/services/samba.disabled";
#endif
    private const string RESTART_SSH_COMMAND = "systemctl restart sshd";
    private const string RESTART_SMB_COMMAND = "systemctl restart smbd";

    public static void EnableSsh()
    {
        if (!IsSshEnabled()) {
            Directory.CreateDirectory(CONFIG_FOLDER);
            File.WriteAllBytes(SSH_CONFIG_PATH, []);
            RestartSsh();
        }
    }

    public static void DisableSsh()
    {
        if (File.Exists(SSH_CONFIG_PATH)) {
            File.Delete(SSH_CONFIG_PATH);
        }

        RestartSsh();
    }

    public static bool IsSshEnabled()
    {
        return File.Exists(SSH_CONFIG_PATH);
    }

    public static void RestartSsh()
    {
        NxProcessHelper.Exec(RESTART_SSH_COMMAND);
    }

    public static void EnableSmb()
    {
        if (IsSmbEnabled()) {
            return;
        }
        
        if (File.Exists(SMB_DISABLED_PATH)) {
            File.Delete(SMB_DISABLED_PATH);
        }

        RestartSmb();
    }

    public static void DisableSmb()
    {
        Directory.CreateDirectory(CONFIG_FOLDER);
        File.WriteAllBytes(SMB_DISABLED_PATH, []);
        RestartSmb();
    }

    public static bool IsSmbEnabled()
    {
        return !File.Exists(SMB_DISABLED_PATH);
    }

    public static void RestartSmb()
    {
        NxProcessHelper.Exec(RESTART_SMB_COMMAND);
    }
}