# Aula 06 — Comunicação entre Serviços — Prática

**Síncrono vs. Assíncrono na prática: sinta a diferença no Postman.**

## Cenário: LojaDev

Uma loja online simplificada com 3 serviços:

| Serviço | Porta | O que faz |
|---------|-------|-----------|
| **LojaApi** | 5090 | Recebe pedidos do cliente (Postman) |
| **PagamentoApi** | 5091 | Aprova/rejeita pagamentos (~1s de latência) |
| **NotificacaoApi** | 5092 | Envia e-mail (~3s de latência, ~20% de falha) |

O serviço de notificação é **propositalmente lento e instável** — é isso que justifica usar comunicação assíncrona.

## Conteúdo desta pasta

| Item | O que é |
|------|---------|
| `LojaDev.slnx` | Solution com os 3 projetos |
| `LojaDev.Compartilhado/` | Modelos compartilhados + fila em memória (`Channel<T>`) |
| `LojaDev.PagamentoApi/` | Serviço de pagamento — **PRONTO** (não alterar) |
| `LojaDev.NotificacaoApi/` | Serviço de notificação — **PRONTO** (não alterar) |
| `LojaDev.LojaApi/` | Serviço principal — **AQUI estão os TODOs** |
| `GABARITO/` | Gabarito dos TODOs (uso do professor) |

## Pré-requisitos

- .NET 8 SDK (ou superior)
- Postman (ou similar)
- 3 terminais abertos (um por serviço)

---

## Roteiro da prática (em duplas — 75 min)

### FASE 1 — Explorar os serviços auxiliares (10 min)

Abra **3 terminais** e suba os serviços auxiliares:

**Terminal 1** — PagamentoApi:
```bash
cd LojaDev.PagamentoApi
dotnet run
```

**Terminal 2** — NotificacaoApi:
```bash
cd LojaDev.NotificacaoApi
dotnet run
```

No **Postman**, teste cada serviço individualmente:

#### Teste 1: PagamentoApi
```
POST http://localhost:5091/api/pagamentos
Content-Type: application/json

{
  "cliente": "Maria Silva",
  "produto": "Notebook Dell",
  "valor": 4500.00
}
```
→ Deve retornar **aprovado = true** em ~1 segundo.

#### Teste 2: PagamentoApi (rejeição)
```
POST http://localhost:5091/api/pagamentos
Content-Type: application/json

{
  "cliente": "João Santos",
  "produto": "Carro Elétrico",
  "valor": 15000.00
}
```
→ Deve retornar **aprovado = false** (valor > R$ 10.000).

#### Teste 3: NotificacaoApi
```
POST http://localhost:5092/api/notificacoes
Content-Type: application/json

{
  "destinatario": "maria@email.com",
  "assunto": "Teste",
  "corpo": "Olá mundo"
}
```
→ Leva **~3 segundos**. Pode **falhar** (~20% das vezes). Repita se falhar.

📝 **Anote**: quanto tempo cada serviço leva para responder?

---

### FASE 2 — Implementar o fluxo SÍNCRONO (20 min)

**Terminal 3** — LojaApi:
```bash
cd LojaDev.LojaApi
dotnet run
```

Abra `LojaDev.LojaApi/Controllers/PedidosController.cs` e complete:

- **TODO 1** — Chamar PagamentoApi via HTTP

        var respostaPagamento = await _httpClient.PostAsJsonAsync("http://localhost:5091/api/pagamentos", request);
        
        if (!respostaPagamento.IsSuccessStatusCode)
        {
            return StatusCode((int)respostaPagamento.StatusCode, "Erro ao processar pagamento.");
        }

        var resultadoPagamento = await respostaPagamento.Content.ReadFromJsonAsync<PagamentoResponse>();

        if (resultadoPagamento == null || !resultadoPagamento.Aprovado)
        {
            stopwatch.Stop();
            return BadRequest(new { mensagem = "Pagamento rejeitado.", tempoTotalMs = stopwatch.ElapsedMilliseconds });
       }

- **TODO 2** — Chamar NotificacaoApi via HTTP

        var notificacao = new NotificacaoRequest
        {
            Destinatario = $"{request.Cliente.ToLower().Replace(" ", "")}@email.com",
            Assunto = "Pedido Confirmado",
            Corpo = $"Seu pedido do produto {request.Produto} foi aprovado com sucesso!"
        };

        var respostaNotificacao = await _httpClient.PostAsJsonAsync("http://localhost:5092/api/notificacoes", notificacao);

        stopwatch.Stop();

        if (!respostaNotificacao.IsSuccessStatusCode)
        {
            return StatusCode(500, new { mensagem = "Pedido pago, mas falhou ao enviar notificação síncrona.", tempoTotalMs = stopwatch.ElapsedMilliseconds });
        }

        return Ok(new { mensagem = "Pedido processado de forma síncrona com sucesso!", tempoTotalMs = stopwatch.ElapsedMilliseconds });
    }

    [HttpPost("assincrono")]
    public async Task<IActionResult> CriarPedidoAssincrono([FromBody] PedidoRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

Após completar, reinicie a LojaApi (`Ctrl+C` e `dotnet run` de novo).

No Postman, teste o fluxo síncrono:

```
POST http://localhost:5090/api/pedidos/sincrono
Content-Type: application/json

{
  "cliente": "Maria Silva",
  "produto": "Notebook Dell",
  "valor": 4500.00
}
```

📝 **Anote o `tempoTotalMs`** — deve ser **~4-5 segundos** (1s pagamento + 3s notificação).

🔁 Repita 3 vezes. Alguma falhou? O que aconteceu com a resposta quando a notificação falha?

---

### FASE 3 — Implementar o fluxo ASSÍNCRONO (25 min)

Agora complete os TODOs restantes:

Em `Controllers/PedidosController.cs`:
- **TODO 3** — Chamar PagamentoApi (copie do TODO 1)

        var respostaPagamento = await _httpClient.PostAsJsonAsync("http://localhost:5091/api/pagamentos", request);

        if (!respostaPagamento.IsSuccessStatusCode)
        {
            return StatusCode((int)respostaPagamento.StatusCode, "Erro ao processar pagamento.");
        }

        var resultadoPagamento = await respostaPagamento.Content.ReadFromJsonAsync<PagamentoResponse>();

        if (resultadoPagamento == null || !resultadoPagamento.Aprovado)
        {
            stopwatch.Stop();
            return BadRequest(new { mensagem = "Pagamento rejeitado.", tempoTotalMs = stopwatch.ElapsedMilliseconds });
        }

- **TODO 4** — Publicar evento na fila

        var notificacao = new NotificacaoRequest
        {
            Destinatario = $"{request.Cliente.ToLower().Replace(" ", "")}@email.com",
            Assunto = "Pedido Confirmado",
            Corpo = $"Seu pedido do produto {request.Produto} foi aprovado com sucesso!"
        };

        await _fila.PublicarAsync(notificacao);

        stopwatch.Stop();

- **TODO 5** — Retornar `Accepted()` (HTTP 202)

        return Accepted(new { mensagem = "Pedido recebido e pagamento aprovado. Notificação sendo enviada em background.", tempoTotalMs = stopwatch.ElapsedMilliseconds });
    }

Em `Servicos/NotificacaoBackground.cs`:
- **TODO 6** — Consumir eventos da fila com `await foreach`

        await foreach (var notificacao in _fila.ConsumirAsync(stoppingToken))
        {

- **TODO 7** — Chamar NotificacaoApi via HTTP (com try/catch)

            try
            {
                var resposta = await _httpClient.PostAsJsonAsync("http://localhost:5092/api/notificacoes", notificacao, stoppingToken);

                if (resposta.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[BACKGROUND] E-mail enviado com sucesso para: {notificacao.Destinatario}");
                }
                else
                {
                    Console.WriteLine($"[BACKGROUND] Falha no serviço de e-mail ({resposta.StatusCode}) ao notificar: {notificacao.Destinatario}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BACKGROUND] Erro ao tentar conectar na NotificacaoApi: {ex.Message}");
            }
        }
    }
}

Reinicie a LojaApi e teste o fluxo assíncrono no Postman:

```
POST http://localhost:5090/api/pedidos/assincrono
Content-Type: application/json

{
  "cliente": "Maria Silva",
  "produto": "Notebook Dell",
  "valor": 4500.00
}
```

📝 **Anote o `tempoTotalMs`** — deve ser **~1 segundo** (só pagamento!).

👀 **Olhe o terminal da LojaApi**: a notificação é processada em background, DEPOIS que a resposta já foi pro Postman.

---

### FASE 4 — Experimentação e análise (15 min)

#### Experimento 1: Rajada de pedidos
No Postman, envie **5 pedidos assíncronos** em sequência rápida. Observe:
- Os 5 retornam rápido (~1s cada)?

Os 5 pedidos retornaram na faixa de ~1000ms a 1100ms cada.

- O background processa os 5 na sequência? (olhe os logs)

O BackgroundService consumiu os 5 mensagens sequencialmente no terminal da LojaApi.

- Alguma notificação falhou? O que aconteceu? (a resposta do pedido mudou?)

Quando a notificação falhou, apareceu log de erro no console da LojaApi, mas a resposta pro Postman continuou dando HTTP 202 com status de sucesso.

#### Experimento 2: Notificação fora do ar
Pare o NotificacaoApi (`Ctrl+C` no Terminal 2). Envie um pedido:
- **Síncrono** (`/api/pedidos/sincrono`) → O que acontece?

Retornou HTTP 500 no Postman depois de demorar alguns segundos tentando a conexão. O cliente recebe mensagem de falha, embora o pagamento já tenha sido aprovado.

- **Assíncrono** (`/api/pedidos/assincrono`) → O que acontece?

O Postman respondeu em ~1s com HTTP 202 normalmente. O erro de conexão apareceu apenas nos logs do terminal da LojaApi.

Suba o NotificacaoApi de novo e observe os logs.

#### Experimento 3: Pagamento rejeitado
Envie um pedido com valor > R$ 10.000 em ambos os fluxos:
```json
{
  "cliente": "João Santos",
  "produto": "Carro Elétrico",
  "valor": 15000.00
}
```
- O pedido rejeitado gera notificação? Por quê?

Se a compra for menor que 10.000, o código interrompe a execução no cheque do pagamento e faz o retorno de BadRequest() (HTTP 400) e mensagem de notificação é gerada ou enviada para a fila, pois a condição para notificar é a aprovação do pagamento.

---

### FASE 5 — Reflexão (5 min)

Responda no caderno ou em um comentário no código:

1. **Por que o pagamento é síncrono nos dois fluxos?** Poderia ser assíncrono?

O pagamento é síncrono porque a regra de negócio exige saber se a venda foi aprovada na hora para dar o retorno pro cliente na tela. Poderia ser assíncrono caso o sistema trabalhasse no modelo "pedido recebido", onde a compra é aceita primeiro e o pagamento é processado em background.

2. **O que acontece se a fila perder um evento?** (Lembre: o `Channel<T>` está em memória — se a aplicação reiniciar, o que acontece com os eventos na fila?)

Como o Channel<T> salva as mensagens na RAM da aplicação, se a LojaApi for reiniciada ou cair enquanto houver eventos pendentes na fila, todas essas mensagens serão apagadas da memória e o e-mail não será enviado para esses clientes.

3. **Em produção, o que substituiria o `Channel<T>`?** Cite pelo menos uma tecnologia.

Em ambiente de produção, seria utilizado um Message Broker ou fila distribuída persistente em disco/cluster, como Apache Kafka ou serviços de nuvem como AWS SQS.

4. **Se a NotificacaoApi falhar, como o consumidor da fila deveria reagir?** (Dica: o gabarito tem a resposta parcial, mas pesquise sobre "dead-letter queue" e "retry with backoff").

O consumidor deve tentar reenviar após alguns segundos ou minutos. Caso a falha persista após um número máximo de tentativas, a mensagem deve ser descartada da fila principal e enviada para uma Dead-Letter Queue para análise posterior e não travar o consumo dos demais eventos.

5. **Compare os tempos de resposta** que você anotou:

| Fluxo | Tempo de resposta | Notificação chega? |
|-------|-------------------|--------------------|
| Síncrono | ~4200 ms | x Imediata / x Pode falhar |
| Assíncrono | ~1050 ms | x Em background / ☐ Garantida? |

---

## Entregável

- API funcionando nos dois fluxos (prints do Postman: síncrono e assíncrono, com os tempos)
- Tabela comparativa preenchida (Fase 5, pergunta 5)
- Respostas das perguntas 1-4

## Dica importante

Olhe os **logs nos terminais** — eles contam a história completa do que está acontecendo em cada serviço. No fluxo assíncrono, preste atenção na ORDEM dos logs: a resposta do Postman aparece ANTES da notificação ser processada!

## Gabarito

`GABARITO/PedidosController.Gabarito.cs.txt` e `GABARITO/NotificacaoBackground.Gabarito.cs.txt` — versões completas com todos os TODOs resolvidos (professor: não distribuir antes).
