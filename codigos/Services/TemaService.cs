using System.Drawing;

namespace PotirendabaApp.Services
{
    /// <summary>
    /// Serviço singleton de tema (Claro / Escuro).
    /// Todas as telas consultam aqui para aplicar cores consistentes.
    /// </summary>
    public static class TemaService
    {
        // ── Estado global ─────────────────────────────────────────────────────
        public static bool ModoEscuro { get; private set; } = false;

        // ── Evento disparado ao trocar tema ───────────────────────────────────
        public static event System.Action TemaAlterado;

        public static void Alternar()
        {
            ModoEscuro = !ModoEscuro;
            TemaAlterado?.Invoke();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PALETA DINÂMICA — use estas propriedades em vez de cores fixas
        // ══════════════════════════════════════════════════════════════════════

        // Fundo principal da form
        public static Color FundoForm =>
            ModoEscuro ? Color.FromArgb(28, 28, 30) : Color.FromArgb(245, 248, 245);

        // Fundo de painéis e cards
        public static Color FundoPainel =>
            ModoEscuro ? Color.FromArgb(44, 44, 46) : Color.White;

        // Fundo de inputs
        public static Color FundoInput =>
            ModoEscuro ? Color.FromArgb(58, 58, 62) : Color.White;

        // Fundo de inputs somente-leitura
        public static Color FundoInputRO =>
            ModoEscuro ? Color.FromArgb(48, 48, 52) : Color.FromArgb(245, 245, 245);

        // Texto principal
        public static Color TextoPrincipal =>
            ModoEscuro ? Color.FromArgb(230, 230, 230) : Color.FromArgb(30, 30, 30);

        // Texto secundário / labels
        public static Color TextoSecundario =>
            ModoEscuro ? Color.FromArgb(160, 160, 165) : Color.FromArgb(100, 100, 100);

        // Borda de inputs e separadores
        public static Color Borda =>
            ModoEscuro ? Color.FromArgb(70, 70, 75) : Color.FromArgb(200, 200, 200);

        // Cabeçalho do DataGridView
        public static Color GridCabecalho =>
            ModoEscuro ? Color.FromArgb(55, 55, 58) : Color.FromArgb(200, 200, 200);

        // Linha alternada do grid
        public static Color GridAlternada =>
            ModoEscuro ? Color.FromArgb(52, 52, 55) : Color.FromArgb(240, 248, 240);

        // Verde principal (fixo nos dois temas)
        public static Color Verde     => Color.FromArgb(56, 161, 78);
        public static Color VerdeDark => Color.FromArgb(38, 120, 56);

        // Ícone do botão de tema
        public static string IconeTema => ModoEscuro ? "☀" : "🌙";
        public static string TooltipTema => ModoEscuro ? "Tema Claro" : "Tema Escuro";
    }
}
