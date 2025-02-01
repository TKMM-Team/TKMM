namespace Tkmm.Core.Logging;

public static class LogInfo
{
    public const string GENERIC_USERNAME = "%username%";
    
    private static readonly string _userName = Path.GetFileName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    public static string HideUsername(this string content)
    {
        return content.Replace(_userName, GENERIC_USERNAME);
    }
}