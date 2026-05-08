using System.Drawing;
using System.Windows.Forms;
using PotirendabaApp.Services;

namespace PotirendabaApp.Services
{
    /// <summary>
    /// Percorre a árvore de controles de um Form e aplica as cores do TemaService.
    /// Chame AplicarEm(this) no Load e no handler de TemaAlterado.
    /// </summary>
    public static class TemaAplicador
    {
        public static void AplicarEm(Form form)
        {
            form.BackColor = TemaService.FundoForm;
            AplicarRecursivo(form.Controls);
        }

        private static void AplicarRecursivo(Control.ControlCollection controles)
        {
            foreach (Control c in controles)
            {
                switch (c)
                {
                    // ── Painéis de conteúdo ───────────────────────────────────
                    case Panel p when p.Tag?.ToString() == "conteudo":
                        p.BackColor = TemaService.FundoPainel;
                        break;

                    // ── Labels comuns ─────────────────────────────────────────
                    case Label lbl when lbl.Tag?.ToString() == "principal":
                        lbl.ForeColor = TemaService.TextoPrincipal;
                        lbl.BackColor = Color.Transparent;
                        break;

                    case Label lbl when lbl.Tag?.ToString() == "secundario":
                        lbl.ForeColor = TemaService.TextoSecundario;
                        lbl.BackColor = Color.Transparent;
                        break;

                    case Label lbl when lbl.BackColor != TemaService.Verde
                                     && lbl.BackColor != TemaService.VerdeDark:
                        lbl.ForeColor = TemaService.TextoPrincipal;
                        lbl.BackColor = Color.Transparent;
                        break;

                    // ── TextBoxes editáveis ───────────────────────────────────
                    case TextBox txt when !txt.ReadOnly:
                        txt.BackColor = TemaService.FundoInput;
                        txt.ForeColor = TemaService.TextoPrincipal;
                        break;

                    // ── TextBoxes somente-leitura ─────────────────────────────
                    case TextBox txt when txt.ReadOnly:
                        txt.BackColor = TemaService.FundoInputRO;
                        txt.ForeColor = TemaService.TextoPrincipal;
                        break;

                    // ── ComboBox ──────────────────────────────────────────────
                    case ComboBox cmb:
                        cmb.BackColor = TemaService.FundoInput;
                        cmb.ForeColor = TemaService.TextoPrincipal;
                        break;

                    // ── CheckBox ─────────────────────────────────────────────
                    case CheckBox chk:
                        chk.ForeColor = TemaService.TextoPrincipal;
                        chk.BackColor = Color.Transparent;
                        break;

                    // ── DataGridView ──────────────────────────────────────────
                    case DataGridView grid:
                        AplicarGrid(grid);
                        break;

                    // ── Painéis genéricos (não barra verde) ───────────────────
                    case Panel pan when pan.BackColor != TemaService.Verde
                                    && pan.BackColor != TemaService.VerdeDark:
                        pan.BackColor = TemaService.FundoForm;
                        break;
                }

                // Recursão nos filhos
                if (c.HasChildren) AplicarRecursivo(c.Controls);
            }
        }

        public static void AplicarGrid(DataGridView grid)
        {
            grid.BackgroundColor = TemaService.FundoPainel;

            // Células normais
            grid.DefaultCellStyle.BackColor          = TemaService.FundoPainel;
            grid.DefaultCellStyle.ForeColor          = TemaService.TextoPrincipal;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(180, 220, 180);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 20, 20);

            // Linhas alternadas
            grid.AlternatingRowsDefaultCellStyle.BackColor = TemaService.GridAlternada;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = TemaService.TextoPrincipal;

            // Cabeçalho — fundo cinza claro e texto SEMPRE preto (legível nos dois temas)
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 200, 200);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(20, 20, 20);
            grid.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            grid.EnableHeadersVisualStyles               = false;

            grid.GridColor = TemaService.Borda;
            grid.Invalidate();
        }
    }
}
