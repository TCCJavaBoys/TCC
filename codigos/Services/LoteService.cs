using System;
using System.Collections.Generic;
using System.Linq;
using PotirendabaApp.Data;
using PotirendabaApp.Models;

namespace PotirendabaApp.Services
{
    /// <summary>Ação sugerida para um lote próximo do vencimento.</summary>
    public enum AcaoSugerida { Nenhuma, Promocao, Descarte }

    /// <summary>Resultado do débito de um lote específico durante uma venda (usado para montar o ItemVenda correspondente).</summary>
    public class ConsumoLote
    {
        public int LoteId     { get; set; }
        public int Quantidade { get; set; }
    }

    public static class LoteService
    {
        // Limiares usados para decidir a ação sugerida. Centralizados aqui
        // para poderem ser ajustados/justificados em um único lugar (3.1/3.3).
        public const int DiasLimitePromocao = 7; // <= 7 dias: sugerir promoção
        public const int DiasLimiteDescarte = 3; // <= 3 dias (ou vencido): sugerir descarte

        // ── Leitura ───────────────────────────────────────────────────────────
        public static List<Lote> Listar(int? produtoId = null, bool somenteAtivos = true)
            => DatabaseHelper.ListarLotes(produtoId, somenteAtivos);

        /// <summary>Próximo ID sequencial, para exibir no cadastro antes de salvar (mesmo padrão do ProdutoService.ProximoId).</summary>
        public static int ProximoId()
        {
            var todos = DatabaseHelper.ListarLotes(somenteAtivos: false);
            int proximo = 1;
            foreach (var l in todos)
                if (l.Id >= proximo) proximo = l.Id + 1;
            return proximo;
        }

        /// <summary>Lotes com estoque > 0 cuja validade está dentro da janela de alerta, já ordenados por FEFO.</summary>
        public static List<Lote> ListarProximosValidade(int diasAntecedencia = DiasLimitePromocao)
            => DatabaseHelper.ListarLotes(somenteAtivos: true)
                .Where(l => l.Quantidade > 0 && l.DiasParaVencer <= diasAntecedencia)
                .OrderBy(l => l.DiasParaVencer)
                .ToList();

        public static AcaoSugerida Avaliar(Lote lote)
        {
            if (lote.DiasParaVencer <= DiasLimiteDescarte) return AcaoSugerida.Descarte;
            if (lote.DiasParaVencer <= DiasLimitePromocao) return AcaoSugerida.Promocao;
            return AcaoSugerida.Nenhuma;
        }

        // ── Escrita ───────────────────────────────────────────────────────────
        public static int Inserir(Lote lote)
        {
            if (string.IsNullOrWhiteSpace(lote.DataEntrada))
                lote.DataEntrada = DateTime.Now.ToString("yyyy-MM-dd");
            return DatabaseHelper.InserirLote(lote);
        }

        public static void Atualizar(Lote lote) => DatabaseHelper.AtualizarLote(lote);

        /// <summary>
        /// Núcleo da regra FEFO: consome "quantidade" unidades do produto,
        /// sempre priorizando o lote com validade mais próxima primeiro.
        /// Retorna a lista de (LoteId, Quantidade) efetivamente debitados,
        /// que o VendaService usa para gerar uma linha de ItemVenda por lote.
        /// </summary>
        public static List<ConsumoLote> ConsumirFEFO(int produtoId, int quantidade)
        {
            var consumos = new List<ConsumoLote>();
            var lotes = DatabaseHelper.ListarLotes(produtoId, somenteAtivos: true)
                .Where(l => l.Quantidade > 0)
                .OrderBy(l => l.DataValidade) // FEFO: vence primeiro, sai primeiro
                .ToList();

            int restante = quantidade;
            foreach (var lote in lotes)
            {
                if (restante <= 0) break;
                int consumir = Math.Min(restante, lote.Quantidade);
                DatabaseHelper.DebitarLote(lote.Id, consumir);
                consumos.Add(new ConsumoLote { LoteId = lote.Id, Quantidade = consumir });
                restante -= consumir;
            }

            // Se não havia lote suficiente cadastrado para cobrir a venda
            // (ex.: produto sem controle de lote ainda migrado), a diferença
            // fica sem LoteId — mantém o sistema funcionando sem travar o PDV.
            if (restante > 0)
                consumos.Add(new ConsumoLote { LoteId = 0, Quantidade = restante });

            return consumos;
        }
    }
}
