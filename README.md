# CRUD de Contatos - .NET 8 (Arquitetura Modular por Dominio)

## 1. Visao geral

Este projeto implementa uma API REST de contatos em .NET 8 com foco em separacao clara de responsabilidades por modulo de dominio.

Objetivos da refatoracao:

- Organizar o codigo em src/Features e src/Shared.
- Eliminar estrutura por pasta de caso de uso.
- Preservar contrato HTTP e comportamento funcional.
- Manter stack atual (ASP.NET Core + EF Core SQL Server).

## 2. Stack

- .NET 8
- ASP.NET Core Web API (Controllers)
- Entity Framework Core 8 (SQL Server)
- Swagger / OpenAPI
- xUnit
- Moq
- Microsoft.AspNetCore.Mvc.Testing
- EF Core InMemory (testes)

## 3. Estrutura do projeto

```text
crud-net/
├── crud-net.sln
├── README.md
├── crud-net/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── crud-net.csproj
│   └── src/
│       ├── Features/
│       │   └── Contacts/
│       │       ├── Controllers/
│       │       │   └── ContactController.cs
│       │       ├── UseCases/
│       │       │   ├── CreateContactUseCase.cs
│       │       │   ├── ListActiveContactsUseCase.cs
│       │       │   ├── GetActiveContactDetailsUseCase.cs
│       │       │   ├── UpdateActiveContactUseCase.cs
│       │       │   ├── ActivateContactUseCase.cs
│       │       │   ├── DeactivateContactUseCase.cs
│       │       │   └── DeleteContactUseCase.cs
│       │       ├── DTOs/
│       │       │   ├── CreateContactInputDto.cs
│       │       │   ├── UpdateActiveContactInputDto.cs
│       │       │   ├── ContactResponseDto.cs
│       │       │   └── ContactListItemResponseDto.cs
│       │       ├── Domain/
│       │       │   ├── Entities/
│       │       │   ├── Errors/
│       │       │   └── Services/
│       │       ├── Repositories/
│       │       │   └── IContactRepository.cs
│       │       ├── Infrastructure/
│       │       │   └── Persistence/
│       │       │       ├── AppDbContext.cs
│       │       │       ├── Configurations/
│       │       │       └── Repositories/
│       │       ├── Validations/
│       │       └── Mappings/
│       └── Shared/
│           └── Errors/
└── tests/
    └── crud-net.Tests/
```

## 4. Responsabilidades por camada

- Controllers: recebe HTTP, valida entrada e chama use case.
- UseCases: concentra regra de negocio da acao.
- Domain: entidade, servicos de dominio, invariantes e erros.
- Repositories: contrato de persistencia.
- Infrastructure: implementacao concreta com EF Core.
- DTOs: payload de entrada e saida da API.
- Shared: somente itens realmente compartilhados entre modulos.

## 5. Rotas e status

Base path: /api/contacts

- POST /api/contacts -> 201
- GET /api/contacts -> 200
- GET /api/contacts/{id} -> 200
- PUT /api/contacts/{id} -> 200
- PATCH /api/contacts/{id}/activate -> 200
- PATCH /api/contacts/{id}/deactivate -> 200
- DELETE /api/contacts/{id} -> 204

## 6. Regras de negocio preservadas

- Nome obrigatorio e minimo de 3 caracteres.
- Data de nascimento valida e nao futura.
- Contato precisa ser maior de idade (18+).
- Sexo permitido: MASCULINO, FEMININO, OUTRO (internamente enum Male, Female, Other; NotSpecified invalido).
- Listagem e detalhe retornam somente contatos ativos.
- Nao permite atualizar contato inativo.
- Exclusao logica (soft delete).

## 7. Padrao de erro

Tratamento global converte excecoes de dominio em resposta padronizada:

```json
{
  "code": "CONTACT_VALIDATION_ERROR",
  "message": "Contact validation failed.",
  "errors": {
    "DateOfBirth": ["Contact must be an adult."]
  }
}
```

Codigos suportados:

- CONTACT_VALIDATION_ERROR -> 400
- CONTACT_NOT_FOUND -> 404
- INVALID_ID -> 400
- INTERNAL_SERVER_ERROR -> 500

## 8. Como rodar a aplicacao

### 8.1 Pre-requisitos

- SDK .NET 8
- SQL Server disponivel

### 8.2 Connection string

Configurar em crud-net/appsettings.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=SEU_BANCO;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Ou por variavel de ambiente:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
```

### 8.3 Executar

```bash
dotnet restore
dotnet run --project ./crud-net/crud-net.csproj
```

Swagger: /swagger

## 9. Testes

Executar todos:

```bash
dotnet test ./tests/crud-net.Tests/crud-net.Tests.csproj
```

Cobertura implementada:

- Unitarios de dominio (Contact e ContactAgeCalculator).
- Unitarios de validacao (ContactInputValidator).
- Unitarios de use cases com mock de repositorio (Moq).
- Integracao HTTP cobrindo os 7 fluxos principais + cenarios de erro padronizado.
- Integracao de repositorio com EF Core InMemory.

## 10. Observacoes de arquitetura

- Sem pasta por caso de uso.
- Modulo de contatos isolado em src/Features/Contacts.
- Itens compartilhados globais em src/Shared.
- Sem migracao de stack: EF Core e SQL Server mantidos.
