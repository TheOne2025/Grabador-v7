using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Gma.System.MouseKeyHook;
using MacroRecorderPro.Models;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace MacroRecorderPro.Services
{
    public class MacroRecordingService : IDisposable
    {
        private IKeyboardMouseEvents? globalHook;
        private readonly SettingsService settingsService;
        private readonly WinFormsTimer recordingTimer;
        private DateTime recordingStartTime;
        private Point lastMousePosition;
        private DateTime lastActionTime;

        // Estado de grabación
        public bool IsRecording { get; private set; }
        public bool IsPaused { get; private set; }
        public Macro? CurrentMacro { get; private set; }
        public TimeSpan RecordingDuration { get; private set; }
        public int ActionCount => CurrentMacro?.ActionCount ?? 0;

        // Eventos
        public event Action<bool>? RecordingStateChanged;
        public event Action<MacroAction>? ActionRecorded;
        public event Action<TimeSpan>? RecordingTimeUpdated;
        public event Action<Macro>? MacroCompleted;

        public MacroRecordingService(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            
            recordingTimer = new WinFormsTimer();
            recordingTimer.Interval = 100; // Update every 100ms
            recordingTimer.Tick += UpdateRecordingTime;
            
            lastMousePosition = Cursor.Position;
            lastActionTime = DateTime.Now;
        }

        public void StartRecording(string? macroName = null)
        {
            if (IsRecording) return;

            try
            {
                // Crear nueva macro
                CurrentMacro = new Macro
                {
                    Name = macroName ?? $"Macro_{DateTime.Now:yyyyMMdd_HHmmss}",
                    CreatedDate = DateTime.Now
                };

                // Configurar hook global
                globalHook = Hook.GlobalEvents();
                SetupEventHandlers();

                // Iniciar grabación
                IsRecording = true;
                IsPaused = false;
                recordingStartTime = DateTime.Now;
                lastActionTime = DateTime.Now;
                lastMousePosition = Cursor.Position;

                recordingTimer.Start();
                RecordingStateChanged?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar la grabación: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void StopRecording()
        {
            if (!IsRecording) return;

            try
            {
                // Detener timer y hooks
                recordingTimer.Stop();
                globalHook?.Dispose();
                globalHook = null;

                // Finalizar macro
                if (CurrentMacro != null)
                {
                    CurrentMacro.LastModified = DateTime.Now;
                    MacroCompleted?.Invoke(CurrentMacro);
                }

                // Actualizar estado
                IsRecording = false;
                IsPaused = false;
                RecordingDuration = TimeSpan.Zero;

                RecordingStateChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detener la grabación: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PauseRecording()
        {
            if (!IsRecording) return;

            IsPaused = !IsPaused;
            
            if (IsPaused)
            {
                recordingTimer.Stop();
            }
            else
            {
                recordingTimer.Start();
                lastActionTime = DateTime.Now;
            }

            RecordingStateChanged?.Invoke(IsRecording);
        }

        public void ToggleRecording()
        {
            if (IsRecording)
                StopRecording();
            else
                StartRecording();
        }

        public async Task<bool> PlayMacro(Macro macro, int repeatCount = 1, int speedPercentage = 100)
        {
            if (macro == null || macro.Actions.Count == 0) return false;

            try
            {
                for (int repeat = 0; repeat < repeatCount; repeat++)
                {
                    foreach (var action in macro.Actions)
                    {
                        await ExecuteAction(action, speedPercentage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reproducir macro: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SetupEventHandlers()
        {
            if (globalHook == null) return;

            var settings = settingsService.GetRecordingSettings();

            // Mouse events
            if (settings.RecordMouseMovements)
                globalHook.MouseMove += OnMouseMove;
            
            if (settings.RecordMouseClicks)
            {
                globalHook.MouseDown += OnMouseDown;
                globalHook.MouseUp += OnMouseUp;
                globalHook.MouseDoubleClick += OnMouseDoubleClick;
            }

            if (settings.RecordMouseScroll)
                globalHook.MouseWheel += OnMouseWheel;

            // Keyboard events
            if (settings.RecordKeyboard)
            {
                globalHook.KeyDown += OnKeyDown;
                globalHook.KeyUp += OnKeyUp;
            }
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!IsRecording || IsPaused) return;

            var settings = settingsService.GetRecordingSettings();
            var currentPos = new Point(e.X, e.Y);
            
            // Check movement precision
            int distance = Math.Abs(currentPos.X - lastMousePosition.X) + 
                          Math.Abs(currentPos.Y - lastMousePosition.Y);
            
            if (distance >= settings.MouseMovementPrecision)
            {
                RecordAction(new MacroAction
                {
                    ActionType = MacroActionType.MouseMove,
                    Position = currentPos,
                    Timestamp = DateTime.Now
                });

                lastMousePosition = currentPos;
            }
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (!IsRecording || IsPaused) return;

            RecordAction(new MacroAction
            {
                ActionType = MacroActionType.MouseClick,
                Position = new Point(e.X, e.Y),
                MouseButton = ConvertMouseButton(e.Button),
                Timestamp = DateTime.Now
            });
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (!IsRecording || IsPaused) return;

            // Optionally record mouse up events for precise timing
        }

        private void OnMouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (!IsRecording || IsPaused) return;

            RecordAction(new MacroAction
            {
                ActionType = MacroActionType.MouseDoubleClick,
                Position = new Point(e.X, e.Y),
                MouseButton = ConvertMouseButton(e.Button),
                Timestamp = DateTime.Now
            });
        }

        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            if (!IsRecording || IsPaused) return;

            RecordAction(new MacroAction
            {
                ActionType = MacroActionType.MouseScroll,
                Position = new Point(e.X, e.Y),
                ScrollDelta = e.Delta,
                Timestamp = DateTime.Now
            });
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (!IsRecording || IsPaused) return;

            var settings = settingsService.GetRecordingSettings();
            
            // Ignore system keys if configured
            if (settings.IgnoreSystemKeys && IsSystemKey(e.KeyCode))
                return;

            RecordAction(new MacroAction
            {
                ActionType = MacroActionType.KeyDown,
                KeyCode = e.KeyCode.ToString(),
                Timestamp = DateTime.Now
            });
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (!IsRecording || IsPaused) return;

            var settings = settingsService.GetRecordingSettings();
            
            if (settings.IgnoreSystemKeys && IsSystemKey(e.KeyCode))
                return;

            RecordAction(new MacroAction
            {
                ActionType = MacroActionType.KeyUp,
                KeyCode = e.KeyCode.ToString(),
                Timestamp = DateTime.Now
            });
        }

        private void RecordAction(MacroAction action)
        {
            if (CurrentMacro == null) return;

            // Calculate delay from last action
            var timeSinceLastAction = (action.Timestamp - lastActionTime).TotalMilliseconds;
            var settings = settingsService.GetRecordingSettings();
            
            if (timeSinceLastAction >= settings.MinDelayBetweenActions && 
                timeSinceLastAction <= settings.MaxDelayBetweenActions)
            {
                action.DelayAfter = (int)timeSinceLastAction;
            }

            CurrentMacro.AddAction(action);
            lastActionTime = action.Timestamp;

            ActionRecorded?.Invoke(action);
        }

        private async Task ExecuteAction(MacroAction action, int speedPercentage)
        {
            // Apply speed adjustment to delays
            int adjustedDelay = action.DelayAfter * 100 / speedPercentage;
            
            switch (action.ActionType)
            {
                case MacroActionType.MouseMove:
                    Cursor.Position = action.Position;
                    break;

                case MacroActionType.MouseClick:
                    await SimulateMouseClick(action.Position, action.MouseButton);
                    break;

                case MacroActionType.MouseDoubleClick:
                    await SimulateMouseDoubleClick(action.Position, action.MouseButton);
                    break;

                case MacroActionType.MouseScroll:
                    await SimulateMouseScroll(action.Position, action.ScrollDelta);
                    break;

                case MacroActionType.KeyDown:
                case MacroActionType.KeyPress:
                    await SimulateKeyPress(action.KeyCode);
                    break;

                case MacroActionType.TypeText:
                    await SimulateTypeText(action.Text);
                    break;

                case MacroActionType.Wait:
                    await Task.Delay(adjustedDelay);
                    break;
            }

            if (adjustedDelay > 0)
                await Task.Delay(adjustedDelay);
        }

        private async Task SimulateMouseClick(Point position, MouseButton button)
        {
            // TODO: Implement mouse click simulation
            // This would typically use Windows API calls
            await Task.Delay(1);
        }

        private async Task SimulateMouseDoubleClick(Point position, MouseButton button)
        {
            await SimulateMouseClick(position, button);
            await Task.Delay(50);
            await SimulateMouseClick(position, button);
        }

        private async Task SimulateMouseScroll(Point position, int delta)
        {
            // TODO: Implement mouse scroll simulation
            await Task.Delay(1);
        }

        private async Task SimulateKeyPress(string keyCode)
        {
            // TODO: Implement key press simulation
            await Task.Delay(1);
        }

        private async Task SimulateTypeText(string text)
        {
            // TODO: Implement text typing simulation
            await Task.Delay(1);
        }

        private MouseButton ConvertMouseButton(System.Windows.Forms.MouseButtons button)
        {
            return button switch
            {
                System.Windows.Forms.MouseButtons.Left => MouseButton.Left,
                System.Windows.Forms.MouseButtons.Right => MouseButton.Right,
                System.Windows.Forms.MouseButtons.Middle => MouseButton.Middle,
                System.Windows.Forms.MouseButtons.XButton1 => MouseButton.XButton1,
                System.Windows.Forms.MouseButtons.XButton2 => MouseButton.XButton2,
                _ => MouseButton.Left
            };
        }

        private bool IsSystemKey(Keys keyCode)
        {
            return keyCode switch
            {
                Keys.LWin or Keys.RWin or Keys.Apps or Keys.PrintScreen or 
                Keys.Alt or Keys.Tab or Keys.Escape => true,
                _ => false
            };
        }

        private void UpdateRecordingTime(object? sender, EventArgs e)
        {
            if (IsRecording && !IsPaused)
            {
                RecordingDuration = DateTime.Now - recordingStartTime;
                RecordingTimeUpdated?.Invoke(RecordingDuration);
            }
        }

        public void Dispose()
        {
            recordingTimer?.Dispose();
            globalHook?.Dispose();
        }
    }
}
