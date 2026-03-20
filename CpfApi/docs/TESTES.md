# Documentação de Testes — CpfApi

## Visão Geral

O projeto `CpfApi.Tests` contém uma suite xUnit (.NET 8) com **60 testes** cobrindo validators, services, middleware e integração de endpoints. Nenhuma dependência externa é necessária: o `HttpClient` e os serviços de negócio são mockados em memória.

## Estrutura

```
CpfApi.Tests/
├── CpfApi.Tests.csproj
├── Validators/
│   ├── CpfValidatorTests.cs     (12 testes)
│   └── CnpjValidatorTests.cs    (10 testes)
├── Services/
│   ├── CpfServiceTests.cs       (7 testes)
│   └── CnpjServiceTests.cs      (6 testes)
├── Middleware/
│   └── BearerAuthMiddlewareTests.cs  (6 testes)
└── Integration/
    └── ApiIntegrationTests.cs   (8 testes + ApiFactory)
```

**Total: 60 testes | 0 falhas**

---

## Como Rodar

```bash
# A partir da raiz do repositório
cd CpfApi.Tests
dotnet test

# Com output detalhado
dotnet test --verbosity normal

# Filtrar por categoria
dotnet test --filter "FullyQualifiedName~Validators"
dotnet test --filter "FullyQualifiedName~Services"
dotnet test --filter "FullyQualifiedName~Middleware"
dotnet test --filter "FullyQualifiedName~Integration"
```

---

## Testes por Categoria

### Validators (22 testes)

Testes puramente unitários das funções `Strip`, `Format` e `Validate`.

#### `CpfValidatorTests` (12 testes)

| Teste | Descrição |
|---|---|
| `Validate_ValidCpf_ReturnsTrue` | CPF válido nos formatos formatado e sem pontuação |
| `Validate_AllSameDigits_ReturnsFalse` | Rejeita `111.111.111-11`, `000...`, `999...` |
| `Validate_WrongLength_ReturnsFalse` | Rejeita strings com 0, 10 e 12 dígitos |
| `Validate_InvalidCheckDigit_ReturnsFalse` | Rejeita CPFs com 1º ou 2º dígito verificador errado |
| `Strip_FormattedCpf_ReturnsDigitsOnly` | `529.982.247-25` → `52998224725` |
| `Strip_AlreadyStripped_ReturnsSameValue` | Idempotente em strings sem formatação |
| `Format_RawDigits_ReturnsFormatted` | `52998224725` → `529.982.247-25` |
| `Format_AlreadyFormatted_ReturnsFormatted` | Idempotente em string já formatada |
| `Format_WrongLength_ReturnsInputUnchanged` | Retorna entrada original se não tiver 11 dígitos |

#### `CnpjValidatorTests` (10 testes)

| Teste | Descrição |
|---|---|
| `Validate_ValidCnpj_ReturnsTrue` | CNPJ válido em três formatos diferentes |
| `Validate_AllSameDigits_ReturnsFalse` | Rejeita `00000000000000`, `111...`, `999...` |
| `Validate_WrongLength_ReturnsFalse` | Rejeita strings com 13 e 15 dígitos |
| `Validate_InvalidCheckDigit_ReturnsFalse` | Rejeita CNPJs com 1º ou 2º DV errado |
| `Strip_FormattedCnpj_ReturnsDigitsOnly` | `11.222.333/0001-81` → `11222333000181` |
| `Strip_AlreadyStripped_ReturnsSameValue` | Idempotente |
| `Format_RawDigits_ReturnsFormatted` | `11222333000181` → `11.222.333/0001-81` |
| `Format_WrongLength_ReturnsInputUnchanged` | Retorna entrada original se não tiver 14 dígitos |

---

### Services (13 testes)

Testes do `CpfService` e `CnpjService` usando `HttpMessageHandler` customizado — nenhum servidor real é chamado.

#### `CpfServiceTests` (7 testes)

| Teste | Comportamento verificado |
|---|---|
| `ConsultarAsync_SuccessResponse_ReturnsCpfResponse` | HTTP 200 com JSON válido → retorna `CpfResponse` com CPF formatado |
| `ConsultarAsync_NotFound_ThrowsCpfNotFoundException` | HTTP 404 → lança `CpfNotFoundException` |
| `ConsultarAsync_ServerError_ThrowsCpfServiceUnavailableException` | HTTP 500 → lança `CpfServiceUnavailableException` |
| `ConsultarAsync_ErrorStatusInBody_ThrowsCpfServiceUnavailableException` | JSON com `"status": "ERROR"` → lança exceção com a mensagem do campo `message` |
| `ConsultarAsync_NetworkError_ThrowsCpfServiceUnavailableException` | `HttpRequestException` → lança `CpfServiceUnavailableException` |
| `ConsultarAsync_WithToken_AppendsTokenToUrl` | Com `ReceitaWsToken` configurado → URL inclui `?token=...` |
| `ConsultarAsync_WithoutToken_DoesNotAppendToken` | Sem token configurado → URL não inclui `token` |

#### `CnpjServiceTests` (6 testes)

| Teste | Comportamento verificado |
|---|---|
| `ConsultarAsync_SuccessResponse_ReturnsCnpjResponse` | HTTP 200 → retorna `CnpjResponse` com todos os campos mapeados |
| `ConsultarAsync_NotFound_ThrowsCnpjNotFoundException` | HTTP 404 → lança `CnpjNotFoundException` |
| `ConsultarAsync_ServerError_ThrowsCnpjServiceUnavailableException` | HTTP 500 → lança `CnpjServiceUnavailableException` |
| `ConsultarAsync_ErrorStatusInBody_ThrowsCnpjServiceUnavailableException` | JSON com `"status": "ERROR"` → mensagem de erro propagada |
| `ConsultarAsync_NetworkError_ThrowsCnpjServiceUnavailableException` | `HttpRequestException` → lança `CnpjServiceUnavailableException` |
| `ConsultarAsync_WithToken_AppendsTokenToUrl` | `ReceitaWsCnpjToken` configurado → `?token=...` na URL |

---

### Middleware (6 testes)

Testes do `BearerAuthMiddleware` com `DefaultHttpContext` em memória.

| Teste | Comportamento verificado |
|---|---|
| `InvokeAsync_ValidToken_CallsNext` | Token correto → delega para o próximo handler (status 200) |
| `InvokeAsync_MissingAuthHeader_Returns401` | Sem header `Authorization` → 401 |
| `InvokeAsync_WrongToken_Returns401` | Token diferente do configurado → 401, handler não é chamado |
| `InvokeAsync_BearerPrefixMissing_Returns401` | Header sem prefixo `Bearer ` → 401 |
| `InvokeAsync_HealthPath_SkipsAuth` | Rota `/health` → middleware ignorado, handler chamado sem token |
| `InvokeAsync_EmptyConfiguredToken_Returns401` | `ApiToken` vazio na configuração → 401 (serviço mal configurado) |

---

### Integration (8 testes)

Testes de ponta a ponta usando `WebApplicationFactory<Program>`. O `ICpfService` e o `ICnpjService` são substituídos por implementações fake via `ConfigureTestServices` — nenhuma chamada HTTP externa ocorre.

| Teste | Endpoint | Status esperado |
|---|---|---|
| `Health_ReturnsOk` | `GET /health` | 200 |
| `ConsultaCpf_WithoutToken_Returns401` | `GET /consulta/cpf/529...` (sem token) | 401 |
| `ConsultaCpf_InvalidCpf_Returns400` | `GET /consulta/cpf/000.000.000-00` | 400 |
| `ConsultaCpf_ValidCpf_Returns200` | `GET /consulta/cpf/529.982.247-25` | 200 + body correto |
| `ConsultaCpf_NotFound_Returns404` | CPF que o fake service rejeita | 404 |
| `ConsultaCnpj_WithoutToken_Returns401` | `GET /consulta/cnpj/11.222.333/0001-81` (sem token) | 401 |
| `ConsultaCnpj_InvalidCnpj_Returns400` | `GET /consulta/cnpj/00.000.000/0000-00` | 400 |
| `ConsultaCnpj_ValidCnpj_Returns200` | `GET /consulta/cnpj/11.222.333/0001-81` | 200 + body correto |

---

## Dependências de Teste

| Pacote | Versão | Uso |
|---|---|---|
| `xunit` | 2.7.0 | Framework de testes |
| `xunit.runner.visualstudio` | 2.5.7 | Runner para `dotnet test` |
| `Microsoft.NET.Test.Sdk` | 17.9.0 | SDK de testes |
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.0 | `WebApplicationFactory` para testes de integração |

Sem Moq ou outras bibliotecas de mock: todos os doubles de teste são implementações `file`-scoped no próprio arquivo de teste.
