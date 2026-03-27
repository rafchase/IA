# Documentação da API — CpfApi

## Visão Geral

API REST em C# (.NET 8) para consulta de **CPF** e **CNPJ** na Receita Federal via [ReceitaWS](https://www.receitaws.com.br/). A API valida os documentos localmente (formato e dígitos verificadores mod-11) antes de encaminhar a consulta ao serviço externo.

**Base URL (Docker):** `http://localhost:8081`
**Base URL (local):** `http://localhost:8080`

---

## Autenticação

Todas as rotas (exceto `/health`) exigem o header:

```
Authorization: Bearer <ApiToken>
```

O token é configurado via variável de ambiente `ApiToken`.

---

## Endpoints

### `GET /health`

Verifica se o serviço está no ar. **Não requer autenticação.**

**Resposta 200:**
```json
{ "status": "ok" }
```

---

### `GET /consulta/cpf/{cpf}`

Consulta os dados de um CPF na Receita Federal.

#### Parâmetros de Path

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `cpf` | string | CPF com 11 dígitos. Aceita formato `000.000.000-00` ou somente dígitos `00000000000` |

#### Resposta 200 — Sucesso

```json
{
  "cpf": "529.982.247-25",
  "nome": "FULANO DE TAL",
  "situacao": "REGULAR"
}
```

| Campo | Tipo | Descrição |
|---|---|---|
| `cpf` | string | CPF formatado (`000.000.000-00`) |
| `nome` | string | Nome completo conforme Receita Federal |
| `situacao` | string | Situação cadastral (ex: `REGULAR`, `IRREGULAR`, `SUSPENSA`) |

---

### `GET /consulta/cnpj/{cnpj}`

Consulta os dados de um CNPJ na Receita Federal.

#### Parâmetros de Path

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `cnpj` | string | CNPJ com 14 dígitos. Aceita formato `00.000.000/0000-00` ou somente dígitos |

> **Nota:** CNPJs formatados contêm `/` na URL (ex: `/consulta/cnpj/11.222.333/0001-81`). A rota usa parâmetro catch-all para lidar com isso corretamente.

#### Resposta 200 — Sucesso

```json
{
  "cnpj": "11.222.333/0001-81",
  "nome": "EMPRESA EXEMPLO LTDA",
  "fantasia": "EXEMPLO",
  "situacao": "ATIVA",
  "abertura": "01/01/2000"
}
```

| Campo | Tipo | Descrição |
|---|---|---|
| `cnpj` | string | CNPJ formatado (`00.000.000/0000-00`) |
| `nome` | string | Razão social |
| `fantasia` | string | Nome fantasia |
| `situacao` | string | Situação cadastral (ex: `ATIVA`, `BAIXADA`, `INAPTA`) |
| `abertura` | string | Data de abertura no formato `DD/MM/AAAA` |

---

## Tabela de Códigos de Resposta

| HTTP Status | Situação |
|---|---|
| `200 OK` | Consulta realizada com sucesso |
| `400 Bad Request` | CPF/CNPJ com formato inválido ou dígitos verificadores incorretos |
| `401 Unauthorized` | Token ausente, sem prefixo `Bearer` ou valor incorreto |
| `404 Not Found` | CPF/CNPJ não encontrado na base da Receita Federal |
| `503 Service Unavailable` | API externa indisponível ou timeout de 10s atingido |
| `500 Internal Server Error` | Erro interno inesperado |

### Formato dos erros (4xx / 5xx)

```json
{ "error": "mensagem descritiva do erro" }
```

---

## Exemplos com curl

### CPF

```bash
# Consulta com CPF em dígitos puros
curl -s \
  -H "Authorization: Bearer supersecret123" \
  http://localhost:8081/consulta/cpf/52998224725 | jq .

# Consulta com CPF formatado
curl -s \
  -H "Authorization: Bearer supersecret123" \
  "http://localhost:8081/consulta/cpf/529.982.247-25" | jq .

# CPF inválido → 400
curl -s \
  -H "Authorization: Bearer supersecret123" \
  http://localhost:8081/consulta/cpf/00000000000 | jq .

# Sem token → 401
curl -s http://localhost:8081/consulta/cpf/52998224725 | jq .

# Token errado → 401
curl -s \
  -H "Authorization: Bearer token-errado" \
  http://localhost:8081/consulta/cpf/52998224725 | jq .
```

### CNPJ

```bash
# Consulta com CNPJ formatado (notar as aspas por causa da barra)
curl -s \
  -H "Authorization: Bearer supersecret123" \
  "http://localhost:8081/consulta/cnpj/11.222.333/0001-81" | jq .

# Consulta com CNPJ em dígitos puros
curl -s \
  -H "Authorization: Bearer supersecret123" \
  http://localhost:8081/consulta/cnpj/11222333000181 | jq .

# CNPJ inválido → 400
curl -s \
  -H "Authorization: Bearer supersecret123" \
  "http://localhost:8081/consulta/cnpj/00.000.000/0000-00" | jq .
```

### Health check

```bash
curl -s http://localhost:8081/health
```

---

## Exemplos com Postman

### Configurar Environment

| Variável | Valor |
|---|---|
| `base_url` | `http://localhost:8081` |
| `token` | `supersecret123` |

### Consultar CPF

- **Method:** `GET`
- **URL:** `{{base_url}}/consulta/cpf/52998224725`
- **Headers:** `Authorization: Bearer {{token}}`

### Consultar CNPJ

- **Method:** `GET`
- **URL:** `{{base_url}}/consulta/cnpj/11222333000181`
- **Headers:** `Authorization: Bearer {{token}}`

### Testar 401

- **Method:** `GET`
- **URL:** `{{base_url}}/consulta/cpf/52998224725`
- *(sem header Authorization)*

### Testar 400

- **Method:** `GET`
- **URL:** `{{base_url}}/consulta/cpf/11111111111`
- **Headers:** `Authorization: Bearer {{token}}`

---

## Observações sobre a ReceitaWS

- A [ReceitaWS](https://www.receitaws.com.br/) é um serviço externo pago. **Sem token**, a API retorna `404` mesmo para documentos válidos.
- Configure `ReceitaWsToken` e/ou `ReceitaWsCnpjToken` no `.env` para habilitar consultas reais.
- O timeout de resposta da ReceitaWS é de **10 segundos**; após isso, a API retorna `503`.
- A validação local (formato e dígitos verificadores) ocorre **antes** de qualquer chamada externa; um CPF/CNPJ inválido retorna `400` sem consumir cota da ReceitaWS.
