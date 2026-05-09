using System;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    /// <summary>
    /// Modal de seleção de período para relatórios que precisam de data inicial e final.
    /// Abrir com ShowDialog() — use DataInicial e DataFinal após DialogResult.OK.
    /// </summary>
    public class FrmPeriodoRelatorio : Form
    {
        // ── Propriedades públicas (lidas pelo Form pai após OK) ───────────────
        public DateTime DataInicial => dtInicial.Value.Date;
        public DateTime DataFinal   => dtFinal.Value.Date;

        // ── Controles ─────────────────────────────────────────────────────────
        private DateTimePicker dtInicial;
        private DateTimePicker dtFinal;
        private Button         btnImprimir;
        private Button         btnVoltar;
        private Label          lblPergunta;
        private Label          lblAte;
        private Panel          pConteudo;

        // ── Ação de impressão injetada pelo chamador ──────────────────────────
        private readonly Action<DateTime, DateTime> _acaoImprimir;

        /// <param name="acaoImprimir">
        /// Método a executar ao clicar em Imprimir.
        /// Recebe (dataInicial, dataFinal).
        /// Pode ser null — útil para integração futura com RDLC/FastReport.
        /// </param>
        public FrmPeriodoRelatorio(Action<DateTime, DateTime> acaoImprimir = null)
        {
            _acaoImprimir = acaoImprimir;
            InitUI();
            AplicarTema();
            TemaService.TemaAlterado += AplicarTema;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaService.TemaAlterado -= AplicarTema;
            base.OnFormClosed(e);
        }

        private void InitUI()
        {
            // ── Form ──────────────────────────────────────────────────────────
            Text            = "Período do Relatório";
            Size            = new Size(420, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            KeyPreview      = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            // ── Painel central ────────────────────────────────────────────────
            pConteudo = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(24, 18, 24, 18)
            };
            Controls.Add(pConteudo);

            // ── Ícone de aviso + pergunta ─────────────────────────────────────
            var lblIcone = new Label
            {
                Text      = "⚠",
                Font      = new Font("Segoe UI Symbol", 16f),
                AutoSize  = true,
                Location  = new Point(24, 18),
                BackColor = Color.Transparent
            };
            pConteudo.Controls.Add(lblIcone);

            lblPergunta = new Label
            {
                Text      = "Qual período gostaria de imprimir o relatório?",
                Font      = new Font("Segoe UI", 9.5f),
                AutoSize  = false,
                Size      = new Size(330, 36),
                Location  = new Point(58, 20),
                BackColor = Color.Transparent
            };
            pConteudo.Controls.Add(lblPergunta);

            // ── DateTimePickers ───────────────────────────────────────────────
            dtInicial = new DateTimePicker
            {
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today,
                Location = new Point(24, 72),
                Size     = new Size(130, 26),
                Font     = new Font("Segoe UI", 9.5f)
            };
            pConteudo.Controls.Add(dtInicial);

            lblAte = new Label
            {
                Text      = "Até",
                AutoSize  = true,
                Location  = new Point(162, 76),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            pConteudo.Controls.Add(lblAte);

            dtFinal = new DateTimePicker
            {
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today,
                Location = new Point(196, 72),
                Size     = new Size(130, 26),
                Font     = new Font("Segoe UI", 9.5f)
            };
            pConteudo.Controls.Add(dtFinal);

            // ── Botões ────────────────────────────────────────────────────────
            btnImprimir = MakeBtn("🖨 Imprimir", TemaService.VerdeDark, new Point(24, 118));
            btnImprimir.Click += BtnImprimir_Click;
            pConteudo.Controls.Add(btnImprimir);

            btnVoltar = MakeBtn("Voltar", Color.FromArgb(100, 100, 100), new Point(232, 118));
            btnVoltar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            pConteudo.Controls.Add(btnVoltar);
        }

        // ── Evento Imprimir ───────────────────────────────────────────────────
        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            if (DataInicial > DataFinal)
            {
                MessageBox.Show(
                    "A data inicial não pode ser maior que a data final.",
                    "Período inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Executa a ação injetada (RDLC, FastReport, etc.)
            _acaoImprimir?.Invoke(DataInicial, DataFinal);

            DialogResult = DialogResult.OK;
            Close();
        }

        // ── Tema ──────────────────────────────────────────────────────────────
        private void AplicarTema()
        {
            BackColor               = TemaService.FundoForm;
            pConteudo.BackColor     = TemaService.FundoForm;
            lblPergunta.ForeColor   = TemaService.TextoPrincipal;
            lblAte.ForeColor        = TemaService.TextoPrincipal;
            dtInicial.CalendarForeColor = TemaService.TextoPrincipal;
            dtFinal.CalendarForeColor   = TemaService.TextoPrincipal;
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private static Button MakeBtn(string texto, Color cor, Point local)
        {
            var b = new Button
            {
                Text      = texto,
                Location  = local,
                Size      = new Size(120, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = cor,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.MouseEnter += (s, e) => b.BackColor = Lighten(cor, 20);
            b.MouseLeave += (s, e) => b.BackColor = cor;
            return b;
        }

        private static Color Lighten(Color c, int a) =>
            Color.FromArgb(
                Math.Min(c.R + a, 255),
                Math.Min(c.G + a, 255),
                Math.Min(c.B + a, 255));
    }
}
