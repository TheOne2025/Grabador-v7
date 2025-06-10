using System;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using MacroRecorderPro.Models;
using MacroRecorderPro.Services;

namespace MacroRecorderPro.Controls
{
    public partial class FilesPanel : UserControl
    {
        private readonly MacroRecordingService recordingService;
        
        // Controles principales
        private GradientIconButton recordButton;
        private GradientIconButton pauseButton;
        private GradientIconButton stopButton;
        private GradientIconButton playButton;
        private Label timeDisplayLabel;
        private Label actionCountLabel;
        private RichTextBox actionLogTextBox;
        private ModernProgressBar recordingProgressBar;
        
        // Configuración rápida
        private CheckBox recordMouseCheckBox;
        private CheckBox recordClicksCheckBox;
        private CheckBox recordKeyboardCheckBox;
        private CheckBox recordScrollCheckBox;

        public FilesPanel(MacroRecordingService recordingService)
        {
            this.recordingService = recordingService;
            InitializeComponent();
            SetupEventHandlers();
            UpdateControlStates();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(50, 50, 50);
            this.AutoScroll = true;

            CreateRecordingControls();
            CreateActionLog();
            CreateQuickSettings();
        }

        private void CreateRecordingControls()
        {
            // Sección de controles de grabación
            var controlsSection = new GradientPanel
            {
                Size = new Size(this.Width - 60, 200),
                Location = new Point(30, 30),
                GradientStart = Color.FromArgb(64, 64, 64),
                GradientEnd = Color.FromArgb(56, 56, 56),
                CornerRadius = 12,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var titleLabel = new Label
            {
                Text = "🎯 Controles de Grabación",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            controlsSection.Controls.Add(titleLabel);

            // Botones de control
            recordButton = new GradientIconButton
            {
                Text = "",
                IconChar = IconChar.Circle,
                IconColor = Color.White,
                IconSize = 32,
                Size = new Size(80, 80),
                Location = new Point(30, 60),
                GradientStart = Color.FromArgb(220, 50, 50),
                GradientEnd = Color.FromArgb(180, 30, 30),
                CornerRadius = 40,
                FlatStyle = FlatStyle.Flat
            };
            recordButton.FlatAppearance.BorderSize = 0;
            recordButton.Click += RecordButton_Click;
            controlsSection.Controls.Add(recordButton);

            pauseButton = new GradientIconButton
            {
                Text = "",
                IconChar = IconChar.Pause,
                IconColor = Color.White,
                IconSize = 20,
                Size = new Size(50, 50),
                Location = new Point(130, 75),
                GradientStart = Color.FromArgb(100, 100, 100),
                GradientEnd = Color.FromArgb(80, 80, 80),
                CornerRadius = 8,
                Enabled = false
            };
            pauseButton.Click += PauseButton_Click;
            controlsSection.Controls.Add(pauseButton);

            stopButton = new GradientIconButton
            {
                Text = "",
                IconChar = IconChar.Stop,
                IconColor = Color.White,
                IconSize = 20,
                Size = new Size(50, 50),
                Location = new Point(190, 75),
                GradientStart = Color.FromArgb(100, 100, 100),
                GradientEnd = Color.FromArgb(80, 80, 80),
                CornerRadius = 8,
                Enabled = false
            };
            stopButton.Click += StopButton_Click;
            controlsSection.Controls.Add(stopButton);

            playButton = new GradientIconButton
            {
                Text = "",
                IconChar = IconChar.Play,
                IconColor = Color.White,
                IconSize = 20,
                Size = new Size(50, 50),
                Location = new Point(250, 75),
                GradientStart = Color.FromArgb(76, 175, 80),
                GradientEnd = Color.FromArgb(56, 142, 60),
                CornerRadius = 8
            };
            playButton.Click += PlayButton_Click;
            controlsSection.Controls.Add(playButton);

            // Información de grabación
            timeDisplayLabel = new Label
            {
                Text = "00:00:00",
                ForeColor = Color.White,
                Font = new Font("Courier New", 24, FontStyle.Bold),
                Location = new Point(350, 70),
                AutoSize = true
            };
            controlsSection.Controls.Add(timeDisplayLabel);

            actionCountLabel = new Label
            {
                Text = "Acciones: 0",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Location = new Point(350, 110),
                AutoSize = true
            };
            controlsSection.Controls.Add(actionCountLabel);

            // Barra de progreso
            recordingProgressBar = new ModernProgressBar
            {
                Size = new Size(controlsSection.Width - 60, 8),
                Location = new Point(30, 160),
                ProgressColor = Color.FromArgb(255, 80, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            controlsSection.Controls.Add(recordingProgressBar);

            this.Controls.Add(controlsSection);
        }

        private void CreateActionLog()
        {
            // Sección de registro de acciones
            var logSection = new GradientPanel
            {
                Size = new Size(this.Width - 60, 250),
                Location = new Point(30, 250),
                GradientStart = Color.FromArgb(64, 64, 64),
                GradientEnd = Color.FromArgb(56, 56, 56),
                CornerRadius = 12,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var logTitleLabel = new Label
            {
                Text = "👁️ Registro en Tiempo Real",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            logSection.Controls.Add(logTitleLabel);

            actionLogTextBox = new RichTextBox
            {
                Size = new Size(logSection.Width - 40, logSection.Height - 60),
                Location = new Point(20, 45),
                BackColor = Color.FromArgb(26, 26, 26),
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 10, FontStyle.Regular),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            actionLogTextBox.Text = "Esperando acciones...\n";
            logSection.Controls.Add(actionLogTextBox);

            this.Controls.Add(logSection);
        }

        private void CreateQuickSettings()
        {
            // Sección de configuración rápida
            var settingsSection = new GradientPanel
            {
                Size = new Size(this.Width - 60, 120),
                Location = new Point(30, 520),
                GradientStart = Color.FromArgb(64, 64, 64),
                GradientEnd = Color.FromArgb(56, 56, 56),
                CornerRadius = 12,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var settingsTitleLabel = new Label
            {
                Text = "⚙️ Configuración de Grabación",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            settingsSection.Controls.Add(settingsTitleLabel);

            // Checkboxes de configuración
            recordMouseCheckBox = new CheckBox
            {
                Text = "Grabar movimientos del ratón",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(30, 50),
                AutoSize = true,
                Checked = true
            };
            settingsSection.Controls.Add(recordMouseCheckBox);

            recordClicksCheckBox = new CheckBox
            {
                Text = "Grabar clics del ratón",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(250, 50),
                AutoSize = true,
                Checked = true
            };
            settingsSection.Controls.Add(recordClicksCheckBox);

            recordKeyboardCheckBox = new CheckBox
            {
                Text = "Grabar pulsaciones de teclado",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(30, 80),
                AutoSize = true,
                Checked = true
            };
            settingsSection.Controls.Add(recordKeyboardCheckBox);

            recordScrollCheckBox = new CheckBox
            {
                Text = "Grabar desplazamiento de rueda",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(250, 80),
                AutoSize = true,
                Checked = false
            };
            settingsSection.Controls.Add(recordScrollCheckBox);

            this.Controls.Add(settingsSection);
        }

        private void SetupEventHandlers()
        {
            recordingService.RecordingStateChanged += OnRecordingStateChanged;
            recordingService.ActionRecorded += OnActionRecorded;
            recordingService.RecordingTimeUpdated += OnRecordingTimeUpdated;
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            if (!recordingService.IsRecording)
            {
                // Solicitar nombre del macro
                using (var dialog = new MacroNameDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        recordingService.StartRecording(dialog.MacroName);
                    }
                }
            }
            else
            {
                recordingService.StopRecording();
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            recordingService.PauseRecording();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            recordingService.StopRecording();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (recordingService.CurrentMacro != null && recordingService.CurrentMacro.Actions.Count > 0)
            {
                var result = MessageBox.Show("¿Reproducir el macro grabado?", "Confirmar", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    _ = recordingService.PlayMacro(recordingService.CurrentMacro);
                }
            }
            else
            {
                MessageBox.Show("No hay macro grabado para reproducir.", "Información", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnRecordingStateChanged(bool isRecording)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                UpdateControlStates();
            }));
        }

        private void OnActionRecorded(MacroAction action)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                AddActionToLog(action);
                UpdateActionCount();
            }));
        }

        private void OnRecordingTimeUpdated(TimeSpan duration)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                timeDisplayLabel.Text = duration.ToString(@"hh\:mm\:ss");
            }));
        }

        private void UpdateControlStates()
        {
            bool isRecording = recordingService.IsRecording;
            bool isPaused = recordingService.IsPaused;

            // Actualizar botón de grabación
            if (isRecording)
            {
                recordButton.IconChar = IconChar.Square;
                recordButton.GradientStart = Color.FromArgb(255, 80, 80);
                recordButton.GradientEnd = Color.FromArgb(200, 40, 40);
            }
            else
            {
                recordButton.IconChar = IconChar.Circle;
                recordButton.GradientStart = Color.FromArgb(220, 50, 50);
                recordButton.GradientEnd = Color.FromArgb(180, 30, 30);
            }

            // Actualizar otros botones
            pauseButton.Enabled = isRecording;
            stopButton.Enabled = isRecording;

            if (isPaused)
            {
                pauseButton.IconChar = IconChar.Play;
                pauseButton.GradientStart = Color.FromArgb(76, 175, 80);
                pauseButton.GradientEnd = Color.FromArgb(56, 142, 60);
            }
            else
            {
                pauseButton.IconChar = IconChar.Pause;
                pauseButton.GradientStart = Color.FromArgb(255, 152, 0);
                pauseButton.GradientEnd = Color.FromArgb(230, 120, 0);
            }

            // Actualizar controles de configuración
            recordMouseCheckBox.Enabled = !isRecording;
            recordClicksCheckBox.Enabled = !isRecording;
            recordKeyboardCheckBox.Enabled = !isRecording;
            recordScrollCheckBox.Enabled = !isRecording;

            this.Invalidate();
        }

        private void AddActionToLog(MacroAction action)
        {
            Color actionColor = action.ActionType switch
            {
                MacroActionType.MouseMove => Color.LightGreen,
                MacroActionType.MouseClick => Color.Orange,
                MacroActionType.KeyPress => Color.LightBlue,
                _ => Color.LightGray
            };

            string timestamp = action.Timestamp.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {action}\n";

            actionLogTextBox.SelectionStart = actionLogTextBox.TextLength;
            actionLogTextBox.SelectionLength = 0;
            actionLogTextBox.SelectionColor = actionColor;
            actionLogTextBox.AppendText(logEntry);
            actionLogTextBox.SelectionColor = actionLogTextBox.ForeColor;
            actionLogTextBox.ScrollToCaret();
        }

        private void UpdateActionCount()
        {
            int count = recordingService.ActionCount;
            actionCountLabel.Text = $"Acciones: {count}";
            
            // Actualizar barra de progreso (simulada)
            recordingProgressBar.Value = Math.Min(100, count * 2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                recordingService.RecordingStateChanged -= OnRecordingStateChanged;
                recordingService.ActionRecorded -= OnActionRecorded;
                recordingService.RecordingTimeUpdated -= OnRecordingTimeUpdated;
            }
            base.Dispose(disposing);
        }
    }

    // Diálogo para solicitar nombre del macro
    public partial class MacroNameDialog : Form
    {
        private TextBox nameTextBox;
        private Button okButton;
        private Button cancelButton;

        public string MacroName { get; private set; } = "";

        public MacroNameDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Nombre del Macro";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(50, 50, 50);

            var titleLabel = new Label
            {
                Text = "Introduce un nombre para el macro:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            nameTextBox = new ModernTextBox
            {
                Size = new Size(350, 30),
                Location = new Point(20, 60),
                Text = $"Macro_{DateTime.Now:yyyyMMdd_HHmmss}"
            };
            this.Controls.Add(nameTextBox);

            okButton = new GradientIconButton
            {
                Text = "Aceptar",
                Size = new Size(100, 35),
                Location = new Point(190, 120),
                GradientStart = Color.FromArgb(76, 175, 80),
                GradientEnd = Color.FromArgb(56, 142, 60)
            };
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            cancelButton = new GradientIconButton
            {
                Text = "Cancelar",
                Size = new Size(100, 35),
                Location = new Point(300, 120),
                GradientStart = Color.FromArgb(150, 150, 150),
                GradientEnd = Color.FromArgb(120, 120, 120)
            };
            cancelButton.Click += CancelButton_Click;
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            MacroName = nameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(MacroName))
            {
                MessageBox.Show("Por favor, introduce un nombre válido.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
