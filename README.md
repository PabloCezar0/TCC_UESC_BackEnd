# 💼 Plataforma Web de Cálculo de Comissões — Rota Transportes

> Back-end em .NET para automação, centralização e auditoria do cálculo de comissões de agências parceiras.

---

## 📋 Sobre o Projeto

Este sistema foi desenvolvido como Trabalho de Conclusão de Curso (TCC) na **Universidade Estadual de Santa Cruz (UESC)**, no contexto do **Programa de Residência de Software** — uma parceria entre o CEPEDI e a empresa **Rota Transportes**.

O objetivo principal foi substituir um processo manual baseado em planilhas eletrônicas por uma **API RESTful robusta, auditável e escalável**, capaz de:

- Importar automaticamente dados de vendas de uma API externa
- Aplicar regras de comissionamento dinâmicas por agência
- Processar deduções fiscais (notas fiscais)
- Registrar um histórico completo de auditoria de todas as operações

---

## 🚨 Problema Resolvido

O processo anterior dependia de planilhas descentralizadas, o que causava:

- **Erros financeiros** por digitação e quebra de fórmulas
- **Ineficiência operacional**: fechamento contábil atrasado mensalmente
- **Falta de rastreabilidade**: impossível auditar quem alterou o quê e quando
- **Desgaste nas relações comerciais** com as agências parceiras por divergências nos valores

---

## 🏗️ Arquitetura

O projeto adota **Clean Architecture**, organizado em 6 camadas:

```
Rota.Domain          → Entidades e regras de negócio puras (sem dependências externas)
Rota.Application     → Casos de uso, serviços e DTOs
Rota.Infra.Data      → Entity Framework Core + MySQL (repositórios e migrações)
Rota.Infrastructure  → Serviços externos (AWS S3)
Rota.Infra.IoC       → Configuração de Injeção de Dependência
Rota.API             → Controllers, Middlewares, JWT, Swagger
```

---

## 🛠️ Tecnologias

| Categoria | Tecnologia |
|---|---|
| Linguagem | C# |
| Plataforma | .NET 8 |
| Banco de Dados | MySQL 8.0 |
| ORM | Entity Framework Core (Code-First) |
| Armazenamento de Arquivos | AWS S3 |
| Autenticação | JWT (JSON Web Token) |
| Hash de Senhas | Argon2 |
| Documentação da API | Swagger / OpenAPI (Swashbuckle) |
| Containerização | Docker Compose |
| Gerenciamento de Tarefas | AirTable (Kanban) |

---

## ✅ Funcionalidades

### Módulo de Usuários e Autenticação
- Login com e-mail e senha via JWT stateless
- Controle de acesso baseado em perfis (RBAC): **Administrador**, **Financeiro** e **Gestor**
- Cadastro, edição e soft delete de usuários
- Definição de senha via link enviado por e-mail com token

### Módulo de Agências
- CRUD de agências parceiras
- Configuração individual de taxas de comissão por categoria (Passagem, Seguro, Encomenda, Link, Volume Especial)
- Vinculação de regra de comissão personalizada ou uso do cálculo padrão
- Recálculo manual de comissão por agência e período

### Módulo de Transações
- Importação automática de vendas via API externa (agendada para o dia 26 de cada mês)
- Importação manual avulsa para recuperação de dados legados ou falhas
- Cadastro de transações manuais com flag `IsManual = true`
- Transações automáticas são imutáveis; apenas manuais podem ser editadas ou excluídas
- Qualquer alteração dispara recálculo automático da comissão da agência

### Módulo de Notas Fiscais
- Cadastro de notas fiscais com upload de PDF via `multipart/form-data`
- Arquivo armazenado no AWS S3; apenas a URL é salva no banco
- Valor da nota é deduzido automaticamente no cálculo de comissão
- Inclusão ou remoção de nota dispara recálculo imediato

### Módulo de Comissões
- Cálculo em lote (batch processing) por agência e período
- Fórmula padrão: soma de cada categoria × sua taxa percentual − total de notas fiscais
- Dashboard analítico com KPIs: Total de Vendas, Total de Descontos e Comissão Líquida
- Detalhamento por categoria com drill-down até os lançamentos individuais

### Módulo de Regras Personalizadas
- Criação de fórmulas matemáticas dinâmicas armazenadas como texto no banco
- Motor de cálculo interpreta as expressões em tempo de execução
- Ambiente de simulação (Sandbox / Preview) para testar a fórmula antes de salvar
- Validação por whitelist via expressão regular para prevenir injeção de código
- Guia de variáveis integrado (VP, VL, VS, VV, VE, TP, TL, TS, TV, TE, DED)

### Módulo de Auditoria
- Log automático de todas as operações de escrita (Create, Update, Delete) via `AuditingInterceptor` no EF Core
- Registra: usuário responsável, data/hora, tabela, ação, valores antigos e novos
- Distingue entre ações de usuários logados e ações automáticas do sistema
- Recálculo gera trilha forense: Delete do valor antigo + Create do valor novo

---

## 📐 Modelo de Dados

O banco de dados gira em torno da tabela `agencies` como pivô central, relacionando-se com:

- `transactions` — vendas brutas (automáticas e manuais)
- `invoices` — notas fiscais de dedução
- `invoicefiles` — metadados e URLs dos PDFs no S3
- `monthlycommissions` — resultado consolidado por agência/mês
- `commissionrules` — fórmulas personalizadas de cálculo
- `users` — controle de acesso com RBAC
- `auditlogs` — histórico imutável de todas as operações

> Todos os valores financeiros utilizam o tipo `DECIMAL(18,2)` para evitar erros de arredondamento de ponto flutuante.

---

## 🔐 Segurança

- **JWT stateless**: cada requisição carrega o token assinado com as permissões do usuário, sem necessidade de sessão no servidor
- **Argon2**: vencedor da Password Hashing Competition, resistente a ataques de força bruta por GPU/ASIC
- **Soft Delete**: usuários removidos são marcados como inativos, sem apagar o registro
- **Whitelist de fórmulas**: expressão regular valida as fórmulas personalizadas antes da execução, prevenindo SQL Injection e execução de código malicioso

---

## 🚀 Como Executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker e Docker Compose](https://docs.docker.com/compose/)
- Credenciais AWS S3 configuradas

### Subindo o ambiente

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/seu-repositorio.git
cd seu-repositorio

# Suba os containers (MySQL + API + serviço de migração)
docker compose up --build
```

> O container `rota_migrations` executa automaticamente todas as migrações pendentes antes de a API iniciar, garantindo que o banco esteja sempre sincronizado.

### Acessando a documentação

Com a aplicação rodando, acesse o Swagger em:

```
http://localhost:{porta}/swagger
```

---

## 📌 Principais Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/token/LoginUser` | Autenticação — retorna token JWT |
| `POST` | `/api/transactions/import-by-date` | Importação automática de transações por período |
| `POST` | `/api/transactions/manual` | Cadastro de transação manual |
| `POST` | `/api/invoices` | Upload de nota fiscal (multipart/form-data) |
| `GET` | `/api/monthlycommissions` | Consulta de comissões consolidadas |
| `PATCH` | `/api/agency/{id}/commission-rule` | Vincula regra de comissão a uma agência |

---

## 📊 Metodologia

O desenvolvimento seguiu uma abordagem ágil híbrida (**Scrumban**):

- **Scrum**: Sprints, Daily Scrums e Sprint Reviews para alinhamento e feedback contínuo
- **Kanban**: Quadro visual no AirTable para gestão do fluxo de tarefas e visibilidade do progresso

---

## 👨‍💻 Autor

**Pablo Cezar Moreira Carvalho**
Bacharel em Ciência da Computação — UESC

Orientador: Prof. Dr. Álvaro Vinícius de Souza Coêlho

---

## 📄 Licença

Este projeto foi desenvolvido para fins acadêmicos e comerciais no contexto da Residência de Software CEPEDI × Rota Transportes.
