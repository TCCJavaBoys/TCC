# Documentação do Projeto - Mini ERP

## 📌 Visão Geral

Este documento descreve a estrutura técnica, modelagem de dados e decisões arquiteturais do sistema Mini ERP.

---

# 🏗 Arquitetura

O sistema foi desenvolvido utilizando Arquitetura em Camadas:

- UI (WinForms)
- Application (Serviços)
- Domain (Entidades)
- Infrastructure (Repositórios / Banco de Dados)

Essa separação permite:

- Manutenção facilitada
- Melhor organização
- Testabilidade
- Escalabilidade futura

---

# 🗄 Banco de Dados

## 🔹 Padrão de Nomenclatura

- Tabelas no singular
- PascalCase
- Chave primária: Id
- Chaves estrangeiras: NomeTabelaId

Exemplo:

Cliente
Produto
Venda
VendaItem
ContaReceber

---

## 🔹 Principais Tabelas

### Cliente
- Id
- Nome
- Telefone
- DataCadastro
- Ativo

### Produto
- Id
- Nome
- ValorVenda
- EstoqueAtual
- DataCadastro

### Venda
- Id
- ClienteId
- DataVenda
- ValorTotal
- FormaPagamento

### VendaItem
- Id
- VendaId
- ProdutoId
- Quantidade
- ValorUnitario

### ContaReceber
- Id
- ClienteId
- VendaId
- Valor
- DataVencimento
- DataPagamento
- Status

---

# 🔄 Metodologia

O desenvolvimento seguiu princípios do Scrum:

- Planejamento de funcionalidades
- Divisão em Sprints
- Incrementos evolutivos
- Testes a cada módulo implementado

---

# 📈 Melhorias Futuras

- Controle de Usuário e Permissões
- Relatórios Gerenciais
- Dashboard de Indicadores
- Migração para SQL Server
