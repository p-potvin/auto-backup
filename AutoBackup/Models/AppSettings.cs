namespace AutoBackup.Models;

/// <summary>
/// Global application settings persisted across sessions.
/// </summary>
public class AppSettings
{
    /// <summary>Minimize to system tray instead of taskbar when closing the window.</summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>Whether to start AutoBackup automatically at Windows login.</summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>Maximum number of lines kept in the in-memory log buffer.</summary>
    public int MaxLogLines { get; set; } = 2000;

    /// <summary>Theme override: null = follow system, "Light", "Dark".</summary>
    public string? Theme { get; set; }
}
