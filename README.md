# 💰 Desafio PagueVeloz - Sistema de Transações Financeiras

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-144%20Passing-success?style=for-the-badge&logo=xunit)](https://xunit.net/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)

> **Sistema bancário digital completo** desenvolvido com **Clean Architecture**, **CQRS**, **Domain-Driven Design** e padrões de alta disponibilidade. Implementa operações financeiras críticas com garantias de **idempotência**, **consistência transacional** e **controle de concorrência**.

---

## 📋 Índice

- [✨ Sobre o Projeto](#-sobre-o-projeto)
- [🏗️ Arquitetura](#️-arquitetura)
- [🛠️ Tecnologias](#️-tecnologias)
- [🎯 Funcionalidades](#-funcionalidades)
- [🚀 Quick Start](#-quick-start)
- [🐳 Docker](#-docker)
- [💻 Execução Local](#-execução-local)
- [🧪 Testes](#-testes)
- [📚 Documentação da API](#-documentação-da-api)
- [🔐 Segurança](#-segurança)
- [💡 Decisões Técnicas](#-decisões-técnicas)
- [📊 Estrutura do Projeto](#-estrutura-do-projeto)
- [🌟 Diferenciais](#-diferenciais)
- [📈 Métricas de Qualidade](#-métricas-de-qualidade)

---

## ✨ Sobre o Projeto

Sistema completo de gerenciamento de **transações financeiras digitais** desenvolvido como parte do desafio técnico da **PagueVeloz**. A solução implementa um backend robusto para operações bancárias, incluindo:

- 💳 **Gestão de Clientes** com validação de documentos (CPF/CNPJ)
- 🏦 **Contas Digitais** com saldo, limites e controle de status
- 💸 **Transações Financeiras** (crédito, débito, transferência, reserva, captura, estorno)
- 🔒 **Idempotência** garantida para todas as operações
- ⚡ **Processamento Assíncrono** com Outbox Pattern
- 📊 **Auditoria Completa** de eventos de domínio
- 🛡️ **Controle de Concorrência** com locks pessimistas

---

## 🏗️ Arquitetura

### **Clean Architecture + DDD + CQRS + Event Sourcing**

A aplicação segue os princípios de **Clean Architecture** com separação clara de responsabilidades em 4 camadas:

**📱 API Layer → 🎯 Application Layer → 🧩 Domain Layer → 🔧 Infrastructure Layer**

### **Padrões Arquiteturais Implementados:**

| Padrão | Descrição | Benefício |
|--------|-----------|-----------|
| **Clean Architecture** | Separação em camadas com dependências unidirecionais | Testabilidade, manutenibilidade, independência de frameworks |
| **CQRS** | Separação entre Commands (escrita) e Queries (leitura) | Performance, escalabilidade, clareza de intenção |
| **Domain-Driven Design** | Modelagem rica de domínio com agregados e value objects | Regras de negócio centralizadas, código expressivo |
| **Repository Pattern** | Abstração de acesso a dados | Testabilidade, troca de providers |
| **Unit of Work** | Gerenciamento transacional | Consistência ACID |
| **Outbox Pattern** | Publicação garantida de eventos | Consistência eventual, zero mensagens perdidas |
| **Result Pattern** | Retorno estruturado de operações | Tratamento de erros funcional |
| **Pipeline Behavior** | Cross-cutting concerns | Logging, validação e transação centralizados |

---

## 🛠️ Tecnologias

### **Core Stack:**
- **.NET 9.0** - Framework principal
- **C# 13** - Linguagem com recursos modernos
- **ASP.NET Core 9** - Web API Framework
- **Entity Framework Core 9** - ORM
- **SQL Server 2022** - Banco de dados relacional

### **Libraries & Frameworks:**
- **MediatR** (13.1.0) - CQRS e Pipeline de Mediação
- **FluentValidation** (11.11.0) - Validações declarativas
- **AutoMapper** (13.0.1) - Mapeamento objeto-objeto
- **Scalar** (1.2.45) - Documentação interativa OpenAPI

### **Observabilidade:**
- **Health Checks** - Monitoramento de saúde da aplicação
- **Structured Logging** - Logs estruturados com contexto
- **Rate Limiting** - Proteção contra abuso e DDoS

### **Testing:**
- **xUnit** (2.9.2) - Framework de testes
- **FluentAssertions** (6.12.2) - Assertions expressivas
- **WebApplicationFactory** - Testes de integração end-to-end
- **SQLite In-Memory** - Banco de dados para testes
- **Bogus** - Geração de dados de teste

### **Infrastructure:**
- **Docker** - Containerização
- **Docker Compose** - Orquestração local

---

## 🎯 Funcionalidades

### **👤 Gestão de Clientes**
- ✅ Cadastro de clientes (Pessoa Física e Jurídica)
- ✅ Validação automática de CPF/CNPJ com dígito verificador
- ✅ Validação de e-mail
- ✅ Verificação de duplicidade de documento
- ✅ Consulta por ID
- ✅ Soft delete (ativação/desativação)

### **💳 Gestão de Contas**
- ✅ Criação de contas digitais vinculadas a clientes
- ✅ Geração automática de número de conta único
- ✅ Configuração de limite de crédito
- ✅ Consulta de saldo em tempo real (disponível + reservado + limite)
- ✅ Extrato detalhado por período
- ✅ Listagem de transações
- ✅ Bloqueio/desbloqueio de contas

### **💸 Operações Financeiras**

#### **Crédito**
Adiciona saldo à conta com validação de valor mínimo e registro de descrição.

#### **Débito**
Retira saldo da conta validando saldo disponível + limite de crédito, prevenindo saldo negativo além do limite.

#### **Transferência**
Operação atômica entre duas contas (débito + crédito na mesma transação) com validação de ambas as contas.

#### **Reserva (Pré-autorização)**
Bloqueia saldo para operação futura, movendo de "disponível" para "reservado".

#### **Captura**
Confirma uma reserva anterior, removendo saldo do "reservado" e finalizando a operação.

#### **Cancelamento de Reserva**
Reverte uma reserva, devolvendo saldo para "disponível" como operação compensatória.

#### **Estorno**
Reverte qualquer transação processada, executando operação inversa com rastreabilidade completa.

### **🔒 Recursos Avançados**

#### **Idempotência**
- ✅ Chave única por operação (IdempotencyKey)
- ✅ Mesma request = mesma resposta (sem duplicação)
- ✅ Suporta retry seguro
- ✅ Validação em memória e banco de dados

#### **Controle de Concorrência**
- ✅ Locks pessimistas (UPDLOCK + ROWLOCK) em operações críticas
- ✅ Previne race conditions em débitos simultâneos
- ✅ Garante integridade do saldo em cenários de alta concorrência

#### **Consistência Eventual**
- ✅ Outbox Pattern para publicação de eventos
- ✅ Background service processa mensagens a cada 5 segundos
- ✅ Retry automático com backoff exponencial
- ✅ Dead letter queue para mensagens falhadas

---

## 🚀 Quick Start

### **Executar com Docker (Recomendado):**

    git clone https://github.com/slucasdev/desafio-pague-veloz.git
    cd desafio-pague-veloz
    docker-compose up -d
    curl http://localhost:5000/health

✅ **API disponível em:** http://localhost:5000  
✅ **Swagger/Scalar em:** http://localhost:5000/scalar/v1

---

## 🐳 Docker

### **Arquitetura Docker:**

- **sqlserver**: SQL Server 2022 (porta 1433, volume persistente, health check integrado)
- **api**: API .NET 9 (porta 5000, multi-stage build otimizado, non-root user, auto-restart)

### **Comandos Úteis:**

    docker-compose up -d              # Iniciar
    docker-compose logs -f            # Ver logs em tempo real
    docker-compose ps                 # Ver status e health
    docker-compose down               # Parar tudo
    docker-compose down -v            # Parar E limpar dados
    docker-compose up -d --build      # Rebuild após mudanças
    docker exec -it desafio-pagueveloz-api /bin/bash  # Acessar bash

### **Health Checks:**

    curl http://localhost:5000/health          # Health geral (API + Database)
    curl http://localhost:5000/health/ready    # Readiness (verifica DB)
    curl http://localhost:5000/health/live     # Liveness (verifica API)
    curl http://localhost:5000/health/details  # Detalhes completos com métricas

---

## 💻 Execução Local

### **Pré-requisitos:**
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2022](https://www.microsoft.com/sql-server) ou Docker
- [Git](https://git-scm.com/)

### **Passo a Passo:**

    git clone https://github.com/slucasdev/desafio-pague-veloz.git
    cd desafio-pague-veloz
    dotnet restore
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=DesafioPagueVeloz;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;" --project src/SL.DesafioPagueVeloz.Api
    dotnet ef database update --project src/SL.DesafioPagueVeloz.Infrastructure --startup-project src/SL.DesafioPagueVeloz.Api
    dotnet run --project src/SL.DesafioPagueVeloz.Api

Acesse: http://localhost:5000 (API) e http://localhost:5000/scalar/v1 (Swagger)

---

## 🧪 Testes

### **Cobertura de Testes:**

    📊 Total: 144 testes ✅ (100% passing)
    ├── 🌐 Integration Tests (API)............ 18 testes
    ├── 🧩 Domain Tests (Entities)............ 74 testes
    ├── ✅ Application Tests (Validators)...... 28 testes
    └── 🔧 Infrastructure Tests............... 24 testes

### **Executar Testes:**

    dotnet test
    dotnet test --logger "console;verbosity=detailed"
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
    dotnet test --filter "FullyQualifiedName~Api.Tests"         # Apenas integração
    dotnet test --filter "FullyQualifiedName~Domain.Tests"      # Apenas domínio
    dotnet test --filter "FullyQualifiedName~Application.Tests" # Apenas validators

### **Tipos de Testes Implementados:**

- **Integration Tests**: Testes end-to-end com WebApplicationFactory e SQLite in-memory
- **Unit Tests**: Testes de entidades, agregados, value objects e eventos de domínio
- **Validation Tests**: Testes de todos os validators com cenários válidos e inválidos
- **Infrastructure Tests**: Testes de repositórios e persistência

---

## 📚 Documentação da API

### **Base URL:** http://localhost:5000

### **Endpoints Principais:**

#### **Clientes**
- **POST** /api/clientes - Criar novo cliente
- **GET** /api/clientes/{id} - Obter cliente por ID

#### **Contas**
- **POST** /api/contas - Criar nova conta
- **GET** /api/contas/{id}/saldo - Consultar saldo
- **GET** /api/contas/{id}/extrato - Obter extrato por período
- **GET** /api/contas/{id}/transacoes - Listar transações

#### **Transações**
- **POST** /api/transacoes/creditar - Adicionar saldo
- **POST** /api/transacoes/debitar - Retirar saldo
- **POST** /api/transacoes/transferir - Transferir entre contas
- **POST** /api/transacoes/reservar - Reservar saldo
- **POST** /api/transacoes/capturar - Capturar reserva
- **POST** /api/transacoes/cancelar-reserva - Cancelar reserva
- **POST** /api/transacoes/estornar - Estornar transação

#### **Monitoramento**
- **GET** /health - Health check geral
- **GET** /health/ready - Readiness probe (DB)
- **GET** /health/live - Liveness probe (API)
- **GET** /health/details - Detalhes + métricas

### **Documentação Interativa:**

Acesse http://localhost:5000/scalar/v1 para documentação interativa com todos os endpoints, schemas, testar requests direto no navegador e exemplos de código em múltiplas linguagens.

---

## 🔐 Segurança

### **Rate Limiting:**
- **Global:** 100 requests/minuto (todas as rotas)
- **Transações:** 30 requests/minuto (rotas de transação)
- **Resposta:** HTTP 429 com Retry-After header

**Proteção contra:** DDoS, Brute force, Abuso de API, Consumo excessivo de recursos

### **Validações:**
- FluentValidation em todos os commands
- Validação de CPF/CNPJ com dígito verificador
- Validação de e-mail (RFC 5322)
- Sanitização de inputs
- Prevenção de SQL Injection (EF Core parametrizado)

### **Outras Proteções:**
- Global Exception Handler
- User Secrets (desenvolvimento)
- Non-root Docker container (segurança)
- TLS/SSL em produção

---

## 💡 Decisões Técnicas

### **1. Por que Clean Architecture?**
Separação em camadas com dependências unidirecionais garante testabilidade completa, facilita troca de frameworks e aumenta a longevidade do código.

### **2. Por que CQRS com MediatR?**
Commands e Queries separados com pipeline behaviors reutilizáveis (logging, validation, transaction) mantém handlers focados e permite escalabilidade independente.

### **3. Por que Outbox Pattern?**
Garante que eventos sejam publicados mesmo em falhas, salvando na mesma transação do banco com retry automático e backoff exponencial.

### **4. Por que Locks Pessimistas?**
Previne race conditions em débitos simultâneos garantindo integridade do saldo, serializando operações na mesma conta com performance aceitável (lock de linha).

### **5. Por que Idempotency?**
Permite retry seguro sem duplicação de transações, essencial para resiliência em redes instáveis (padrão da indústria: Stripe, PayPal).

### **6. Por que SQLite nos Testes?**
Mais realista que InMemory pois valida constraints, suporta transações e tem sintaxe compatível via repository override.

### **7. Por que Result Pattern?**
Tratamento de erros funcional sem exceptions para fluxo de negócio, com melhor performance e API consistente.

---

## 📊 Estrutura do Projeto

    desafio-pague-veloz/
    ├── src/
    │   ├── SL.DesafioPagueVeloz.Api/
    │   │   ├── Controllers/              # Endpoints REST
    │   │   ├── Middleware/               # Global Exception Handler
    │   │   └── Program.cs                # Startup + DI Container
    │   ├── SL.DesafioPagueVeloz.Application/
    │   │   ├── Commands/                 # Write operations (CQRS)
    │   │   ├── Queries/                  # Read operations (CQRS)
    │   │   ├── Handlers/                 # Command/Query handlers
    │   │   ├── Validators/               # FluentValidation
    │   │   ├── Behaviors/                # Pipeline
    │   │   ├── DTOs/                     # Data Transfer Objects
    │   │   └── Mappings/                 # AutoMapper profiles
    │   ├── SL.DesafioPagueVeloz.Domain/
    │   │   ├── Entities/                 # Agregados
    │   │   ├── ValueObjects/             # Documento
    │   │   ├── Events/                   # Domain Events
    │   │   ├── Enums/                    # TipoOperacao, StatusConta
    │   │   ├── Exceptions/               # Business exceptions
    │   │   └── Interfaces/               # Repository interfaces
    │   └── SL.DesafioPagueVeloz.Infrastructure/
    │       ├── Persistence/              # EF Core, Repositories, UoW
    │       ├── Migrations/               # EF Core Migrations
    │       ├── BackgroundServices/       # OutboxProcessorService
    │       └── Messaging/                # Event Publisher
    ├── tests/
    │   ├── SL.DesafioPagueVeloz.Api.Tests/         # 18 integration tests
    │   ├── SL.DesafioPagueVeloz.Application.Tests/ # 28 validator tests
    │   ├── SL.DesafioPagueVeloz.Domain.Tests/      # 74 unit tests
    │   └── SL.DesafioPagueVeloz.Infrastructure.Tests/ # 24 tests
    ├── Dockerfile
    ├── docker-compose.yml
    ├── .dockerignore
    └── README.md

---

## 🌟 Diferenciais do Projeto

### **Implementados:**
- [x] **144 testes** (100% passing) - unit + integration + validation + infrastructure
- [x] **Clean Architecture** com separação clara de responsabilidades
- [x] **CQRS** para segregação de leitura/escrita
- [x] **Domain-Driven Design** com rich domain model
- [x] **Outbox Pattern** para consistência eventual
- [x] **Idempotência** nativa em todas as operações
- [x] **Locks Pessimistas** para controle de concorrência
- [x] **Health Checks** (Kubernetes-ready com ready/live probes)
- [x] **Rate Limiting** (proteção contra abuso e DDoS)
- [x] **Docker** (deploy em 1 comando)
- [x] **OpenAPI/Swagger** com Scalar
- [x] **Global Exception Handler**
- [x] **Pipeline Behaviors** (logging, validation, transaction)
- [x] **Background Services** (processamento assíncrono)
- [x] **Migrations** automáticas em startup
- [x] **User Secrets** (segurança em development)

### **Extras que Impressionam:**
- ✅ Repository override para testes (evita poluir código de produção)
- ✅ Custom WebApplicationFactory com SQLite
- ✅ Structured logging com contexto
- ✅ Retry policies no SQL Server (resilience)
- ✅ Multi-stage Docker build (otimizado)
- ✅ Non-root container user (segurança)
- ✅ Comentários XML nos controllers
- ✅ Validação de CPF/CNPJ com dígito verificador

---

## 📈 Métricas de Qualidade

    📊 Testes:          144/144 ✅ (100% passing)
    🎯 Cobertura:       > 85% (estimado)
    📦 Arquitetura:     Clean Architecture ✅
    🔒 Segurança:       Rate Limiting + Validations ✅
    🐳 Deploy:          Docker + Compose ✅
    📖 Documentação:    README + OpenAPI ✅
    ♻️ Manutenibilidade: Alta (SOLID, DRY, KISS)

---

## 🚧 Melhorias Futuras

### **Curto Prazo:**
- [ ] Autenticação JWT
- [ ] Autorização baseada em roles
- [ ] Paginação nas listagens
- [ ] API Versioning
- [ ] Resilience patterns com Polly (Circuit Breaker)

### **Médio Prazo:**
- [ ] Message Broker real (RabbitMQ/Azure Service Bus)
- [ ] Cache distribuído (Redis)
- [ ] Observabilidade com OpenTelemetry
- [ ] CI/CD com GitHub Actions
- [ ] Testes de carga (k6/NBomber)

---

## 🤝 Como Executar

**Opção 1: Docker (Mais Fácil)**

    docker-compose up -d

**Opção 2: Local (Para Desenvolvimento)**

    dotnet restore
    dotnet ef database update --project src/SL.DesafioPagueVeloz.Infrastructure --startup-project src/SL.DesafioPagueVeloz.Api
    dotnet run --project src/SL.DesafioPagueVeloz.Api

**Opção 3: Apenas Testes**

    dotnet test

---

## 📞 Contato

**Desenvolvedor:** Sergio Lucas  
**GitHub:** [@slucasdev](https://github.com/slucasdev)  
**LinkedIn:** [linkedin.com/in/sergio-lucas](https://linkedin.com/in/sergio-lucas)  
**Email:** slucasdev@hotmail.com

---

## 📄 Licença

Este projeto está sob a licença MIT. Consulte o arquivo LICENSE para mais detalhes.

---

## 🙏 Agradecimentos

- **PagueVeloz** pelo desafio técnico inspirador
- **Comunidade .NET Brasil** pelo suporte e conhecimento compartilhado
- **Microsoft** pela excelente documentação e ferramentas
- **Arquitetura Limpa** por Uncle Bob Martin

---

<div align="center">

### ⭐ Se este projeto foi útil para você, considere dar uma estrela no GitHub! ⭐

**Desenvolvido por [Sergio Lucas](https://github.com/slucasdev)**

</div>