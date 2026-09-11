using System.Text;
using System.Text.Json;
using LojaDev.Compartilhado.Fila;
using LojaDev.Compartilhado.Modelos;

namespace LojaDev.LojaApi.Servicos;

/// <summary>
/// Serviço de background que consome eventos da fila e envia
/// notificações chamando a NotificacaoApi.
///
/// BackgroundService roda em uma thread separada da API, sem
/// bloquear as requisições HTTP. Ele fica "escutando" a fila
/// e processa cada evento assim que chega.
///
/// Em produção, este seria um worker/consumidor separado
/// conectado a um broker de mensagens (RabbitMQ, Azure Service Bus, etc.).
/// </summary>
public class NotificacaoBackground : BackgroundService
{
    private readonly FilaDePedidos _fila;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    public NotificacaoBackground(
        FilaDePedidos fila,
        IHttpClientFactory httpFactory,
        IConfiguration config)
    {
        _fila = fila;
        _httpFactory = httpFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🔄 NotificacaoBackground iniciado — aguardando eventos na fila...");

        // ╔═══════════════════════════════════════════════════════╗
        // ║  TODO 6: Consumir eventos da fila                     ║
        // ╚═══════════════════════════════════════════════════════╝
        await foreach (var evento in _fila.ConsumirAsync(stoppingToken))
        {
            Console.WriteLine($"📥 [BACKGROUND] Evento recebido — Pedido {evento.PedidoId}");
            await ProcessarEvento(evento);
        }
    }

    /// <summary>
    /// Processa um evento da fila: chama NotificacaoApi via HTTP.
    /// Se falhar, loga o erro (em produção, recolocaria na fila).
    /// </summary>
    private async Task ProcessarEvento(EventoPedidoAprovado evento)
    {
        // ╔═══════════════════════════════════════════════════════╗
        // ║  TODO 7: Chamar NotificacaoApi via HTTP               ║
        // ╚═══════════════════════════════════════════════════════╝
        try
        {
            var urlNotificacao = _config["ServicoNotificacao"];
            var client = _httpFactory.CreateClient();

            var body = new
            {
                Destinatario = evento.Cliente,
                Assunto = $"Pedido {evento.PedidoId} confirmado!",
                Corpo = $"Seu pedido de {evento.Produto} (R$ {evento.Valor:F2}) foi aprovado."
            };

            var json = JsonSerializer.Serialize(body);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            var resposta = await client.PostAsync($"{urlNotificacao}/api/notificacoes", conteudo);

            if (resposta.IsSuccessStatusCode)
            {
                Console.WriteLine($"📥 ✅ [BACKGROUND] Notificação enviada — Pedido {evento.PedidoId}");
            }
            else
            {
                Console.WriteLine($"📥 ⚠️ [BACKGROUND] Falha na notificação — Pedido {evento.PedidoId} (status {resposta.StatusCode})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"📥 ❌ [BACKGROUND] Erro ao notificar — Pedido {evento.PedidoId}: {ex.Message}");
        }
    }
}
