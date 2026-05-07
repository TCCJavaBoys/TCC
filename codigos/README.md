# Potirendaba – Prefeitura Municipal
Sistema Desktop C# / Windows Forms / SQLite

## Estrutura de arquivos

```
PotirendabaApp/
├── Program.cs                          ← Entrada + InicializarBanco()
├── MainForm.cs                         ← Tela inicial com menu lateral
├── RoundButton.cs                      ← Controle customizado
├── PotirendabaApp.csproj               ← .NET 8 + Microsoft.Data.Sqlite
│
├── Models/
│   ├── Aluno.cs                        ← Modelo de Aluno
│   └── Produto.cs                      ← Modelo de Produto
│
├── Data/
│   └── DatabaseHelper.cs               ← DAL: CRUD Alunos + Produtos
│
└── Forms/
    ├── ListagemAlunosForm.cs           ← Grid + filtros de Alunos
    ├── CadastroAlunoForm.cs            ← Cadastro/edição de Aluno
    ├── ListagemProdutosForm.cs         ← Grid + filtros de Produtos
    └── CadastroProdutoForm.cs          ← Cadastro/edição de Produto
```

## Requisitos

| Item       | Versão mínima         |
|------------|-----------------------|
| .NET SDK   | 8.0                   |
| SO         | Windows 10/11         |
| Visual Studio | 2022 (opcional)    |

## Como executar

### Linha de comando
```bash
cd PotirendabaApp
dotnet run
```

### Visual Studio 2022
Abra `PotirendabaApp.csproj` → pressione **F5**

O arquivo `potirendaba.db` (SQLite) é criado automaticamente
na mesma pasta do executável na primeira execução.

## Navegação

```
MainForm
├── 👤 Aluno   → ListagemAlunosForm   → CadastroAlunoForm
└── 📦 Estoque → ListagemProdutosForm → CadastroProdutoForm
```

## Fluxo de salvamento (ambos os módulos)

1. Usuário clica **Salvar**
2. "Voce deseja salvar o cadastro?" → Sim/Não
3. Se **Sim** → salva no SQLite
4. "Voce deseja continuar cadastrando?" → Sim/Não
   - **Sim** → limpa campos, permite novo cadastro
   - **Não** → fecha e atualiza o grid automaticamente

## Personalizar links

Em `MainForm.cs` troque os valores abaixo:
```csharp
OpenUrl("https://wa.me/5517999999999");           // WhatsApp
OpenUrl("https://www.facebook.com/SuaPagina");    // Facebook
OpenUrl("https://github.com/seu-usuario/repo");   // GitHub
```

## Tabelas do banco

```sql
Alunos  (Id, Nome, Telefone, Data, Sala)
Produtos(Id, Nome, Data, Valor REAL, Estoque INTEGER)
```
