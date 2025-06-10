using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace MacroRecorderPro.Models
{
    // Tipos de acciones de macro
    public enum MacroActionType
    {
        MouseMove,
        MouseClick,
        MouseDoubleClick,
        MouseRightClick,
        MouseScroll,
        KeyPress,
        KeyDown,
        KeyUp,
        TypeText,
        Wait
    }

    // Botones del ratón
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }

    // Acción individual de macro
    public class MacroAction
    {
        public int Id { get; set; }
        public MacroActionType ActionType { get; set; }
        public DateTime Timestamp { get; set; }
        public Point Position { get; set; }
        public MouseButton MouseButton { get; set; }
        public string KeyCode { get; set; } = "";
        public string Text { get; set; } = "";
        public int ScrollDelta { get; set; }
        public int DelayAfter { get; set; } // Retraso después de la acción en ms

        public override string ToString()
        {
            return ActionType switch
            {
                MacroActionType.MouseMove => $"Mouse move to ({Position.X}, {Position.Y})",
                MacroActionType.MouseClick => $"{MouseButton} click at ({Position.X}, {Position.Y})",
                MacroActionType.MouseDoubleClick => $"{MouseButton} double-click at ({Position.X}, {Position.Y})",
                MacroActionType.MouseRightClick => $"Right click at ({Position.X}, {Position.Y})",
                MacroActionType.MouseScroll => $"Mouse scroll {ScrollDelta} at ({Position.X}, {Position.Y})",
                MacroActionType.KeyPress => $"Key press: {KeyCode}",
                MacroActionType.KeyDown => $"Key down: {KeyCode}",
                MacroActionType.KeyUp => $"Key up: {KeyCode}",
                MacroActionType.TypeText => $"Type: \"{Text}\"",
                MacroActionType.Wait => $"Wait {DelayAfter}ms",
                _ => "Unknown action"
            };
        }
    }

    // Macro completa
    public class Macro
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public List<MacroAction> Actions { get; set; } = new List<MacroAction>();
        public int RepeatCount { get; set; } = 1;
        public bool RepeatInfinite { get; set; } = false;
        public int PlaybackSpeed { get; set; } = 100; // Porcentaje de velocidad
        public string HotkeyToPlay { get; set; } = "";
        public string Category { get; set; } = "General";
        public Color CategoryColor { get; set; } = Color.Gray;

        [JsonIgnore]
        public int ActionCount => Actions.Count;

        [JsonIgnore]
        public TimeSpan Duration
        {
            get
            {
                if (Actions.Count == 0) return TimeSpan.Zero;
                var first = Actions[0].Timestamp;
                var last = Actions[Actions.Count - 1].Timestamp;
                return last - first;
            }
        }

        [JsonIgnore]
        public string DurationString => Duration.ToString(@"mm\:ss");

        public void AddAction(MacroAction action)
        {
            action.Id = Actions.Count;
            Actions.Add(action);
            LastModified = DateTime.Now;
        }

        public void RemoveAction(int index)
        {
            if (index >= 0 && index < Actions.Count)
            {
                Actions.RemoveAt(index);
                // Reindexar las acciones
                for (int i = 0; i < Actions.Count; i++)
                {
                    Actions[i].Id = i;
                }
                LastModified = DateTime.Now;
            }
        }

        public void ClearActions()
        {
            Actions.Clear();
            LastModified = DateTime.Now;
        }
    }

    // Configuración de grabación
    public class RecordingSettings
    {
        public bool RecordMouseMovements { get; set; } = true;
        public bool RecordMouseClicks { get; set; } = true;
        public bool RecordKeyboard { get; set; } = true;
        public bool RecordMouseScroll { get; set; } = false;
        public int MouseMovementPrecision { get; set; } = 2; // Pixels entre movimientos
        public int MinDelayBetweenActions { get; set; } = 10; // ms
        public int MaxDelayBetweenActions { get; set; } = 5000; // ms
        public bool OptimizeMousePaths { get; set; } = true;
        public bool IgnoreSystemKeys { get; set; } = true;
        public bool RecordWindowChanges { get; set; } = false;
    }

    // Configuración de reproducción
    public class PlaybackSettings
    {
        public int DefaultSpeed { get; set; } = 100; // Porcentaje
        public bool ConfirmBeforePlay { get; set; } = true;
        public bool ShowPlaybackProgress { get; set; } = true;
        public bool PlaySounds { get; set; } = true;
        public bool AllowInterruption { get; set; } = true;
        public int MaxRepeats { get; set; } = 1000;
    }

    // Estadísticas de uso
    public class UsageStatistics
    {
        public int TotalMacrosCreated { get; set; } = 0;
        public int TotalActionsRecorded { get; set; } = 0;
        public TimeSpan TotalRecordingTime { get; set; } = TimeSpan.Zero;
        public int TotalMacrosPlayed { get; set; } = 0;
        public TimeSpan TotalTimeSaved { get; set; } = TimeSpan.Zero;
        public DateTime FirstUse { get; set; } = DateTime.Now;
        public DateTime LastUse { get; set; } = DateTime.Now;
        public Dictionary<string, int> ActionTypeCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MacroUsageCount { get; set; } = new Dictionary<string, int>();
        public List<DateTime> DailyUsage { get; set; } = new List<DateTime>();

        public void RecordAction(MacroActionType actionType)
        {
            string key = actionType.ToString();
            if (ActionTypeCount.ContainsKey(key))
                ActionTypeCount[key]++;
            else
                ActionTypeCount[key] = 1;

            TotalActionsRecorded++;
            LastUse = DateTime.Now;
        }

        public void RecordMacroPlay(string macroName)
        {
            if (MacroUsageCount.ContainsKey(macroName))
                MacroUsageCount[macroName]++;
            else
                MacroUsageCount[macroName] = 1;

            TotalMacrosPlayed++;
            LastUse = DateTime.Now;
        }

        public void RecordDailyUsage()
        {
            var today = DateTime.Today;
            if (!DailyUsage.Contains(today))
            {
                DailyUsage.Add(today);
            }
        }

        public double GetEfficiencyPercentage()
        {
            if (TotalActionsRecorded == 0) return 0;
            return Math.Min(100, (TotalMacrosPlayed * 10.0 / TotalActionsRecorded) * 100);
        }

        public int GetCurrentStreak()
        {
            if (DailyUsage.Count == 0) return 0;

            DailyUsage.Sort();
            int streak = 1;
            for (int i = DailyUsage.Count - 2; i >= 0; i--)
            {
                if ((DailyUsage[i + 1] - DailyUsage[i]).TotalDays == 1)
                    streak++;
                else
                    break;
            }
            return streak;
        }

        public int GetBestStreak()
        {
            if (DailyUsage.Count == 0) return 0;

            DailyUsage.Sort();
            int bestStreak = 1;
            int currentStreak = 1;

            for (int i = 1; i < DailyUsage.Count; i++)
            {
                if ((DailyUsage[i] - DailyUsage[i - 1]).TotalDays == 1)
                {
                    currentStreak++;
                    bestStreak = Math.Max(bestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }
            return bestStreak;
        }
    }

    // Configuración de hotkeys
    public class HotkeyConfiguration
    {
        public string StartStopRecording { get; set; } = "F9";
        public string PauseResumeRecording { get; set; } = "F10";
        public string PlayLastMacro { get; set; } = "F11";
        public string StopPlayback { get; set; } = "F12";
        public string QuickScreenshot { get; set; } = "PrintScreen";
        public Dictionary<string, string> CustomMacroHotkeys { get; set; } = new Dictionary<string, string>();

        public bool IsValidHotkey(string hotkey)
        {
            if (string.IsNullOrWhiteSpace(hotkey)) return false;
            
            // Lista de teclas válidas
            var validKeys = new[]
            {
                "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
                "Ctrl+A", "Ctrl+B", "Ctrl+C", "Ctrl+D", "Ctrl+E", "Ctrl+F", "Ctrl+G", "Ctrl+H",
                "Ctrl+I", "Ctrl+J", "Ctrl+K", "Ctrl+L", "Ctrl+M", "Ctrl+N", "Ctrl+O", "Ctrl+P",
                "Ctrl+Q", "Ctrl+R", "Ctrl+S", "Ctrl+T", "Ctrl+U", "Ctrl+V", "Ctrl+W", "Ctrl+X",
                "Ctrl+Y", "Ctrl+Z", "Alt+F1", "Alt+F2", "Alt+F3", "Alt+F4", "Shift+F1", "Shift+F2"
            };

            return Array.Exists(validKeys, k => k.Equals(hotkey, StringComparison.OrdinalIgnoreCase));
        }
    }

    // Configuración general de la aplicación
    public class AppSettings
    {
        public RecordingSettings Recording { get; set; } = new RecordingSettings();
        public PlaybackSettings Playback { get; set; } = new PlaybackSettings();
        public HotkeyConfiguration Hotkeys { get; set; } = new HotkeyConfiguration();
        public string Theme { get; set; } = "Dark";
        public string Language { get; set; } = "Spanish";
        public bool StartWithWindows { get; set; } = false;
        public bool MinimizeToSystemTray { get; set; } = false;
        public bool ShowNotifications { get; set; } = true;
        public bool PlaySounds { get; set; } = true;
        public string MacroSaveDirectory { get; set; } = "";
        public bool AutoBackup { get; set; } = true;
        public int BackupRetentionDays { get; set; } = 30;
        public bool EncryptMacros { get; set; } = false;
        public string LicenseKey { get; set; } = "";
        public bool IsProVersion { get; set; } = false;
        public int MemoryLimit { get; set; } = 512; // MB

        public void SetDefaultMacroDirectory()
        {
            if (string.IsNullOrEmpty(MacroSaveDirectory))
            {
                MacroSaveDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MacroRecorder");
            }
        }
    }

    // Información de licencia
    public class LicenseInfo
    {
        public string Key { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string LicenseType { get; set; } = "Trial"; // Trial, Personal, Professional, Enterprise
        public bool IsValid => DateTime.Now <= ExpiryDate && !string.IsNullOrEmpty(Key);
        public int DaysRemaining => Math.Max(0, (ExpiryDate - DateTime.Now).Days);
    }
}
