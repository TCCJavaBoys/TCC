using System;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    public class ListagemProdutosForm : Form
    {
        private static readonly Color Verde     = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeDark = Color.FromArgb(38, 120, 56);
        private static readonly Color Fundo     = Color.FromArgb(245, 248, 245);

        private TextBox      _txtPesquisa;
        private Button       _btnIncluir, _btnVoltar, _btnBuscar;
        private CheckBox     _chkProduto, _chkCodigo;
        private ComboBox     _cmbStatus;
        private DataGridView _grid;

        public ListagemProdutosForm()
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
            Text            = "Cadastro de Produto - Listagem";
            Size            = new Size(920, 600);
            MinimumSize     = new Size(660, 420);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            BackColor       = Fundo;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = Padding.Empty, Margin = Padding.Empty, BackColor = Fundo
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 136));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            // ── ROW 0: Barra verde (span 2 colunas) ──────────────────────────
            var topBar = new Panel { Dock = DockStyle.Fill, BackColor = Verde };

            topBar.Controls.Add(new Label
            {
                Text = "🏛", Font = new Font("Segoe UI Emoji", 18f),
                ForeColor = Color.White, AutoSize = false,
                Size = new Size(48, 48), Location = new Point(8, 11),
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent
            });

            topBar.Controls.Add(new Label
            {
                Text = "Cadastro de Produto",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(62, 24), BackColor = Color.Transparent
            });

            _txtPesquisa = new TextBox
            {
                Location = new Point(260, 22), Size = new Size(190, 28),
                Font = new Font("Segoe UI", 10f), PlaceholderText = "Pesquisa"
            };
            _btnBuscar = new Button
            {
                Text = "🔍", Location = new Point(456, 20), Size = new Size(36, 30),
                FlatStyle = FlatStyle.Flat, BackColor = VerdeDark,
                ForeColor = Color.White, Font = new Font("Segoe UI Emoji", 11f),
                Cursor = Cursors.Hand
            };
            _btnBuscar.FlatAppearance.BorderSize = 0;
            _btnBuscar.Click    += (s, e) => CarregarGrid();
            _txtPesquisa.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) CarregarGrid(); };

            _btnIncluir = MakeTopBtn("Incluir", VerdeDark);
            _btnIncluir.MouseEnter += (s, e) => _btnIncluir.BackColor = Color.FromArgb(50, 150, 70);
            _btnIncluir.MouseLeave += (s, e) => _btnIncluir.BackColor = VerdeDark;
            _btnIncluir.Click += BtnIncluir_Click;

            _btnVoltar = MakeTopBtn("Voltar", Color.FromArgb(40, 40, 40));
            _btnVoltar.Click += (s, e) => Close();

            void PosicionarBotoes()
            {
                _btnVoltar.Location  = new Point(topBar.Width - 100, 20);
                _btnIncluir.Location = new Point(topBar.Width - 200, 20);
            }
            topBar.Resize        += (s, e) => PosicionarBotoes();
            topBar.HandleCreated += (s, e) => PosicionarBotoes();

            topBar.Controls.AddRange(new Control[]
                { _txtPesquisa, _btnBuscar, _btnIncluir, _btnVoltar });

            table.Controls.Add(topBar, 0, 0);
            table.SetColumnSpan(topBar, 2);

            // ── ROW 1, COL 0: Filtros ─────────────────────────────────────────
            var pFiltros = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };

            pFiltros.Controls.Add(new Label
            {
                Text = "Filtros", Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = true, Location = new Point(10, 12), BackColor = Color.Transparent
            });

            _chkProduto = new CheckBox
            {
                Text = "Produto", Checked = true, AutoSize = true,
                Location = new Point(10, 36), Font = new Font("Segoe UI", 9f)
            };
            _chkProduto.CheckedChanged += (s, e) => CarregarGrid();

            _chkCodigo = new CheckBox
            {
                Text = "Codigo", Checked = false, AutoSize = true,
                Location = new Point(10, 60), Font = new Font("Segoe UI", 9f)
            };
            _chkCodigo.CheckedChanged += (s, e) => CarregarGrid();

            pFiltros.Controls.Add(new Label
            {
                Text = "Status:", Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                AutoSize = true, Location = new Point(10, 90), BackColor = Color.Transparent
            });

            _cmbStatus = new ComboBox
            {
                Location = new Point(10, 108), Size = new Size(112, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5f)
            };
            _cmbStatus.Items.AddRange(new[] { "Ativos", "Inativos", "Todos" });
            _cmbStatus.SelectedIndex = 0;
            _cmbStatus.SelectedIndexChanged += (s, e) => CarregarGrid();

            pFiltros.Controls.AddRange(new Control[] { _chkProduto, _chkCodigo, _cmbStatus });
            table.Controls.Add(pFiltros, 0, 1);

            // ── ROW 1, COL 1: Grid ────────────────────────────────────────────
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32, ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9f),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(220, 220, 220),
                EnableHeadersVisualStyles = false
            };
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 200, 200);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(20, 20, 20);
            _grid.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            _grid.DefaultCellStyle.SelectionBackColor     = Color.FromArgb(180, 220, 180);
            _grid.DefaultCellStyle.SelectionForeColor     = Color.Black;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 240);

            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colId",    HeaderText="ID",       FillWeight=7,  MinimumWidth=44 });
            _grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name="colEditar",  HeaderText="",  Text="✏",
                UseColumnTextForButtonValue=true, FillWeight=6, MinimumWidth=32,
                FlatStyle=FlatStyle.Flat
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colNome",  HeaderText="Nome",     FillWeight=28, MinimumWidth=80  });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colValor", HeaderText="Valor",    FillWeight=13, MinimumWidth=60  });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colEstq",  HeaderText="Estoque",  FillWeight=10, MinimumWidth=50  });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colData",  HeaderText="Cadastro", FillWeight=14, MinimumWidth=70  });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colStatus",HeaderText="Status",   FillWeight=10, MinimumWidth=60  });

            _grid.CellClick       += Grid_CellClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            table.Controls.Add(_grid, 1, 1);

            // ── ROW 2: Rodapé (span 2 colunas) ───────────────────────────────
            var rodape = new Panel { Dock = DockStyle.Fill, BackColor = Verde };
            table.Controls.Add(rodape, 0, 2);
            table.SetColumnSpan(rodape, 2);

            Controls.Add(table);
        }

        private void CarregarGrid()
        {
            var status = _cmbStatus?.SelectedIndex switch
            {
                1 => FiltroStatus.Inativos,
                2 => FiltroStatus.Todos,
                _ => FiltroStatus.Ativos
            };
            var lista = ProdutoService.Listar(
                _txtPesquisa?.Text.Trim() ?? "",
                _chkProduto?.Checked ?? true,
                _chkCodigo?.Checked  ?? false,
                false, status);

            _grid.Rows.Clear();
            foreach (var p in lista)
            {
                int idx = _grid.Rows.Add(
                    p.Id, "✏", p.Nome,
                    $"R$ {p.Valor:F2}", p.Estoque.ToString(), p.Data,
                    p.Ativo ? "✓ Ativo" : "✗ Inativo");
                if (!p.Ativo)
                {
                    _grid.Rows[idx].DefaultCellStyle.ForeColor = Color.Gray;
                    _grid.Rows[idx].DefaultCellStyle.Font =
                        new Font("Segoe UI", 9f, FontStyle.Italic);
                }
            }
        }

        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "colEditar") return;
            AbrirEdicao(e.RowIndex);
        }
        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) AbrirEdicao(e.RowIndex);
        }
        private void AbrirEdicao(int row)
        {
            int id    = Convert.ToInt32(_grid.Rows[row].Cells["colId"].Value);
            var todos = ProdutoService.Listar(status: FiltroStatus.Todos);
            var prod  = todos.Find(p => p.Id == id);
            if (prod == null) return;
            using var form = new CadastroProdutoForm(prod);
            if (form.ShowDialog() == DialogResult.OK) CarregarGrid();
        }
        private void BtnIncluir_Click(object sender, EventArgs e)
        {
            using var form = new CadastroProdutoForm();
            if (form.ShowDialog() == DialogResult.OK) CarregarGrid();
        }

        private static Button MakeTopBtn(string t, Color cor)
        {
            var b = new Button
            {
                Text = t, Size = new Size(92, 30), FlatStyle = FlatStyle.Flat,
                BackColor = cor, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void AplicarTema() => TemaAplicador.AplicarEm(this);
    }
}
