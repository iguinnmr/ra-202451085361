# HANDOUT — AULA 05

## Escolha o Banco

*Persistência em arquiteturas distribuídas — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

> **Nomes:** Igor Nóbrega Moreira e Andrey Henrique Silverio Horta  **Turma:** Arquitetura de Aplicações Web (Thalles)  **Data:** 03 / 09 / 2026

## CENÁRIO 01 — TechStore — o catálogo camaleão

E-commerce com 80 mil produtos. Cada categoria tem atributos completamente diferentes: livro tem autor e número de páginas; notebook tem RAM e CPU; camiseta tem tamanho e cor.

- A cada categoria nova, o time faz ALTER TABLE e a tabela produtos já tem 92 colunas (a maioria NULL)
- O produto é quase sempre lido INTEIRO, de uma vez, para montar a página
- Novos atributos surgem toda semana — o marketing não espera o DBA
- Relatórios cruzando categorias são raros

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     X Documento     ☐ Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):
    O modelo de documentos é o mais adequado porque cada categoria de produto possui atributos diferentes, como autor em livros, RAM em notebooks e tamanho em camisetas. Esse modelo permite que cada produto tenha seus próprios campos, evitando uma tabela com muitas colunas vazias e alterações frequentes no banco. Além disso, como os produtos são normalmente lidos inteiros para montar a página, manter todas as informações juntas em um único documento torna o acesso mais simples e eficiente.

3. Principal risco da escolha:
    A flexibilidade do banco de documentos pode causar falta de padronização nos dados. Diferentes produtos podem acabar usando nomes, formatos ou tipos diferentes para o mesmo atributo, dificultando consultas, validações e a geração de relatórios. Por isso, é necessário definir regras de validação e manter uma boa organização dos documentos.

## CENÁRIO 02 — MegaCart — o carrinho da Black Friday

Serviço de carrinho de compras de um varejista gigante. Na Black Friday são milhões de leituras e escritas por minuto.

- O acesso é SEMPRE pela chave: “carrinho do cliente 12345” — nunca por busca ou filtro
- Todo carrinho expira automaticamente em 48h (TTL)
- Latência precisa ser de poucos milissegundos
- Perder um carrinho é chato, mas NÃO é tragédia — o cliente remonta

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     X Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):
    O modelo chave-valor é ideal porque o carrinho é sempre acessado diretamente pela chave do cliente, sem necessidade de consultas ou filtros complexos. Além disso, suporta milhões de leituras e escritas com baixa latência e permite configurar TTL de 48 horas, removendo os carrinhos automaticamente após esse período.

3. Principal risco da escolha:
    O principal risco é a menor capacidade para consultas e relacionamentos complexos, já que o modelo é otimizado para acesso direto pela chave. Além disso, em caso de falha ou perda de dados, alguns carrinhos podem desaparecer.

## CENÁRIO 03 — PayBank — dinheiro não pode evaporar

Módulo de transferências de um banco. Uma transferência debita uma conta e credita outra — as duas operações têm que acontecer JUNTAS ou nenhuma acontece.

- Consistência forte exigida por lei — saldo errado é multa do Banco Central
- Auditoria cruza contas, clientes, agências e transações em relatórios complexos (joins)
- O esquema dos dados é estável há 10 anos
- Volume alto, mas previsível

**Sua análise:**

1. Modelo recomendado:   X Relacional     ☐ Documento     ☐ Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):
    O modelo relacional é ideal porque exige consistência forte e transações, garantindo que o débito e o crédito aconteçam juntos ou nenhum aconteça. Além disso, o sistema precisa realizar joins e consultas complexas entre contas, clientes, agências e transações. Como o esquema é estável há anos e o volume é previsível, a estrutura relacional atende bem ao cenário.

3. Principal risco da escolha:
    O principal risco é a menor flexibilidade para mudanças no esquema e a possibilidade de o banco ter dificuldades de escala caso o volume de transações cresça muito além do previsto.

## CENÁRIO 04 — FriendLink — amigos dos seus amigos

Rede social profissional em que o produto principal é a indicação: “pessoas que você talvez conheça” e “quem pode te apresentar à empresa X”.

- As consultas dominantes percorrem RELACIONAMENTOS: amigos dos amigos, caminhos de indicação com até 6 níveis
- Em banco relacional, cada nível vira um self-join — com 6 níveis a consulta já não responde
- Os dados de perfil são simples; o valor está nas CONEXÕES
- O grafo cresce milhões de arestas por dia

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     ☐ Chave-valor     X Grafo

2. Justificativa (mínimo 2 fatores do contexto):
    Ideal porque o principal valor do sistema está nas conexões entre pessoas. Consultas como amigos dos amigos e caminhos de indicação de até 6 níveis são mais eficientes em grafos, evitando vários self-joins do modelo relacional. Além disso, o banco é projetado para representar e percorrer relacionamentos mesmo com milhões de novas conexões por dia.

3. Principal risco da escolha:
    O principal risco é a complexidade e o custo para escalar o grafo, principalmente com milhões de novas conexões e consultas. Também pode ser mais difícil manter e operar do que um banco relacional tradicional.

## DESAFIO

1. Escolha um dos cenários e responda: se a rede particionar (metade dos servidores não enxerga a outra metade), o que o sistema deve fazer — parar de responder para não errar, ou continuar respondendo mesmo arriscando dados desatualizados? Qual letra do CAP vocês sacrificariam e por quê?

    No cenário 3 (PayBank — dinheiro não pode evaporar), caso a rede particione, o sistema deve parar de responder durante a partição, pois é mais seguro impedir uma transferência do que correr o risco de gerar saldos incorretos. Nesse caso, sacrificamos a Disponibilidade (A) para manter a Consistência (C) e a Tolerância à Partição (P), já que o banco não pode aceitar dados desatualizados ou inconsistentes.
