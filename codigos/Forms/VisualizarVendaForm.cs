using System;
using PotirendabaApp.Services;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PotirendabaApp.Models;

namespace PotirendabaApp.Forms
{
    /// <summary>
    /// Exibe todos os detalhes de uma venda em modo somente leitura,
    /// simulando o layout do PDV mas sem permitir edições.
    /// </summary>
    public class VisualizarVendaForm : Form
    {
        // ── Paleta idêntica ao PDV ────────────────────────────────────────────
        private static readonly Color Verde       = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeDark   = Color.FromArgb(38, 120, 56);
        private static readonly Color FundoForm   = Color.FromArgb(235, 238, 235);
        private static readonly Color FundoPainel = Color.White;
        private static readonly Color TextoLabel  = Color.FromArgb(40, 40, 40);
        private static readonly Color TextoSub    = Color.FromArgb(100, 100, 100);
        private static readonly Color FundoRO     = Color.FromArgb(245, 245, 245); // read-only

        private readonly Venda _venda;

        // Controles
        private DataGridView _grid;
        private Label        _lblTotalValor, _lblTotalQtd;

        public VisualizarVendaForm(int vendaId)
        {
            _venda = VendaService.BuscarComItens(vendaId);
            if (_venda == null)
            {
                MessageBox.Show("Venda nao encontrada.", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Load += (s, e) => Close();
                return;
            }
            InitUI();
            AplicarTema();
            TemaService.TemaAlterado += AplicarTema;
            PopularDados();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  INTERFACE
        // ══════════════════════════════════════════════════════════════════════
        private void InitUI()
        {
            Text            = $"Visualizar Venda #{_venda.Id}";
            Size            = new Size(980, 620);
            MinimumSize     = new Size(900, 560);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = FundoForm;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            Icon            = LogoHelper.GetIcon() ?? Icon;

            ConstruirTopBar();

            int topo = 62, altura = Height - topo - 50;

            var pEsq    = PainelBranco(8,    topo, 295, altura);
            var pCentro = PainelBranco(311,  topo, 430, altura);
            var pDir    = PainelBranco(749,  topo, 218, altura);

            ConstruirEsquerda(pEsq);
            ConstruirCentro(pCentro);
            ConstruirDireita(pDir);

            Controls.AddRange(new Control[] { pEsq, pCentro, pDir });
        }

        // ── Top bar ───────────────────────────────────────────────────────────
        private void ConstruirTopBar()
        {
            var bar = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Verde };

            bar.Controls.Add(LogoHelper.CriarPictureBox(8, 0, 44, 54));

            // Badge "SOMENTE LEITURA" centralizado na barra
            bar.Controls.Add(new Label
            {
                Text = $"Venda #{_venda.Id}  —  SOMENTE LEITURA",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(200, 16), BackColor = Color.Transparent
            });

            Controls.Add(bar);
        }

        // ── Coluna esquerda: dados do cabeçalho ───────────────────────────────
        private void ConstruirEsquerda(Panel p)
        {
            int cx = 12, cw = p.Width - 24, cy = 10;

            // ── Seção Produto ─────────────────────────────────────────────────
            p.Controls.Add(SecaoLabel("Produto", cx, cy, cw)); cy += 24;

            p.Controls.Add(SubLabel("Codigo", cx, cy)); cy += 16;
            p.Controls.Add(CampoRO(_venda.Id.ToString(), cx, cy, 80));

            // Linha quantidade | valor
            p.Controls.Add(SubLabel("Quantidade", cx, cy + 36));
            p.Controls.Add(SubLabel("Valor unit.",  cx + 102, cy + 36));
            cy += 52;
            int qtdTotal = 0;
            decimal valorMedio = 0;
            if (_venda.Itens.Count > 0)
            {
                foreach (var i in _venda.Itens) qtdTotal += i.Quantidade;
                valorMedio = _venda.ValorTotal / qtdTotal;
            }
            p.Controls.Add(CampoRO(qtdTotal.ToString(), cx,      cy, 90));
            p.Controls.Add(CampoRO($"R$ {_venda.ValorTotal:F2}", cx + 102, cy, cw - 102));
            cy += 34;

            // Desconto (calculado)
            p.Controls.Add(SubLabel("Desconto total", cx, cy)); cy += 16;
            decimal descTotal = 0;
            foreach (var i in _venda.Itens) descTotal += i.Desconto;
            p.Controls.Add(CampoRO(descTotal > 0 ? $"R$ {descTotal:F2}" : "Sem desconto",
                cx, cy, cw)); cy += 34;

            // Valor total
            var txtValTotal = CampoRO($"R$ {_venda.ValorTotal:F2}", cx, cy, cw);
            txtValTotal.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
            txtValTotal.ForeColor = VerdeDark;
            txtValTotal.TextAlign = HorizontalAlignment.Center;
            p.Controls.Add(txtValTotal); cy += 42;

            // Separador
            p.Controls.Add(Separador(cx, cy, cw)); cy += 14;

            // ── Seção Aluno ───────────────────────────────────────────────────
            p.Controls.Add(SecaoLabel("Aluno", cx, cy, cw)); cy += 24;
            p.Controls.Add(CampoRO(
                string.IsNullOrWhiteSpace(_venda.Cliente) ? "(nao informado)" : _venda.Cliente,
                cx, cy, cw)); cy += 42;

            // ── Seção Pagamento ───────────────────────────────────────────────
            p.Controls.Add(SecaoLabel("Forma de pagamento", cx, cy, cw)); cy += 24;
            p.Controls.Add(CampoRO(_venda.FormaPagamento, cx, cy, cw)); cy += 42;

            // ── Data/hora ─────────────────────────────────────────────────────
            p.Controls.Add(SecaoLabel("Data / Hora", cx, cy, cw)); cy += 24;
            p.Controls.Add(CampoRO(_venda.Data, cx, cy, cw)); cy += 42;

            // ── Status ────────────────────────────────────────────────────────
            p.Controls.Add(SecaoLabel("Status", cx, cy, cw)); cy += 24;
            var lblStatus = CampoRO(_venda.Status, cx, cy, cw);
            lblStatus.ForeColor = _venda.Status == "Ativa"
                ? Color.FromArgb(34, 130, 55) : Color.FromArgb(180, 50, 50);
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            p.Controls.Add(lblStatus);
        }

        // ── Coluna central: itens da venda ────────────────────────────────────
        private void ConstruirCentro(Panel p)
        {
            p.Controls.Add(new Label
            {
                Text = "Produtos da Venda", AutoSize = true,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = TextoLabel, Location = new Point(10, 10),
                BackColor = Color.Transparent
            });
            p.Controls.Add(new Panel { Location = new Point(10, 34), Size = new Size(60, 3),
                BackColor = Verde });

            _grid = new DataGridView
            {
                Location  = new Point(0, 44), Size = new Size(p.Width, p.Height - 44),
                ReadOnly  = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode    = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor  = FundoPainel, BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 30, RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9f),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(220, 220, 220),
                DefaultCellStyle =
                {
                    BackColor = FundoPainel, ForeColor = TextoLabel,
                    SelectionBackColor = Color.FromArgb(180, 220, 180),
                    SelectionForeColor = Color.Black
                },
                AlternatingRowsDefaultCellStyle =
                    { BackColor = Color.FromArgb(245, 252, 245) }
            };
            _grid.ColumnHeadersDefaultCellStyle.BackColor = TemaService.GridCabecalho;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(20, 20, 20);
            _grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9f, FontStyle.Bold);
            _grid.EnableHeadersVisualStyles = false;

            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "cId",   HeaderText = "ID",       FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "cNome", HeaderText = "Produto",  FillWeight = 42 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "cQtd",  HeaderText = "Qtd",      FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "cUnit", HeaderText = "Unit.",    FillWeight = 18 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "cDesc", HeaderText = "Desconto", FillWeight = 14 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "cVal",  HeaderText = "Total",    FillWeight = 18 });

            p.Controls.Add(_grid);
        }

        // ── Coluna direita: totais + botão Fechar ─────────────────────────────
        private void ConstruirDireita(Panel p)
        {
            int cx = 10, cw = p.Width - 20, cy = 10;

            // Total da venda
            p.Controls.Add(new Label { Text = "Total de venda",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = TextoLabel,
                AutoSize = true, Location = new Point(cx, cy), BackColor = Color.Transparent });
            cy += 20;
            p.Controls.Add(new Panel { Location = new Point(cx, cy),
                Size = new Size(50, 3), BackColor = Verde }); cy += 8;

            // Caixa do valor
            var boxTotal = new Panel { Location = new Point(cx, cy),
                Size = new Size(cw, 72), BackColor = FundoPainel };
            boxTotal.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(180, 180, 180), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, boxTotal.Width - 1, boxTotal.Height - 1);
            };
            _lblTotalValor = new Label
            {
                Text = _venda.ValorTotal.ToString("F2").Replace(".", ","),
                Font = new Font("Segoe UI", 26f, FontStyle.Bold), ForeColor = TextoLabel,
                AutoSize = false, Size = new Size(cw - 4, 68), Location = new Point(2, 2),
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent
            };
            boxTotal.Controls.Add(_lblTotalValor);
            p.Controls.Add(boxTotal); cy += 82;

            // Total de itens
            p.Controls.Add(new Label { Text = "Total de produtos",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = TextoLabel,
                AutoSize = true, Location = new Point(cx, cy), BackColor = Color.Transparent });
            cy += 20;
            p.Controls.Add(new Panel { Location = new Point(cx, cy),
                Size = new Size(50, 3), BackColor = Verde }); cy += 8;

            var boxQtd = new Panel { Location = new Point(cx, cy),
                Size = new Size(cw, 44), BackColor = FundoPainel };
            boxQtd.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(180, 180, 180), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, boxQtd.Width - 1, boxQtd.Height - 1);
            };
            _lblTotalQtd = new Label
            {
                Text = _venda.Itens.Count.ToString(),
                Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = TextoLabel,
                AutoSize = false, Size = new Size(cw - 4, 40), Location = new Point(2, 2),
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent
            };
            boxQtd.Controls.Add(_lblTotalQtd);
            p.Controls.Add(boxQtd); cy += 56;

            // Informações extras
            cy += 8;
            p.Controls.Add(InfoLabel("Data:", _venda.Data, cx, ref cy, cw));
            p.Controls.Add(InfoLabel("Aluno:", _venda.Cliente, cx, ref cy, cw));
            p.Controls.Add(InfoLabel("Pagamento:", _venda.FormaPagamento, cx, ref cy, cw));

            // Botão Fechar (único botão — sem ações destrutivas)
            int btnY = p.Height - 54;
            var btnFechar = new Button
            {
                Text = "Fechar", Location = new Point(cx, btnY), Size = new Size(cw, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(90, 90, 90), ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnFechar.FlatAppearance.BorderSize = 0;
            btnFechar.MouseEnter += (s, e) => btnFechar.BackColor = Color.FromArgb(110, 110, 110);
            btnFechar.MouseLeave += (s, e) => btnFechar.BackColor = Color.FromArgb(90, 90, 90);
            btnFechar.Click += (s, e) => Close();
            p.Controls.Add(btnFechar);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  POPULAR DADOS
        // ══════════════════════════════════════════════════════════════════════
        private void PopularDados()
        {
            _grid.Rows.Clear();
            foreach (var item in _venda.Itens)
            {
                _grid.Rows.Add(
                    item.ProdutoId,
                    item.NomeProduto,
                    $"{item.Quantidade}x",
                    $"R$ {item.ValorUnit:F2}",
                    item.Desconto > 0 ? $"R$ {item.Desconto:F2}" : "—",
                    $"R$ {item.ValorTotal:F2}");
            }

            _lblTotalValor.Text = _venda.ValorTotal.ToString("F2").Replace(".", ",");
            _lblTotalQtd.Text   = _venda.Itens.Count.ToString();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS DE UI
        // ══════════════════════════════════════════════════════════════════════
        private Panel PainelBranco(int x, int y, int w, int h) => new Panel
        {
            Location = new Point(x, y), Size = new Size(w, h),
            BackColor = FundoPainel, BorderStyle = BorderStyle.None
        };

        private Label SecaoLabel(string t, int x, int y, int w)
        {
            var lbl = new Label
            {
                Text = t, AutoSize = false, Size = new Size(w, 18),
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextoLabel, BackColor = Color.Transparent
            };
            lbl.Paint += (s, e) =>
            {
                using var pen = new Pen(Verde, 2);
                e.Graphics.DrawLine(pen, 0, lbl.Height - 2,
                    Math.Min(t.Length * 7, w), lbl.Height - 2);
            };
            return lbl;
        }

        private Label SubLabel(string t, int x, int y) => new Label
        {
            Text = t, AutoSize = true, Location = new Point(x, y),
            Font = new Font("Segoe UI", 8f), ForeColor = TextoSub,
            BackColor = Color.Transparent
        };

        /// <summary>Campo de texto somente leitura estilizado.</summary>
        private TextBox CampoRO(string valor, int x, int y, int w) => new TextBox
        {
            Text = valor, Location = new Point(x, y), Size = new Size(w, 28),
            Font = new Font("Segoe UI", 9f), ReadOnly = true, TabStop = false,
            BackColor = FundoRO, BorderStyle = BorderStyle.FixedSingle,
            ForeColor = TextoLabel
        };

        private Panel Separador(int x, int y, int w) => new Panel
        {
            Location = new Point(x, y), Size = new Size(w, 1),
            BackColor = Color.FromArgb(210, 210, 210)
        };

        /// <summary>Linha de informação: "Rótulo: valor" compacto.</summary>
        private Panel InfoLabel(string rotulo, string valor, int x, ref int cy, int cw)
        {
            var p = new Panel { Location = new Point(x, cy),
                Size = new Size(cw, 28), BackColor = Color.Transparent };
            p.Controls.Add(new Label { Text = rotulo,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = TextoSub,
                AutoSize = true, Location = new Point(0, 6) });
            p.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(valor) ? "—" : valor,
                Font = new Font("Segoe UI", 8.5f), ForeColor = TextoLabel,
                AutoSize = true, Location = new Point(72, 6)
            });
            cy += 30;
            return p;
        }
 
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaService.TemaAlterado -= AplicarTema;
            base.OnFormClosed(e);
        }


        /// <summary>Aplica o tema atual (claro/escuro) em todos os controles.</summary>
        private void AplicarTema()
        {
            TemaAplicador.AplicarEm(this);
        }


    }
}