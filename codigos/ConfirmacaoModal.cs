using System;
using PotirendabaApp.Services;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PotirendabaApp.Forms
{
    /// <summary>
    /// Modal de confirmação reutilizável.
    /// Exibe uma mensagem centralizada com botões Sim/Não,
    /// bloqueando interação com a janela pai (ShowDialog).
    /// </summary>
    public class ConfirmacaoModal : Form
    {
        public bool Confirmado { get; private set; } = false;

        private static readonly Color FundoModal  = Color.White;
        private static readonly Color BordaModal  = Color.FromArgb(200, 200, 200);
        private static readonly Color VerdeBotao  = Color.FromArgb(45, 140, 65);
        private static readonly Color VermelhoBot = Color.FromArgb(200, 50, 50);

        public ConfirmacaoModal(string mensagem)
        {
            // ── Form ─────────────────────────────────────────────────────────
            FormBorderStyle = FormBorderStyle.None;   // sem borda nativa
            StartPosition   = FormStartPosition.CenterParent;
            Size            = new Size(420, 160);
            BackColor       = FundoModal;
            ShowInTaskbar   = false;

            // ── Borda arredondada via Region ─────────────────────────────────
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 12, 12));

            // ── Ícone de aviso ───────────────────────────────────────────────
            var lblIco = new Label
            {
                Text      = "⚠",
                Font      = new Font("Segoe UI Emoji", 16f),
                ForeColor = Color.FromArgb(200, 140, 0),
                AutoSize  = false,
                Size      = new Size(40, 40),
                Location  = new Point(20, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            Controls.Add(lblIco);

            // ── Mensagem ─────────────────────────────────────────────────────
            var lblMsg = new Label
            {
                Text      = mensagem,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize  = false,
                Size      = new Size(340, 50),
                Location  = new Point(64, 16),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            Controls.Add(lblMsg);

            // ── Separador ────────────────────────────────────────────────────
            var sep = new Panel
            {
                Location  = new Point(0, 80),
                Size      = new Size(420, 1),
                BackColor = BordaModal
            };
            Controls.Add(sep);

            // ── Botão Sim ────────────────────────────────────────────────────
            var btnSim = MakeBotao("Sim", 110, 100, VerdeBotao);
            btnSim.Click += (s, e) =>
            {
                Confirmado    = true;
                DialogResult  = DialogResult.Yes;
                Close();
            };

            // ── Botão Não ────────────────────────────────────────────────────
            var btnNao = MakeBotao("Nao", 220, 100, VermelhoBot);
            btnNao.Click += (s, e) =>
            {
                Confirmado   = false;
                DialogResult = DialogResult.No;
                Close();
            };

            Controls.AddRange(new Control[] { btnSim, btnNao });

            // Fechar ao pressionar Escape
            AplicarTema();
            TemaService.TemaAlterado += AplicarTema;
            KeyPreview     = true;
            KeyDown       += (s, e) => { if (e.KeyCode == Keys.Escape) btnNao.PerformClick(); };
        }

        private Button MakeBotao(string texto, int x, int y, Color cor)
        {
            var btn = new Button
            {
                Text      = texto,
                Location  = new Point(x, y),
                Size      = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = cor,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            // Hover feedback
            btn.MouseEnter += (s, e) =>
                btn.BackColor = Lighten(cor, 25);
            btn.MouseLeave += (s, e) =>
                btn.BackColor = cor;

            return btn;
        }

        // Sombra leve ao redor do modal
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(BordaModal, 1.5f);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        private static Color Lighten(Color c, int a) =>
            Color.FromArgb(Math.Min(c.R+a,255), Math.Min(c.G+a,255), Math.Min(c.B+a,255));

        // Win32 para bordas arredondadas sem borda nativa
        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
            int left, int top, int right, int bottom, int width, int height);

        /// <summary>Aplica o tema atual (claro/escuro) em todos os controles.</summary>
        private void AplicarTema()
        {
            TemaAplicador.AplicarEm(this);
        }



        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaService.TemaAlterado -= AplicarTema;
            base.OnFormClosed(e);
        }


    }
}
