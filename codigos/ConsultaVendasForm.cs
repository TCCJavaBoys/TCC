using System;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Data;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    public class ConsultaVendasForm : Form
    {
        private static readonly Color Verde      = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeBotao = Color.FromArgb(45, 140, 65);
        private static readonly Color Fundo      = Color.FromArgb(245, 248, 245);

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
            Size            = new Size(940, 600);
            MinimumSize     = new Size(680, 440);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;

            // ADD #1 — Top
            var topBar = new Panel { Dock=DockStyle.Top, Height=54, BackColor=Verde };
            topBar.Controls.Add(new Label {
                Text="🏛", Font=new Font("Segoe UI Emoji",18f), ForeColor=Color.White,
                AutoSize=false, Size=new Size(48,48), Location=new Point(6,3),
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent });
            _txtPesquisa = new TextBox {
                Location=new Point(68,14), Size=new Size(280,28),
                Font=new Font("Segoe UI",10f), PlaceholderText="Pesquisa" };
            _btnBuscar = new Button {
                Text="🔍", Location=new Point(354,12), Size=new Size(36,30),
                FlatStyle=FlatStyle.Flat, BackColor=VerdeBotao, ForeColor=Color.White,
                Font=new Font("Segoe UI Emoji",12f), Cursor=Cursors.Hand };
            _btnBuscar.FlatAppearance.BorderSize=0;
            _btnBuscar.Click += (s,e) => CarregarGrid();
            _txtPesquisa.KeyDown += (s,e) => { if(e.KeyCode==Keys.Enter) CarregarGrid(); };
            topBar.Controls.AddRange(new Control[]{ _txtPesquisa, _btnBuscar });
            Controls.Add(topBar);

            // ADD #2 — Bottom
            Controls.Add(new Panel { Dock=DockStyle.Bottom, Height=48, BackColor=Verde });

            // ADD #3 — Left: filtros (antes do Fill!)
            var pFiltros = new Panel { Dock=DockStyle.Left, Width=136, BackColor=Fundo };
            pFiltros.Controls.Add(new Label {
                Text="Filtros", Font=new Font("Segoe UI",9f,FontStyle.Bold),
                AutoSize=true, Location=new Point(8,8), BackColor=Color.Transparent });
            _chkNome   = MakeChk("Nome",   8, 30);
            _chkCodigo = MakeChk("Codigo", 8, 52);
            _chkData   = MakeChk("Data",   8, 74);
            _chkValor  = MakeChk("Valor",  8, 96);
            pFiltros.Controls.AddRange(new Control[]
                { _chkNome, _chkCodigo, _chkData, _chkValor });
            Controls.Add(pFiltros);

            // ADD #4 — Fill: container central (ÚLTIMO!)
            var pCentral = new Panel { Dock = DockStyle.Fill };

            // Barra de ações dentro do central
            var pAcoes = new Panel { Dock=DockStyle.Top, Height=46, BackColor=Fundo };
            pAcoes.Controls.Add(new Label {
                Text="Consulta de venda",
                Font=new Font("Segoe UI",10f,FontStyle.Underline|FontStyle.Bold),
                AutoSize=true, Location=new Point(8,13), BackColor=Color.Transparent });

            _btnPDV = new Button {
                Text="PDV", Location=new Point(200,8), Size=new Size(88,30),
                FlatStyle=FlatStyle.Flat, BackColor=VerdeBotao, ForeColor=Color.White,
                Font=new Font("Segoe UI",9f,FontStyle.Bold), Cursor=Cursors.Hand };
            _btnPDV.FlatAppearance.BorderSize=0;
            _btnPDV.MouseEnter += (s,e) => _btnPDV.BackColor=Lighten(VerdeBotao,20);
            _btnPDV.MouseLeave += (s,e) => _btnPDV.BackColor=VerdeBotao;
            _btnPDV.Click += BtnPDV_Click;

            _btnVoltar = new Button {
                Text="Voltar", Size=new Size(88,30),
                FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(100,100,100),
                ForeColor=Color.White, Font=new Font("Segoe UI",9f,FontStyle.Bold),
                Cursor=Cursors.Hand, Anchor=AnchorStyles.Top|AnchorStyles.Right };
            _btnVoltar.FlatAppearance.BorderSize=0;
            _btnVoltar.Click += (s,e) => Close();
            void ReposicionarVoltar() =>
                _btnVoltar.Location = new Point(pAcoes.Width - 96, 8);
            pAcoes.Resize        += (s,e) => ReposicionarVoltar();
            pAcoes.HandleCreated += (s,e) => ReposicionarVoltar();
            pAcoes.Controls.AddRange(new Control[]{ _btnPDV, _btnVoltar });

            // Grid (Fill — último dentro do central)
            _grid = new DataGridView {
                Dock=DockStyle.Fill,
                ReadOnly=true, AllowUserToAddRows=false, AllowUserToDeleteRows=false,
                SelectionMode=DataGridViewSelectionMode.FullRowSelect, MultiSelect=false,
                BackgroundColor=Color.White, BorderStyle=BorderStyle.None,
                ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight=30, RowHeadersVisible=false,
                Font=new Font("Segoe UI",9f),
                AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,
                GridColor=Color.FromArgb(220,220,220) };
            _grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(200,200,200);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor=Color.FromArgb(20,20,20);
            _grid.ColumnHeadersDefaultCellStyle.Font=new Font("Segoe UI",9f,FontStyle.Bold);
            _grid.EnableHeadersVisualStyles=false;
            _grid.DefaultCellStyle.SelectionBackColor=Color.FromArgb(180,220,180);
            _grid.DefaultCellStyle.SelectionForeColor=Color.Black;
            _grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(240,248,240);

            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colId",     HeaderText="ID",        FillWeight=7,  MinimumWidth=44 });
            _grid.Columns.Add(new DataGridViewButtonColumn {
                Name="colEdit",     HeaderText="",  Text="✏",
                UseColumnTextForButtonValue=true, FillWeight=6, MinimumWidth=32, FlatStyle=FlatStyle.Flat });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colCliente",HeaderText="Cliente",   FillWeight=28, MinimumWidth=80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colValor",  HeaderText="Valor",     FillWeight=14, MinimumWidth=60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colData",   HeaderText="Data/Hora", FillWeight=22, MinimumWidth=100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colPgto",   HeaderText="Pagamento", FillWeight=16, MinimumWidth=70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name="colStatus", HeaderText="Status",    FillWeight=9,  MinimumWidth=50 });

            _grid.CellClick       += Grid_CellClick;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            pCentral.Controls.Add(pAcoes);
            pCentral.Controls.Add(_grid);
            Controls.Add(pCentral);
        }

        private CheckBox MakeChk(string t, int x, int y) => new CheckBox {
            Text=t, Checked=false, AutoSize=true, Location=new Point(x,y),
            Font=new Font("Segoe UI",9f) };

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
                _grid.Rows.Add(v.Id,"✏",v.Cliente,
                    $"R$ {v.ValorTotal:F2}", v.Data, v.FormaPagamento, v.Status);
        }

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

        private void AplicarTema() => TemaAplicador.AplicarEm(this);
        private static Color Lighten(Color c,int a)=>Color.FromArgb(
            Math.Min(c.R+a,255),Math.Min(c.G+a,255),Math.Min(c.B+a,255));
    }
}
