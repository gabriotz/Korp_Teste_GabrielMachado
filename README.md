# Korp_Teste_GabrielMachado

Sistema de emissão de Notas Fiscais desenvolvido como teste técnico para a Korp ERP.

## Como rodar

### Pré-requisitos
- Docker e Docker Compose instalados
- Criar o arquivo `.env` na raiz do projeto (ver seção abaixo)

### Variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto com o seguinte conteúdo:

```
GEMINI_API_KEY=sua_chave_aqui
```

> A chave pode ser obtida gratuitamente em https://aistudio.google.com/apikey

### Subindo o projeto

```bash
docker-compose up --build
```

### URLs disponíveis

| Serviço | URL |
|---|---|
| Frontend | http://localhost:4200 |
| EstoqueService API | http://localhost:8081 |
| FaturamentoService API | http://localhost:8082 |

---

## Funcionalidades

### Cadastro de Produtos
- Cadastro com código (único), descrição e saldo inicial
- Listagem ordenada por descrição
- Validação de código duplicado

### Notas Fiscais
- Criação com numeração sequencial automática
- Múltiplos itens por nota
- Validação de produto duplicado na mesma nota
- Validação de quantidade contra saldo disponível
- Status: `Aberta` ou `Fechada`

### Impressão de Nota
- Muda o status para `Fechada`
- Debita o saldo de cada produto no EstoqueService
- Exibe feedback de sucesso ou erro para o usuário
- Não permite reimprimir nota já fechada

### Cenário de Falha
- Se o EstoqueService estiver indisponível, o FaturamentoService retorna `503` com mensagem amigável
- Para demonstrar: `docker stop <container_estoque>` e tente criar ou imprimir uma nota

### Resumo com IA
- Botão "IA Summary" em cada nota fiscal
- Chama o Google Gemini para gerar um resumo profissional em português
- Exibe o resultado em um modal na tela

---

## Decisões técnicas

### Arquitetura
- **Microsserviços**: dois serviços independentes (`EstoqueService` e `FaturamentoService`), cada um com seu próprio banco PostgreSQL
- **Clean Architecture** em ambos os serviços: camadas Domain, Application, Infrastructure e API com dependências apontando para dentro
- **Comunicação HTTP** entre os serviços via `IHttpClientFactory` com timeout de 5 segundos

### Backend — ASP.NET Core 10 com C#
- **Framework**: ASP.NET Core 10 com Entity Framework Core
- **Banco de dados**: PostgreSQL (um por serviço), via `Npgsql.EntityFrameworkCore.PostgreSQL`
- **LINQ**: usado em todas as queries — `OrderBy`, `AnyAsync`, `MaxAsync`, `Include`, `FirstOrDefaultAsync`, `AsNoTracking`
- **Migrations automáticas**: `MigrateAsync()` no startup com retry de até 5 tentativas
- **Exceções tipadas**: cada cenário de erro tem sua própria exception (`ProdutoNaoEncontradoException`, `ProdutoDuplicadoException`, `SaldoInsuficienteException`, etc.)
- **Middleware global de exceções**: mapeia cada tipo de exception para o HTTP status correto (404, 409, 422, 503) sem poluir os controllers com try/catch
- **CORS**: configurado para aceitar requisições do frontend Angular

### Requisitos opcionais implementados

#### Tratamento de Concorrência
- Criação de nota fiscal usa transaction com lock no banco
- Duas requisições simultâneas não geram notas com número duplicado
- Para testar: script PowerShell disponível na seção de testes

#### Idempotência
- Endpoint `POST /notas-fiscais` e `POST /notas-fiscais/{id}/imprimir` suportam o header `Idempotency-Key`
- Se a mesma key for enviada duas vezes, retorna o resultado cacheado sem reprocessar
- Chaves armazenadas na tabela `IdempotencyRecords` no banco do FaturamentoService

#### Inteligência Artificial
- Integração com Google Gemini (`gemini-2.5-flash`) via REST API
- Endpoint `POST /notas-fiscais/{id}/resumo` gera um resumo profissional da nota em português
- Configurado via variável de ambiente `GEMINI_API_KEY`
- Em caso de falha da IA, retorna mensagem de fallback sem quebrar o sistema

### Frontend — Angular 18
- **Componentes standalone** com lazy loading via `loadComponent`
- **Ciclos de vida utilizados**: `ngOnInit` para carregar dados, `ngOnDestroy` para cancelar subscriptions
- **RxJS**:
  - `takeUntil` com `Subject` para cancelar subscriptions no destroy
  - `finalize` para resetar estado de loading independente de sucesso ou erro
- **Reactive Forms** com `FormGroup`, `FormArray` e `Validators` para o formulário de notas fiscais
- **Validações no formulário de nota**: produto duplicado entre itens, quantidade acima do saldo disponível
- **ChangeDetectorRef**: usado para forçar detecção de mudanças nas respostas assíncronas
- **Modal de resumo IA**: implementado em HTML/SCSS puro, sem Angular Material
- **Nginx**: configurado com `try_files` para suportar refresh em rotas do SPA

### Bibliotecas utilizadas
| Biblioteca | Finalidade |
|---|---|
| `@angular/forms` | Reactive Forms para formulários com validação |
| `@angular/common/http` | HttpClient para chamadas à API |
| `@angular/router` | Roteamento com lazy loading |
| `rxjs` | Gerenciamento de streams assíncronos |
| `@angular/material` | Incluído no projeto (theming base) |

---

## Testando cenários específicos

### Cenário de falha do EstoqueService
```bash
# Para o container do Estoque
docker stop korp_teste_gabrielmachado-estoque-1

# Tenta criar uma nota — esperado: 503 "Serviço de Estoque indisponível."
POST http://localhost:8082/notas-fiscais

# Sobe o Estoque novamente
docker start korp_teste_gabrielmachado-estoque-1
```

### Testando concorrência
```powershell
$body = '{"itens":[{"produtoId":1,"quantidade":1}]}'
$job1 = Start-Job { Invoke-RestMethod -Uri "http://localhost:8082/notas-fiscais" -Method POST -Body $using:body -ContentType "application/json" }
$job2 = Start-Job { Invoke-RestMethod -Uri "http://localhost:8082/notas-fiscais" -Method POST -Body $using:body -ContentType "application/json" }
Receive-Job $job1 -Wait
Receive-Job $job2 -Wait
# Esperado: uma cria com sucesso, outra retorna erro — sem números duplicados
```

### Testando idempotência
```powershell
$body = '{"itens":[{"produtoId":1,"quantidade":1}]}'
$headers = @{ "Idempotency-Key" = "minha-chave-unica" }
Invoke-RestMethod -Uri "http://localhost:8082/notas-fiscais" -Method POST -Body $body -ContentType "application/json" -Headers $headers
Invoke-RestMethod -Uri "http://localhost:8082/notas-fiscais" -Method POST -Body $body -ContentType "application/json" -Headers $headers
# Esperado: as duas retornam a mesma nota, sem duplicata
```
