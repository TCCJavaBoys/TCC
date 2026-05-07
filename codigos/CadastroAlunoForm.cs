using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PotirendabaApp.Data;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    public class CadastroAlunoForm : Form
    {
        private static readonly Color Verde      = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeBotao = Color.FromArgb(45, 140, 65);
        private static readonly Color Vermelho   = Color.FromArgb(180, 40, 40);

        private TextBox  _txtId, _txtNome, _txtTelefone, _txtData, _txtSala;
        private CheckBox _chkAtivo;
        private Button   _btnSalvar, _btnVoltar;
        private Panel    _topBar, _content;
        private Aluno    _alunoEdicao;

        public CadastroAlunoForm(Aluno alunoEdicao = null)
        {
            _alunoEdicao = alunoEdicao;
            InitUI();
            PreencherIdAutomatico();
            if (_alunoEdicao != null) PreencherCampos();
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
            Text            = "Cadastro de Aluno";
            Size            = new Size(560, 480);
            MinimumSize     = new Size(500, 440);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            StartPosition   = FormStartPosition.CenterParent;
            KeyPreview      = true;

            _topBar = new Panel { Dock=DockStyle.Top, Height=54, BackColor=Verde };
            _topBar.Controls.Add(new Label {
                Text="🏛", Font=new Font("Segoe UI Emoji",18f), ForeColor=Color.White,
                AutoSize=false, Size=new Size(48,48), Location=new Point(6,3),
                TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Transparent });
            Controls.Add(_topBar);

            _content = new Panel {
                Location=new Point(20,70), Size=new Size(504,330),
                BorderStyle=BorderStyle.FixedSingle,
                Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom };
            Controls.Add(_content);

            _content.Controls.Add(new Label {
                Text="Cadastro de aluno",
                Font=new Font("Segoe UI",10f,FontStyle.Underline|FontStyle.Bold),
                AutoSize=true, Location=new Point(10,10), BackColor=Color.Transparent });

            // ID + Ativo
            _content.Controls.Add(MakeLbl("ID", 10, 42));
            _txtId = MakeTxt(10, 60, 90, soLeitura:true);
            _txtId.TabStop = false;
            _content.Controls.Add(_txtId);

            _chkAtivo = new CheckBox {
                Text="Ativo", Checked=true, AutoSize=true,
                Location=new Point(115, 63),
                Font=new Font("Segoe UI",9f,FontStyle.Bold),
                ForeColor=Color.FromArgb(34,130,55) };
            _chkAtivo.CheckedChanged += (s,e) =>
                _chkAtivo.ForeColor = _chkAtivo.Checked
                    ? Color.FromArgb(34,130,55) : Vermelho;
            _content.Controls.Add(_chkAtivo);

            // Data
            _content.Controls.Add(MakeLbl("Data", 330, 42));
            _txtData = MakeTxt(330, 60, 154, soLeitura:false);
            _txtData.Text = DateTime.Now.ToString("dd/MM/yyyy");
            _txtData.TabIndex = 4;
            _txtData.Anchor = AnchorStyles.Top|AnchorStyles.Right;
            _content.Controls.Add(_txtData);

            // Nome
            _content.Controls.Add(MakeLbl("Nome", 10, 100));
            _txtNome = MakeTxt(10, 118, 474, soLeitura:false);
            _txtNome.TabIndex = 1;
            _txtNome.Anchor = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
            _content.Controls.Add(_txtNome);

            // Sala
            _content.Controls.Add(MakeLbl("Sala", 330, 154));
            _txtSala = MakeTxt(330, 172, 154, soLeitura:false);
            _txtSala.TabIndex = 3;
            _txtSala.Anchor = AnchorStyles.Top|AnchorStyles.Right;
            _content.Controls.Add(_txtSala);

            // Telefone
            _content.Controls.Add(MakeLbl("Telefone", 10, 154));
            _txtTelefone = MakeTxt(10, 172, 210, soLeitura:false);
            _txtTelefone.MaxLength = 15;
            _txtTelefone.TabIndex  = 2;
            _txtTelefone.KeyDown  += Telefone_KeyDown;
            _content.Controls.Add(_txtTelefone);

            // Botões Salvar e Voltar (sem Inativar)
            _btnSalvar = MakeBtn("Salvar", 330, 416, VerdeBotao);
            _btnSalvar.TabIndex = 5;
            _btnSalvar.Anchor   = AnchorStyles.Bottom|AnchorStyles.Right;
            _btnSalvar.Click   += BtnSalvar_Click;

            _btnVoltar = MakeBtn("Voltar", 436, 416, Color.FromArgb(100,100,100));
            _btnVoltar.TabIndex = 6;
            _btnVoltar.Anchor   = AnchorStyles.Bottom|AnchorStyles.Right;
            _btnVoltar.Click   += (s,e) => Close();

            Controls.AddRange(new Control[]{ _btnSalvar, _btnVoltar });

            KeyDown += (s,e) => {
                if (e.KeyCode==Keys.Enter && ActiveControl!=_btnSalvar) {
                    e.SuppressKeyPress=true;
                    SelectNextControl(ActiveControl,true,true,true,true); }};
        }

        private void PreencherIdAutomatico()
        {
            if (_alunoEdicao != null) return;
            var todos = DatabaseHelper.ListarAlunos(status:DatabaseHelper.FiltroStatus.Todos);
            int proximo = 1;
            foreach (var a in todos) if (a.Id >= proximo) proximo = a.Id + 1;
            _txtId.Text = proximo.ToString();
        }

        private void Telefone_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            if (e.KeyCode==Keys.Tab||e.KeyCode==Keys.Enter||e.KeyCode==Keys.Left||
                e.KeyCode==Keys.Right||e.KeyCode==Keys.Home||e.KeyCode==Keys.End)
            { e.SuppressKeyPress=false; return; }
            string digitos = Regex.Replace(_txtTelefone.Text, @"\D", "");
            if      (e.KeyCode==Keys.Back)   { if (digitos.Length>0) digitos=digitos[..^1]; }
            else if (e.KeyCode==Keys.Delete) { digitos=""; }
            else {
                char c = ObterChar(e);
                if (!char.IsDigit(c)||digitos.Length>=11) return;
                digitos+=c; }
            _txtTelefone.Text = FormatarTel(digitos);
            _txtTelefone.SelectionStart = _txtTelefone.Text.Length;
        }

        private static char ObterChar(KeyEventArgs e)
        {
            if (e.KeyCode>=Keys.D0&&e.KeyCode<=Keys.D9&&!e.Shift)
                return (char)('0'+(e.KeyCode-Keys.D0));
            if (e.KeyCode>=Keys.NumPad0&&e.KeyCode<=Keys.NumPad9)
                return (char)('0'+(e.KeyCode-Keys.NumPad0));
            return '\0';
        }

        private static string FormatarTel(string d) => d.Length switch {
            0      => "", 1 or 2 => $"({d}",
            <= 6   => $"({d[..2]}) {d[2..]}",
            <= 10  => $"({d[..2]}) {d[2..6]}-{d[6..]}",
            _      => $"({d[..2]}) {d[2..7]}-{d[7..]}" };

        private void PreencherCampos()
        {
            _txtId.Text       = _alunoEdicao.Id.ToString();
            _txtNome.Text     = _alunoEdicao.Nome;
            _txtTelefone.Text = _alunoEdicao.Telefone;
            _txtData.Text     = _alunoEdicao.Data;
            _txtSala.Text     = _alunoEdicao.Sala;
            _chkAtivo.Checked = _alunoEdicao.Ativo;
            _chkAtivo.ForeColor = _alunoEdicao.Ativo
                ? Color.FromArgb(34,130,55) : Vermelho;
        }

        private void LimparCampos()
        {
            _alunoEdicao=null; _txtId.Text=""; _txtNome.Clear();
            _txtTelefone.Clear(); _txtData.Text=DateTime.Now.ToString("dd/MM/yyyy");
            _txtSala.Clear(); _chkAtivo.Checked=true;
            PreencherIdAutomatico(); _txtNome.Focus();
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(_txtNome.Text)) {
                MessageBox.Show("O campo Nome e obrigatorio.","Validacao",
                    MessageBoxButtons.OK,MessageBoxIcon.Warning);
                _txtNome.Focus(); return false; }
            return true;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            if (MessageBox.Show("Voce deseja salvar o cadastro?","Confirmar",
                MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes) return;

            var aluno = new Aluno {
                Id=_alunoEdicao?.Id??0, Nome=_txtNome.Text.Trim(),
                Telefone=_txtTelefone.Text.Trim(), Data=_txtData.Text.Trim(),
                Sala=_txtSala.Text.Trim(), Ativo=_chkAtivo.Checked };

            if (_alunoEdicao==null) DatabaseHelper.InserirAluno(aluno);
            else                    DatabaseHelper.AtualizarAluno(aluno);

            if (MessageBox.Show("Voce deseja continuar cadastrando?","Continuar",
                MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)
                LimparCampos();
            else { DialogResult=DialogResult.OK; Close(); }
        }

        private static Label MakeLbl(string t,int x,int y)=>new Label {
            Text=t,Font=new Font("Segoe UI",9f,FontStyle.Bold),
            AutoSize=true,Location=new Point(x,y),BackColor=Color.Transparent };
        private static TextBox MakeTxt(int x,int y,int w,bool soLeitura)=>new TextBox {
            Location=new Point(x,y),Size=new Size(w,28),Font=new Font("Segoe UI",9.5f),
            ReadOnly=soLeitura,BackColor=soLeitura?Color.FromArgb(225,225,225):Color.White,
            BorderStyle=BorderStyle.FixedSingle };
        private static Button MakeBtn(string t,int x,int y,Color cor) {
            var b=new Button { Text=t,Location=new Point(x,y),Size=new Size(96,34),
                FlatStyle=FlatStyle.Flat,BackColor=cor,ForeColor=Color.White,
                Font=new Font("Segoe UI",9f,FontStyle.Bold),Cursor=Cursors.Hand };
            b.FlatAppearance.BorderSize=0;
            b.MouseEnter+=(s,e)=>b.BackColor=Lighten(cor,20);
            b.MouseLeave+=(s,e)=>b.BackColor=cor;
            return b; }
        private void AplicarTema() => TemaAplicador.AplicarEm(this);
        private static Color Lighten(Color c,int a)=>Color.FromArgb(
            Math.Min(c.R+a,255),Math.Min(c.G+a,255),Math.Min(c.B+a,255));
    }
}
