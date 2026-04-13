using System.Text.Json;
using AutoBackup.Models;

namespace AutoBackup.Services;

/// <summary>
/// Persists <see cref="BackupJob"/> configurations and <see cref="AppSettings"/>
/// as JSON files in the user's local app-data directory.
/// </summary>
public sealed class SettingsService
{
    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VaultWares", "AutoBackup");

    private static readonly string JobsFile = Path.Combine(DataDir, "jobs.json");
    private static readonly string SettingsFile = Path.Combine(DataDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static string LogFilePath => Path.Combine(DataDir, "logs", "autobackup.log");

    // -------------------------------------------------------------------------

    public List<BackupJob> LoadJobs()
    {
        try
        {
            if (!File.Exists(JobsFile))
                return [];

            var json = File.ReadAllText(JobsFile);
            if (string.IsNullOrWhiteSpace(json)) return [];
            
            return JsonSerializer.Deserialize<List<BackupJob>>(json, JsonOpts) ?? [];
        }
        catch (JsonException)
        {
            // If the file is corrupt, we return an empty list.
            return [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    public void SaveJobs(IEnumerable<BackupJob> jobs)
    {
        Directory.CreateDirectory(DataDir);
        var json = JsonSerializer.Serialize(jobs, JsonOpts);
        File.WriteAllText(JobsFile, json);
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFile))
                return new AppSettings();

            var json = File.ReadAllText(SettingsFile);
            if (string.IsNullOrWhiteSpace(json)) return new AppSettings();

            return JsonSerializer.Deserialize<AppSettings>(json, JsonOpts) ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        Directory.CreateDirectory(DataDir);
        var json = JsonSerializer.Serialize(settings, JsonOpts);
        File.WriteAllText(SettingsFile, json);
    }
}
