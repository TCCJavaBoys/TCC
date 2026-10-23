using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    /// <summary>
    /// Cadastro/edição de um lote de um produto específico.
    /// O produto é sempre fixo (vem da tela de Lotes do produto) — não dá
    /// pra trocar o produto de um lote já criado, então esse campo é
    /// somente leitura.
    /// </summary>
    public class CadastroLoteForm : Form
    {
        private static readonly Color Verde      = Color.FromArgb(56, 161, 78);
        private static readonly Color VerdeBotao = Color.FromArgb(45, 140, 65);
        private static readonly Color Vermelho   = Color.FromArgb(180, 40, 40);

        private TextBox       _txtId, _txtProduto, _txtQuantidade, _txtEntrada;
        private MaskedTextBox _txtValidade;
        private CheckBox _chkAtivo;
        private Button   _btnSalvar, _btnVoltar;
        private Panel    _topBar, _content;

        private readonly Produto _produto;
        private readonly Lote    _loteEdicao;

        public CadastroLoteForm(Produto produto, Lote loteEdicao = null)
        {
            _produto    = produto;
            _loteEdicao = loteEdicao;
            InitUI();
            if (_loteEdicao != null) PreencherCampos();
            else
            {
                _txtId.Text      = LoteService.ProximoId().ToString();
                _txtEntrada.Text = DateTime.Now.ToString("dd/MM/yyyy");
            }
            AplicarTema();
            TemaService.TemaAlterado += AplicarTema;
            ActiveControl = _txtQuantidade; // Enter já começa pulando pro campo certo
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TemaService.TemaAlterado -= AplicarTema;
            base.OnFormClosed(e);
        }

        private void InitUI()
        {
            Text = "Cadastro de Lote";
            Size = new Size(520, 440);
            MinimumSize = new Size(480, 420);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true; StartPosition = FormStartPosition.CenterParent; KeyPreview = true;
            Icon = LogoHelper.GetIcon() ?? Icon;

            _topBar = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Verde };
            _topBar.Controls.Add(LogoHelper.CriarPictureBox(8, 0, 44, 54));
            _topBar.Controls.Add(new Label
            {
                Text = "Lote de Produto", Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(62, 17), BackColor = Color.Transparent
            });
            Controls.Add(_topBar);

            _content = new Panel
            {
                Location = new Point(20, 70), Size = new Size(464, 290),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            Controls.Add(_content);

            _content.Controls.Add(new Label
            {
                Text = "Cadastro de Lote", Font = new Font("Segoe UI", 10f, FontStyle.Underline | FontStyle.Bold),
                AutoSize = true, Location = new Point(10, 10), BackColor = Color.Transparent
            });

            _content.Controls.Add(MakeLbl("ID", 10, 42));
            _txtId = MakeTxt(10, 60, 90, true); _txtId.TabStop = false;
            _content.Controls.Add(_txtId);

            _chkAtivo = new CheckBox
            {
                Text = "Ativo", Checked = true, AutoSize = true, Location = new Point(115, 63),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(34, 130, 55),
                TabStop = false
            };
            _chkAtivo.CheckedChanged += (s, e) =>
                _chkAtivo.ForeColor = _chkAtivo.Checked ? Color.FromArgb(34, 130, 55) : Vermelho;
            _content.Controls.Add(_chkAtivo);

            _content.Controls.Add(MakeLbl("Produto", 10, 100));
            _txtProduto = MakeTxt(10, 118, 434, true); _txtProduto.TabStop = false;
            _txtProduto.Text = _produto?.Nome ?? "";
            _txtProduto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _content.Controls.Add(_txtProduto);

            _content.Controls.Add(MakeLbl("Quantidade", 10, 156));
            _txtQuantidade = MakeTxt(10, 174, 200, false); _txtQuantidade.TabIndex = 1;
            _txtQuantidade.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true; };
            _content.Controls.Add(_txtQuantidade);

            _content.Controls.Add(MakeLbl("Validade", 244, 156));
            _txtValidade = new MaskedTextBox
            {
                Mask = "00/00/0000", Location = new Point(244, 174), Size = new Size(200, 28),
                Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle,
                TabIndex = 2, Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _content.Controls.Add(_txtValidade);

            _content.Controls.Add(MakeLbl("Data de Entrada", 10, 212));
            _txtEntrada = MakeTxt(10, 230, 200, true); _txtEntrada.TabStop = false;
            _content.Controls.Add(_txtEntrada);

            var lblAjuda = new Label
            {
                Text = "Dica: cada remessa nova de mercadoria vira um lote novo,\n" +
                       "mesmo que seja do mesmo produto de um lote já existente.",
                Font = new Font("Segoe UI", 8f, FontStyle.Italic), ForeColor = Color.Gray,
                AutoSize = true, Location = new Point(10, 264), BackColor = Color.Transparent
            };
            _content.Controls.Add(lblAjuda);

            _btnSalvar = MakeBtn("Salvar", 290, 376, VerdeBotao);
            _btnSalvar.TabIndex = 3; _btnSalvar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnSalvar.Click += BtnSalvar_Click;

            _btnVoltar = MakeBtn("Voltar", 396, 376, Color.FromArgb(100, 100, 100));
            _btnVoltar.TabIndex = 4; _btnVoltar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnVoltar.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { _btnSalvar, _btnVoltar });
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && ActiveControl != _btnSalvar)
                {
                    e.SuppressKeyPress = true;
                    SelectNextControl(ActiveControl, true, true, true, true);
                }
            };
        }

        private void PreencherCampos()
        {
            _txtId.Text = _loteEdicao.Id.ToString();
            _txtQuantidade.Text = _loteEdicao.Quantidade.ToString();
            _txtValidade.Text = ParaBr(_loteEdicao.DataValidade);
            _txtEntrada.Text = ParaBr(_loteEdicao.DataEntrada);
            _chkAtivo.Checked = _loteEdicao.Ativo;
            _chkAtivo.ForeColor = _loteEdicao.Ativo ? Color.FromArgb(34, 130, 55) : Vermelho;
        }

        private bool Validar(out int quantidade, out string validadeIso)
        {
            quantidade = 0; validadeIso = "";

            // 1) Campos obrigatórios em branco
            if (string.IsNullOrWhiteSpace(_txtQuantidade.Text))
            {
                MessageBox.Show("Campo em branco: Quantidade.", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtQuantidade.Focus(); return false;
            }
            if (!_txtValidade.MaskCompleted)
            {
                MessageBox.Show("Campo em branco: Validade.", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtValidade.Focus(); return false;
            }

            // 2) Valores válidos
            if (!int.TryParse(_txtQuantidade.Text, out quantidade) || quantidade <= 0)
            {
                MessageBox.Show("Informe uma quantidade válida (maior que zero).", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtQuantidade.Focus(); return false;
            }

            if (!DateTime.TryParseExact(_txtValidade.Text.Trim(), "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var validade))
            {
                MessageBox.Show("Informe a validade no formato dd/mm/aaaa.", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtValidade.Focus(); return false;
            }

            validadeIso = validade.ToString("yyyy-MM-dd");
            return true;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (!Validar(out int quantidade, out string validadeIso)) return;
            if (MessageBox.Show("Deseja salvar este lote?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var lote = new Lote
            {
                Id           = _loteEdicao?.Id ?? 0,
                ProdutoId    = _produto.Id,
                Quantidade   = quantidade,
                DataValidade = validadeIso,
                DataEntrada  = _loteEdicao?.DataEntrada ?? DateTime.Now.ToString("yyyy-MM-dd"),
                Ativo        = _chkAtivo.Checked
            };

            if (_loteEdicao == null) LoteService.Inserir(lote);
            else                     LoteService.Atualizar(lote);

            DialogResult = DialogResult.OK;
            Close();
        }

        private static string ParaBr(string iso) =>
            DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d.ToString("dd/MM/yyyy") : iso;

        private static Label MakeLbl(string t, int x, int y) => new Label
        {
            Text = t, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            AutoSize = true, Location = new Point(x, y), BackColor = Color.Transparent
        };
        private static TextBox MakeTxt(int x, int y, int w, bool ro) => new TextBox
        {
            Location = new Point(x, y), Size = new Size(w, 28), Font = new Font("Segoe UI", 9.5f),
            ReadOnly = ro, BackColor = ro ? Color.FromArgb(225, 225, 225) : Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        private static Button MakeBtn(string t, int x, int y, Color cor)
        {
            var b = new Button
            {
                Text = t, Location = new Point(x, y), Size = new Size(96, 34),
                FlatStyle = FlatStyle.Flat, BackColor = cor, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.MouseEnter += (s, e) => b.BackColor = Lighten(cor, 20);
            b.MouseLeave += (s, e) => b.BackColor = cor;
            return b;
        }
        private void AplicarTema() => TemaAplicador.AplicarEm(this);
        private static Color Lighten(Color c, int a) => Color.FromArgb(
            Math.Min(c.R + a, 255), Math.Min(c.G + a, 255), Math.Min(c.B + a, 255));
    }
}
