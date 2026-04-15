# CRUD de Contatos - .NET 8 (Vertical Slice)

## 1. Visão Geral

Este projeto implementa uma API REST para gerenciamento de contatos, seguindo os requisitos do teste técnico:

- Criar, listar, visualizar, editar, ativar, desativar e excluir contatos.
- Considerar apenas contatos ativos para listagem, visualização e edição.
- Calcular idade em tempo de execução.
- Garantir regras de maioridade e consistência da data de nascimento.

A solução foi estruturada em Vertical Slice por caso de uso, com foco em baixo acoplamento, alta coesão e testabilidade.

## 2. Tecnologias Utilizadas

- .NET 8
- ASP.NET Core Minimal APIs
- Entity Framework Core 8 (SQL Server)
- Swagger / OpenAPI
- xUnit
- Microsoft.AspNetCore.Mvc.Testing
- EF Core InMemory (testes)

## 3. Estrutura do Projeto

```text
crud-net/
├── crud-net.sln
├── README.md
├── .gitignore
├── crud-net/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── crud-net.csproj
│   ├── Features/
│   │   └── Contacts/
│   │       ├── ContactsModule.cs
│   │       ├── Create/
│   │       ├── ListActive/
│   │       ├── GetActiveById/
│   │       ├── UpdateActive/
│   │       ├── Activate/
│   │       ├── Deactivate/
│   │       ├── Delete/
│   │       └── Shared/
│   └── Infrastructure/
│       └── Persistence/
│           ├── AppDbContext.cs
│           ├── Configurations/
│           └── Repositories/
└── tests/
    └── crud-net.Tests/
        ├── Common/
        ├── Integration/
        ├── Persistence/
        ├── ContactAgeCalculatorTests.cs
        └── ContactValidationTests.cs
```

## 4. Como Rodar a Aplicação

### 4.1 Pré-requisitos

- SDK .NET 8 instalado
- SQL Server disponível (local, container ou remoto)

### 4.2 Configuração de conexão

A conexão está em `crud-net/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CrudNet;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Você pode sobrescrever por variável de ambiente:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
```

### 4.3 Restaurar e executar

```bash
dotnet restore
dotnet run --project ./crud-net/crud-net.csproj
```

Ao subir, abra o Swagger na URL exibida no terminal e acesse `/swagger`.

## 5. Endpoints da API

Base path: `/api/contacts`

### 5.1 Criar contato

- `POST /api/contacts`

Exemplo de request:

```json
{
  "name": "Maria Silva",
  "dateOfBirth": "1990-01-10",
  "gender": 1
}
```

### 5.2 Listar contatos ativos

- `GET /api/contacts`

### 5.3 Obter detalhes de contato ativo

- `GET /api/contacts/{id}`

### 5.4 Atualizar contato ativo

- `PUT /api/contacts/{id}`

### 5.5 Ativar contato

- `PATCH /api/contacts/{id}/activate`

### 5.6 Desativar contato

- `PATCH /api/contacts/{id}/deactivate`

### 5.7 Excluir contato

- `DELETE /api/contacts/{id}`

Observação: a exclusão é lógica (soft delete). O registro não é removido fisicamente, mas deixa de aparecer nas consultas normais.

## 6. Regras de Negócio

As regras foram centralizadas em componentes de domínio e validação:

- Nome obrigatório e com no mínimo 3 caracteres.
- Data de nascimento não pode ser maior que a data atual.
- Idade não pode ser igual a 0.
- Contato deve ser maior de idade (>= 18 anos).
- Sexo deve ser um valor válido do enum e diferente de `NotSpecified`.
- Idade é calculada em tempo de execução, não persistida.
- Listagem, visualização e edição consideram apenas contatos ativos.

Enum `Gender`:

- `0` = NotSpecified (inválido para criação/edição)
- `1` = Female
- `2` = Male
- `3` = Other

## 7. Como a Aplicação Funciona Internamente

Fluxo simplificado de uma requisição:

1. Endpoint do slice recebe o request.
2. Validação de regras de entrada.
3. Repositório busca/persiste dados via EF Core.
4. Entidade de domínio aplica comportamento (ativar, desativar, excluir).
5. Mapping para DTO de resposta com idade calculada em runtime.

Esse fluxo evita lógica de negócio espalhada e facilita testes por responsabilidade.

## 8. Arquitetura Adotada e Motivações

### 8.1 Por que Vertical Slice

A estrutura por caso de uso foi escolhida para:

- Organizar por funcionalidade (e não por tipo técnico horizontal).
- Reduzir acoplamento entre features.
- Facilitar evolução incremental e manutenção.
- Melhorar rastreabilidade: cada operação fica no seu slice.

### 8.2 Onde estão as responsabilidades

- `Features/Contacts/*`: casos de uso (endpoints + contratos + validações + mapeamento).
- `Features/Contacts/Shared`: componentes reutilizados entre slices.
- `Infrastructure/Persistence`: acesso a dados e configuração de banco.
- `tests/*`: testes por comportamento (unitários e integração).

## 9. Decisões de Design (SOLID)

- SRP: cada endpoint/slice possui responsabilidade específica.
- OCP: novas operações podem ser adicionadas em novos slices sem alterar os existentes.
- LSP: contratos simples e previsíveis em repositórios/DTOs.
- ISP: interface de repositório enxuta com operações necessárias ao domínio atual.
- DIP: endpoints dependem de abstrações (`IContactRepository`, `IAppClock`), não de detalhes concretos.

## 10. Testes Automatizados

### 10.1 Rodar os testes

```bash
dotnet test ./tests/crud-net.Tests/crud-net.Tests.csproj
```

### 10.2 Tipos de teste implementados

- Unitários:
  - cálculo de idade
  - validações de regra de negócio
- Integração de API (WebApplicationFactory + InMemory):
  - criação válida/inválida
  - comportamento de desativação na listagem
  - exclusão com retorno 404 no GET
  - cenários de ativo/inativo para GET/PUT
  - tentativa de ativar contato menor de idade
- Persistência (EF InMemory):
  - add + get
  - listagem apenas de ativos
  - soft delete respeitando query filter

## 11. Banco de Dados e Migrations

O projeto já está configurado para SQL Server.

Se quiser versionar schema com migrations EF Core:

```bash
dotnet tool install --global dotnet-ef

dotnet ef migrations add InitialCreate \
  --project ./crud-net/crud-net.csproj \
  --startup-project ./crud-net/crud-net.csproj \
  --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update \
  --project ./crud-net/crud-net.csproj \
  --startup-project ./crud-net/crud-net.csproj
```

## 12. Critérios do Teste Técnico Atendidos

- API REST em .NET Core.
- Persistência em banco relacional (SQL Server via EF Core).
- Separação de regras de negócio da apresentação.
- Estrutura arquitetural por Vertical Slice.
- Boas práticas OO e princípios SOLID.
- Cobertura de testes unitários e de integração.

---

Se quiser, posso complementar este README com:

- coleção de exemplos para Postman/Insomnia,
- seção de troubleshooting (erros comuns de conexão SQL Server),
- pipeline sugerido para CI (build + test).
