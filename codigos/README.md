# Documentação Técnica do Código

## 📁 Estrutura de Pastas

src/
│
├── MiniERP.UI
├── MiniERP.Application
├── MiniERP.Domain
└── MiniERP.Infrastructure

---

# 📦 Camadas

## UI
Responsável pela interface gráfica (WinForms).

Contém:
- Forms
- Eventos de botão
- Validações básicas de entrada

---

## Application
Responsável por:

- Regras de negócio
- Orquestração de serviços
- Comunicação entre UI e Domain

---

## Domain
Contém:

- Entidades
- Regras de domínio
- Modelos de negócio

Exemplo:

Cliente.cs
Produto.cs
Venda.cs

---

## Infrastructure

Responsável por:

- Acesso ao banco de dados
- Repositórios
- Conexão SQLite

---

# 🔐 Boas Práticas Utilizadas

- Separação de responsabilidades
- Nomenclatura padronizada
- Métodos curtos e objetivos
- Uso de DTOs quando necessário
- Evitar lógica de negócio na UI

---

# ⚙ Padrões Aplicados

- Repository Pattern
- Camadas bem definidas
- Injeção de Dependência (se aplicável)

---

# 🧪 Testes

(Adicionar caso implemente testes futuramente)
