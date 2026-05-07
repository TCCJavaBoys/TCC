using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PotirendabaApp
{
    /// <summary>
    /// Botão com cantos arredondados/circulares e efeito hover.
    /// O fundo fora do círculo herda a cor do controle pai (sem quadrado branco).
    /// </summary>
    public class RoundButton : Button
    {
        // ── Propriedades ──────────────────────────────────────────────────────
        private int _cornerRadius = 20;
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        private Color _normalColor = Color.FromArgb(56, 161, 78);
        public Color NormalColor
        {
            get => _normalColor;
            set { _normalColor = value; Invalidate(); }
        }

        private Color _hoverColor = Color.FromArgb(80, 190, 100);
        public Color HoverColor
        {
            get => _hoverColor;
            set { _hoverColor = value; Invalidate(); }
        }

        private bool _isHovered;

        public RoundButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize           = 0;
            FlatAppearance.MouseOverBackColor   = Color.Transparent;
            FlatAppearance.MouseDownBackColor   = Color.Transparent;
            BackColor = Color.Transparent;
            Cursor    = Cursors.Hand;
        }

        // ── Pintar o fundo com a cor do pai (elimina quadrado branco) ─────────
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent != null)
                e.Graphics.Clear(Parent.BackColor);
            else
                e.Graphics.Clear(BackColor);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true; Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false; Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode   = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // 1. Fundo igual ao pai (garante que os cantos fiquem transparentes)
            if (Parent != null) g.Clear(Parent.BackColor);

            // 2. Círculo / arredondado colorido
            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            int r    = Math.Min(_cornerRadius, Math.Min(rect.Width, rect.Height) / 2);
            Color bg = _isHovered ? _hoverColor : _normalColor;

            using var path  = RoundedRect(rect, r);
            using var brush = new SolidBrush(bg);
            g.FillPath(brush, path);

            // 3. Brilho sutil no hover
            if (_isHovered)
            {
                using var glow = new Pen(Color.FromArgb(60, Color.White), 2);
                g.DrawPath(glow, path);
            }

            // 4. Texto / emoji centralizado
            var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var tb = new SolidBrush(ForeColor);
            g.DrawString(Text, Font, tb, new RectangleF(0, 0, Width, Height), sf);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d  = radius * 2;
            var gp = new GraphicsPath();
            gp.AddArc(r.X,         r.Y,          d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            gp.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}
