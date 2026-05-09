using System;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms.Modais
{
    public class ModalAlunos : Form
    {
        public Aluno AlunoSelecionado { get; private set; }

        private static readonly Color Verde      = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeBotao = Color.FromArgb(45, 140, 65);

        private TextBox      _txtBusca;
        private ComboBox     _cmbStatus;
        private DataGridView _grid;

        public ModalAlunos()
        {
            InitUI();
            CarregarGrid();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaService.TemaAlterado -= AplicarTema;
            base.OnFormClosed(e);
        }

        private void InitUI()
        {
            Text="Selecionar Aluno"; Size=new Size(700,520);
            MinimumSize=new Size(500,380); StartPosition=FormStartPosition.CenterParent;
            FormBorderStyle=FormBorderStyle.Sizable; MaximizeBox=true; BackColor=TemaService.FundoForm;

            var table=new TableLayoutPanel{
                Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,
                CellBorderStyle=TableLayoutPanelCellBorderStyle.None,
                Padding=Padding.Empty,Margin=Padding.Empty};
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute,50));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute,46));
            table.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute,52));

            var header=new Panel{Dock=DockStyle.Fill,BackColor=Verde};
            header.Controls.Add(new Label{
                Text="Selecionar Aluno",
                Font=new Font("Segoe UI",13f,FontStyle.Bold),ForeColor=Color.White,
                AutoSize=true,Location=new Point(14,12),BackColor=Color.Transparent});
            table.Controls.Add(header,0,0);

            var pFiltros=new Panel{Dock=DockStyle.Fill,BackColor=TemaService.FundoInputRO};
            _txtBusca=new TextBox{
                Location=new Point(8,10),Size=new Size(240,26),
                Font=new Font("Segoe UI",9.5f),PlaceholderText="Buscar aluno..."};
            _txtBusca.TextChanged+=(s,e)=>CarregarGrid();
            var lblExibir=new Label{
                Text="Exibir:",AutoSize=true,Location=new Point(256,13),
                Font=new Font("Segoe UI",9f,FontStyle.Bold),BackColor=Color.Transparent};
            _cmbStatus=new ComboBox{
                Location=new Point(308,10),Size=new Size(160,26),
                DropDownStyle=ComboBoxStyle.DropDownList,Font=new Font("Segoe UI",9f)};
            _cmbStatus.Items.AddRange(new[]{"Somente Ativos","Somente Inativos","Todos"});
            _cmbStatus.SelectedIndex=0;
            _cmbStatus.SelectedIndexChanged+=(s,e)=>CarregarGrid();
            pFiltros.Controls.AddRange(new Control[]{_txtBusca,lblExibir,_cmbStatus});
            table.Controls.Add(pFiltros,0,1);

            _grid=new DataGridView{
                Dock=DockStyle.Fill,ReadOnly=true,
                AllowUserToAddRows=false,AllowUserToDeleteRows=false,
                SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,
                BackgroundColor=TemaService.FundoPainel,BorderStyle=BorderStyle.None,
                ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight=30,ColumnHeadersVisible=true,
                RowHeadersVisible=false,Font=new Font("Segoe UI",9.5f),
                AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,
                GridColor=Color.FromArgb(220,220,220),EnableHeadersVisualStyles=false};
            _grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(200,200,200);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor=Color.FromArgb(20,20,20);
            _grid.ColumnHeadersDefaultCellStyle.Font=new Font("Segoe UI",9.5f,FontStyle.Bold);
            _grid.DefaultCellStyle.BackColor=TemaService.FundoPainel;
            _grid.DefaultCellStyle.ForeColor=Color.FromArgb(20,20,20);
            _grid.DefaultCellStyle.SelectionBackColor=Verde;
            _grid.DefaultCellStyle.SelectionForeColor=Color.White;
            _grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(240,248,240);
            _grid.Columns.Add(new DataGridViewTextBoxColumn{Name="colId",    HeaderText="ID",       FillWeight=8});
            _grid.Columns.Add(new DataGridViewTextBoxColumn{Name="colNome",  HeaderText="Nome",     FillWeight=40});
            _grid.Columns.Add(new DataGridViewTextBoxColumn{Name="colSala",  HeaderText="Sala",     FillWeight=12});
            _grid.Columns.Add(new DataGridViewTextBoxColumn{Name="colTel",   HeaderText="Telefone", FillWeight=24});
            _grid.Columns.Add(new DataGridViewTextBoxColumn{Name="colStatus",HeaderText="Status",   FillWeight=16});
            _grid.CellDoubleClick+=(s,e)=>Selecionar();
            table.Controls.Add(_grid,0,2);

            var pRodape=new Panel{Dock=DockStyle.Fill,BackColor=TemaService.FundoInputRO};
            pRodape.Controls.Add(new Label{
                Text="Duplo clique ou Enter para selecionar",
                Font=new Font("Segoe UI",8f),ForeColor=Color.Gray,
                AutoSize=true,Location=new Point(10,18)});
            var btnSel=MakeBtn("Selecionar",VerdeBotao);
            var btnFec=MakeBtn("Fechar",Color.FromArgb(110,110,110));
            btnSel.Click+=(s,e)=>Selecionar(); btnFec.Click+=(s,e)=>Close();
            void Repos(){btnFec.Location=new Point(pRodape.Width-106,10);
                         btnSel.Location=new Point(pRodape.Width-210,10);}
            pRodape.Resize+=(s,e)=>Repos(); pRodape.HandleCreated+=(s,e)=>Repos();
            pRodape.Controls.AddRange(new Control[]{btnSel,btnFec});
            table.Controls.Add(pRodape,0,3);
            Controls.Add(table);

            KeyPreview=true;
            KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter)Selecionar();
                              if(e.KeyCode==Keys.Escape)Close();};
        }

        private void CarregarGrid()
        {
            var status = _cmbStatus.SelectedIndex switch {
                1 => FiltroStatus.Inativos,
                2 => FiltroStatus.Todos,
                _ => FiltroStatus.Ativos };

            // ── Camada de Serviço ─────────────────────────────────────────────
            var lista = AlunoService.Listar(
                _txtBusca?.Text.Trim() ?? "",
                porNome: true, porCodigo: false,
                ordenarPorNome: true, status: status);

            _grid.Rows.Clear();
            foreach (var a in lista)
            {
                int idx = _grid.Rows.Add(a.Id, a.Nome, a.Sala, a.Telefone,
                    a.Ativo ? "✓ Ativo" : "✗ Inativo");
                if (!a.Ativo) {
                    _grid.Rows[idx].DefaultCellStyle.ForeColor=Color.Gray;
                    _grid.Rows[idx].DefaultCellStyle.Font=new Font("Segoe UI",9.5f,FontStyle.Italic);
                    _grid.Rows[idx].DefaultCellStyle.BackColor=Color.FromArgb(248,248,248); }
            }
        }

        private void Selecionar()
        {
            if (_grid.CurrentRow==null) return;
            int    id   = Convert.ToInt32(_grid.CurrentRow.Cells["colId"].Value);
            string stat = _grid.CurrentRow.Cells["colStatus"].Value?.ToString()??"";
            if (stat.Contains("Inativo")) {
                MessageBox.Show("Este aluno esta inativo.","Aluno Inativo",
                    MessageBoxButtons.OK,MessageBoxIcon.Warning); return; }

            // ── Camada de Serviço ─────────────────────────────────────────────
            AlunoSelecionado = AlunoService.BuscarPorId(id);
            if (AlunoSelecionado!=null) { DialogResult=DialogResult.OK; Close(); }
        }

        private void AplicarTema()
        {
            // Form e painéis
            BackColor = TemaService.FundoForm;
            foreach (Control ctrl in Controls)
                if (ctrl is Panel p) p.BackColor = TemaService.FundoInputRO;

            // Grid
            if (_grid != null)
            {
                _grid.BackgroundColor                           = TemaService.FundoPainel;
                _grid.DefaultCellStyle.BackColor                = TemaService.FundoPainel;
                _grid.DefaultCellStyle.ForeColor                = TemaService.TextoPrincipal;
                _grid.AlternatingRowsDefaultCellStyle.BackColor = TemaService.GridAlternada;
                _grid.AlternatingRowsDefaultCellStyle.ForeColor = TemaService.TextoPrincipal;
                _grid.ColumnHeadersDefaultCellStyle.BackColor   = TemaService.GridCabecalho;
                _grid.ColumnHeadersDefaultCellStyle.ForeColor   = Color.FromArgb(20,20,20);
                _grid.GridColor = TemaService.Borda;
                _grid.Invalidate();
            }

            // Campos de busca e filtros
            if (_txtBusca != null)
            {
                _txtBusca.BackColor = TemaService.FundoInput;
                _txtBusca.ForeColor = TemaService.TextoPrincipal;
            }
        }

        private static Button MakeBtn(string t,Color cor){
            var b=new Button{Text=t,Size=new Size(96,32),FlatStyle=FlatStyle.Flat,
                BackColor=cor,ForeColor=Color.White,
                Font=new Font("Segoe UI",9f,FontStyle.Bold),Cursor=Cursors.Hand};
            b.FlatAppearance.BorderSize=0;
            b.MouseEnter+=(s,e)=>b.BackColor=Lighten(cor,20);
            b.MouseLeave+=(s,e)=>b.BackColor=cor;
            return b;}
        private static Color Lighten(Color c,int a)=>Color.FromArgb(
            Math.Min(c.R+a,255),Math.Min(c.G+a,255),Math.Min(c.B+a,255));
    }
}
