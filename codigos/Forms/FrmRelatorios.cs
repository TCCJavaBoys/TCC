using System;
using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Services;

namespace PotirendabaApp.Forms
{
    /// <summary>
    /// Tela principal de relatórios do sistema PotirendabaApp.
    /// Lista os relatórios disponíveis e gerencia a abertura do modal de período.
    /// Para integrar com RDLC/FastReport: implemente os métodos GerarRelatorio*().
    /// </summary>
    public class FrmRelatorios : Form
    {
        // ── Controles ─────────────────────────────────────────────────────────
        private Panel        _topBar, _bottomBar;
        private TextBox      _txtPesquisa;
        private Button       _btnBuscar;
        private Panel        _pConteudo;
        private Label        _lblTituloSecao;

        // Botões dos relatórios (nomes padronizados conforme especificação)
        private Panel _btnRelatorioVendas;
        private Panel _btnRelatorioProdutos;
        private Panel _btnRelatorioClientes;
        private Panel _btnRelatorioCaixa;
        private Panel _btnRelatorioMaisVendidos;
        private Panel _btnRelatorioVendasCliente;

        public FrmRelatorios()
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
        //  INTERFACE
        // ══════════════════════════════════════════════════════════════════════
        private void InitUI()
        {
            Text            = "Relatórios";
            Size            = new Size(680, 520);
            MinimumSize     = new Size(520, 420);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;

            // ── TableLayoutPanel raiz ─────────────────────────────────────────
            var root = new TableLayoutPanel
            {
                Dock            = DockStyle.Fill,
                ColumnCount     = 1,
                RowCount        = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding         = Padding.Empty,
                Margin          = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // header
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // conteúdo
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // rodapé

            // ── Row 0: Barra verde superior ───────────────────────────────────
            _topBar = new Panel { Dock = DockStyle.Fill, BackColor = TemaService.Verde };

            // Logo/ícone
            _topBar.Controls.Add(new Label
            {
                Text      = "🏛",
                Font      = new Font("Segoe UI Emoji", 18f),
                ForeColor = Color.White,
                AutoSize  = false,
                Size      = new Size(50, 50),
                Location  = new Point(8, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            });

            // Campo de pesquisa
            _txtPesquisa = new TextBox
            {
                Location        = new Point(68, 16),
                Size            = new Size(240, 28),
                Font            = new Font("Segoe UI", 10f),
                PlaceholderText = "Pesquisa",
                BorderStyle     = BorderStyle.FixedSingle
            };
            _txtPesquisa.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) FiltrarRelatorios(_txtPesquisa.Text);
            };

            _btnBuscar = new Button
            {
                Text      = "🔍",
                Location  = new Point(314, 14),
                Size      = new Size(36, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = TemaService.VerdeDark,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI Emoji", 12f),
                Cursor    = Cursors.Hand
            };
            _btnBuscar.FlatAppearance.BorderSize = 0;
            _btnBuscar.Click += (s, e) => FiltrarRelatorios(_txtPesquisa.Text);

            _topBar.Controls.AddRange(new Control[] { _txtPesquisa, _btnBuscar });
            root.Controls.Add(_topBar, 0, 0);

            // ── Row 1: Conteúdo ───────────────────────────────────────────────
            _pConteudo = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(40, 20, 40, 20)
            };

            // Título da seção
            _lblTituloSecao = new Label
            {
                Text      = "Relatório",
                Font      = new Font("Segoe UI", 10f, FontStyle.Underline | FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(0, 0),
                BackColor = Color.Transparent
            };
            _pConteudo.Controls.Add(_lblTituloSecao);

            // Botões de relatório (cards clicáveis)
            _btnRelatorioVendas       = CriarCardRelatorio("📊", "Vendas por Período",         0);
            _btnRelatorioProdutos     = CriarCardRelatorio("📦", "Estoque de Produtos",        1);
            _btnRelatorioClientes     = CriarCardRelatorio("👤", "Cadastro de Clientes",       2);
            _btnRelatorioCaixa        = CriarCardRelatorio("💰", "Caixa do Dia",               3);
            _btnRelatorioMaisVendidos = CriarCardRelatorio("🏆", "Produtos Mais Vendidos",     4);
            _btnRelatorioVendasCliente= CriarCardRelatorio("📈", "Vendas por Cliente",         5);

            // Eventos de clique
            _btnRelatorioVendas.Click       += (s, e) => AbrirRelatorioComPeriodo("Vendas por Período");
            _btnRelatorioProdutos.Click     += (s, e) => GerarRelatorioDireto("Estoque de Produtos");
            _btnRelatorioClientes.Click     += (s, e) => GerarRelatorioDireto("Cadastro de Clientes");
            _btnRelatorioCaixa.Click        += (s, e) => AbrirRelatorioComPeriodo("Caixa do Dia");
            _btnRelatorioMaisVendidos.Click += (s, e) => AbrirRelatorioComPeriodo("Produtos Mais Vendidos");
            _btnRelatorioVendasCliente.Click+= (s, e) => AbrirRelatorioComPeriodo("Vendas por Cliente");

            // Também aceita clique nos labels internos
            foreach (Control ctrl in _btnRelatorioVendas.Controls)
                ctrl.Click += (s, e) => AbrirRelatorioComPeriodo("Vendas por Período");
            foreach (Control ctrl in _btnRelatorioProdutos.Controls)
                ctrl.Click += (s, e) => GerarRelatorioDireto("Estoque de Produtos");
            foreach (Control ctrl in _btnRelatorioClientes.Controls)
                ctrl.Click += (s, e) => GerarRelatorioDireto("Cadastro de Clientes");
            foreach (Control ctrl in _btnRelatorioCaixa.Controls)
                ctrl.Click += (s, e) => AbrirRelatorioComPeriodo("Caixa do Dia");
            foreach (Control ctrl in _btnRelatorioMaisVendidos.Controls)
                ctrl.Click += (s, e) => AbrirRelatorioComPeriodo("Produtos Mais Vendidos");
            foreach (Control ctrl in _btnRelatorioVendasCliente.Controls)
                ctrl.Click += (s, e) => AbrirRelatorioComPeriodo("Vendas por Cliente");

            _pConteudo.Controls.AddRange(new Control[]
                { _btnRelatorioVendas, _btnRelatorioProdutos, _btnRelatorioClientes,
                  _btnRelatorioCaixa, _btnRelatorioMaisVendidos, _btnRelatorioVendasCliente });

            // Reposicionar cards ao redimensionar
            _pConteudo.Resize += (s, e) => ReposicionarCards();
            root.Controls.Add(_pConteudo, 0, 1);

            // ── Row 2: Barra verde inferior ───────────────────────────────────
            _bottomBar = new Panel { Dock = DockStyle.Fill, BackColor = TemaService.Verde };
            root.Controls.Add(_bottomBar, 0, 2);
            Controls.Add(root);
        }

        // ── Criar card de relatório ───────────────────────────────────────────
        private Panel CriarCardRelatorio(string icone, string titulo, int indice)
        {
            var card = new Panel
            {
                Size        = new Size(560, 46),
                Location    = new Point(0, 36 + indice * 58),
                Cursor      = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblIcone = new Label
            {
                Text      = icone,
                Font      = new Font("Segoe UI Symbol", 13f),
                AutoSize  = false,
                Size      = new Size(40, 44),
                Location  = new Point(6, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };

            var lblTitulo = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 10f, FontStyle.Underline),
                AutoSize  = true,
                Location  = new Point(48, 12),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };

            card.Controls.AddRange(new Control[] { lblIcone, lblTitulo });

            // Hover — destaque sutil
            card.MouseEnter  += (s, e) => card.BackColor = TemaService.GridAlternada;
            card.MouseLeave  += (s, e) => card.BackColor = TemaService.FundoPainel;
            lblIcone.MouseEnter += (s, e) => card.BackColor = TemaService.GridAlternada;
            lblIcone.MouseLeave += (s, e) => card.BackColor = TemaService.FundoPainel;
            lblTitulo.MouseEnter += (s, e) => card.BackColor = TemaService.GridAlternada;
            lblTitulo.MouseLeave += (s, e) => card.BackColor = TemaService.FundoPainel;

            return card;
        }

        // ── Reposicionar cards responsivamente ────────────────────────────────
        private void ReposicionarCards()
        {
            int largura = _pConteudo.ClientSize.Width - 80;
            if (largura < 100) return;

            var cards = new[] { _btnRelatorioVendas, _btnRelatorioProdutos, _btnRelatorioClientes,
                                _btnRelatorioCaixa, _btnRelatorioMaisVendidos, _btnRelatorioVendasCliente };
            int cy = 36;
            foreach (var card in cards)
            {
                card.Width    = largura;
                card.Location = new Point(0, cy);
                cy += 58;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LÓGICA DOS RELATÓRIOS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre o modal de período e, ao confirmar, chama GerarRelatorioVendas().
        /// Relatórios que precisam de intervalo de datas passam por aqui.
        /// </summary>
        private void AbrirRelatorioComPeriodo(string nome = "Vendas por Período")
        {
            Action<DateTime, DateTime> acao = nome switch
            {
                "Caixa do Dia"              => (i, f) => GerarRelatorioCaixa(i),
                "Produtos Mais Vendidos"    => GerarRelatorioMaisVendidos,
                "Vendas por Cliente"        => GerarRelatorioVendasCliente,
                _                           => GerarRelatorioVendas
            };
            using var frm = new FrmPeriodoRelatorio(acao);
            frm.ShowDialog(this);
        }

        /// <summary>
        /// Gera relatório diretamente (sem modal de período).
        /// Produtos cadastrados e Clientes cadastrados passam por aqui.
        /// </summary>
        private void GerarRelatorioDireto(string nomeRelatorio)
        {
            switch (nomeRelatorio)
            {
                case "Estoque de Produtos":
                    GerarRelatorioProdutos();
                    break;
                case "Cadastro de Clientes":
                    GerarRelatorioClientes();
                    break;
            }
        }

        // ── Stubs de geração (integrar RDLC / FastReport aqui) ───────────────

        /// <summary>
        /// TODO: Integrar com RDLC/FastReport.
        /// Recebe o período selecionado no FrmPeriodoRelatorio.
        /// </summary>
        private void GerarRelatorioVendas(DateTime dataInicial, DateTime dataFinal)
        {
            try
            {
                RelatorioService.ImprimirVendas(dataInicial, dataFinal);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar relatório:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>TODO: Integrar com RDLC/FastReport.</summary>
        private void GerarRelatorioProdutos()
        {
            try
            {
                RelatorioService.ImprimirProdutos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar relatório:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>TODO: Integrar com RDLC/FastReport.</summary>
        private void GerarRelatorioClientes()
        {
            try
            {
                RelatorioService.ImprimirClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar relatório:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Pesquisa / filtro de relatórios ──────────────────────────────────
        private void GerarRelatorioCaixa(DateTime data)
        {
            try   { RelatorioService.ImprimirCaixaDia(data); }
            catch (Exception ex) { MostrarErro(ex); }
        }

        private void GerarRelatorioMaisVendidos(DateTime ini, DateTime fim)
        {
            try   { RelatorioService.ImprimirProdutosMaisVendidos(ini, fim); }
            catch (Exception ex) { MostrarErro(ex); }
        }

        private void GerarRelatorioVendasCliente(DateTime ini, DateTime fim)
        {
            try   { RelatorioService.ImprimirVendasPorCliente(ini, fim); }
            catch (Exception ex) { MostrarErro(ex); }
        }

        private static void MostrarErro(Exception ex) =>
            MessageBox.Show($"Erro ao gerar relatório:\n{ex.Message}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void FiltrarRelatorios(string texto)
        {
            var termos = new[]
            {
                ("vendas",   _btnRelatorioVendas),
                ("produtos", _btnRelatorioProdutos),
                ("cliente",  _btnRelatorioClientes),
                ("aluno",    _btnRelatorioClientes)
            };

            bool filtrar = !string.IsNullOrWhiteSpace(texto);
            string t = texto.ToLower().Trim();

            _btnRelatorioVendas.Visible       = !filtrar || "vendas por período".Contains(t);
            _btnRelatorioProdutos.Visible     = !filtrar || "estoque de produtos".Contains(t);
            _btnRelatorioClientes.Visible     = !filtrar || "cadastro de clientes".Contains(t);
            _btnRelatorioCaixa.Visible        = !filtrar || "caixa do dia".Contains(t);
            _btnRelatorioMaisVendidos.Visible = !filtrar || "produtos mais vendidos".Contains(t);
            _btnRelatorioVendasCliente.Visible= !filtrar || "vendas por cliente".Contains(t);

            ReposicionarCards();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TEMA
        // ══════════════════════════════════════════════════════════════════════
        private void AplicarTema()
        {
            BackColor              = TemaService.FundoForm;
            _pConteudo.BackColor   = TemaService.FundoForm;
            _lblTituloSecao.ForeColor = TemaService.TextoPrincipal;
            _txtPesquisa.BackColor = TemaService.FundoInput;
            _txtPesquisa.ForeColor = TemaService.TextoPrincipal;

            // Cards
            foreach (var card in new[] { _btnRelatorioVendas, _btnRelatorioProdutos, _btnRelatorioClientes,
                                         _btnRelatorioCaixa, _btnRelatorioMaisVendidos, _btnRelatorioVendasCliente })
            {
                card.BackColor = TemaService.FundoPainel;
                foreach (Control c in card.Controls)
                {
                    c.BackColor = Color.Transparent;
                    c.ForeColor = c is Label lbl && lbl.Font.Underline
                        ? TemaService.Verde
                        : TemaService.TextoPrincipal;
                }
            }
        }
    }
}
