using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using PotirendabaApp.Data;
using PotirendabaApp.Models;

namespace PotirendabaApp.Services
{
    /// <summary>
    /// Gera e exibe relatórios via PrintDocument nativo do Windows.
    /// Sem dependências externas — funciona em qualquer máquina.
    /// </summary>
    public static class RelatorioService
    {
        // ── Fontes e cores ────────────────────────────────────────────────────
        private static readonly Font FntTitulo    = new Font("Segoe UI", 14f, FontStyle.Bold);
        private static readonly Font FntSubtitulo = new Font("Segoe UI", 9f,  FontStyle.Italic);
        private static readonly Font FntCabCol    = new Font("Segoe UI", 9f,  FontStyle.Bold);
        private static readonly Font FntDado      = new Font("Segoe UI", 9f);
        private static readonly Font FntRodape    = new Font("Segoe UI", 8f,  FontStyle.Italic);

        private static readonly Color CorVerde    = Color.FromArgb(56, 161, 78);
        private static readonly Color CorCinzaClr = Color.FromArgb(240, 245, 240);
        private static readonly Color CorBorda    = Color.FromArgb(180, 180, 180);

        // ── Margens ───────────────────────────────────────────────────────────
        private const int MargemEsq  = 40;
        private const int MargemDir  = 40;
        private const int MargemTopo = 40;
        private const int AltLinha   = 26;
        private const int AltCabeca  = 34;

        // ═════════════════════════════════════════════════════════════════════
        //  RELATÓRIO 1 — Comportamento de Vendas (com período)
        // ═════════════════════════════════════════════════════════════════════
        public static void ImprimirVendas(DateTime dataIni, DateTime dataFim)
        {
            // Buscar todas as vendas e filtrar por período
            var todasVendas = DatabaseHelper.ListarVendas();
            var filtradas   = new List<Venda>();
            foreach (var v in todasVendas)
            {
                if (DateTime.TryParse(v.Data, out DateTime dv))
                    if (dv.Date >= dataIni.Date && dv.Date <= dataFim.Date)
                        filtradas.Add(v);
            }

            // Colunas: ID | Cliente | Valor | Data | Pagamento
            var colunas = new[] { "ID", "Cliente", "Valor", "Data/Hora", "Pagamento" };
            var larguras = new[] { 40, 140, 130, 120, 100 };

            // Montar linhas
            var linhas = new List<string[]>();
            decimal totalGeral = 0;
            foreach (var v in filtradas)
            {
                totalGeral += v.ValorTotal;
                linhas.Add(new[]
                {
                    v.Id.ToString(),
                    v.Cliente,
                    $"R$ {v.ValorTotal:F2}",
                    v.Data,
                    v.FormaPagamento
                });
            }

            string titulo    = "Relatório de Vendas por Período";
            string subtitulo = $"Período: {dataIni:dd/MM/yyyy} até {dataFim:dd/MM/yyyy}  |  " +
                               $"Total: R$ {totalGeral:F2}  |  Registros: {filtradas.Count}";

            ExibirPreview(titulo, subtitulo, colunas, larguras, linhas);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  RELATÓRIO 2 — Produtos Cadastrados
        // ═════════════════════════════════════════════════════════════════════
        public static void ImprimirProdutos()
        {
            var produtos = ProdutoService.ListarTodos();

            var colunas  = new[] { "ID", "Nome", "Valor", "Estoque", "Cadastro", "Status" };
            var larguras = new[] { 40, 130, 130, 90, 90, 70 };

            var linhas = new List<string[]>();
            foreach (var p in produtos)
            {
                linhas.Add(new[]
                {
                    p.Id.ToString(),
                    p.Nome,
                    $"R$ {p.Valor:F2}",
                    p.Estoque.ToString(),
                    p.Data,
                    p.Ativo ? "Ativo" : "Inativo"
                });
            }

            string subtitulo = $"Total: {produtos.Count} produto(s)  |  " +
                               $"Ativos: {produtos.FindAll(p => p.Ativo).Count}  |  " +
                               $"Inativos: {produtos.FindAll(p => !p.Ativo).Count}";

            ExibirPreview("Relatório de Estoque de Produtos", subtitulo, colunas, larguras, linhas);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  RELATÓRIO 3 — Clientes (Alunos) Cadastrados
        // ═════════════════════════════════════════════════════════════════════
        public static void ImprimirClientes()
        {
            var alunos = AlunoService.ListarTodos();

            var colunas  = new[] { "ID", "Nome", "Sala", "Telefone", "Cadastro", "Status" };
            var larguras = new[] { 40, 170, 50, 110, 90, 70 };

            var linhas = new List<string[]>();
            foreach (var a in alunos)
            {
                linhas.Add(new[]
                {
                    a.Id.ToString(),
                    a.Nome,
                    a.Sala,
                    a.Telefone,
                    a.Data,
                    a.Ativo ? "Ativo" : "Inativo"
                });
            }

            string subtitulo = $"Total: {alunos.Count} cliente(s)  |  " +
                               $"Ativos: {alunos.FindAll(a => a.Ativo).Count}  |  " +
                               $"Inativos: {alunos.FindAll(a => !a.Ativo).Count}";

            ExibirPreview("Relatório de Cadastro de Clientes", subtitulo, colunas, larguras, linhas);
        }


        // ═════════════════════════════════════════════════════════════════════
        //  RELATÓRIO 4 — Caixa do Dia
        // ═════════════════════════════════════════════════════════════════════
        public static void ImprimirCaixaDia(DateTime data)
        {
            var todasVendas = DatabaseHelper.ListarVendas();
            var vendas = new List<Venda>();
            foreach (var v in todasVendas)
                if (DateTime.TryParse(v.Data, out DateTime dv) && dv.Date == data.Date)
                    vendas.Add(v);

            var totaisPgto = new Dictionary<string, decimal>();
            decimal totalGeral = 0;
            foreach (var v in vendas)
            {
                string pgto = string.IsNullOrWhiteSpace(v.FormaPagamento) ? "Não informado" : v.FormaPagamento;
                if (!totaisPgto.ContainsKey(pgto)) totaisPgto[pgto] = 0;
                totaisPgto[pgto] += v.ValorTotal;
                totalGeral       += v.ValorTotal;
            }

            var colunas  = new[] { "Forma de Pagamento", "Qtd. Vendas", "Total Arrecadado" };
            var larguras = new[] { 200, 100, 160 };
            var linhas   = new List<string[]>();

            foreach (var kv in totaisPgto)
            {
                int qtd = vendas.FindAll(v => (v.FormaPagamento ?? "Não informado") == kv.Key).Count;
                linhas.Add(new[] { kv.Key, qtd.ToString(), $"R$ {kv.Value:F2}" });
            }
            linhas.Add(new[] { "TOTAL GERAL", vendas.Count.ToString(), $"R$ {totalGeral:F2}" });

            string subtitulo = $"Data: {data:dd/MM/yyyy}  |  Vendas: {vendas.Count}  |  Total: R$ {totalGeral:F2}";
            ExibirPreview("Relatório de Caixa do Dia", subtitulo, colunas, larguras, linhas);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  RELATÓRIO 5 — Produtos Mais Vendidos
        // ═════════════════════════════════════════════════════════════════════
        public static void ImprimirProdutosMaisVendidos(DateTime dataIni, DateTime dataFim)
        {
            var todasVendas = DatabaseHelper.ListarVendas();
            var ranking = new Dictionary<string, (int qtd, decimal total)>();

            foreach (var v in todasVendas)
            {
                if (!DateTime.TryParse(v.Data, out DateTime dv)) continue;
                if (dv.Date < dataIni.Date || dv.Date > dataFim.Date) continue;
                var vComItens = DatabaseHelper.BuscarVendaComItens(v.Id);
                if (vComItens?.Itens == null) continue;
                foreach (var item in vComItens.Itens)
                {
                    string nome = item.NomeProduto ?? $"Produto {item.ProdutoId}";
                    if (!ranking.ContainsKey(nome)) ranking[nome] = (0, 0m);
                    ranking[nome] = (ranking[nome].qtd + item.Quantidade,
                                     ranking[nome].total + item.ValorTotal);
                }
            }

            var lista = new List<(string nome, int qtd, decimal total)>();
            foreach (var kv in ranking)
                lista.Add((kv.Key, kv.Value.qtd, kv.Value.total));
            lista.Sort((a, b) => b.qtd.CompareTo(a.qtd));

            var colunas  = new[] { "Pos.", "Produto", "Qtd. Vendida", "Total Arrecadado" };
            var larguras = new[] { 40, 200, 110, 150 };
            var linhas   = new List<string[]>();
            int pos = 1;
            foreach (var item in lista)
                linhas.Add(new[] { $"{pos++}°", item.nome, item.qtd.ToString(), $"R$ {item.total:F2}" });
            if (linhas.Count == 0)
                linhas.Add(new[] { "-", "Nenhuma venda no período", "-", "-" });

            string subtitulo = $"Período: {dataIni:dd/MM/yyyy} até {dataFim:dd/MM/yyyy}  |  {lista.Count} produto(s)";
            ExibirPreview("Relatório de Produtos Mais Vendidos", subtitulo, colunas, larguras, linhas);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  RELATÓRIO 6 — Vendas por Cliente
        // ═════════════════════════════════════════════════════════════════════
        public static void ImprimirVendasPorCliente(DateTime dataIni, DateTime dataFim)
        {
            var todasVendas = DatabaseHelper.ListarVendas();
            var ranking = new Dictionary<string, (int qtd, decimal total)>();

            foreach (var v in todasVendas)
            {
                if (!DateTime.TryParse(v.Data, out DateTime dv)) continue;
                if (dv.Date < dataIni.Date || dv.Date > dataFim.Date) continue;
                string cliente = string.IsNullOrWhiteSpace(v.Cliente) ? "Não identificado" : v.Cliente;
                if (!ranking.ContainsKey(cliente)) ranking[cliente] = (0, 0m);
                ranking[cliente] = (ranking[cliente].qtd + 1,
                                    ranking[cliente].total + v.ValorTotal);
            }

            var lista = new List<(string nome, int qtd, decimal total)>();
            foreach (var kv in ranking)
                lista.Add((kv.Key, kv.Value.qtd, kv.Value.total));
            lista.Sort((a, b) => b.total.CompareTo(a.total));

            var colunas  = new[] { "Pos.", "Cliente", "Qtd. Compras", "Total Gasto" };
            var larguras = new[] { 40, 210, 110, 140 };
            var linhas   = new List<string[]>();
            int pos = 1;
            foreach (var item in lista)
                linhas.Add(new[] { $"{pos++}°", item.nome, item.qtd.ToString(), $"R$ {item.total:F2}" });
            if (linhas.Count == 0)
                linhas.Add(new[] { "-", "Nenhuma venda no período", "-", "-" });

            string subtitulo = $"Período: {dataIni:dd/MM/yyyy} até {dataFim:dd/MM/yyyy}  |  {lista.Count} cliente(s)";
            ExibirPreview("Relatório de Vendas por Cliente", subtitulo, colunas, larguras, linhas);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  MOTOR DE IMPRESSÃO — genérico para os 3 relatórios
        // ═════════════════════════════════════════════════════════════════════
        private static void ExibirPreview(
            string titulo, string subtitulo,
            string[] colunas, int[] larguras,
            List<string[]> linhas)
        {
            int paginaAtual = 0;
            int linhaAtual  = 0;

            var doc = new PrintDocument();
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.Margins   = new Margins(40, 40, 40, 40);

            doc.PrintPage += (sender, e) =>
            {
                var g      = e.Graphics;
                float pW   = e.PageBounds.Width;
                float pH   = e.PageBounds.Height;
                float x    = MargemEsq;
                float y    = MargemTopo;
                float maxY = pH - MargemTopo - 30; // espaço para rodapé
                paginaAtual++;

                // ── Cabeçalho da página ───────────────────────────────────────
                // Faixa verde
                g.FillRectangle(new SolidBrush(CorVerde),
                    x, y, pW - MargemEsq - MargemDir, 56);

                g.DrawString("Prefeitura Municipal de Potirendaba",
                    FntSubtitulo, Brushes.White, x + 8, y + 4);
                g.DrawString(titulo,
                    FntTitulo, Brushes.White, x + 8, y + 18);
                g.DrawString($"Emissão: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    FntSubtitulo, Brushes.White,
                    pW - MargemDir - 180, y + 4);

                y += 60;

                // Subtítulo (totais/período)
                g.DrawString(subtitulo, FntSubtitulo,
                    new SolidBrush(Color.FromArgb(60, 60, 60)), x, y);
                y += 18;

                // Linha separadora
                g.DrawLine(new Pen(CorBorda, 1), x, y, pW - MargemDir, y);
                y += 6;

                // ── Cabeçalho das colunas ─────────────────────────────────────
                float tabelaW = pW - MargemEsq - MargemDir;
                g.FillRectangle(new SolidBrush(Color.FromArgb(210, 232, 210)),
                    x, y, tabelaW, AltCabeca);

                // Borda superior e inferior do cabeçalho
                g.DrawLine(new Pen(CorBorda, 1f), x, y, x + tabelaW, y);
                g.DrawLine(new Pen(CorBorda, 1f), x, y + AltCabeca, x + tabelaW, y + AltCabeca);

                float cx = x;
                for (int i = 0; i < colunas.Length; i++)
                {
                    // Linha vertical esquerda de cada coluna
                    g.DrawLine(new Pen(CorBorda, 0.8f), cx, y, cx, y + AltCabeca);

                    g.DrawString(colunas[i], FntCabCol,
                        new SolidBrush(Color.FromArgb(20, 20, 20)),
                        new RectangleF(cx + 6, y + 7, larguras[i] - 8, AltCabeca),
                        StringFormat.GenericDefault);
                    cx += larguras[i];
                }
                // Linha vertical direita
                g.DrawLine(new Pen(CorBorda, 0.8f), cx, y, cx, y + AltCabeca);
                y += AltCabeca;

                // ── Linhas de dados ───────────────────────────────────────────
                bool alternar = false;
                while (linhaAtual < linhas.Count)
                {
                    // Verificar se cabe mais uma linha
                    if (y + AltLinha > maxY) break;

                    // Fundo alternado
                    if (alternar)
                        g.FillRectangle(new SolidBrush(CorCinzaClr),
                            x, y, pW - MargemEsq - MargemDir, AltLinha);

                    // Borda inferior de cada linha
                    g.DrawLine(new Pen(Color.FromArgb(200, 210, 200), 0.8f),
                        x, y + AltLinha - 1, x + tabelaW, y + AltLinha - 1);

                    // Células + linhas verticais
                    cx = x;
                    var linha = linhas[linhaAtual];
                    for (int col = 0; col < colunas.Length && col < linha.Length; col++)
                    {
                        // Linha vertical esquerda
                        g.DrawLine(new Pen(CorBorda, 0.8f), cx, y, cx, y + AltLinha);

                        // Colorir status
                        Brush br = Brushes.Black;
                        if (colunas[col] == "Status")
                            br = linha[col] == "Ativo"
                                ? new SolidBrush(Color.FromArgb(30, 120, 50))
                                : new SolidBrush(Color.FromArgb(160, 40, 40));

                        g.DrawString(linha[col], FntDado, br,
                            new RectangleF(cx + 6, y + 5, larguras[col] - 8, AltLinha),
                            StringFormat.GenericDefault);
                        cx += larguras[col];
                    }
                    // Linha vertical direita
                    g.DrawLine(new Pen(CorBorda, 0.8f), cx, y, cx, y + AltLinha);

                    y += AltLinha;
                    linhaAtual++;
                    alternar = !alternar;
                }

                // ── Borda externa da tabela ──────────────────────────────────
                g.DrawRectangle(new Pen(CorBorda, 1.2f),
                    x, MargemTopo + 60 + 24,
                    tabelaW,
                    y - (MargemTopo + 60 + 24));

                // ── Rodapé ────────────────────────────────────────────────────
                string rodape = $"Página {paginaAtual}  |  Sistema PotirendabaApp  |  " +
                                $"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}";
                g.DrawLine(new Pen(CorBorda, 0.5f),
                    x, pH - MargemTopo - 20, pW - MargemDir, pH - MargemTopo - 20);
                g.DrawString(rodape, FntRodape,
                    new SolidBrush(Color.FromArgb(120, 120, 120)),
                    x, pH - MargemTopo - 16);

                // Há mais páginas?
                e.HasMorePages = linhaAtual < linhas.Count;
            };

            // ── Exibir preview antes de imprimir ─────────────────────────────
            using var preview = new PrintPreviewDialog
            {
                Document    = doc,
                Text        = titulo,
                WindowState = FormWindowState.Maximized,
                ShowIcon    = false
            };

            // Remover botão fechar do PrintPreview (opcional)
            preview.StartPosition = FormStartPosition.CenterScreen;
            preview.ShowDialog();
        }
    }
}
