# CpfApi — Consulta de CPF e CNPJ

API REST em C# (.NET 8) para consulta de dados de **CPF** e **CNPJ** na Receita Federal, com autenticação via Bearer Token.

## Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| C# / .NET | 8.0 | Linguagem e runtime |
| ASP.NET Core Minimal API | 8.0 | Framework HTTP |
| ReceitaWS | — | API externa de consulta |
| Docker | 24+ | Containerização |
| xUnit | 2.7 | Testes unitários e de integração |

## Quickstart com Docker

```bash
# 1. Configurar variáveis de ambiente
cp CpfApi/.env.example CpfApi/.env
# Edite CpfApi/.env e defina ApiToken com um valor seguro

# 2. Build e inicialização (a partir da raiz do repositório)
docker compose up --build -d

# 3. Verificar saúde
curl http://localhost:8081/health

# 4. Consultar CPF
curl -H "Authorization: Bearer supersecret123" \
  http://localhost:8081/consulta/cpf/529.982.247-25

# 5. Consultar CNPJ
curl -H "Authorization: Bearer supersecret123" \
  "http://localhost:8081/consulta/cnpj/11.222.333/0001-81"
```

> A API fica exposta na porta **8081** pelo docker-compose (mapeada para 8080 no container).

## Quickstart local (sem Docker)

```bash
cd CpfApi

# Configurar variáveis
cp .env.example .env
# Edite .env

# Rodar
dotnet run
```

O servidor inicia na porta `8080` por padrão.

## Estrutura do Projeto

```
CpfApi/
├── Program.cs                        # Entry point / registro de rotas
├── CpfApi.csproj
├── appsettings.json
├── .env / .env.example
├── Dockerfile
├── Models/
│   ├── CpfResponse.cs                # DTO de resposta CPF
│   ├── CnpjResponse.cs               # DTO de resposta CNPJ
│   ├── ReceitaWsResponse.cs          # DTO ReceitaWS (CPF)
│   └── ReceitaWsCnpjResponse.cs      # DTO ReceitaWS (CNPJ)
├── Services/
│   ├── ICpfService.cs / CpfService.cs
│   └── ICnpjService.cs / CnpjService.cs
├── Validators/
│   ├── CpfValidator.cs               # Validação mod-11 (CPF)
│   └── CnpjValidator.cs              # Validação mod-11 (CNPJ)
├── Middleware/
│   └── BearerAuthMiddleware.cs       # Autenticação Bearer Token
└── docs/
    ├── API.md                        # Endpoints, exemplos, tabela de erros
    └── TESTES.md                     # Suite de testes e cobertura
```

## Documentação

- [Documentação da API](docs/API.md) — endpoints, exemplos curl/Postman, tabela de erros
- [Documentação de Testes](docs/TESTES.md) — suite xUnit, cobertura, como rodar

## Rodando os Testes

```bash
cd CpfApi.Tests
dotnet test
```

60 testes — todos passando. Nenhuma dependência externa necessária (HttpClient e serviços são mockados).

## Variáveis de Ambiente

| Variável | Obrigatória | Padrão | Descrição |
|---|---|---|---|
| `ApiToken` | Sim | — | Bearer Token para autenticação da API |
| `ReceitaWsToken` | Não | — | Token da assinatura ReceitaWS (CPF) |
| `ReceitaWsUrl` | Não | `https://www.receitaws.com.br/v1/cpf` | URL base da API externa (CPF) |
| `ReceitaWsCnpjToken` | Não | — | Token da assinatura ReceitaWS (CNPJ) |
| `ReceitaWsCnpjUrl` | Não | `https://www.receitaws.com.br/v1/cnpj` | URL base da API externa (CNPJ) |
| `ASPNETCORE_URLS` | Não | `http://+:8080` | Porta do servidor (configurado no Dockerfile) |
