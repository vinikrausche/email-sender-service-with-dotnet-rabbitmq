# email-sender-service-with-dotnet-rabbitmq

Bilingual documentation: [English](#english) | [Português](#portugues)

## English

### 1. Overview

`email-sender-service-with-dotnet-rabbitmq` is a .NET service that sends emails via SMTP.

It works asynchronously:

1. Your API receives a request to send an email.
2. The API publishes the message to RabbitMQ.
3. A background consumer reads the queue.
4. The consumer sends the email through SMTP (MailKit).

### 2. Architecture

```text
Client -> HTTP API (/api/EmailSender) -> RabbitMQ queue (email-sender)
                                              |
                                              v
                                    RabbitMqConsumer (BackgroundService)
                                              |
                                              v
                                      SMTP server (e.g., Gmail)
```

### 3. Tech stack

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- RabbitMQ (`RabbitMQ.Client`)
- MailKit (SMTP)
- Newtonsoft.Json
- xUnit (unit tests)
- Docker + Docker Compose

### 4. Main components

- `Controllers/EmailSender.cs`: Receives HTTP requests and publishes messages to queue `email-sender`.
- `Messaging/Producer/RabbitMqProducer.cs`: Declares queue and publishes messages.
- `Messaging/Consumer/RabbitMqConsumer.cs`: Background consumer that processes messages and sends email.
- `Infra/MailKitSender.cs`: SMTP integration (connect, authenticate, send).
- `Factory/RabbitMqFactory.cs`: Creates/reuses RabbitMQ connection from environment variables.
- `Records/SendEmailRequest.cs`: Request contract with validation attributes.
- `init.bash`: Starts Docker Compose with build.
- `test.sh`: Runs tests and bootstrap test project if needed.

### 5. Environment variables

The container reads SMTP settings from `.env`:

| Variable | Description | Example |
|---|---|---|
| `EMAIL__HOST` | SMTP host | `smtp.gmail.com` |
| `EMAIL__PORT` | SMTP port | `587` |
| `EMAIL__USER` | SMTP username/login | `your-account@gmail.com` |
| `EMAIL__PASS` | SMTP password (app password recommended) | `abcd efgh ijkl mnop` |
| `EMAIL__FROMNAME` | Display name in sender | `Email Sender` |
| `EMAIL__FROMEMAIL` | Sender email address | `your-account@gmail.com` |

Use `.env.example` as base:

```bash
cp .env.example .env
```

Important for Gmail:

- Use your full Gmail address as `EMAIL__USER`.
- Enable 2FA and create an App Password.
- App password with spaces is accepted by this project (spaces are normalized before authentication).

### 6. Running with Docker (`init.bash`)

Start all services:

```bash
./init.bash
```

Run in detached mode:

```bash
./init.bash -d
```

Services:

- API: `http://localhost:8080`
- RabbitMQ Management UI: `http://localhost:15672` (default: `guest` / `guest`)

Stop environment:

```bash
docker compose down
```

### 6.1 Running locally without Docker (optional)

If you prefer running the API directly with the .NET SDK:

1. Start RabbitMQ only:

```bash
docker compose up rabbitmq -d
```

2. Load `.env` variables in your shell and run API:

```bash
set -a
source .env
set +a
dotnet run
```

### 7. API routes

#### POST `/api/EmailSender`

Publishes an email request to RabbitMQ queue `email-sender`.

Request body:

```json
{
  "to": "recipient@example.com",
  "toName": "Recipient Name",
  "subject": "Test email",
  "body": "Hello from API + RabbitMQ + MailKit"
}
```

Validation rules (`SendEmailRequest`):

- `to`: required + valid email format
- `toName`: required + minimum length 1
- `subject`: required + max length 100
- `body`: required + minimum length 1

Response:

- `202 Accepted` when message is queued successfully.

#### GET `/weatherforecast`

Sample endpoint from default template (not part of email flow).

#### OpenAPI (Development only)

When `ASPNETCORE_ENVIRONMENT=Development`, OpenAPI is mapped by `Program.cs`.

### 8. How to test the API

#### Option A: VS Code/IDE `.http` file

Use `EmailSender.http`:

1. Start services (`./init.bash`).
2. Send `POST /api/EmailSender`.
3. Check container logs to confirm processing.

#### Option B: `curl`

```bash
curl -X POST "http://localhost:8080/api/EmailSender" \
  -H "Content-Type: application/json" \
  -d '{
    "to":"recipient@example.com",
    "toName":"Recipient Name",
    "subject":"Test email",
    "body":"Hello from API + RabbitMQ + MailKit"
  }'
```

Inspect logs:

```bash
docker compose logs -f emailsender
```

### 9. Unit tests

Run all tests:

```bash
./test.sh
```

Or directly:

```bash
dotnet test EmailSender.sln
```

Current unit tests include:

- `RabbitMqFactoryConfigurationTests`
  - Reads RabbitMQ config from env/config.
  - Uses defaults when values are missing or invalid.
- `SendEmailRequestValidationTests`
  - Valid payload scenario.
  - Invalid email scenario.
  - Empty `ToName` scenario.
  - Empty `Body` scenario.

### 10. Queue behavior details

- Queue name: `email-sender`
- Queue is declared as durable.
- Consumer uses manual ack (`autoAck: false`).
- `BasicQos` prefetch count is `5`.
- Invalid JSON or processing failures are `NACK` with `requeue: false`.

### 11. Troubleshooting

- `535 5.7.8 Username and Password not accepted`:
  - Confirm `EMAIL__USER` is the same account that generated the app password.
  - Confirm `.env` values and recreate containers (`docker compose up --build --force-recreate`).
- API returns `202` but no email received:
  - Check `emailsender` logs.
  - Confirm SMTP credentials and sender address.
- RabbitMQ access:
  - Ensure RabbitMQ container is healthy before API starts (already configured in `docker-compose.yml`).

### 12. Security notes

- Never commit `.env`.
- Use app passwords/secrets only in local secret stores or CI/CD secret managers.
- Rotate credentials if accidentally exposed.

---

## Portugues

### 1. Visão geral

`email-sender-service-with-dotnet-rabbitmq` é um serviço .NET que envia e-mails via SMTP.

O fluxo é assíncrono:

1. A API recebe a requisição de envio.
2. A API publica a mensagem no RabbitMQ.
3. Um consumidor em background lê a fila.
4. O consumidor envia o e-mail via SMTP (MailKit).

### 2. Arquitetura

```text
Cliente -> API HTTP (/api/EmailSender) -> Fila RabbitMQ (email-sender)
                                              |
                                              v
                                   RabbitMqConsumer (BackgroundService)
                                              |
                                              v
                                  Servidor SMTP (ex.: Gmail)
```

### 3. Stack tecnológica

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- RabbitMQ (`RabbitMQ.Client`)
- MailKit (SMTP)
- Newtonsoft.Json
- xUnit (testes unitários)
- Docker + Docker Compose

### 4. Componentes principais

- `Controllers/EmailSender.cs`: Recebe HTTP e publica na fila `email-sender`.
- `Messaging/Producer/RabbitMqProducer.cs`: Declara fila e publica mensagens.
- `Messaging/Consumer/RabbitMqConsumer.cs`: Consumidor em background que processa e envia e-mail.
- `Infra/MailKitSender.cs`: Integração SMTP (conexão, autenticação, envio).
- `Factory/RabbitMqFactory.cs`: Cria/reaproveita conexão RabbitMQ com variáveis de ambiente.
- `Records/SendEmailRequest.cs`: Contrato da requisição com validações.
- `init.bash`: Sobe Docker Compose com build.
- `test.sh`: Executa testes e cria bootstrap do projeto de testes se necessário.

### 5. Variáveis de ambiente

O container lê as configurações SMTP do `.env`:

| Variável | Descrição | Exemplo |
|---|---|---|
| `EMAIL__HOST` | Host SMTP | `smtp.gmail.com` |
| `EMAIL__PORT` | Porta SMTP | `587` |
| `EMAIL__USER` | Usuário/login SMTP | `sua-conta@gmail.com` |
| `EMAIL__PASS` | Senha SMTP (senha de app recomendada) | `abcd efgh ijkl mnop` |
| `EMAIL__FROMNAME` | Nome exibido no remetente | `Email Sender` |
| `EMAIL__FROMEMAIL` | E-mail do remetente | `sua-conta@gmail.com` |

Use o `.env.example` como base:

```bash
cp .env.example .env
```

Importante para Gmail:

- Use o Gmail completo em `EMAIL__USER`.
- Ative 2FA e gere uma Senha de App.
- Senha com espaços é aceita neste projeto (os espaços são normalizados antes da autenticação).

### 6. Subindo com Docker (`init.bash`)

Subir todos os serviços:

```bash
./init.bash
```

Subir em background:

```bash
./init.bash -d
```

Serviços:

- API: `http://localhost:8080`
- UI de gerenciamento RabbitMQ: `http://localhost:15672` (padrão: `guest` / `guest`)

Parar ambiente:

```bash
docker compose down
```

### 6.1 Rodando local sem Docker (opcional)

Se preferir executar a API direto com o .NET SDK:

1. Suba apenas o RabbitMQ:

```bash
docker compose up rabbitmq -d
```

2. Carregue variáveis do `.env` e execute a API:

```bash
set -a
source .env
set +a
dotnet run
```

### 7. Rotas da API

#### POST `/api/EmailSender`

Publica uma solicitação de e-mail na fila `email-sender`.

Payload:

```json
{
  "to": "recipient@example.com",
  "toName": "Recipient Name",
  "subject": "Test email",
  "body": "Hello from API + RabbitMQ + MailKit"
}
```

Regras de validação (`SendEmailRequest`):

- `to`: obrigatório + formato de e-mail válido
- `toName`: obrigatório + tamanho mínimo 1
- `subject`: obrigatório + tamanho máximo 100
- `body`: obrigatório + tamanho mínimo 1

Resposta:

- `202 Accepted` quando enfileirar com sucesso.

#### GET `/weatherforecast`

Endpoint de exemplo do template padrão (não faz parte do fluxo de e-mail).

#### OpenAPI (apenas Development)

Quando `ASPNETCORE_ENVIRONMENT=Development`, o OpenAPI é mapeado no `Program.cs`.

### 8. Como testar a API

#### Opção A: arquivo `.http` na IDE

Use `EmailSender.http`:

1. Suba os serviços (`./init.bash`).
2. Envie `POST /api/EmailSender`.
3. Confira os logs do container para validar o processamento.

#### Opção B: `curl`

```bash
curl -X POST "http://localhost:8080/api/EmailSender" \
  -H "Content-Type: application/json" \
  -d '{
    "to":"recipient@example.com",
    "toName":"Recipient Name",
    "subject":"Test email",
    "body":"Hello from API + RabbitMQ + MailKit"
  }'
```

Ver logs:

```bash
docker compose logs -f emailsender
```

### 9. Testes unitários

Rodar todos os testes:

```bash
./test.sh
```

Ou direto:

```bash
dotnet test EmailSender.sln
```

Testes atuais cobrem:

- `RabbitMqFactoryConfigurationTests`
  - Leitura de configuração RabbitMQ via env/config.
  - Fallback para valores padrão em caso de ausência/erro.
- `SendEmailRequestValidationTests`
  - Cenário válido.
  - E-mail inválido.
  - `ToName` vazio.
  - `Body` vazio.

### 10. Detalhes da fila

- Nome da fila: `email-sender`
- Fila declarada como durável.
- Consumidor com ack manual (`autoAck: false`).
- `BasicQos` com `prefetchCount = 5`.
- JSON inválido ou falhas de processamento geram `NACK` com `requeue: false`.

### 11. Troubleshooting

- `535 5.7.8 Username and Password not accepted`:
  - Garanta que `EMAIL__USER` é a mesma conta que gerou a senha de app.
  - Confira o `.env` e recrie os containers (`docker compose up --build --force-recreate`).
- API retorna `202` mas não envia e-mail:
  - Verifique logs do `emailsender`.
  - Confirme credenciais SMTP e e-mail remetente.
- Acesso ao RabbitMQ:
  - Verifique que o container RabbitMQ está saudável antes da API iniciar (já configurado no `docker-compose.yml`).

### 12. Segurança

- Nunca versione o `.env`.
- Use segredos em cofres de segredo locais/CI, não no código.
- Se algo vazar, revogue e gere novas credenciais.
