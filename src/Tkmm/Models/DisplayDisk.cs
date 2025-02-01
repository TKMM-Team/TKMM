namespace Tkmm.Models;

public class DisplayDisk(DriveInfo driveInfo)
{
    public DriveInfo Drive { get; } = driveInfo;

    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    public string DisplayName { get; }
        = $"{driveInfo.VolumeLabel ?? driveInfo.DriveType.ToString()} ({driveInfo.Name})";

    public override string ToString()
    {
        return DisplayName;
    }
}