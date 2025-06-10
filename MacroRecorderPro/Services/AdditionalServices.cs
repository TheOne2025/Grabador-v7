using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using MacroRecorderPro.Models;

namespace MacroRecorderPro.Services
{
    // Servicio de configuración
    public class SettingsService
    {
        private readonly string settingsPath;
        private AppSettings currentSettings;

        public SettingsService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MacroRecorderPro");
            Directory.CreateDirectory(appDataPath);
            settingsPath = Path.Combine(appDataPath, "settings.json");
            
            LoadSettings();
        }

        public AppSettings GetSettings() => currentSettings;
        public RecordingSettings GetRecordingSettings() => currentSettings.Recording;
        public PlaybackSettings GetPlaybackSettings() => currentSettings.Playback;
        public HotkeyConfiguration GetHotkeySettings() => currentSettings.Hotkeys;

        public void SaveSettings(AppSettings settings)
        {
            currentSettings = settings;
            SaveToFile();
        }

        public void UpdateRecordingSettings(RecordingSettings settings)
        {
            currentSettings.Recording = settings;
            SaveToFile();
        }

        public void UpdatePlaybackSettings(PlaybackSettings settings)
        {
            currentSettings.Playback = settings;
            SaveToFile();
        }

        public void UpdateHotkeySettings(HotkeyConfiguration settings)
        {
            currentSettings.Hotkeys = settings;
            SaveToFile();
        }

        public void SaveLicense(string licenseKey)
        {
            currentSettings.LicenseKey = licenseKey;
            currentSettings.IsProVersion = ValidateLicense(licenseKey);
            SaveToFile();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    currentSettings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    currentSettings = new AppSettings();
                }
                
                currentSettings.SetDefaultMacroDirectory();
            }
            catch
            {
                currentSettings = new AppSettings();
                currentSettings.SetDefaultMacroDirectory();
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonConvert.SerializeObject(currentSettings, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar configuración: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateLicense(string license)
        {
            // Implementar validación de licencia
            return !string.IsNullOrWhiteSpace(license) && license.Length >= 10;
        }
    }

    // Servicio de hotkeys globales
    public class HotkeyService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Dictionary<string, int> registeredHotkeys;
        private readonly Dictionary<int, Action> hotkeyActions;
        private int nextHotkeyId = 1;

        public HotkeyService()
        {
            registeredHotkeys = new Dictionary<string, int>();
            hotkeyActions = new Dictionary<int, Action>();
        }

        public bool RegisterHotkey(string hotkey, Action action)
        {
            try
            {
                if (registeredHotkeys.ContainsKey(hotkey))
                {
                    UnregisterHotkey(hotkey);
                }

                var (modifiers, keyCode) = ParseHotkey(hotkey);
                int id = nextHotkeyId++;

                // Para este ejemplo, no registraremos hotkeys reales del sistema
                // En una implementación completa, usarías RegisterHotKey de Windows API
                registeredHotkeys[hotkey] = id;
                hotkeyActions[id] = action;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UnregisterHotkey(string hotkey)
        {
            if (registeredHotkeys.TryGetValue(hotkey, out int id))
            {
                // UnregisterHotKey(IntPtr.Zero, id);
                registeredHotkeys.Remove(hotkey);
                hotkeyActions.Remove(id);
            }
        }

        private (int modifiers, int keyCode) ParseHotkey(string hotkey)
        {
            int modifiers = 0;
            string key = hotkey;

            if (hotkey.Contains("Ctrl+"))
            {
                modifiers |= 0x0002; // MOD_CONTROL
                key = hotkey.Replace("Ctrl+", "");
            }
            if (hotkey.Contains("Alt+"))
            {
                modifiers |= 0x0001; // MOD_ALT
                key = key.Replace("Alt+", "");
            }
            if (hotkey.Contains("Shift+"))
            {
                modifiers |= 0x0004; // MOD_SHIFT
                key = key.Replace("Shift+", "");
            }

            // Convertir tecla a código virtual
            int keyCode = key switch
            {
                "F9" => 0x78,
                "F10" => 0x79,
                "F11" => 0x7A,
                "F12" => 0x7B,
                _ => 0
            };

            return (modifiers, keyCode);
        }

        public void Dispose()
        {
            foreach (var hotkey in registeredHotkeys.Keys.ToArray())
            {
                UnregisterHotkey(hotkey);
            }
        }
    }

    // Servicio de gestión de archivos
    public class FileManagerService
    {
        private readonly SettingsService settingsService;
        private readonly string macroDirectory;

        public FileManagerService(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.macroDirectory = settingsService.GetSettings().MacroSaveDirectory;
            Directory.CreateDirectory(macroDirectory);
        }

        public async Task<bool> SaveMacro(Macro macro)
        {
            try
            {
                var fileName = $"{macro.Name}_{macro.Id}.macro";
                var filePath = Path.Combine(macroDirectory, fileName);
                var json = JsonConvert.SerializeObject(macro, Formatting.Indented);
                
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar macro: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task<Macro?> LoadMacro(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonConvert.DeserializeObject<Macro>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar macro: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public List<string> GetMacroFiles()
        {
            try
            {
                return Directory.GetFiles(macroDirectory, "*.macro").ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<Macro>> LoadAllMacros()
        {
            var macros = new List<Macro>();
            var files = GetMacroFiles();

            foreach (var file in files)
            {
                var macro = await LoadMacro(file);
                if (macro != null)
                {
                    macros.Add(macro);
                }
            }

            return macros;
        }

        public bool DeleteMacro(string macroId)
        {
            try
            {
                var files = Directory.GetFiles(macroDirectory, $"*_{macroId}.macro");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar macro: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task<bool> ExportMacro(Macro macro, string exportPath, string format = "json")
        {
            try
            {
                string content = format.ToLower() switch
                {
                    "json" => JsonConvert.SerializeObject(macro, Formatting.Indented),
                    "xml" => ConvertToXml(macro),
                    "ahk" => ConvertToAutoHotkey(macro),
                    _ => JsonConvert.SerializeObject(macro, Formatting.Indented)
                };

                await File.WriteAllTextAsync(exportPath, content);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar macro: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task<bool> ImportMacro(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLower();
                Macro? macro = null;

                switch (extension)
                {
                    case ".macro":
                    case ".json":
                        var json = await File.ReadAllTextAsync(filePath);
                        macro = JsonConvert.DeserializeObject<Macro>(json);
                        break;
                    case ".xml":
                        macro = ImportFromXml(filePath);
                        break;
                    default:
                        throw new NotSupportedException($"Formato de archivo no soportado: {extension}");
                }

                if (macro != null)
                {
                    // Generar nuevo ID para evitar conflictos
                    macro.Id = Guid.NewGuid().ToString();
                    macro.Name += "_imported";
                    return await SaveMacro(macro);
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar macro: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public async Task<bool> CreateBackup()
        {
            try
            {
                var backupDir = Path.Combine(macroDirectory, "Backups");
                Directory.CreateDirectory(backupDir);

                var backupFile = Path.Combine(backupDir, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                
                // Aquí implementarías la compresión real
                // Por simplicidad, solo copiamos los archivos
                var macroFiles = GetMacroFiles();
                foreach (var file in macroFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var backupPath = Path.Combine(backupDir, fileName);
                    File.Copy(file, backupPath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear backup: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string ConvertToXml(Macro macro)
        {
            // Implementación básica de conversión a XML
            return $"<Macro><Name>{macro.Name}</Name><Actions>{macro.Actions.Count}</Actions></Macro>";
        }

        private string ConvertToAutoHotkey(Macro macro)
        {
            // Implementación básica de conversión a AutoHotkey
            var ahk = $"; Macro: {macro.Name}\n";
            ahk += $"; Created: {macro.CreatedDate}\n\n";

            foreach (var action in macro.Actions)
            {
                ahk += action.ActionType switch
                {
                    MacroActionType.MouseClick => $"Click {action.Position.X}, {action.Position.Y}\n",
                    MacroActionType.KeyPress => $"Send, {action.KeyCode}\n",
                    MacroActionType.TypeText => $"Send, {action.Text}\n",
                    _ => $"; {action}\n"
                };
            }

            return ahk;
        }

        private Macro? ImportFromXml(string filePath)
        {
            // Implementación básica de importación desde XML
            return new Macro { Name = "Imported from XML" };
        }
    }

    // Servicio de estadísticas
    public class StatisticsService
    {
        private readonly SettingsService settingsService;
        private readonly string statisticsPath;
        private UsageStatistics currentStats;

        public StatisticsService(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MacroRecorderPro");
            statisticsPath = Path.Combine(appDataPath, "statistics.json");
            
            LoadStatistics();
        }

        public UsageStatistics GetStatistics() => currentStats;

        public void RecordMacroCreated(Macro macro)
        {
            currentStats.TotalMacrosCreated++;
            currentStats.TotalActionsRecorded += macro.ActionCount;
            currentStats.TotalRecordingTime = currentStats.TotalRecordingTime.Add(macro.Duration);
            currentStats.RecordDailyUsage();
            SaveStatistics();
        }

        public void RecordMacroPlayed(string macroName, TimeSpan duration)
        {
            currentStats.RecordMacroPlay(macroName);
            currentStats.TotalTimeSaved = currentStats.TotalTimeSaved.Add(duration);
            currentStats.RecordDailyUsage();
            SaveStatistics();
        }

        public void RecordAction(MacroActionType actionType)
        {
            currentStats.RecordAction(actionType);
            SaveStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                if (File.Exists(statisticsPath))
                {
                    var json = File.ReadAllText(statisticsPath);
                    currentStats = JsonConvert.DeserializeObject<UsageStatistics>(json) ?? new UsageStatistics();
                }
                else
                {
                    currentStats = new UsageStatistics();
                }
            }
            catch
            {
                currentStats = new UsageStatistics();
            }
        }

        private void SaveStatistics()
        {
            try
            {
                var json = JsonConvert.SerializeObject(currentStats, Formatting.Indented);
                File.WriteAllText(statisticsPath, json);
            }
            catch
            {
                // Silently fail - statistics are not critical
            }
        }
    }
}
