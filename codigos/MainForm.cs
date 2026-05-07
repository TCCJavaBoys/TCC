using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using PotirendabaApp.Data;
using PotirendabaApp.Forms;
using PotirendabaApp.Services;

namespace PotirendabaApp
{
    public class MainForm : Form
    {
        // ── Constantes de layout ──────────────────────────────────────────────
        private const int BAR_HEIGHT    = 54;
        private const int SIDEBAR_WIDTH = 64;
        private const int BTN_SIZE      = 44;
        private const int BTN_MARGIN    = 10;

        // ── Paleta estática (barra verde nunca muda) ──────────────────────────
        private static readonly Color Green      = Color.FromArgb(56, 161, 78);
        private static readonly Color GreenDark  = Color.FromArgb(38, 120, 56);
        private static readonly Color GreenLight = Color.FromArgb(80, 190, 100);

        // ── Controles ─────────────────────────────────────────────────────────
        private Panel       _topBar, _bottomBar, _sidebar;
        private Label       _lblDate, _lblTitle, _lblSubtitle;
        private Panel       _logoPanel;
        private RoundButton _btnTema;                  // ← botão sol/lua
        private RoundButton _btnAluno, _btnEstoque, _btnVenda, _btnRelatorio;
        private RoundButton _btnWhatsApp, _btnFacebook, _btnGitHub;

        public MainForm()
        {
            InitializeComponent();
            ApplyLayout();

            // Aplica tema inicial e assina o evento global
            AplicarTema();
            TemaService.TemaAlterado += AplicarTema;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaService.TemaAlterado -= AplicarTema;
            base.OnFormClosed(e);
        }

        private void InitializeComponent()
        {
            Text            = "Potirendaba - Prefeitura Municipal";
            Size            = new Size(860, 560);
            MinimumSize     = new Size(720, 460);
            StartPosition   = FormStartPosition.CenterScreen;
            Font            = new Font("Segoe UI", 9f);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
        }

        private void ApplyLayout()
        {
            BuildTopBar();
            BuildSidebar();
            BuildCenter();
            BuildBottomBar();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TOP BAR
        // ══════════════════════════════════════════════════════════════════════
        private void BuildTopBar()
        {
            _topBar = new Panel
                { Dock = DockStyle.Top, Height = BAR_HEIGHT, BackColor = Green };

            // ── Botão de tema (sol/lua) no lugar do selo ──────────────────────
            _btnTema = new RoundButton
            {
                Size         = new Size(42, 42),
                Location     = new Point(8, 6),
                Font         = new Font("Segoe UI Emoji", 16f),
                FlatStyle    = FlatStyle.Flat,
                ForeColor    = Color.White,
                Cursor       = Cursors.Hand,
                CornerRadius = 21,
                NormalColor  = GreenDark,
                HoverColor   = GreenLight
            };
            _btnTema.FlatAppearance.BorderSize = 0;
            _btnTema.Click += (s, e) =>
            {
                TemaService.Alternar();           // muda estado global
                AtualizarBotaoTema();             // atualiza ícone imediato
            };
            new ToolTip().SetToolTip(_btnTema, TemaService.TooltipTema);
            AtualizarBotaoTema();
            _topBar.Controls.Add(_btnTema);

            // ── Data ──────────────────────────────────────────────────────────
            _lblDate = new Label
            {
                Text      = DateTime.Now.ToString("dd/MM/yyyy"),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize  = false, TextAlign = ContentAlignment.MiddleCenter,
                Size      = new Size(120, BAR_HEIGHT),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            _lblDate.Location = new Point(_topBar.Width - _lblDate.Width - 8, 0);
            _topBar.Controls.Add(_lblDate);
            _topBar.Resize += (s, e) =>
                _lblDate.Location = new Point(_topBar.Width - _lblDate.Width - 8, 0);

            Controls.Add(_topBar);
        }

        private void AtualizarBotaoTema()
        {
            _btnTema.Text = TemaService.IconeTema;
            new ToolTip().SetToolTip(_btnTema, TemaService.TooltipTema);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SIDEBAR
        // ══════════════════════════════════════════════════════════════════════
        private void BuildSidebar()
        {
            _sidebar = new Panel
                { Width = SIDEBAR_WIDTH, BackColor = Green, Dock = DockStyle.Left };

            int top = BAR_HEIGHT + BTN_MARGIN;

            _btnAluno     = MakeSideButton("👤", "Aluno",            top); top += BTN_SIZE + BTN_MARGIN;
            _btnEstoque   = MakeSideButton("📦", "Estoque/Produto",  top); top += BTN_SIZE + BTN_MARGIN;
            _btnVenda     = MakeSideButton("💲", "Venda",            top); top += BTN_SIZE + BTN_MARGIN;
            _btnRelatorio = MakeSideButton("📋", "Relatorio",        top);

            _btnAluno.Click     += (s, e) => AbrirForm(new ListagemAlunosForm());
            _btnEstoque.Click   += (s, e) => AbrirForm(new ListagemProdutosForm());
            _btnVenda.Click     += (s, e) => AbrirVendas();
            _btnRelatorio.Click += (s, e) => OnNavClick("Relatorio");

            _sidebar.Controls.AddRange(new Control[]
                { _btnAluno, _btnEstoque, _btnVenda, _btnRelatorio });
            Controls.Add(_sidebar);
        }

        private RoundButton MakeSideButton(string emoji, string tooltip, int top)
        {
            var btn = new RoundButton
            {
                Size = new Size(BTN_SIZE, BTN_SIZE),
                Location = new Point((SIDEBAR_WIDTH - BTN_SIZE) / 2, top),
                Text = emoji, Font = new Font("Segoe UI Emoji", 15f),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                Cursor = Cursors.Hand, CornerRadius = BTN_SIZE / 2,
                HoverColor = GreenLight, NormalColor = GreenDark
            };
            btn.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(btn, tooltip);
            return btn;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CENTRO
        // ══════════════════════════════════════════════════════════════════════
        private void BuildCenter()
        {
            _logoPanel = new Panel
                { Size = new Size(180, 180), BackColor = Color.Transparent };
            _logoPanel.Paint += DrawLogo;

            _lblTitle = new Label
            {
                Text = "POTIRENDABA", AutoSize = true,
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            _lblSubtitle = new Label
            {
                Text = "PREFEITURA MUNICIPAL", AutoSize = true,
                Font = new Font("Segoe UI", 12f),
                BackColor = Color.Transparent
            };

            Controls.AddRange(new Control[] { _logoPanel, _lblTitle, _lblSubtitle });
            Resize += (s, e) => CenterContent();
            Load   += (s, e) => CenterContent();
        }

        private void CenterContent()
        {
            int cx = (SIDEBAR_WIDTH + ClientSize.Width) / 2;
            int cy = (BAR_HEIGHT + ClientSize.Height - BAR_HEIGHT) / 2;
            _logoPanel.Location  = new Point(cx - _logoPanel.Width / 2,  cy - _logoPanel.Height / 2 - 50);
            _lblTitle.Location   = new Point(cx - _lblTitle.Width / 2,   _logoPanel.Bottom + 12);
            _lblSubtitle.Location= new Point(cx - _lblSubtitle.Width / 2,_lblTitle.Bottom + 4);
        }

        private void DrawLogo(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = _logoPanel.Width, h = _logoPanel.Height, pad = 6;

            using var shieldBrush = new SolidBrush(Color.FromArgb(240, 255, 240));
            var shield = new GraphicsPath();
            shield.AddArc(pad, pad, 40, 40, 180, 90);
            shield.AddArc(w - pad - 40, pad, 40, 40, 270, 90);
            shield.AddLine(w - pad, h / 2 + 20, w / 2, h - pad);
            shield.AddLine(w / 2, h - pad, pad, h / 2 + 20);
            shield.CloseFigure();
            g.FillPath(shieldBrush, shield);
            using var shieldPen = new Pen(Green, 3);
            g.DrawPath(shieldPen, shield);

            for (int i = 0; i < 3; i++)
            {
                using var stripe = new SolidBrush(i == 1 ? GreenDark : Green);
                g.FillRectangle(stripe, 28 + i * 40, 50, 26, 70);
            }
            using var bandBrush = new SolidBrush(GreenDark);
            g.FillRectangle(bandBrush, pad + 2, h - 38, w - pad * 2 - 4, 22);
            using var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("POTIRENDABA", new Font("Segoe UI", 6f, FontStyle.Bold),
                Brushes.White, new RectangleF(pad, h - 38, w - pad * 2, 22), sf);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BOTTOM BAR
        // ══════════════════════════════════════════════════════════════════════
        private void BuildBottomBar()
        {
            _bottomBar = new Panel
                { Dock = DockStyle.Bottom, Height = BAR_HEIGHT, BackColor = Green };

            int by = (BAR_HEIGHT - BTN_SIZE) / 2;

            _btnWhatsApp = MakeBottomButton("💬", "WhatsApp", 10, by, Color.FromArgb(37, 211, 102));
            _btnWhatsApp.Click += (s, e) => OpenUrl("https://wa.me/5517999999999");

            _btnFacebook = MakeBottomButton("f", "Facebook", 0, by, Color.FromArgb(24, 119, 242));
            _btnFacebook.Font   = new Font("Segoe UI", 14f, FontStyle.Bold);
            _btnFacebook.Click += (s, e) => OpenUrl("https://www.facebook.com/PrefeituraDePotirendaba");

            _btnGitHub = MakeBottomButton("⌨", "GitHub", 0, by, Color.FromArgb(33, 33, 33));
            _btnGitHub.Click += (s, e) => OpenUrl("https://github.com/seu-usuario/potirendaba");

            _bottomBar.Controls.AddRange(new Control[] { _btnWhatsApp, _btnFacebook, _btnGitHub });
            _bottomBar.Resize        += (s, e) => PositionSocialButtons();
            _bottomBar.HandleCreated += (s, e) => PositionSocialButtons();
            Controls.Add(_bottomBar);
        }

        private void PositionSocialButtons()
        {
            int by = (BAR_HEIGHT - BTN_SIZE) / 2;
            _btnGitHub.Location   = new Point(_bottomBar.Width - BTN_SIZE - 10, by);
            _btnFacebook.Location = new Point(_btnGitHub.Left - BTN_SIZE - 8, by);
        }

        private RoundButton MakeBottomButton(string icon, string tooltip,
                                             int x, int y, Color accent)
        {
            var btn = new RoundButton
            {
                Size = new Size(BTN_SIZE, BTN_SIZE), Location = new Point(x, y),
                Text = icon, Font = new Font("Segoe UI Emoji", 15f),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                Cursor = Cursors.Hand, CornerRadius = BTN_SIZE / 2,
                HoverColor = Lighten(accent, 30), NormalColor = accent
            };
            btn.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(btn, tooltip);
            return btn;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  APLICAR TEMA — chamado no início e toda vez que o tema mudar
        // ══════════════════════════════════════════════════════════════════════
        private void AplicarTema()
        {
            // Form e área central
            BackColor         = TemaService.FundoForm;
            _lblTitle.ForeColor    = TemaService.TextoPrincipal;
            _lblSubtitle.ForeColor = TemaService.TextoSecundario;

            // Redesenha logo (usa BackColor do pai)
            _logoPanel.Invalidate();

            // Força repintura dos RoundButtons do sidebar
            // (eles herdam BackColor do pai via OnPaintBackground)
            _sidebar.Refresh();
            _bottomBar.Refresh();
            _topBar.Refresh();

            Refresh();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  NAVEGAÇÃO
        // ══════════════════════════════════════════════════════════════════════
        private void AbrirForm(Form form)
        {
            using (form) form.ShowDialog(this);
        }

        private void AbrirVendas()
        {
            using var pdv = new PDVForm();
            pdv.ShowDialog(this);
            if (pdv.CancelouVenda)
            {
                using var consulta = new ConsultaVendasForm();
                consulta.ShowDialog(this);
            }
        }

        private void OnNavClick(string section) =>
            MessageBox.Show($"Modulo '{section}' ainda nao implementado.",
                "Potirendaba", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private static void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex)
            {
                MessageBox.Show($"Nao foi possivel abrir:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Color Lighten(Color c, int a) =>
            Color.FromArgb(Math.Min(c.R+a,255), Math.Min(c.G+a,255), Math.Min(c.B+a,255));
    }
}
