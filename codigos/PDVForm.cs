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
        private static readonly Color Verde       = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeDark   = Color.FromArgb(38, 120, 56);
        private static readonly Color Vermelho    = Color.FromArgb(180, 40, 40);
        // Cores dinâmicas via TemaService (não fixas)
        private static Color FundoForm   => TemaService.FundoForm;
        private static Color FundoBranco => TemaService.FundoPainel;
        private static readonly Color TextoEscuro = Color.FromArgb(40, 40, 40);
        private static readonly Color TextoCinza  = Color.FromArgb(100, 100, 100);

        private readonly List<ItemVenda> _itens = new();
        private Produto  _produtoAtual = null;
        private Aluno    _alunoAtual   = null;
        public  bool     CancelouVenda { get; private set; } = false;

        private TextBox  _txtCodigo, _txtQtd, _txtValorUnit, _txtDesconto, _txtValorTotal;
        private TextBox  _txtAluno;
        private Label    _lblNomeProduto;
        private Button   _btnLupaProduto, _btnLupaAluno, _btnAddItem;
        private ComboBox _cmbPagamento;
        private Panel    _pAlunoBox, _pPagtoBox;
        private DataGridView _grid;
        private Label    _lblTotalValor, _lblTotalQtd;
        private Button   _btnFinalizar, _btnCancelar;

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

        private void InitUI()
        {
            Text            = "PDV - Ponto de Venda";
            Size            = new Size(1000, 640);
            MinimumSize     = new Size(820, 520);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = FundoForm;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;

            // ══════════════════════════════════════════════════════════════════
            // TableLayoutPanel RAIZ: 3 linhas × 3 colunas
            //
            //  ┌─────────────────────────────────────────────────────────────┐
            //  │  Row 0 (54px)  Header verde          [span 3 cols]          │
            //  ├──────────┬──────────────────────┬───────────────────────────┤
            //  │Col0 310px│ Col1 *               │ Col2 230px                │
            //  │ Entrada  │ Grid itens           │ Totais + Botões           │
            //  │          │                      │                           │
            //  ├──────────┴──────────────────────┴───────────────────────────┤
            //  │  Row 2 (48px)  Rodapé verde          [span 3 cols]          │
            //  └─────────────────────────────────────────────────────────────┘
            // ══════════════════════════════════════════════════════════════════

            var root = new TableLayoutPanel
            {
                Dock=DockStyle.Fill, ColumnCount=3, RowCount=3,
                CellBorderStyle=TableLayoutPanelCellBorderStyle.None,
                Padding=Padding.Empty, Margin=Padding.Empty,
                BackColor=FundoForm
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // header verde
            root.RowStyles.Add(new RowStyle(SizeType.Percent,  100));  // conteúdo
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));   // rodapé verde

            // ── Row 0: Header verde ───────────────────────────────────────────
            var header = new Panel { Dock=DockStyle.Fill, BackColor=Verde };
            header.Controls.Add(new Label {
                Text="🏛", Font=new Font("Segoe UI Emoji",18f), ForeColor=Color.White,
                AutoSize=false, Size=new Size(50,50), Location=new Point(6,2),
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent });
            header.Controls.Add(new Label {
                Text="PDV - Ponto de Venda",
                Font=new Font("Segoe UI",13f,FontStyle.Bold), ForeColor=Color.White,
                AutoSize=true, Location=new Point(62,16), BackColor=Color.Transparent });
            root.Controls.Add(header, 0, 0);
            root.SetColumnSpan(header, 3);

            // ── Row 1, Col 0: Painel de entrada ──────────────────────────────
            var pEsq = new Panel {
                Dock=DockStyle.Fill, BackColor=FundoBranco,
                Padding=new Padding(10,10,8,8) };
            MontarColunaEsquerda(pEsq);
            root.Controls.Add(pEsq, 0, 1);

            // ── Row 1, Col 1: Grid de itens ───────────────────────────────────
            var pCentro = new Panel { Dock=DockStyle.Fill, BackColor=FundoBranco };
            MontarColunaCentral(pCentro);
            root.Controls.Add(pCentro, 1, 1);

            // ── Row 1, Col 2: Totais e ações ──────────────────────────────────
            var pDir = new Panel {
                Dock=DockStyle.Fill, BackColor=FundoForm,
                Padding=new Padding(8) };
            MontarColunaDireita(pDir);
            root.Controls.Add(pDir, 2, 1);

            // ── Row 2: Rodapé verde ───────────────────────────────────────────
            var rodape = new Panel { Dock=DockStyle.Fill, BackColor=Verde };
            root.Controls.Add(rodape, 0, 2);
            root.SetColumnSpan(rodape, 3);

            Controls.Add(root);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  COLUNA CENTRAL — título + DataGridView
        // ══════════════════════════════════════════════════════════════════════

        // ══════════════════════════════════════════════════════════════════════
        //  COLUNA ESQUERDA — coordenadas absolutas, responsivas via Resize
        // ══════════════════════════════════════════════════════════════════════
        private void MontarColunaEsquerda(Panel p)
        {
            int cy = 8;

            // ── Produto ───────────────────────────────────────────────────────
            p.Controls.Add(RotuloAbs("Produto", 0, cy, 270)); cy += 20;

            _txtCodigo = TxtAbs("Codigo do produto", 0, cy, 234);
            _txtCodigo.KeyDown  += (s,e) => { if(e.KeyCode==Keys.Enter) BuscarPorCodigo(); };
            _txtCodigo.Leave    += (s,e) => { if(!string.IsNullOrWhiteSpace(_txtCodigo.Text)) BuscarPorCodigo(); };
            _btnLupaProduto = BotaoLupa(238, cy);
            _btnLupaProduto.Click += BtnLupaProduto_Click;
            p.Controls.AddRange(new Control[]{ _txtCodigo, _btnLupaProduto }); cy += 32;

            _lblNomeProduto = new Label {
                Text="—", Location=new Point(0, cy), Size=new Size(270, 16),
                Font=new Font("Segoe UI",8.5f,FontStyle.Italic),
                ForeColor=TextoCinza, BackColor=Color.Transparent };
            p.Controls.Add(_lblNomeProduto); cy += 22;

            // ── Quantidade / Valor ────────────────────────────────────────────
            p.Controls.Add(RotuloAbs("Quantidade   /   Valor unitário", 0, cy, 270)); cy += 20;

            _txtQtd = TxtAbs("Qtd", 0, cy, 100);
            _txtQtd.KeyPress    += (s,e) => { if(!char.IsDigit(e.KeyChar)&&e.KeyChar!='\b') e.Handled=true; };
            _txtQtd.TextChanged += RecalcularItem;
            _txtValorUnit = TxtAbs("", 108, cy, 162);
            _txtValorUnit.ReadOnly  = true;
            _txtValorUnit.BackColor = TemaService.FundoInputRO;
            p.Controls.AddRange(new Control[]{ _txtQtd, _txtValorUnit }); cy += 34;

            // ── Desconto ──────────────────────────────────────────────────────
            p.Controls.Add(RotuloAbs("Desconto (ex: 10% ou 1.50)", 0, cy, 270)); cy += 20;
            _txtDesconto = TxtAbs("Desconto", 0, cy, 270);
            _txtDesconto.TextChanged += RecalcularItem;
            p.Controls.Add(_txtDesconto); cy += 32;

            _txtValorTotal = new TextBox {
                Location=new Point(0, cy), Size=new Size(270, 28), ReadOnly=true, TabStop=false,
                Font=new Font("Segoe UI",9f), BackColor=TemaService.FundoInputRO,
                TextAlign=HorizontalAlignment.Center, Text="Valor total do produto",
                BorderStyle=BorderStyle.FixedSingle };
            p.Controls.Add(_txtValorTotal); cy += 36;

            _btnAddItem = new Button {
                Text="+ Adicionar Item", Location=new Point(0, cy), Size=new Size(270, 32),
                FlatStyle=FlatStyle.Flat, BackColor=VerdeDark, ForeColor=Color.White,
                Font=new Font("Segoe UI",9f,FontStyle.Bold), Cursor=Cursors.Hand };
            _btnAddItem.FlatAppearance.BorderSize=0;
            _btnAddItem.MouseEnter += (s,e) => _btnAddItem.BackColor=Verde;
            _btnAddItem.MouseLeave += (s,e) => _btnAddItem.BackColor=VerdeDark;
            _btnAddItem.Click += BtnAddItem_Click;
            p.Controls.Add(_btnAddItem); cy += 44;

            p.Controls.Add(new Panel { Location=new Point(0,cy), Size=new Size(270,1),
                BackColor=Color.FromArgb(200,200,200) }); cy += 14;

            // ── Aluno ─────────────────────────────────────────────────────────
            _pAlunoBox = new Panel { Location=new Point(0,cy), Size=new Size(270,54),
                BackColor=Color.Transparent };
            _pAlunoBox.Controls.Add(RotuloAbs("Aluno", 0, 0, 270));
            _txtAluno = TxtAbs("Selecionar aluno...", 0, 20, 234);
            _txtAluno.ReadOnly=true; _txtAluno.Cursor=Cursors.Hand;
            _txtAluno.Click += BtnLupaAluno_Click;
            _btnLupaAluno = BotaoLupa(238, 20);
            _btnLupaAluno.Click += BtnLupaAluno_Click;
            _pAlunoBox.Controls.AddRange(new Control[]{ _txtAluno, _btnLupaAluno });
            p.Controls.Add(_pAlunoBox); cy += 62;

            // ── Forma de Pagamento ────────────────────────────────────────────
            _pPagtoBox = new Panel { Location=new Point(0,cy), Size=new Size(270,54),
                BackColor=Color.Transparent };
            _pPagtoBox.Controls.Add(RotuloAbs("Forma de pagamento", 0, 0, 270));
            _cmbPagamento = new ComboBox {
                Location=new Point(0,20), Size=new Size(270,28),
                DropDownStyle=ComboBoxStyle.DropDownList,
                Font=new Font("Segoe UI",9f), FlatStyle=FlatStyle.Flat };
            _cmbPagamento.Items.Add("Selecione");
            foreach(var f in VendaService.FormasPagamento) _cmbPagamento.Items.Add(f);
            _cmbPagamento.SelectedIndex=0;
            _cmbPagamento.SelectedIndexChanged += (s,e) => LimparDestaque(_pPagtoBox);
            _pPagtoBox.Controls.Add(_cmbPagamento);
            p.Controls.Add(_pPagtoBox);
        }

        private static Label RotuloAbs(string t, int x, int y, int w) => new Label {
            Text=t, Location=new Point(x,y), Size=new Size(w,18), AutoSize=false,
            Font=new Font("Segoe UI",8.5f,FontStyle.Bold), ForeColor=Color.FromArgb(40,40,40),
            BackColor=Color.Transparent };

        private static TextBox TxtAbs(string ph, int x, int y, int w) => new TextBox {
            PlaceholderText=ph, Location=new Point(x,y), Size=new Size(w,26),
            Font=new Font("Segoe UI",9f), BackColor=TemaService.FundoInput,
            BorderStyle=BorderStyle.FixedSingle };

        private void MontarColunaCentral(Panel p)
        {
            var lblTitulo = new Label {
                Text="Produtos", Dock=DockStyle.Top, Height=36,
                Font=new Font("Segoe UI",12f,FontStyle.Bold), ForeColor=TextoEscuro,
                TextAlign=ContentAlignment.MiddleLeft,
                Padding=new Padding(8,0,0,0), BackColor=Color.Transparent };
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
            _grid.ColumnHeadersDefaultCellStyle.BackColor = TemaService.GridCabecalho;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = TextoEscuro;
            _grid.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI",9f,FontStyle.Bold);
            _grid.DefaultCellStyle.BackColor = TemaService.FundoPainel;
            _grid.DefaultCellStyle.ForeColor = TextoEscuro;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(180,220,180);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.AlternatingRowsDefaultCellStyle.BackColor = TemaService.GridAlternada;

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
        //  COLUNA DIREITA — TableLayoutPanel com totais e botões
        // ══════════════════════════════════════════════════════════════════════
        private void MontarColunaDireita(Panel p)
        {
            var tbl = new TableLayoutPanel {
                Dock=DockStyle.Fill, ColumnCount=1, RowCount=8,
                CellBorderStyle=TableLayoutPanelCellBorderStyle.None,
                Padding=Padding.Empty, Margin=Padding.Empty, BackColor=FundoForm };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent,  100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 4));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            tbl.Controls.Add(LblDir("Total de venda"), 0, 0);

            var boxTotal = new Panel { Dock=DockStyle.Fill, BackColor=FundoBranco };
            boxTotal.Paint += (s,e) => {
                using var pen = new System.Drawing.Pen(Color.FromArgb(180,180,180),1);
                e.Graphics.DrawRectangle(pen,0,0,boxTotal.Width-1,boxTotal.Height-1); };
            _lblTotalValor = new Label {
                Dock=DockStyle.Fill, Text="0,00",
                Font=new Font("Segoe UI",26f,FontStyle.Bold), ForeColor=TextoEscuro,
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent };
            boxTotal.Controls.Add(_lblTotalValor);
            tbl.Controls.Add(boxTotal, 0, 1);

            tbl.Controls.Add(LblDir("Total de produtos"), 0, 2);

            var boxQtd = new Panel { Dock=DockStyle.Fill, BackColor=FundoBranco };
            boxQtd.Paint += (s,e) => {
                using var pen = new System.Drawing.Pen(Color.FromArgb(180,180,180),1);
                e.Graphics.DrawRectangle(pen,0,0,boxQtd.Width-1,boxQtd.Height-1); };
            _lblTotalQtd = new Label {
                Dock=DockStyle.Fill, Text="0",
                Font=new Font("Segoe UI",18f,FontStyle.Bold), ForeColor=TextoEscuro,
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent };
            boxQtd.Controls.Add(_lblTotalQtd);
            tbl.Controls.Add(boxQtd, 0, 3);

            tbl.Controls.Add(new Panel { BackColor=Color.Transparent }, 0, 4);
            tbl.Controls.Add(new Panel { BackColor=Color.Transparent }, 0, 5);

            _btnFinalizar = BotaoAcao("Finalizar", Color.FromArgb(45,140,65));
            _btnFinalizar.MouseEnter += (s,e) => _btnFinalizar.BackColor=Verde;
            _btnFinalizar.MouseLeave += (s,e) => _btnFinalizar.BackColor=Color.FromArgb(45,140,65);
            _btnFinalizar.Click += BtnFinalizar_Click;
            tbl.Controls.Add(_btnFinalizar, 0, 6);

            _btnCancelar = BotaoAcao("Cancelar", Vermelho);
            _btnCancelar.MouseEnter += (s,e) => _btnCancelar.BackColor=Color.FromArgb(210,55,55);
            _btnCancelar.MouseLeave += (s,e) => _btnCancelar.BackColor=Vermelho;
            _btnCancelar.Click += BtnCancelar_Click;
            tbl.Controls.Add(_btnCancelar, 0, 7);

            p.Controls.Add(tbl);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS DE UI
        // ══════════════════════════════════════════════════════════════════════
        private static Label LblDir(string t) => new Label {
            Dock=DockStyle.Fill, Text=t,
            Font=new Font("Segoe UI",10f,FontStyle.Bold), ForeColor=Color.FromArgb(40,40,40),
            TextAlign=ContentAlignment.BottomLeft, BackColor=Color.Transparent };



        private static Button BotaoLupa(int x, int y) {
            var b = new Button { Text="🔍", Location=new Point(x,y), Size=new Size(30,26),
                FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(38,120,56),
                ForeColor=Color.White, Font=new Font("Segoe UI Emoji",10f), Cursor=Cursors.Hand };
            b.FlatAppearance.BorderSize=0;
            b.MouseEnter += (s,e) => b.BackColor=Color.FromArgb(56,161,78);
            b.MouseLeave += (s,e) => b.BackColor=Color.FromArgb(38,120,56);
            return b; }

        private static Button BotaoAcao(string t, Color cor) {
            var b = new Button { Dock=DockStyle.Fill, Text=t,
                FlatStyle=FlatStyle.Flat, BackColor=cor, ForeColor=Color.White,
                Font=new Font("Segoe UI",13f,FontStyle.Bold), Cursor=Cursors.Hand };
            b.FlatAppearance.BorderSize=0;
            return b; }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA
        // ══════════════════════════════════════════════════════════════════════
        private void BuscarPorCodigo()
        {
            if (!int.TryParse(_txtCodigo.Text.Trim(), out int id)) return;
            var prod = ProdutoService.BuscarPorId(id);
            if (prod == null) { MostrarAviso("Produto nao encontrado."); return; }
            if (!prod.Ativo) {
                MostrarAviso($"Produto '{prod.Nome}' esta inativo.");
                _lblNomeProduto.Text = $"[INATIVO] {prod.Nome}";
                _lblNomeProduto.ForeColor = Color.FromArgb(200,60,60); return; }
            _lblNomeProduto.ForeColor = TextoCinza;
            PreencherProduto(prod);
        }

        private void BtnLupaProduto_Click(object sender, EventArgs e)
        {
            using var modal = new ModalProdutos();
            if (modal.ShowDialog(this)==DialogResult.OK && modal.ProdutoSelecionado!=null)
                PreencherProduto(modal.ProdutoSelecionado);
        }

        private void PreencherProduto(Produto prod)
        {
            _produtoAtual        = prod;
            _txtCodigo.Text      = prod.Id.ToString();
            _lblNomeProduto.Text = prod.Nome;
            _txtValorUnit.Text   = $"R$ {prod.Valor:F2}";
            _txtQtd.Text         = "1";
            _txtQtd.Enabled      = true;
            _txtDesconto.Text    = "";
            RecalcularItem(null, null);
            _txtQtd.Focus();
        }

        private void RecalcularItem(object sender, EventArgs e)
        {
            if (_produtoAtual == null) return;
            int.TryParse(_txtQtd.Text.Trim(), out int qtd);
            if (qtd < 1) qtd = 1;
            decimal total = VendaService.CalcularValorItem(_produtoAtual.Valor, qtd, _txtDesconto.Text);
            _txtValorTotal.Text = $"R$ {total:F2}";
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (_produtoAtual == null) { MostrarAviso("Selecione um produto primeiro."); return; }
            int.TryParse(_txtQtd.Text.Trim(), out int qtd);
            if (qtd < 1) qtd = 1;
            decimal total = VendaService.CalcularValorItem(_produtoAtual.Valor, qtd, _txtDesconto.Text);
            var existente = _itens.Find(i => i.ProdutoId==_produtoAtual.Id);
            if (existente != null) {
                existente.Quantidade += qtd;
                existente.Desconto   += _produtoAtual.Valor*qtd - total;
            } else {
                _itens.Add(new ItemVenda {
                    ProdutoId=_produtoAtual.Id, NomeProduto=_produtoAtual.Nome,
                    Quantidade=qtd, ValorUnit=_produtoAtual.Valor,
                    Desconto=_produtoAtual.Valor*qtd - total }); }
            AtualizarGridETotais();
            LimparEntrada();
        }

        private void BtnLupaAluno_Click(object sender, EventArgs e)
        {
            using var modal = new ModalAlunos();
            if (modal.ShowDialog(this)==DialogResult.OK && modal.AlunoSelecionado!=null)
            {
                _alunoAtual    = modal.AlunoSelecionado;
                _txtAluno.Text = $"{_alunoAtual.Nome} (ID {_alunoAtual.Id})";
                LimparDestaque(_pAlunoBox);
            }
        }

        private void AtualizarGridETotais()
        {
            _grid.Rows.Clear();
            decimal totalVenda = 0;
            foreach (var item in _itens)
            {
                totalVenda += item.ValorTotal;
                _grid.Rows.Add(item.ProdutoId, item.NomeProduto,
                    $"{item.Quantidade}x", $"R$ {item.ValorUnit:F2}",
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

        private void BtnFinalizar_Click(object sender, EventArgs e)
        {
            LimparDestaque(_pAlunoBox); LimparDestaque(_pPagtoBox);
            string formaPgto = _cmbPagamento.SelectedIndex > 0
                ? _cmbPagamento.SelectedItem.ToString() : "";
            string nomeAluno = _alunoAtual?.Nome ?? "";
            var res = VendaService.Validar(_itens, nomeAluno, formaPgto);
            if (!res.Valido) {
                if (res.Campo=="aluno")     DestacarPainel(_pAlunoBox);
                if (res.Campo=="pagamento") DestacarPainel(_pPagtoBox);
                MostrarAviso(res.Mensagem); return; }
            decimal tot = 0; foreach(var i in _itens) tot += i.ValorTotal;
            if (MessageBox.Show(
                $"Confirmar venda?\n\nAluno: {nomeAluno}\nPagamento: {formaPgto}\nTotal: R$ {tot:F2}",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question)!=DialogResult.Yes) return;
            try {
                VendaService.Finalizar(_itens, nomeAluno, formaPgto);
                MessageBox.Show($"Venda finalizada!\nTotal: R$ {tot:F2}",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimparVenda();
            } catch(Exception ex) {
                MessageBox.Show($"Erro: {ex.Message}","Erro",MessageBoxButtons.OK,MessageBoxIcon.Error); }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            using var modal = new ConfirmacaoModal("Voce tem certeza que gostaria de cancelar a venda?");
            modal.StartPosition = FormStartPosition.CenterParent;
            modal.ShowDialog(this);
            if (!modal.Confirmado) return;
            LimparVenda();
            CancelouVenda = true;
            DialogResult  = DialogResult.Cancel;
            Close();
        }

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
            _itens.Clear(); _alunoAtual=null; _txtAluno.Text="";
            _cmbPagamento.SelectedIndex=0;
            AtualizarGridETotais(); LimparEntrada();
        }

        private static void DestacarPainel(Panel p) => p.BackColor=Color.FromArgb(255,235,235);
        private static void LimparDestaque(Panel p) => p.BackColor=Color.Transparent;
        private void MostrarAviso(string msg) =>
            MessageBox.Show(msg,"Aviso",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        // ══════════════════════════════════════════════════════════════════════
        //  TEMA — atualiza todos os controles em tempo real
        // ══════════════════════════════════════════════════════════════════════
        private void AplicarTema()
        {
            // Aplica tema recursivamente em todos os controles
            TemaAplicador.AplicarEm(this);

            // Controles com cores fixas que precisam de tratamento especial:

            // Grid de itens da venda
            if (_grid != null)
            {
                _grid.BackgroundColor                          = TemaService.FundoPainel;
                _grid.DefaultCellStyle.BackColor               = TemaService.FundoPainel;
                _grid.DefaultCellStyle.ForeColor               = TemaService.TextoPrincipal;
                _grid.DefaultCellStyle.SelectionBackColor      = Color.FromArgb(180, 220, 180);
                _grid.DefaultCellStyle.SelectionForeColor      = TemaService.TextoPrincipal;
                _grid.AlternatingRowsDefaultCellStyle.BackColor= TemaService.GridAlternada;
                _grid.AlternatingRowsDefaultCellStyle.ForeColor= TemaService.TextoPrincipal;
                _grid.ColumnHeadersDefaultCellStyle.BackColor  = TemaService.GridCabecalho;
                _grid.ColumnHeadersDefaultCellStyle.ForeColor  = Color.FromArgb(20, 20, 20);
                _grid.GridColor = TemaService.Borda;
                _grid.Invalidate();
            }

            // Valor unitário (read-only) — cor diferenciada
            if (_txtValorUnit != null)
                _txtValorUnit.BackColor = TemaService.FundoInputRO;

            // Valor total do item (read-only)
            if (_txtValorTotal != null)
                _txtValorTotal.BackColor = TemaService.FundoInputRO;

            // Painel do aluno com destaque de validação deve manter transparência
            if (_pAlunoBox != null && _pAlunoBox.BackColor != Color.FromArgb(255, 235, 235))
                _pAlunoBox.BackColor = Color.Transparent;
            if (_pPagtoBox != null && _pPagtoBox.BackColor != Color.FromArgb(255, 235, 235))
                _pPagtoBox.BackColor = Color.Transparent;
        }


    }
}
