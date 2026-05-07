using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Forms.Modais;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    public class PDVForm : Form
    {
        // ── Paleta ────────────────────────────────────────────────────────────
        private static readonly Color Verde      = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeDark  = Color.FromArgb(38, 120, 56);
        private static readonly Color Vermelho   = Color.FromArgb(180, 40, 40);
        private static readonly Color FundoForm  = Color.FromArgb(235, 238, 235);
        private static readonly Color FundoBranco= Color.White;
        private static readonly Color TextoEscuro= Color.FromArgb(40, 40, 40);
        private static readonly Color TextoCinza = Color.FromArgb(100, 100, 100);

        // ── Estado da venda ───────────────────────────────────────────────────
        private readonly List<ItemVenda> _itens = new();
        private Produto  _produtoAtual = null;
        private Aluno    _alunoAtual   = null;
        public  bool     CancelouVenda { get; private set; } = false;

        // ── Controles — coluna esquerda ───────────────────────────────────────
        private TextBox  _txtCodigo, _txtQtd, _txtValorUnit, _txtDesconto, _txtValorTotal;
        private TextBox  _txtAluno;
        private Label    _lblNomeProduto;
        private Button   _btnLupaProduto, _btnLupaAluno, _btnAddItem;
        private ComboBox _cmbPagamento;
        private Panel    _pAlunoBox, _pPagtoBox;

        // ── Controles — coluna central ────────────────────────────────────────
        private DataGridView _grid;

        // ── Controles — coluna direita ────────────────────────────────────────
        private Label  _lblTotalValor, _lblTotalQtd;
        private Button _btnFinalizar, _btnCancelar;

        public PDVForm()
        {
            InitUI();
            AplicarTema();
            TemaService.TemaAlterado += AplicarTema;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaService.TemaAlterado -= AplicarTema;
            base.OnFormClosed(e);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  INTERFACE — TableLayoutPanel garante responsividade perfeita
        // ══════════════════════════════════════════════════════════════════════
        private void InitUI()
        {
            Text            = "PDV - Ponto de Venda";
            Size            = new Size(1000, 640);
            MinimumSize     = new Size(820, 520);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = FundoForm;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;

            // ── Top bar (Dock=Top) ────────────────────────────────────────────
            var topBar = new Panel { Dock=DockStyle.Top, Height=54, BackColor=Verde };
            topBar.Controls.Add(new Label {
                Text="🏛", Font=new Font("Segoe UI Emoji",18f), ForeColor=Color.White,
                AutoSize=false, Size=new Size(50,50), Location=new Point(6,2),
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent });
            topBar.Controls.Add(new Label {
                Text="PDV - Ponto de Venda",
                Font=new Font("Segoe UI",13f,FontStyle.Bold), ForeColor=Color.White,
                AutoSize=true, Location=new Point(62,16), BackColor=Color.Transparent });
            Controls.Add(topBar);

            // ── Bottom bar (Dock=Bottom) ──────────────────────────────────────
            Controls.Add(new Panel { Dock=DockStyle.Bottom, Height=48, BackColor=Verde });

            // ── TableLayoutPanel: 3 colunas × 1 linha (Fill) ─────────────────
            // Col 0 = esquerda (310px fixo)
            // Col 1 = central  (*)
            // Col 2 = direita  (230px fixo)
            var table = new TableLayoutPanel
            {
                Dock=DockStyle.Fill,
                ColumnCount=3, RowCount=1,
                CellBorderStyle=TableLayoutPanelCellBorderStyle.None,
                Padding=new Padding(6), Margin=Padding.Empty,
                BackColor=FundoForm
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ── Coluna 0: entrada de dados ────────────────────────────────────
            var pEsq = new Panel {
                Dock=DockStyle.Fill, BackColor=FundoBranco,
                Padding=Padding.Empty };
            ConstruirColunaEsquerda(pEsq);
            table.Controls.Add(pEsq, 0, 0);

            // ── Coluna 1: grid de itens ───────────────────────────────────────
            var pCentro = new Panel { Dock=DockStyle.Fill, BackColor=FundoBranco };
            ConstruirColunaCentral(pCentro);
            table.Controls.Add(pCentro, 1, 0);

            // ── Coluna 2: totais e ações ──────────────────────────────────────
            var pDir = new Panel {
                Dock=DockStyle.Fill, BackColor=FundoForm,
                Padding=new Padding(8,8,8,8) };
            ConstruirColunaDireita(pDir);
            table.Controls.Add(pDir, 2, 0);

            Controls.Add(table);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  COLUNA ESQUERDA — entradas
        // ══════════════════════════════════════════════════════════════════════
        private void ConstruirColunaEsquerda(Panel p)
        {
            // Usamos TableLayoutPanel interno para empilhar as seções
            var tbl = new FlowLayoutPanel {
                Dock=DockStyle.Fill,
                FlowDirection=FlowDirection.TopDown,
                WrapContents=false,
                AutoScroll=true,
                Padding=new Padding(4,6,4,4),
                BackColor=FundoBranco };

            // ── Seção Produto ─────────────────────────────────────────────────
            tbl.Controls.Add(SecLabel("Produto"));

            // Linha código + lupa
            var rowCod = new Panel { Dock=DockStyle.Top, Height=34 };
            _txtCodigo = Txt("Codigo do produto", 0, 2, 224);
            _txtCodigo.KeyDown  += (s,e) => { if(e.KeyCode==Keys.Enter) BuscarPorCodigo(); };
            _txtCodigo.Leave    += (s,e) => { if(!string.IsNullOrWhiteSpace(_txtCodigo.Text)) BuscarPorCodigo(); };
            _btnLupaProduto = Lupa(228, 2);
            _btnLupaProduto.Click += BtnLupaProduto_Click;
            rowCod.Controls.AddRange(new Control[]{ _txtCodigo, _btnLupaProduto });
            tbl.Controls.Add(rowCod);

            // Nome do produto
            _lblNomeProduto = new Label {
                Text="—", AutoSize=false, Width=280, Height=18,
                Font=new Font("Segoe UI",8.5f,FontStyle.Italic),
                ForeColor=TextoCinza, BackColor=Color.Transparent,
                Margin=new Padding(0,0,0,4) };
            tbl.Controls.Add(_lblNomeProduto);

            // Qtd | Valor
            tbl.Controls.Add(SecLabel("Quantidade   /   Valor unitário"));
            var rowQV = new Panel { Dock=DockStyle.Top, Height=32 };
            _txtQtd = Txt("Qtd", 0, 2, 100);
            _txtQtd.KeyPress  += (s,e) => { if(!char.IsDigit(e.KeyChar)&&e.KeyChar!='\b') e.Handled=true; };
            _txtQtd.TextChanged += RecalcularItem;
            _txtValorUnit = Txt("", 108, 2, 148);
            _txtValorUnit.ReadOnly  = true;
            _txtValorUnit.BackColor = Color.FromArgb(245,245,245);
            rowQV.Controls.AddRange(new Control[]{ _txtQtd, _txtValorUnit });
            tbl.Controls.Add(rowQV);

            // Desconto
            tbl.Controls.Add(SecLabel("Desconto (ex: 10% ou 1.50)"));
            _txtDesconto = Txt("Desconto",0,0,280);
            _txtDesconto.TextChanged += RecalcularItem;
            tbl.Controls.Add(_txtDesconto);

            // Valor total do item
            _txtValorTotal = new TextBox {
                Width=280, Height=28,
                Font=new Font("Segoe UI",9f), ReadOnly=true, TabStop=false,
                BackColor=Color.FromArgb(240,245,240),
                TextAlign=HorizontalAlignment.Center, Text="Valor total do produto",
                Margin=new Padding(0,2,0,4) };
            tbl.Controls.Add(_txtValorTotal);

            // Botão adicionar
            _btnAddItem = new Button {
                Text="+ Adicionar Item", Width=280, Height=32,
                FlatStyle=FlatStyle.Flat, BackColor=VerdeDark, ForeColor=Color.White,
                Font=new Font("Segoe UI",9f,FontStyle.Bold), Cursor=Cursors.Hand,
                Margin=new Padding(0,2,0,6) };
            _btnAddItem.FlatAppearance.BorderSize=0;
            _btnAddItem.MouseEnter += (s,e) => _btnAddItem.BackColor=Verde;
            _btnAddItem.MouseLeave += (s,e) => _btnAddItem.BackColor=VerdeDark;
            _btnAddItem.Click += BtnAddItem_Click;
            tbl.Controls.Add(_btnAddItem);

            // Separador
            tbl.Controls.Add(new Panel { Width=280, Height=1, BackColor=Color.FromArgb(210,210,210), Margin=new Padding(0,8,0,8) });

            // ── Seção Aluno ───────────────────────────────────────────────────
            _pAlunoBox = new Panel { Width=280, Height=58, BackColor=Color.Transparent };
            _pAlunoBox.Controls.Add(SecLabel("Aluno"));
            var rowAluno = new Panel { Location=new Point(0,20), Size=new Size(260,32), BackColor=Color.Transparent };
            _txtAluno = Txt("Selecionar aluno...", 0, 2, 226);
            _txtAluno.ReadOnly=true; _txtAluno.Cursor=Cursors.Hand;
            _txtAluno.Click += BtnLupaAluno_Click;
            _btnLupaAluno = Lupa(228, 2);
            _btnLupaAluno.Click += BtnLupaAluno_Click;
            rowAluno.Controls.AddRange(new Control[]{ _txtAluno, _btnLupaAluno });
            _pAlunoBox.Controls.Add(rowAluno);
            tbl.Controls.Add(_pAlunoBox);

            // ── Seção Forma de Pagamento ──────────────────────────────────────
            _pPagtoBox = new Panel { Width=280, Height=58, BackColor=Color.Transparent };
            _pPagtoBox.Controls.Add(SecLabel("Forma de pagamento"));
            _cmbPagamento = new ComboBox {
                Location=new Point(0,22), Size=new Size(270,28),
                DropDownStyle=ComboBoxStyle.DropDownList,
                Font=new Font("Segoe UI",9f), FlatStyle=FlatStyle.Flat };
            _cmbPagamento.Items.Add("Selecione");
            foreach(var f in VendaService.FormasPagamento) _cmbPagamento.Items.Add(f);
            _cmbPagamento.SelectedIndex=0;
            _cmbPagamento.SelectedIndexChanged += (s,e) => LimparDestaque(_pPagtoBox);
            _pPagtoBox.Controls.Add(_cmbPagamento);
            tbl.Controls.Add(_pPagtoBox);

            p.Controls.Add(tbl);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  COLUNA CENTRAL — grid de itens
        // ══════════════════════════════════════════════════════════════════════
        private void ConstruirColunaCentral(Panel p)
        {
            var lblTitulo = new Label {
                Text="Produtos", Dock=DockStyle.Top, Height=34,
                Font=new Font("Segoe UI",12f,FontStyle.Bold),
                ForeColor=TextoEscuro, TextAlign=ContentAlignment.MiddleLeft,
                Padding=new Padding(8,0,0,0), BackColor=FundoBranco };
            p.Controls.Add(lblTitulo);

            _grid = new DataGridView {
                Dock=DockStyle.Fill,
                ReadOnly=true, AllowUserToAddRows=false, AllowUserToDeleteRows=false,
                SelectionMode=DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor=FundoBranco, BorderStyle=BorderStyle.None,
                ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight=30, ColumnHeadersVisible=true,
                RowHeadersVisible=false, Font=new Font("Segoe UI",9f),
                AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,
                GridColor=Color.FromArgb(220,220,220),
                EnableHeadersVisualStyles=false };
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(210,210,210);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = TextoEscuro;
            _grid.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI",9f,FontStyle.Bold);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(180,220,180);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245,252,245);

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name="cId",   HeaderText="ID",      FillWeight=10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name="cNome", HeaderText="Produto",  FillWeight=44 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name="cQtd",  HeaderText="Qtd",      FillWeight=10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name="cUnit", HeaderText="Unit.",    FillWeight=18 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name="cVal",  HeaderText="Total",    FillWeight=18 });
            _grid.Columns.Add(new DataGridViewButtonColumn  {
                Name="cRem", HeaderText="", Text="✕",
                UseColumnTextForButtonValue=true, FillWeight=8, FlatStyle=FlatStyle.Flat });

            _grid.CellClick += Grid_CellClick;
            p.Controls.Add(_grid);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  COLUNA DIREITA — totais e ações
        // ══════════════════════════════════════════════════════════════════════
        private void ConstruirColunaDireita(Panel p)
        {
            // Usamos TableLayoutPanel vertical: tudo empilhado
            var tbl = new TableLayoutPanel {
                Dock=DockStyle.Fill, ColumnCount=1, RowCount=8,
                CellBorderStyle=TableLayoutPanelCellBorderStyle.None,
                Padding=Padding.Empty, Margin=Padding.Empty,
                BackColor=FundoForm };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // label "Total de venda"
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // caixa total valor
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // label "Total produtos"
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // caixa total qtd
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent,  100)); // espaço flexível
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 4));   // gap
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // btn Finalizar
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // btn Cancelar

            tbl.Controls.Add(LblDireita("Total de venda"),    0, 0);

            var boxTotal = new Panel { Dock=DockStyle.Fill, BackColor=FundoBranco };
            boxTotal.Paint += (s,e) => {
                using var pen=new System.Drawing.Pen(Color.FromArgb(180,180,180),1);
                e.Graphics.DrawRectangle(pen,0,0,boxTotal.Width-1,boxTotal.Height-1); };
            _lblTotalValor = new Label {
                Dock=DockStyle.Fill, Text="0,00",
                Font=new Font("Segoe UI",26f,FontStyle.Bold), ForeColor=TextoEscuro,
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent };
            boxTotal.Controls.Add(_lblTotalValor);
            tbl.Controls.Add(boxTotal, 0, 1);

            tbl.Controls.Add(LblDireita("Total de produtos"), 0, 2);

            var boxQtd = new Panel { Dock=DockStyle.Fill, BackColor=FundoBranco };
            boxQtd.Paint += (s,e) => {
                using var pen=new System.Drawing.Pen(Color.FromArgb(180,180,180),1);
                e.Graphics.DrawRectangle(pen,0,0,boxQtd.Width-1,boxQtd.Height-1); };
            _lblTotalQtd = new Label {
                Dock=DockStyle.Fill, Text="0",
                Font=new Font("Segoe UI",18f,FontStyle.Bold), ForeColor=TextoEscuro,
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent };
            boxQtd.Controls.Add(_lblTotalQtd);
            tbl.Controls.Add(boxQtd, 0, 3);

            tbl.Controls.Add(new Panel { BackColor=Color.Transparent }, 0, 4); // espaço

            tbl.Controls.Add(new Panel { BackColor=Color.Transparent }, 0, 5); // gap

            _btnFinalizar = new Button {
                Dock=DockStyle.Fill, Text="Finalizar",
                FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(45,140,65),
                ForeColor=Color.White, Font=new Font("Segoe UI",13f,FontStyle.Bold),
                Cursor=Cursors.Hand };
            _btnFinalizar.FlatAppearance.BorderSize=0;
            _btnFinalizar.MouseEnter += (s,e) => _btnFinalizar.BackColor=Verde;
            _btnFinalizar.MouseLeave += (s,e) => _btnFinalizar.BackColor=Color.FromArgb(45,140,65);
            _btnFinalizar.Click += BtnFinalizar_Click;
            tbl.Controls.Add(_btnFinalizar, 0, 6);

            _btnCancelar = new Button {
                Dock=DockStyle.Fill, Text="Cancelar",
                FlatStyle=FlatStyle.Flat, BackColor=Vermelho,
                ForeColor=Color.White, Font=new Font("Segoe UI",13f,FontStyle.Bold),
                Cursor=Cursors.Hand };
            _btnCancelar.FlatAppearance.BorderSize=0;
            _btnCancelar.MouseEnter += (s,e) => _btnCancelar.BackColor=Color.FromArgb(210,55,55);
            _btnCancelar.MouseLeave += (s,e) => _btnCancelar.BackColor=Vermelho;
            _btnCancelar.Click += BtnCancelar_Click;
            tbl.Controls.Add(_btnCancelar, 0, 7);

            p.Controls.Add(tbl);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS DE UI
        // ══════════════════════════════════════════════════════════════════════
        private Label SecLabel(string t) => new Label {
            Text=t, AutoSize=false, Width=280, Height=18,
            Font=new Font("Segoe UI",8.5f,FontStyle.Bold), ForeColor=TextoEscuro,
            BackColor=Color.Transparent,
            Margin=new Padding(0,6,0,2) };

        private Label LblDireita(string t) => new Label {
            Text=t, Dock=DockStyle.Fill,
            Font=new Font("Segoe UI",10f,FontStyle.Bold), ForeColor=TextoEscuro,
            TextAlign=ContentAlignment.BottomLeft, BackColor=Color.Transparent };

        private TextBox Txt(string ph, int x, int y, int w) => new TextBox {
            PlaceholderText=ph, Location=new Point(x,y), Size=new Size(w,26),
            Font=new Font("Segoe UI",9f), BackColor=FundoBranco,
            BorderStyle=BorderStyle.FixedSingle };

        private Button Lupa(int x, int y) {
            var b=new Button { Text="🔍", Location=new Point(x,y), Size=new Size(30,26),
                FlatStyle=FlatStyle.Flat, BackColor=VerdeDark, ForeColor=Color.White,
                Font=new Font("Segoe UI Emoji",10f), Cursor=Cursors.Hand };
            b.FlatAppearance.BorderSize=0;
            b.MouseEnter += (s,e) => b.BackColor=Verde;
            b.MouseLeave += (s,e) => b.BackColor=VerdeDark;
            return b; }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA — PRODUTO
        // ══════════════════════════════════════════════════════════════════════
        private void BuscarPorCodigo()
        {
            if (!int.TryParse(_txtCodigo.Text.Trim(), out int id)) return;
            var prod = ProdutoService.BuscarPorId(id);
            if (prod == null) { MostrarAviso("Produto nao encontrado."); return; }
            if (!prod.Ativo)
            {
                MostrarAviso($"Produto '{prod.Nome}' esta inativo.");
                _lblNomeProduto.Text = $"[INATIVO] {prod.Nome}";
                _lblNomeProduto.ForeColor = Color.FromArgb(200,60,60);
                return;
            }
            _lblNomeProduto.ForeColor = TextoCinza;
            PreencherProduto(prod);
        }

        private void BtnLupaProduto_Click(object sender, EventArgs e)
        {
            using var modal = new ModalProdutos();
            if (modal.ShowDialog(this) == DialogResult.OK && modal.ProdutoSelecionado != null)
                PreencherProduto(modal.ProdutoSelecionado);
        }

        private void PreencherProduto(Produto prod)
        {
            _produtoAtual       = prod;
            _txtCodigo.Text     = prod.Id.ToString();
            _lblNomeProduto.Text= prod.Nome;
            _txtValorUnit.Text  = $"R$ {prod.Valor:F2}";
            _txtQtd.Text        = "1";
            _txtQtd.Enabled     = true;
            _txtDesconto.Text   = "";
            RecalcularItem(null, null);
            _txtQtd.Focus();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA — RECÁLCULO
        // ══════════════════════════════════════════════════════════════════════
        private void RecalcularItem(object sender, EventArgs e)
        {
            if (_produtoAtual == null) return;
            int.TryParse(_txtQtd.Text.Trim(), out int qtd);
            if (qtd < 1) qtd = 1;
            decimal total = VendaService.CalcularValorItem(
                _produtoAtual.Valor, qtd, _txtDesconto.Text);
            _txtValorTotal.Text = $"R$ {total:F2}";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA — ADICIONAR ITEM
        // ══════════════════════════════════════════════════════════════════════
        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (_produtoAtual == null) { MostrarAviso("Selecione um produto primeiro."); return; }
            int.TryParse(_txtQtd.Text.Trim(), out int qtd);
            if (qtd < 1) qtd = 1;

            decimal total = VendaService.CalcularValorItem(
                _produtoAtual.Valor, qtd, _txtDesconto.Text);

            var existente = _itens.Find(i => i.ProdutoId == _produtoAtual.Id);
            if (existente != null)
            {
                existente.Quantidade += qtd;
                existente.Desconto   += _produtoAtual.Valor * qtd - total;
            }
            else
            {
                _itens.Add(new ItemVenda {
                    ProdutoId   = _produtoAtual.Id,
                    NomeProduto = _produtoAtual.Nome,
                    Quantidade  = qtd,
                    ValorUnit   = _produtoAtual.Valor,
                    Desconto    = _produtoAtual.Valor * qtd - total });
            }
            AtualizarGridETotais();
            LimparEntrada();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA — ALUNO
        // ══════════════════════════════════════════════════════════════════════
        private void BtnLupaAluno_Click(object sender, EventArgs e)
        {
            using var modal = new ModalAlunos();
            if (modal.ShowDialog(this) == DialogResult.OK && modal.AlunoSelecionado != null)
            {
                _alunoAtual    = modal.AlunoSelecionado;
                _txtAluno.Text = $"{_alunoAtual.Nome} (ID {_alunoAtual.Id})";
                LimparDestaque(_pAlunoBox);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA — GRID
        // ══════════════════════════════════════════════════════════════════════
        private void AtualizarGridETotais()
        {
            _grid.Rows.Clear();
            decimal totalVenda = 0;
            foreach (var item in _itens)
            {
                totalVenda += item.ValorTotal;
                _grid.Rows.Add(
                    item.ProdutoId, item.NomeProduto,
                    $"{item.Quantidade}x",
                    $"R$ {item.ValorUnit:F2}",
                    $"R$ {item.ValorTotal:F2}", "✕");
            }
            _lblTotalValor.Text = totalVenda.ToString("F2").Replace(".",",");
            _lblTotalQtd.Text   = _itens.Count.ToString();
        }

        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "cRem") return;
            _itens.RemoveAt(e.RowIndex);
            AtualizarGridETotais();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA — FINALIZAR
        // ══════════════════════════════════════════════════════════════════════
        private void BtnFinalizar_Click(object sender, EventArgs e)
        {
            LimparDestaque(_pAlunoBox);
            LimparDestaque(_pPagtoBox);

            string formaPgto = _cmbPagamento.SelectedIndex > 0
                ? _cmbPagamento.SelectedItem.ToString() : "";
            string nomeAluno = _alunoAtual?.Nome ?? "";

            var resultado = VendaService.Validar(_itens, nomeAluno, formaPgto);
            if (!resultado.Valido)
            {
                if (resultado.Campo == "aluno")    DestacarPainel(_pAlunoBox);
                if (resultado.Campo == "pagamento") DestacarPainel(_pPagtoBox);
                MostrarAviso(resultado.Mensagem);
                return;
            }

            decimal totalVenda = 0;
            foreach (var i in _itens) totalVenda += i.ValorTotal;

            var conf = MessageBox.Show(
                $"Confirmar venda?\n\nAluno: {nomeAluno}\nPagamento: {formaPgto}\nTotal: R$ {totalVenda:F2}",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (conf != DialogResult.Yes) return;

            try
            {
                VendaService.Finalizar(_itens, nomeAluno, formaPgto);
                MessageBox.Show($"Venda finalizada!\nTotal: R$ {totalVenda:F2}",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimparVenda();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA — CANCELAR
        // ══════════════════════════════════════════════════════════════════════
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            using var modal = new ConfirmacaoModal(
                "Voce tem certeza que gostaria de cancelar a venda?");
            modal.StartPosition = FormStartPosition.CenterParent;
            modal.ShowDialog(this);
            if (!modal.Confirmado) return;
            LimparVenda();
            CancelouVenda = true;
            DialogResult  = DialogResult.Cancel;
            Close();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UTILITÁRIOS
        // ══════════════════════════════════════════════════════════════════════
        private void LimparEntrada()
        {
            _produtoAtual        = null;
            _txtCodigo.Text      = "";
            _lblNomeProduto.Text = "—";
            _lblNomeProduto.ForeColor = TextoCinza;
            _txtValorUnit.Text   = "";
            _txtQtd.Text         = "";
            _txtDesconto.Text    = "";
            _txtValorTotal.Text  = "Valor total do produto";
            _txtCodigo.Focus();
        }

        private void LimparVenda()
        {
            _itens.Clear();
            _alunoAtual             = null;
            _txtAluno.Text          = "";
            _cmbPagamento.SelectedIndex = 0;
            AtualizarGridETotais();
            LimparEntrada();
        }

        private static void DestacarPainel(Panel p) =>
            p.BackColor = Color.FromArgb(255, 235, 235);

        private static void LimparDestaque(Panel p) =>
            p.BackColor = Color.Transparent;

        private void MostrarAviso(string msg) =>
            MessageBox.Show(msg, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void AplicarTema() => TemaAplicador.AplicarEm(this);
    }
}
