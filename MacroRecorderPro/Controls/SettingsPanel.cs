using System;
using System.Drawing;
using System.Windows.Forms;
using MacroRecorderPro.Services;

namespace MacroRecorderPro.Controls
{
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(50, 50, 50);

            // Etiqueta temporal
            var label = new Label
            {
                Text = "SettingsPanel - En construcción...",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(50, 50),
                AutoSize = true
            };
            this.Controls.Add(label);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Name = "SettingsPanel";
            this.Size = new Size(800, 600);
            this.ResumeLayout(false);
        }
    }
}
