using System;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Data;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    public class ConsultaVendasForm : Form
    {
        private static readonly Color Verde     = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeDark = Color.FromArgb(38, 120, 56);
        private static readonly Color Fundo     = Color.FromArgb(245, 248, 245);

        private TextBox      _txtPesquisa;
        private Button       _btnBuscar, _btnPDV, _btnVoltar;
        private CheckBox     _chkNome, _chkCodigo, _chkData, _chkValor;
        private DataGridView _grid;

        public ConsultaVendasForm()
        {
            InitUI();
            CarregarGrid();
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
            Text            = "Consulta de Vendas";
            Size            = new Size(960, 620);
            MinimumSize     = new Size(680, 440);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            BackColor       = Fundo;

            // ════════════════════════════════════════════════════════════════
            // TableLayoutPanel RAIZ: 3 linhas × 2 colunas
            //
            //  ┌──────────────────────────────────────────────────────────┐
            //  │  Row 0 (70px)  Header verde           [span 2 cols]      │
            //  ├────────────┬─────────────────────────────────────────────┤
            //  │Col0 (136px)│ Col1 (*)                                    │
            //  │  Filtros   │  DataGridView (Fill)                        │
            //  │            │                                             │
            //  ├────────────┴─────────────────────────────────────────────┤
            //  │  Row 2 (48px)  Rodapé verde           [span 2 cols]      │
            //  └──────────────────────────────────────────────────────────┘
            // ════════════════════════════════════════════════════════════════

            var root = new TableLayoutPanel
            {
                Dock            = DockStyle.Fill,
                ColumnCount     = 2,
                RowCount        = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding         = Padding.Empty,
                Margin          = Padding.Empty,
                BackColor       = Fundo
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 136));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));   // header
            root.RowStyles.Add(new RowStyle(SizeType.Percent,  100));  // conteúdo
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));   // rodapé

            // ── Row 0: Header verde (span 2 colunas) ─────────────────────────
            var header = new Panel { Dock = DockStyle.Fill, BackColor = Verde };

            header.Controls.Add(new Label {
                Text = "🏛", Font = new Font("Segoe UI Emoji", 18f), ForeColor = Color.White,
                AutoSize = false, Size = new Size(48, 48), Location = new Point(8, 11),
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });

            header.Controls.Add(new Label {
                Text = "Consulta de Vendas",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(62, 24), BackColor = Color.Transparent });

            _txtPesquisa = new TextBox {
                Location = new Point(260, 22), Size = new Size(200, 28),
                Font = new Font("Segoe UI", 10f), PlaceholderText = "Pesquisa" };

            _btnBuscar = new Button {
                Text = "🔍", Location = new Point(466, 20), Size = new Size(36, 30),
                FlatStyle = FlatStyle.Flat, BackColor = VerdeDark,
                ForeColor = Color.White, Font = new Font("Segoe UI Emoji", 12f),
                Cursor = Cursors.Hand };
            _btnBuscar.FlatAppearance.BorderSize = 0;
            _btnBuscar.Click     += (s, e) => CarregarGrid();
            _txtPesquisa.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) CarregarGrid(); };

            _btnPDV = MakeBtn("PDV", VerdeDark);
            _btnPDV.MouseEnter += (s, e) => _btnPDV.BackColor = Verde;
            _btnPDV.MouseLeave += (s, e) => _btnPDV.BackColor = VerdeDark;
            _btnPDV.Click += BtnPDV_Click;

            _btnVoltar = MakeBtn("Voltar", Color.FromArgb(40, 40, 40));
            _btnVoltar.Click += (s, e) => Close();

            void PosicionarBotoes()
            {
                _btnVoltar.Location = new Point(header.Width - 100, 20);
                _btnPDV.Location    = new Point(header.Width - 200, 20);
            }
            header.Resize        += (s, e) => PosicionarBotoes();
            header.HandleCreated += (s, e) => PosicionarBotoes();

            header.Controls.AddRange(new Control[]
                { _txtPesquisa, _btnBuscar, _btnPDV, _btnVoltar });

            root.Controls.Add(header, 0, 0);
            root.SetColumnSpan(header, 2);

            // ── Row 1, Col 0: Filtros ─────────────────────────────────────────
            var pFiltros = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };

            pFiltros.Controls.Add(new Label {
                Text = "Filtros", Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = true, Location = new Point(10, 12),
                BackColor = Color.Transparent });

            _chkNome   = MakeChk("Nome",   10, 36);
            _chkCodigo = MakeChk("Codigo", 10, 58);
            _chkData   = MakeChk("Data",   10, 80);
            _chkValor  = MakeChk("Valor",  10, 102);

            pFiltros.Controls.AddRange(new Control[]
                { _chkNome, _chkCodigo, _chkData, _chkValor });

            root.Controls.Add(pFiltros, 0, 1);

            // ── Row 1, Col 1: DataGridView DIRETO na célula ───────────────────
            // Sem painel intermediário — o grid vai direto para a célula [1,1]
            // isso elimina qualquer risco de sobreposição ou tamanho zero
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight   = 32,
                ColumnHeadersVisible  = true,
                RowHeadersVisible     = false,
                Font                  = new Font("Segoe UI", 9f),
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor             = Color.FromArgb(220, 220, 220),
                EnableHeadersVisualStyles = false
            };

            // Estilos definidos APÓS criação (mais confiável no .NET 8)
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 200, 200);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(20, 20, 20);
            _grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9f, FontStyle.Bold);
            _grid.DefaultCellStyle.BackColor          = Color.White;
            _grid.DefaultCellStyle.ForeColor          = Color.FromArgb(20, 20, 20);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(180, 220, 180);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 240);

            // Colunas
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colId",     HeaderText="ID",         FillWeight=7,  MinimumWidth=44 });
            _grid.Columns.Add(new DataGridViewButtonColumn {
                Name="colEdit",     HeaderText="",   Text="✏",
                UseColumnTextForButtonValue=true,
                FillWeight=6, MinimumWidth=32, FlatStyle=FlatStyle.Flat });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colCliente",HeaderText="Cliente",    FillWeight=28, MinimumWidth=80  });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colValor",  HeaderText="Valor",      FillWeight=14, MinimumWidth=60  });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colData",   HeaderText="Data/Hora",  FillWeight=22, MinimumWidth=100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colPgto",   HeaderText="Pagamento",  FillWeight=16, MinimumWidth=70  });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colStatus", HeaderText="Status",     FillWeight=9,  MinimumWidth=50  });

            _grid.CellClick       += Grid_CellClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            root.Controls.Add(_grid, 1, 1);   // grid DIRETO na célula — sem wrapper

            // ── Row 2: Rodapé verde (span 2 colunas) ─────────────────────────
            var rodape = new Panel { Dock = DockStyle.Fill, BackColor = Verde };
            root.Controls.Add(rodape, 0, 2);
            root.SetColumnSpan(rodape, 2);

            Controls.Add(root);
        }

        // ── Carregar dados ────────────────────────────────────────────────────
        private void CarregarGrid()
        {
            var vendas = DatabaseHelper.ListarVendas(
                _txtPesquisa?.Text.Trim() ?? "",
                _chkNome?.Checked   ?? false,
                _chkCodigo?.Checked ?? false,
                _chkData?.Checked   ?? false,
                _chkValor?.Checked  ?? false);

            _grid.Rows.Clear();
            foreach (var v in vendas)
                _grid.Rows.Add(
                    v.Id, "✏", v.Cliente,
                    $"R$ {v.ValorTotal:F2}",
                    v.Data, v.FormaPagamento, v.Status);
        }

        // ── Eventos do grid ───────────────────────────────────────────────────
        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "colEdit") return;
            AbrirVisualizacao(e.RowIndex);
        }
        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) AbrirVisualizacao(e.RowIndex);
        }
        private void AbrirVisualizacao(int row)
        {
            int id = Convert.ToInt32(_grid.Rows[row].Cells["colId"].Value);
            using var form = new VisualizarVendaForm(id);
            form.ShowDialog(this);
        }
        private void BtnPDV_Click(object sender, EventArgs e)
        {
            using var pdv = new PDVForm();
            pdv.ShowDialog(this);
            CarregarGrid();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private CheckBox MakeChk(string t, int x, int y) => new CheckBox {
            Text = t, Checked = false, AutoSize = true,
            Location = new Point(x, y), Font = new Font("Segoe UI", 9f) };

        private static Button MakeBtn(string t, Color cor)
        {
            var b = new Button {
                Text = t, Size = new Size(92, 30), FlatStyle = FlatStyle.Flat,
                BackColor = cor, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void AplicarTema() => TemaAplicador.AplicarEm(this);
    }
}
