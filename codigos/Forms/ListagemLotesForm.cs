using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    /// <summary>
    /// Lista os lotes de um produto específico, já ordenados por FEFO
    /// (validade mais próxima primeiro) e com destaque visual para lotes
    /// que entraram na janela de alerta (promoção / descarte).
    /// </summary>
    public class ListagemLotesForm : Form
    {
        private static readonly Color Verde     = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeDark = Color.FromArgb(38, 120, 56);
        private static readonly Color CorPromocao = Color.FromArgb(255, 243, 205); // amarelo suave
        private static readonly Color CorDescarte = Color.FromArgb(248, 215, 218); // vermelho suave

        private readonly Produto _produto;

        private Button       _btnIncluir, _btnVoltar;
        private ComboBox     _cmbStatus;
        private DataGridView _grid;
        private Label        _lblResumo;

        public ListagemLotesForm(Produto produto)
        {
            _produto = produto;
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
            Text = $"Lotes — {_produto.Nome}";
            Size = new Size(860, 560);
            MinimumSize = new Size(640, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            Icon = LogoHelper.GetIcon() ?? Icon;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
                Padding = Padding.Empty, Margin = Padding.Empty
            };
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            // ── Barra verde superior ─────────────────────────────────────────
            var topBar = new Panel { Dock = DockStyle.Fill, BackColor = Verde };
            topBar.Controls.Add(LogoHelper.CriarPictureBox(8, 0, 44, 70));
            topBar.Controls.Add(new Label
            {
                Text = $"Lotes de: {_produto.Nome}",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(62, 24), BackColor = Color.Transparent
            });

            _cmbStatus = new ComboBox
            {
                Location = new Point(430, 22), Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f)
            };
            _cmbStatus.Items.AddRange(new object[] { "Ativos", "Inativos", "Todos" });
            _cmbStatus.SelectedIndex = 0;
            _cmbStatus.SelectedIndexChanged += (s, e) => CarregarGrid();

            _btnIncluir = MakeTopBtn("Incluir", VerdeDark);
            _btnIncluir.Click += (s, e) =>
            {
                using var form = new CadastroLoteForm(_produto);
                if (form.ShowDialog() == DialogResult.OK) CarregarGrid();
            };

            _btnVoltar = MakeTopBtn("Voltar", Color.FromArgb(40, 40, 40));
            _btnVoltar.Click += (s, e) => Close();

            void PosicionarBotoes()
            {
                _btnVoltar.Location  = new Point(topBar.Width - 100, 20);
                _btnIncluir.Location = new Point(topBar.Width - 200, 20);
            }
            topBar.Resize        += (s, e) => PosicionarBotoes();
            topBar.HandleCreated += (s, e) => PosicionarBotoes();

            topBar.Controls.AddRange(new Control[] { _cmbStatus, _btnIncluir, _btnVoltar });
            table.Controls.Add(topBar, 0, 0);

            // ── Linha de resumo/alerta ────────────────────────────────────────
            _lblResumo = new Label
            {
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(12, 0, 0, 0)
            };
            table.Controls.Add(_lblResumo, 0, 1);

            // ── Grid ──────────────────────────────────────────────────────────
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32, ColumnHeadersVisible = true,
                RowHeadersVisible = false, Font = new Font("Segoe UI", 9f),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colId",     HeaderText = "ID",       FillWeight = 7,  MinimumWidth = 40 });
            _grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colEditar", HeaderText = "", Text = "✏",
                UseColumnTextForButtonValue = true, FillWeight = 6, MinimumWidth = 32,
                FlatStyle = FlatStyle.Flat
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colQtd",     HeaderText = "Quantidade",  FillWeight = 16, MinimumWidth = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colVal",     HeaderText = "Validade",    FillWeight = 16, MinimumWidth = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colDias",    HeaderText = "Dias p/ vencer", FillWeight = 16, MinimumWidth = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colAcao",    HeaderText = "Alerta",      FillWeight = 20, MinimumWidth = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colStatus",  HeaderText = "Status",      FillWeight = 12, MinimumWidth = 70 });

            _grid.CellClick       += Grid_CellClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;
            table.Controls.Add(_grid, 0, 2);

            // ── Rodapé verde ──────────────────────────────────────────────────
            var rodape = new Panel { Dock = DockStyle.Fill, BackColor = Verde };
            table.Controls.Add(rodape, 0, 3);

            Controls.Add(table);
        }

        private void CarregarGrid()
        {
            var todos = LoteService.Listar(_produto.Id, somenteAtivos: false);
            var lista = _cmbStatus.SelectedIndex switch
            {
                1 => todos.Where(l => !l.Ativo),
                2 => todos,
                _ => todos.Where(l => l.Ativo)
            };
            lista = lista.OrderBy(l => l.DataValidade);

            _grid.Rows.Clear();
            int emPromocao = 0, emDescarte = 0;

            foreach (var l in lista)
            {
                var acao = LoteService.Avaliar(l);
                string textoAcao = acao switch
                {
                    AcaoSugerida.Descarte => "⚠ Descartar",
                    AcaoSugerida.Promocao => "🏷 Promoção",
                    _                     => "—"
                };
                if (acao == AcaoSugerida.Promocao) emPromocao++;
                if (acao == AcaoSugerida.Descarte) emDescarte++;

                int idx = _grid.Rows.Add(
                    l.Id, "✏", l.Quantidade,
                    DateTime.TryParse(l.DataValidade, out var dv) ? dv.ToString("dd/MM/yyyy") : l.DataValidade,
                    l.DiasParaVencer < 0 ? "Vencido" : l.DiasParaVencer.ToString(),
                    textoAcao,
                    l.Ativo ? "✓ Ativo" : "✗ Inativo");

                var row = _grid.Rows[idx];
                if (acao == AcaoSugerida.Descarte)      row.DefaultCellStyle.BackColor = CorDescarte;
                else if (acao == AcaoSugerida.Promocao) row.DefaultCellStyle.BackColor = CorPromocao;
                if (!l.Ativo)
                {
                    row.DefaultCellStyle.ForeColor = Color.Gray;
                    row.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
                }
            }

            _lblResumo.Text = (emPromocao + emDescarte) == 0
                ? "Nenhum lote próximo do vencimento no momento."
                : $"⚠ {emDescarte} lote(s) sugerido(s) para descarte  •  🏷 {emPromocao} lote(s) sugerido(s) para promoção";
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
            int id = Convert.ToInt32(_grid.Rows[row].Cells["colId"].Value);
            var lote = LoteService.Listar(_produto.Id, somenteAtivos: false).Find(l => l.Id == id);
            if (lote == null) return;
            using var form = new CadastroLoteForm(_produto, lote);
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

        private void AplicarTema()
        {
            TemaAplicador.AplicarEm(this);
            // Recoloriza o grid preservando o destaque de alerta por cima do tema.
            CarregarGrid();
        }
    }
}
