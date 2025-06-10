using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using FontAwesome.Sharp;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using WinFormsTimer = System.Windows.Forms.Timer;
using MacroRecorderPro.Controls;
using MacroRecorderPro.Services;
using MacroRecorderPro.Models;

namespace MacroRecorderPro
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        // Servicios
        private readonly MacroRecordingService _recordingService;
        private readonly HotkeyService _hotkeyService;
        private readonly FileManagerService _fileManagerService;
        private readonly SettingsService _settingsService;

        // Componentes principales
        private Panel sidebar;
        private Panel contentPanel;
        private Panel highlightPanel;
        private Label pageTitleLabel;
        private WinFormsTimer titleAnimationTimer;
        private Label recordingStatusLabel;

        // Botones del menú lateral
        private IconButton btnRecording;
        private IconButton btnMacros;
        private IconButton btnHotkeys;
        private IconButton btnSettings;
        private IconButton btnAnalytics;
        private IconButton btnFiles;
        private IconButton buyButton;
        private IconButton licenseButton;

        // Controles específicos de cada panel
        private RecordingPanel recordingPanel;
        private MacrosPanel macrosPanel;
        private HotkeysPanel hotkeysPanel;
        private SettingsPanel settingsPanel;
        private AnalyticsPanel analyticsPanel;
        private FilesPanel filesPanel;

        // Panel de licencia
        private GradientPanel licensePanel;
        private TextBox licenseTextBox;
        private GradientIconButton activateLicenseButton;

        // Botones de control de ventana
        private IconButton bellButton;
        private IconButton closeButton;
        private IconButton minimizeButton;
        private IconButton maximizeButton;

        // Variables de estado
        private IconButton? activeSidebarButton;
        private bool isFullscreen = false;
        private int animationAlpha = 0;
        private int animationX;
        private int targetX;
        private string nextTitle = "";

        // Constantes de diseño
        private const int animationStep = 15;
        private const int animationDistance = 40;
        private const int titleMargin = 20;
        private const int sidebarExpandedWidth = 220;
        private const int topButtonSize = 40;
        private const int topButtonSpacing = 10;
        private const int topButtonMargin = 10;
        private const int sidebarButtonStartTop = 20;
        private const int sidebarButtonSpacing = 55;
        private const int sidebarButtonHeight = 45;
        private const int bottomButtonMargin = 20;
        private const int bottomButtonSpacing = 10;

        // Colores del tema
        private static readonly Color sidebarButtonBaseColor = Color.FromArgb(64, 64, 64);
        private static readonly Color sidebarButtonHoverColor = Color.FromArgb(80, 80, 80);
        private static readonly Color sidebarButtonActiveColor = Color.FromArgb(90, 90, 90);
        private static readonly Color buyButtonBaseColor = Color.FromArgb(200, 40, 40);
        private static readonly Color buyButtonHoverColor = Color.FromArgb(220, 60, 60);
        private static readonly Color driverBoosterTopBarColor = Color.FromArgb(40, 40, 40);
        private static readonly Color driverBoosterSidebarColor = Color.FromArgb(35, 35, 35);

        public MainForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            // Inicializar servicios
            _settingsService = new SettingsService();
            _recordingService = new MacroRecordingService(_settingsService);
            _hotkeyService = new HotkeyService();
            _fileManagerService = new FileManagerService(_settingsService);

            InitializeComponent();
            InitInterface();
            SetupEventHandlers();
            SetupHoverAnimations();
            
            // Configurar hotkeys globales
            SetupGlobalHotkeys();
        }

        private void InitInterface()
        {
            this.Text = "🖱️ Macro Recorder Pro";
            this.Width = 1100;
            this.Height = 700;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.StartPosition = FormStartPosition.CenterScreen;

            CreateTopBar();
            CreateSidebar();
            CreateContentPanel();
            CreateContentPanels();
            CreateLicensePanel();

            // Activar panel inicial
            ActivateSidebarButton(btnRecording);
            ShowContent("🖱️ Grabación de Macros");
        }

        private void CreateTopBar()
        {
            Panel topBar = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = driverBoosterTopBarColor
            };

            // Logo y título
            var logoLabel = new Label
            {
                Text = "🖱️ MACRO RECORDER PRO",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 12),
                AutoSize = true
            };
            topBar.Controls.Add(logoLabel);

            // Indicador de estado de grabación
            recordingStatusLabel = new Label
            {
                Text = "● Listo para grabar",
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(300, 15),
                AutoSize = true
            };
            topBar.Controls.Add(recordingStatusLabel);

            // Botones de control de la ventana
            closeButton = CreateTopBarButton(IconChar.Xmark, 0);
            closeButton.Click += (s, e) => this.Close();
            topBar.Controls.Add(closeButton);

            maximizeButton = CreateTopBarButton(IconChar.WindowMaximize, 1);
            maximizeButton.Click += (s, e) => ToggleMaximize();
            topBar.Controls.Add(maximizeButton);

            minimizeButton = CreateTopBarButton(IconChar.Minus, 2);
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            topBar.Controls.Add(minimizeButton);

            bellButton = CreateTopBarButton(IconChar.Bell, 3);
            topBar.Controls.Add(bellButton);

            // Permitir arrastrar la ventana
            topBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            this.Controls.Add(topBar);
        }

        private void CreateSidebar()
        {
            sidebar = new Panel
            {
                Width = sidebarExpandedWidth,
                Height = this.Height,
                Location = new Point(0, 0),
                BackColor = driverBoosterSidebarColor
            };

            highlightPanel = new Panel
            {
                Size = new Size(4, sidebarButtonHeight),
                BackColor = Color.FromArgb(255, 80, 80),
                Location = new Point(0, sidebarButtonStartTop + 50) // +50 por la barra superior
            };
            sidebar.Controls.Add(highlightPanel);

            // Crear botones del menú
            int currentTop = sidebarButtonStartTop + 50; // +50 por la barra superior

            btnRecording = CreateSidebarButton("Grabación", IconChar.RecordVinyl, currentTop);
            btnRecording.Click += (s, e) => SwitchToPanel("recording");
            sidebar.Controls.Add(btnRecording);
            currentTop += sidebarButtonSpacing;

            btnMacros = CreateSidebarButton("Mis Macros", IconChar.List, currentTop);
            btnMacros.Click += (s, e) => SwitchToPanel("macros");
            sidebar.Controls.Add(btnMacros);
            currentTop += sidebarButtonSpacing;

            btnHotkeys = CreateSidebarButton("Teclas Rápidas", IconChar.Keyboard, currentTop);
            btnHotkeys.Click += (s, e) => SwitchToPanel("hotkeys");
            sidebar.Controls.Add(btnHotkeys);
            currentTop += sidebarButtonSpacing;

            btnSettings = CreateSidebarButton("Configuración", IconChar.Cog, currentTop);
            btnSettings.Click += (s, e) => SwitchToPanel("settings");
            sidebar.Controls.Add(btnSettings);
            currentTop += sidebarButtonSpacing;

            btnAnalytics = CreateSidebarButton("Estadísticas", IconChar.ChartBar, currentTop);
            btnAnalytics.Click += (s, e) => SwitchToPanel("analytics");
            sidebar.Controls.Add(btnAnalytics);
            currentTop += sidebarButtonSpacing;

            btnFiles = CreateSidebarButton("Archivos", IconChar.FolderOpen, currentTop);
            btnFiles.Click += (s, e) => SwitchToPanel("files");
            sidebar.Controls.Add(btnFiles);

            // Botones inferiores
            CreateBottomButtons();

            this.Controls.Add(sidebar);
        }

        private void CreateBottomButtons()
        {
            int bottomStart = this.Height - bottomButtonMargin - 2 * sidebarButtonHeight - bottomButtonSpacing - 50;

            buyButton = CreateSidebarButton(
                "Upgrade Pro",
                IconChar.Crown,
                bottomStart,
                buyButtonBaseColor,
                buyButtonHoverColor);
            buyButton.Font = new Font(buyButton.Font, FontStyle.Bold);
            buyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buyButton.Click += (s, e) => OpenUpgradeUrl();
            sidebar.Controls.Add(buyButton);

            licenseButton = CreateSidebarButton(
                "Activar Licencia",
                IconChar.Key,
                bottomStart + sidebarButtonHeight + bottomButtonSpacing);
            licenseButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            licenseButton.Click += (s, e) => ShowLicensePanel();
            sidebar.Controls.Add(licenseButton);
        }

        private void CreateContentPanel()
        {
            contentPanel = new Panel
            {
                Location = new Point(sidebar.Width, 50), // 50 por la barra superior
                Size = new Size(this.Width - sidebar.Width, this.Height - 50),
                BackColor = Color.FromArgb(50, 50, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(contentPanel);
        }

        private void CreateContentPanels()
        {
            // Crear paneles de contenido
            recordingPanel = new RecordingPanel(_recordingService)
            {
                Size = contentPanel.ClientSize,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = true
            };
            contentPanel.Controls.Add(recordingPanel);

            macrosPanel = new MacrosPanel(_recordingService, _fileManagerService)
            {
                Size = contentPanel.ClientSize,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };
            contentPanel.Controls.Add(macrosPanel);

            hotkeysPanel = new HotkeysPanel(_hotkeyService)
            {
                Size = contentPanel.ClientSize,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };
            contentPanel.Controls.Add(hotkeysPanel);

            settingsPanel = new SettingsPanel(_settingsService)
            {
                Size = contentPanel.ClientSize,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };
            contentPanel.Controls.Add(settingsPanel);

            analyticsPanel = new AnalyticsPanel(_recordingService)
            {
                Size = contentPanel.ClientSize,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };
            contentPanel.Controls.Add(analyticsPanel);

            filesPanel = new FilesPanel(_fileManagerService)
            {
                Size = contentPanel.ClientSize,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };
            contentPanel.Controls.Add(filesPanel);
        }

        private void CreateLicensePanel()
        {
            licensePanel = new GradientPanel
            {
                Size = contentPanel.ClientSize,
                Location = new Point(0, 0),
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var licenseForm = new RadialGradientPanel
            {
                Size = new Size(380, 200),
                CenterColor = Color.FromArgb(70, 70, 70),
                SurroundColor = Color.FromArgb(40, 40, 40)
            };

            var licenseLabel = new Label
            {
                Text = "Introduce tu licencia",
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(25, 25)
            };
            licenseForm.Controls.Add(licenseLabel);

            licenseTextBox = new TextBox
            {
                Width = 250,
                Location = new Point(70, 72),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60)
            };
            licenseForm.Controls.Add(licenseTextBox);

            activateLicenseButton = new GradientIconButton
            {
                Text = "Activar",
                IconChar = IconChar.Check,
                Size = new Size(140, 38),
                Location = new Point(120, 120)
            };
            activateLicenseButton.Click += ActivateLicense;
            licenseForm.Controls.Add(activateLicenseButton);

            licensePanel.Controls.Add(licenseForm);
            contentPanel.Controls.Add(licensePanel);

            // Centrar el formulario
            licensePanel.Resize += (s, e) =>
            {
                licenseForm.Location = new Point(
                    (licensePanel.Width - licenseForm.Width) / 2,
                    (licensePanel.Height - licenseForm.Height) / 2);
            };
        }

        private IconButton CreateSidebarButton(string text, IconChar icon, int top,
            Color? baseColor = null, Color? hoverColor = null)
        {
            int sideMargin = 10;
            Color baseCol = baseColor ?? sidebarButtonBaseColor;
            Color hoverCol = hoverColor ?? sidebarButtonHoverColor;
            
            var button = new IconButton
            {
                Text = "  " + text,
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(15, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = baseCol,
                Size = new Size(sidebarExpandedWidth - 2 * sideMargin, sidebarButtonHeight),
                Location = new Point(sideMargin, top),
                Cursor = Cursors.Hand
            };
            
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button.FlatAppearance.MouseDownBackColor = Color.Transparent;

            button.MouseEnter += (s, e) =>
            {
                if (activeSidebarButton != button)
                    button.BackColor = hoverCol;
            };

            button.MouseLeave += (s, e) =>
            {
                if (activeSidebarButton != button)
                    button.BackColor = baseCol;
            };

            return button;
        }

        private IconButton CreateTopBarButton(IconChar icon, int index)
        {
            var btn = new IconButton
            {
                IconChar = icon,
                IconColor = Color.White,
                ForeColor = Color.White,
                IconSize = 20,
                Size = new Size(topButtonSize, topButtonSize),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Location = new Point(
                    this.Width - topButtonMargin - topButtonSize - index * (topButtonSpacing + topButtonSize),
                    5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            return btn;
        }

        private void SwitchToPanel(string panelName)
        {
            // Ocultar todos los paneles
            recordingPanel.Visible = false;
            macrosPanel.Visible = false;
            hotkeysPanel.Visible = false;
            settingsPanel.Visible = false;
            analyticsPanel.Visible = false;
            filesPanel.Visible = false;
            licensePanel.Visible = false;

            // Mostrar el panel correspondiente
            switch (panelName)
            {
                case "recording":
                    recordingPanel.Visible = true;
                    ActivateSidebarButton(btnRecording);
                    ShowContent("🖱️ Grabación de Macros");
                    UpdateRecordingStatus();
                    break;
                case "macros":
                    macrosPanel.Visible = true;
                    ActivateSidebarButton(btnMacros);
                    ShowContent("📝 Mis Macros");
                    macrosPanel.RefreshMacroList();
                    break;
                case "hotkeys":
                    hotkeysPanel.Visible = true;
                    ActivateSidebarButton(btnHotkeys);
                    ShowContent("⌨️ Teclas Rápidas");
                    break;
                case "settings":
                    settingsPanel.Visible = true;
                    ActivateSidebarButton(btnSettings);
                    ShowContent("⚙️ Configuración Avanzada");
                    break;
                case "analytics":
                    analyticsPanel.Visible = true;
                    ActivateSidebarButton(btnAnalytics);
                    ShowContent("📊 Estadísticas de Uso");
                    analyticsPanel.RefreshStatistics();
                    break;
                case "files":
                    filesPanel.Visible = true;
                    ActivateSidebarButton(btnFiles);
                    ShowContent("📁 Gestión de Archivos");
                    filesPanel.RefreshFileList();
                    break;
            }
        }

        private void ActivateSidebarButton(IconButton button)
        {
            if (activeSidebarButton != null && activeSidebarButton != button)
            {
                activeSidebarButton.BackColor = sidebarButtonBaseColor;
            }

            activeSidebarButton = button;
            activeSidebarButton.BackColor = sidebarButtonActiveColor;
            MoveHighlight(button);
        }

        private void MoveHighlight(Control button)
        {
            highlightPanel.Top = button.Top;
        }

        private void ShowContent(string title)
        {
            nextTitle = title;
            StartTitleAnimation();
        }

        private void StartTitleAnimation()
        {
            if (pageTitleLabel != null)
            {
                contentPanel.Controls.Remove(pageTitleLabel);
                pageTitleLabel.Dispose();
            }

            animationAlpha = 0;

            pageTitleLabel = new Label
            {
                Text = nextTitle,
                ForeColor = Color.FromArgb(0, 255, 255, 255),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true
            };

            contentPanel.Controls.Add(pageTitleLabel);

            targetX = titleMargin;
            animationX = targetX + animationDistance;

            pageTitleLabel.Location = new Point(animationX, 20);
            pageTitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            pageTitleLabel.BringToFront();

            if (titleAnimationTimer == null)
            {
                titleAnimationTimer = new WinFormsTimer();
                titleAnimationTimer.Interval = 30;
                titleAnimationTimer.Tick += TitleAnimationTimer_Tick;
            }

            titleAnimationTimer.Start();
        }

        private void TitleAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (animationAlpha < 255)
            {
                animationAlpha += animationStep;
                if (animationAlpha > 255) animationAlpha = 255;
            }

            if (animationX > targetX)
            {
                animationX -= animationStep;
                if (animationX < targetX) animationX = targetX;
            }

            pageTitleLabel.ForeColor = Color.FromArgb(animationAlpha, 255, 255, 255);
            pageTitleLabel.Location = new Point(animationX, 20);

            if (animationAlpha == 255 && animationX == targetX)
                titleAnimationTimer.Stop();
        }

        private void SetupEventHandlers()
        {
            this.Resize += MainForm_Resize;
            _recordingService.RecordingStateChanged += OnRecordingStateChanged;
        }

        private void SetupHoverAnimations()
        {
            new HoverAnimator(closeButton);
            new HoverAnimator(minimizeButton);
            new HoverAnimator(maximizeButton);
            new HoverAnimator(bellButton);
        }

        private void SetupGlobalHotkeys()
        {
            _hotkeyService.RegisterHotkey("F9", () => _recordingService.ToggleRecording());
            _hotkeyService.RegisterHotkey("F10", () => _recordingService.PauseRecording());
            _hotkeyService.RegisterHotkey("F11", () => macrosPanel?.PlayLastMacro());
            _hotkeyService.RegisterHotkey("F12", () => _recordingService.StopRecording());
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (bellButton != null)
            {
                bellButton.Left = this.Width - topButtonMargin - 3 * topButtonSpacing - 4 * topButtonSize;
                minimizeButton.Left = this.Width - topButtonMargin - 2 * topButtonSpacing - 3 * topButtonSize;
                maximizeButton.Left = this.Width - topButtonMargin - topButtonSpacing - 2 * topButtonSize;
                closeButton.Left = this.Width - topButtonMargin - topButtonSize;
            }

            if (sidebar != null)
            {
                sidebar.Height = this.Height;
                contentPanel.Location = new Point(sidebar.Width, 50);
                contentPanel.Size = new Size(this.Width - sidebar.Width, this.Height - 50);
            }
        }

        private void ToggleMaximize()
        {
            if (!isFullscreen)
            {
                this.WindowState = FormWindowState.Maximized;
                maximizeButton.IconChar = IconChar.WindowRestore;
                isFullscreen = true;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                maximizeButton.IconChar = IconChar.WindowMaximize;
                isFullscreen = false;
            }
        }

        private void ShowLicensePanel()
        {
            licensePanel.Visible = true;
            licensePanel.BringToFront();
            licenseTextBox.Focus();
        }

        private void ActivateLicense(object sender, EventArgs e)
        {
            string license = licenseTextBox.Text.Trim();
            if (string.IsNullOrEmpty(license))
            {
                MessageBox.Show("Por favor, introduce una licencia válida.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar licencia (aquí podrías implementar tu lógica de validación)
            bool isValid = ValidateLicense(license);
            
            if (isValid)
            {
                MessageBox.Show("¡Licencia activada correctamente!", "Éxito", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                licensePanel.Visible = false;
                _settingsService.SaveLicense(license);
            }
            else
            {
                MessageBox.Show("Licencia inválida. Por favor, verifica e intenta de nuevo.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateLicense(string license)
        {
            // Implementar lógica de validación de licencia
            // Por ahora, acepta cualquier licencia que no esté vacía
            return !string.IsNullOrWhiteSpace(license) && license.Length >= 10;
        }

        private void OpenUpgradeUrl()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://macrorecorderpro.com/upgrade",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el enlace: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnRecordingStateChanged(bool isRecording)
        {
            UpdateRecordingStatus();
        }

        private void UpdateRecordingStatus()
        {
            if (recordingStatusLabel != null)
            {
                if (_recordingService.IsRecording)
                {
                    recordingStatusLabel.Text = "● Grabando...";
                    recordingStatusLabel.ForeColor = Color.Red;
                }
                else
                {
                    recordingStatusLabel.Text = "● Listo para grabar";
                    recordingStatusLabel.ForeColor = Color.LightGreen;
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hotkeyService?.Dispose();
            _recordingService?.Dispose();
            titleAnimationTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1100, 700);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }
    }
} if (_recordingService.IsPaused)
                {
                    recordingStatusLabel.Text = "⏸ Pausado";
                    recordingStatusLabel.ForeColor = Color.Orange;
                }
                else