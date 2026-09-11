# Atividade — AULA 06

## Síncrono ou Assíncrono?

*Análise de fluxos de comunicação entre serviços — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

> **Nomes:** Igor Nóbrega Moreira e Gabriel Luiz Sá Silva  **Turma:** Arquitetura de Aplicações Web   **Data:** 11 / 09 / 2026

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1. Estilo recomendado:   X Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

[ Cliente / Frontend ] ---( 1. Requisição de Pagamento )---> [ Serviço de Pagamentos ]
                                                                         │
                                                               ( 2. Consulta Saldo/Limite )
                                                                         |
                                                                [ Serviço de Contas ]
                                                                        │
                                                               ( 3. Resposta Aprova/Nega )
                                                                     |
[ Cliente / Frontend ] <---( 4. Retorno do Status da Venda )--- [ Serviço de Pagamentos ]

3. Justificativa (mínimo 2 fatores):

    - Urgência da resposta e bloqueio imediato: O cliente está na tela de checkout esperando a confirmação em tempo real para saber se a compra foi efetuada.  
    - Impossibilidade de aprovação às cegas: A tomada de decisão da venda depende estritamente do retorno imediato do saldo pelo serviço de Contas, tornando a comunicação síncrona obrigatória
4. Principal risco da escolha:

    - O acoplamento temporal: se o serviço de Contas ficar instável ou fora do ar, o fluxo de pagamento do cliente é interrompido instantaneamente, gerando atrito na experiência de compra.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      X Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

[ Usuário ] ---( 1. Cria Conta )---> [ Serviço de Cadastro ] ---( 2. Publica Evento: Conta Criada )---> [ Message Broker / Fila ] ---( 3. Consome Mensagem )---> [ Serviço de E-mail ] ---> [ Provedor de E-mail ]

3. Justificativa (mínimo 2 fatores):

    - Tolerância a atraso: O atraso de alguns segundos ou até de um minuto no recebimento do e-mail de boas-vindas não impacta a usabilidade imediata, já que o usuário quer começar a usar o app logo após o cadastro.  
    - Resiliência a falhas de terceiros: Como o provedor de e-mail apresenta lentidão e falhas esporádicas, a mensageria assíncrona permite realizar novas tentativas automáticas (retries) sem que o usuário perceba ou sofra lentidão no cadastro. 
4. Principal risco da escolha:

    - A eventualidade de atrasos maiores ou falhas persistentes no broker/provedor que deixem o envio pendente por um período prolongado, além da complexidade de rastreabilidade caso o e-mail falhe silenciosamente após esgotar as tentativas.

## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      X Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

[ Cliente / Checkout ] ──( 1. Finaliza Venda )──> [ Serviço de Pedidos ] ──( 2. Publica Evento: Venda Realizada )──> [ Message Broker / Fila ] ---( 3. Consome Assincronamente )---> [ Serviço de Estoque ]

3. Justificativa (mínimo 2 fatores):

    - Tolerância a atrasos controlados: É aceitável que a baixa no estoque ocorra alguns segundos depois da compra, o que desacopla o processamento pesado do momento do checkout. 
    - Absorção de picos de tráfego: Com o aumento de 10x no tráfego em promoções, a fila atua como um amortecedor, impedindo que a sobrecarga no serviço de Estoque derrube o sistema de vendas ou deixe o checkout lento.  
4. Principal risco da escolha:

    - O risco de inconsistência temporária (overselling), onde o sistema pode vender um item nos instantes de pico antes que a mensagem de baixa seja consumida e processada pelo estoque.

## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      X API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

[ App Mobile / Web ] ──( 1. Requisição Consolidada )──> [ API Gateway / BFF ] ──( 2. Chamadas Paralelas )──> [ 5 Microsserviços ] ---( 3. Agrega e Formata Dados )---( 4. Payload Único Otimizado )---> [ App Mobile / Web ]

3. Justificativa (mínimo 2 fatores):
   
    - Otimização para redes ruins e abertura rápida: O BFF centraliza as chamadas no backend, reduzindo o tráfego de rede do cliente móvel e entregando um payload único estruturado sob medida para a tela. 
    - Desacoplamento de contratos e equipes: Permite abstrair a complexidade dos 5 serviços distintos (cada um com seu formato, autenticação e equipe própria), facilitando a criação futura de novas versões, como a web, sem impactar diretamente os clientes.  
4. Principal risco da escolha:

    -   O BFF pode se tornar um ponto único de falha (Single Point of Failure) ou um gargalo de processamento caso não seja bem dimensionado para agregar todas as respostas simultâneas dos serviços de backend.

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?
O que aconteceria no fluxo: Se a mensagem de "Venda Realizada" for entregue duas vezes pelo broker devido à garantia de entrega at least once (pelo menos uma vez), o consumidor processaria a baixa de estoque do mesmo pedido duplicadamente, abatendo unidades a mais do produto de forma incorreta.

    - Como o consumidor deveria se proteger: O serviço consumidor (Estoque) deve implementar a idempotência. Isso pode ser feito registrando o ID único da transação/pedido em uma tabela de controle ou banco de dados antes de efeturar a baixa; caso uma mensagem com o mesmo ID chegue novamente, ela é identificada como duplicada e descartada de forma segura sem alterar o estoque de novo.
