using System;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    public class CadastroProdutoForm : Form
    {
        private static readonly Color Verde      = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeBotao = Color.FromArgb(45, 140, 65);
        private static readonly Color Vermelho   = Color.FromArgb(180, 40, 40);

        private TextBox  _txtId, _txtNome, _txtEstoque, _txtData, _txtValor;
        private CheckBox _chkAtivo;
        private Button   _btnSalvar, _btnVoltar;
        private Panel    _topBar, _content;
        private Produto  _produtoEdicao;

        public CadastroProdutoForm(Produto produtoEdicao = null)
        {
            _produtoEdicao = produtoEdicao;
            InitUI();
            PreencherIdAutomatico();
            if (_produtoEdicao != null) PreencherCampos();
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
            Text="Cadastro de Produto"; Size=new Size(560,480);
            MinimumSize=new Size(500,440); FormBorderStyle=FormBorderStyle.Sizable;
            MaximizeBox=true; StartPosition=FormStartPosition.CenterParent; KeyPreview=true;

            _topBar=new Panel{Dock=DockStyle.Top,Height=54,BackColor=Verde};
            _topBar.Controls.Add(LogoHelper.CriarPictureBox(8, 0, 44, 54));

            _content=new Panel{
                Location=new Point(20,70),Size=new Size(504,330),
                BorderStyle=BorderStyle.FixedSingle,
                Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom};
            Controls.Add(_content);

            _content.Controls.Add(new Label{
                Text="Cadastro Produto",
                Font=new Font("Segoe UI",10f,FontStyle.Underline|FontStyle.Bold),
                AutoSize=true,Location=new Point(10,10),BackColor=Color.Transparent});

            _content.Controls.Add(MakeLbl("ID",10,42));
            _txtId=MakeTxt(10,60,90,true); _txtId.TabStop=false;
            _content.Controls.Add(_txtId);

            _chkAtivo=new CheckBox{
                Text="Ativo",Checked=true,AutoSize=true,Location=new Point(115,63),
                Font=new Font("Segoe UI",9f,FontStyle.Bold),ForeColor=Color.FromArgb(34,130,55)};
            _chkAtivo.CheckedChanged+=(s,e)=>
                _chkAtivo.ForeColor=_chkAtivo.Checked?Color.FromArgb(34,130,55):Vermelho;
            _content.Controls.Add(_chkAtivo);

            _content.Controls.Add(MakeLbl("Data",330,42));
            _txtData=MakeTxt(330,60,154,false);
            _txtData.Text=DateTime.Now.ToString("dd/MM/yyyy");
            _txtData.TabIndex=4; _txtData.Anchor=AnchorStyles.Top|AnchorStyles.Right;
            _content.Controls.Add(_txtData);

            _content.Controls.Add(MakeLbl("Nome",10,100));
            _txtNome=MakeTxt(10,118,474,false); _txtNome.TabIndex=1;
            _txtNome.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
            _content.Controls.Add(_txtNome);

            _content.Controls.Add(MakeLbl("Valor (R$)",330,154));
            _txtValor=MakeTxt(330,172,154,false); _txtValor.TabIndex=3;
            _txtValor.Anchor=AnchorStyles.Top|AnchorStyles.Right;
            _txtValor.KeyPress+=(s,e)=>{
                if(!char.IsDigit(e.KeyChar)&&e.KeyChar!=','&&e.KeyChar!='.'&&e.KeyChar!='\b')
                    e.Handled=true;};
            _content.Controls.Add(_txtValor);

            _content.Controls.Add(MakeLbl("Estoque (qtd)",10,154));
            _txtEstoque=MakeTxt(10,172,210,false); _txtEstoque.TabIndex=2;
            _txtEstoque.KeyPress+=(s,e)=>{
                if(!char.IsDigit(e.KeyChar)&&e.KeyChar!='\b') e.Handled=true;};
            _content.Controls.Add(_txtEstoque);

            _btnSalvar=MakeBtn("Salvar",330,416,VerdeBotao);
            _btnSalvar.TabIndex=5; _btnSalvar.Anchor=AnchorStyles.Bottom|AnchorStyles.Right;
            _btnSalvar.Click+=BtnSalvar_Click;

            _btnVoltar=MakeBtn("Voltar",436,416,Color.FromArgb(100,100,100));
            _btnVoltar.TabIndex=6; _btnVoltar.Anchor=AnchorStyles.Bottom|AnchorStyles.Right;
            _btnVoltar.Click+=(s,e)=>Close();

            Controls.AddRange(new Control[]{_btnSalvar,_btnVoltar});
            KeyDown+=(s,e)=>{
                if(e.KeyCode==Keys.Enter&&ActiveControl!=_btnSalvar){
                    e.SuppressKeyPress=true;
                    SelectNextControl(ActiveControl,true,true,true,true);}};
        }

        private void PreencherIdAutomatico()
        {
            if (_produtoEdicao != null) return;
            _txtId.Text = ProdutoService.ProximoId().ToString();
        }

        private void PreencherCampos()
        {
            _txtId.Text=_produtoEdicao.Id.ToString(); _txtNome.Text=_produtoEdicao.Nome;
            _txtData.Text=_produtoEdicao.Data; _txtValor.Text=_produtoEdicao.Valor.ToString("F2");
            _txtEstoque.Text=_produtoEdicao.Estoque.ToString(); _chkAtivo.Checked=_produtoEdicao.Ativo;
            _chkAtivo.ForeColor=_produtoEdicao.Ativo?Color.FromArgb(34,130,55):Vermelho;
        }

        private void LimparCampos()
        {
            _produtoEdicao=null; _txtId.Text=""; _txtNome.Clear();
            _txtData.Text=DateTime.Now.ToString("dd/MM/yyyy");
            _txtValor.Clear(); _txtEstoque.Clear(); _chkAtivo.Checked=true;
            PreencherIdAutomatico(); _txtNome.Focus();
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(_txtNome.Text)){
                MessageBox.Show("O campo Nome e obrigatorio.","Validacao",
                    MessageBoxButtons.OK,MessageBoxIcon.Warning);
                _txtNome.Focus(); return false;}
            if (!string.IsNullOrWhiteSpace(_txtValor.Text)){
                string vs=_txtValor.Text.Replace(",",".");
                if(!decimal.TryParse(vs,System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,out _)){
                    MessageBox.Show("Valor invalido.","Validacao",
                        MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    _txtValor.Focus(); return false;}}
            return true;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if(!Validar()) return;
            if(MessageBox.Show("Voce deseja salvar o cadastro?","Confirmar",
                MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes) return;

            decimal valor=0m;
            if(!string.IsNullOrWhiteSpace(_txtValor.Text))
                decimal.TryParse(_txtValor.Text.Replace(",","."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,out valor);
            int estoque=0;
            if(!string.IsNullOrWhiteSpace(_txtEstoque.Text))
                int.TryParse(_txtEstoque.Text,out estoque);

            var produto=new Produto{
                Id=_produtoEdicao?.Id??0, Nome=_txtNome.Text.Trim(),
                Data=_txtData.Text.Trim(), Valor=valor, Estoque=estoque,
                Ativo=_chkAtivo.Checked};

            // ── Camada de Serviço ─────────────────────────────────────────────
            if(_produtoEdicao==null) ProdutoService.Inserir(produto);
            else                     ProdutoService.Atualizar(produto);

            if(MessageBox.Show("Voce deseja continuar cadastrando?","Continuar",
                MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)
                LimparCampos();
            else{DialogResult=DialogResult.OK; Close();}
        }

        private static Label MakeLbl(string t,int x,int y)=>new Label{
            Text=t,Font=new Font("Segoe UI",9f,FontStyle.Bold),
            AutoSize=true,Location=new Point(x,y),BackColor=Color.Transparent};
        private static TextBox MakeTxt(int x,int y,int w,bool ro)=>new TextBox{
            Location=new Point(x,y),Size=new Size(w,28),Font=new Font("Segoe UI",9.5f),
            ReadOnly=ro,BackColor=ro?Color.FromArgb(225,225,225):Color.White,
            BorderStyle=BorderStyle.FixedSingle};
        private static Button MakeBtn(string t,int x,int y,Color cor){
            var b=new Button{Text=t,Location=new Point(x,y),Size=new Size(96,34),
                FlatStyle=FlatStyle.Flat,BackColor=cor,ForeColor=Color.White,
                Font=new Font("Segoe UI",9f,FontStyle.Bold),Cursor=Cursors.Hand};
            b.FlatAppearance.BorderSize=0;
            b.MouseEnter+=(s,e)=>b.BackColor=Lighten(cor,20);
            b.MouseLeave+=(s,e)=>b.BackColor=cor;
            return b;}
        private void AplicarTema()=>TemaAplicador.AplicarEm(this);
        private static Color Lighten(Color c,int a)=>Color.FromArgb(
            Math.Min(c.R+a,255),Math.Min(c.G+a,255),Math.Min(c.B+a,255));
    }
}
