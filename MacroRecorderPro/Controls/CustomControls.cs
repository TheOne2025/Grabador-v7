using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace MacroRecorderPro.Controls
{
    // Panel con degradado lineal
    public class GradientPanel : Panel
    {
        public Color GradientStart { get; set; } = Color.FromArgb(70, 70, 70);
        public Color GradientEnd { get; set; } = Color.FromArgb(50, 50, 50);
        public int CornerRadius { get; set; } = 15;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new LinearGradientBrush(this.ClientRectangle, GradientStart, GradientEnd, LinearGradientMode.Vertical);
            using var path = RoundedRect(this.ClientRectangle, CornerRadius);
            e.Graphics.FillPath(brush, path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (CornerRadius > 0)
            {
                using var path = RoundedRect(this.ClientRectangle, CornerRadius);
                this.Region = new Region(path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // Panel con degradado radial
    public class RadialGradientPanel : Panel
    {
        public Color CenterColor { get; set; } = Color.FromArgb(80, 80, 80);
        public Color SurroundColor { get; set; } = Color.FromArgb(40, 40, 40);
        public int CornerRadius { get; set; } = 20;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = this.ClientRectangle;
            using var path = RoundedRect(rect, CornerRadius);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = this.CenterColor,
                SurroundColors = new[] { this.SurroundColor }
            };
            e.Graphics.FillPath(brush, path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (CornerRadius > 0)
            {
                using var path = RoundedRect(this.ClientRectangle, CornerRadius);
                this.Region = new Region(path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // Botón con degradado y esquinas redondeadas
    public class GradientIconButton : IconButton
    {
        public Color GradientStart { get; set; } = Color.FromArgb(220, 40, 40);
        public Color GradientEnd { get; set; } = Color.FromArgb(180, 20, 20);
        public int CornerRadius { get; set; } = 10;

        public GradientIconButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.UseVisualStyleBackColor = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(this.ClientRectangle, CornerRadius);
            using var brush = new LinearGradientBrush(this.ClientRectangle, GradientStart, GradientEnd, LinearGradientMode.Vertical);
            e.Graphics.FillPath(brush, path);
            e.Graphics.SetClip(path);
            
            // Dibujar el texto y el icono
            var textRect = this.ClientRectangle;
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor, 
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (bounds.Width > d && bounds.Height > d)
            {
                path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
                path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
                path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
            }
            else
            {
                path.AddRectangle(bounds);
            }
            return path;
        }
    }

    // Animador de hover para botones
    public class HoverAnimator
    {
        private readonly Control target;
        private readonly WinFormsTimer animationTimer;
        private Color currentColor;
        private Color targetColor;
        private readonly int animationSteps = 10;
        private int stepCount = 0;
        private bool animatingIn;

        public HoverAnimator(Control control)
        {
            target = control;
            currentColor = target.BackColor;
            animationTimer = new WinFormsTimer { Interval = 15 };
            animationTimer.Tick += AnimateStep;

            target.MouseEnter += (s, e) =>
            {
                animatingIn = true;
                targetColor = GetHoverColor();
                if (target is IconButton ib)
                {
                    ib.IconColor = GetHoverColor();
                    ib.ForeColor = GetHoverColor();
                    ib.Invalidate();
                }
                stepCount = 0;
                animationTimer.Start();
            };

            target.MouseLeave += (s, e) =>
            {
                animatingIn = false;
                targetColor = Color.Transparent;
                if (target is IconButton ib)
                {
                    ib.IconColor = Color.White;
                    ib.ForeColor = Color.White;
                    ib.Invalidate();
                }
                stepCount = 0;
                animationTimer.Start();
            };

            target.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    target.BackColor = ControlPaint.Dark(GetHoverColor(), 0.2f);
            };

            target.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    target.BackColor = GetHoverColor();
            };
        }

        private void AnimateStep(object sender, EventArgs e)
        {
            stepCount++;
            float t = stepCount / (float)animationSteps;
            int r = (int)(currentColor.R + t * (targetColor.R - currentColor.R));
            int g = (int)(currentColor.G + t * (targetColor.G - currentColor.G));
            int b = (int)(currentColor.B + t * (targetColor.B - currentColor.B));
            int a = (int)(currentColor.A + t * (targetColor.A - currentColor.A));
            Color interpolated = Color.FromArgb(Math.Max(0, Math.Min(255, a)), 
                                               Math.Max(0, Math.Min(255, r)), 
                                               Math.Max(0, Math.Min(255, g)), 
                                               Math.Max(0, Math.Min(255, b)));
            target.BackColor = interpolated;

            if (stepCount >= animationSteps)
            {
                animationTimer.Stop();
                currentColor = targetColor;
            }
        }

        private Color GetHoverColor()
        {
            if (target is IconButton btn)
            {
                if (btn.IconChar == IconChar.Xmark)
                    return Color.FromArgb(180, 60, 60); // rojo cerrar
                if (btn.IconChar == IconChar.Minus)
                    return Color.FromArgb(70, 70, 70);  // gris minimizar
                if (btn.IconChar == IconChar.Bell)
                    return Color.FromArgb(60, 60, 90);  // azul notificación
                if (btn.IconChar == IconChar.WindowMaximize || btn.IconChar == IconChar.WindowRestore)
                    return Color.FromArgb(70, 70, 100);  // azul oscuro
            }
            return Color.FromArgb(80, 80, 80); // fallback
        }
    }

    // TextBox con estilo moderno
    public class ModernTextBox : TextBox
    {
        private Color borderColor = Color.FromArgb(100, 100, 100);
        private Color focusedBorderColor = Color.FromArgb(255, 80, 80);
        private bool isFocused = false;

        public ModernTextBox()
        {
            this.BorderStyle = BorderStyle.None;
            this.BackColor = Color.FromArgb(60, 60, 60);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            isFocused = true;
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            isFocused = false;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Color currentBorderColor = isFocused ? focusedBorderColor : borderColor;
            using (var pen = new Pen(currentBorderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
            }
        }
    }

    // ComboBox con estilo moderno
    public class ModernComboBox : ComboBox
    {
        public ModernComboBox()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.BackColor = Color.FromArgb(60, 60, 60);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);
            this.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                
                Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected 
                    ? Color.White : Color.LightGray;
                
                using (var brush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(this.Items[e.Index].ToString(), 
                        this.Font, brush, e.Bounds);
                }
                
                e.DrawFocusRectangle();
            }
        }
    }

    // Barra de progreso con estilo moderno
    public class ModernProgressBar : Control
    {
        private int value = 0;
        private int maximum = 100;
        private Color progressColor = Color.FromArgb(255, 80, 80);
        private Color backgroundColor = Color.FromArgb(60, 60, 60);

        public int Value
        {
            get => value;
            set
            {
                this.value = Math.Max(0, Math.Min(maximum, value));
                this.Invalidate();
            }
        }

        public int Maximum
        {
            get => maximum;
            set
            {
                maximum = Math.Max(1, value);
                this.Invalidate();
            }
        }

        public Color ProgressColor
        {
            get => progressColor;
            set
            {
                progressColor = value;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Dibujar fondo
            using (var brush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            
            // Dibujar progreso
            if (value > 0)
            {
                int progressWidth = (int)((double)value / maximum * this.Width);
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, progressWidth, this.Height),
                    progressColor,
                    ControlPaint.Light(progressColor),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, progressWidth, this.Height);
                }
            }
        }
    }
}