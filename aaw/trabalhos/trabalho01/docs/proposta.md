# Proposta — Trabalho Semestral · Arquitetura de Aplicações Web 2026.2

- **Aluno:** Igor Nóbrega Moreira · **Curso/Turma:** Ciência da Computação/ Arquitetura de Aplicações Web
- **Repositório:** github.com/iguinnmr/ra-202451085361/

## 1. Domínio e problema

O projeto é uma loja de roupas masculinas online. A aplicação resolve o problema de
expor o catálogo por categoria (camisas, blusas de frio, calças, shorts e tênis), dar ao
cliente uma jornada completa de compra — navegação, detalhe do produto com seleção de
tamanho, carrinho e histórico de pedidos — e dar ao lojista controle de estoque em tempo
real, evitando vender itens sem estoque disponível no tamanho escolhido.

**Páginas do frontend:** página inicial · menu com 5 categorias (camisas, blusas de frio, calças, shorts e tênis), cada uma listando
seus produtos com descrição básica · página de detalhe do produto (fotos, descrição
completa, seleção de tamanho) · login/cadastro · perfil do cliente com abas (meus
pedidos, dados da conta, carrinho).

## 2. Entidades

**Produto**
- `nome`, `descricao`, `preco`, `categoria` (`camisas` · `blusas-de-frio` · `calcas` ·
  `shorts` · `tenis`), `tamanhosDisponiveis` (array, ex.: P/M/G/GG ou numeração para
  tênis), `estoquePorTamanho` (map tamanho→quantidade), `sku`, `imagens` (array de URLs),
  `ativo`.

**Pedido**
- `cliente` (referência ao usuário), `itens` (array de `{ produtoId, tamanho, quantidade,
  precoUnitario }`), `status` (`carrinho` → `pendente` → `pago` → `enviado`, ou
  `cancelado`), `valorTotal`, `dataCriacao`.

O **carrinho** é o próprio pedido do cliente enquanto está em status `carrinho`: cada
cliente tem no máximo um pedido nesse status por vez. Ao finalizar a compra, o status
muda para `pendente` e o pedido passa a aparecer em "meus pedidos".

**Relacionamento:** Pedido → Produto é N-N (um pedido contém vários produtos, um produto
aparece em vários pedidos), modelado por **referência** (`produtoId` dentro do array
`itens`), e não por documento embutido: o preço e o estoque do produto mudam com o tempo,
então o pedido precisa guardar o preço praticado naquele momento (`precoUnitario`) em vez
de depender do documento do produto poder ser alterado depois.

## 3. Endpoints previstos

- `GET /produtos` — lista, com filtro opcional por categoria
- `GET /produtos/:id` — detalhe de um produto
- `POST /produtos` — cria produto (admin)
- `PUT /produtos/:id` — atualiza produto (admin)
- `DELETE /produtos/:id` — remove produto (admin)
- `GET /pedidos` — lista pedidos do cliente autenticado (usuário vê os próprios; admin
  vê todos), usado tanto na aba "meus pedidos" (status ≠ carrinho) quanto na aba
  "carrinho" (status = carrinho)
- `GET /pedidos/:id` — detalhe de um pedido
- `POST /pedidos/:id/itens` — adiciona um item (produto + tamanho + quantidade) ao
  carrinho aberto do cliente
- `POST /pedidos/:id/finalizar` — fecha o carrinho: muda status para `pendente`,
  recalcula `valorTotal` e valida estoque
- `PATCH /pedidos/:id` — atualiza status do pedido (admin)
- `DELETE /pedidos/:id` — cancela pedido

## 4. Regra de negócio

Ao criar um pedido (`POST /pedidos`), a camada de serviço:
1. Verifica se cada produto/tamanho pedido tem estoque suficiente.
2. Calcula o `valorTotal` somando `precoUnitario × quantidade` de cada item (o preço não
   é confiado ao cliente, é lido do produto no momento da criação).
3. Decrementa o estoque do tamanho correspondente em cada produto.

## 5. Casos de erro

- `produtoId` inexistente no pedido → **404** (`Produto não encontrado`)
- Quantidade pedida maior que o estoque disponível no tamanho → **400** (`Estoque
  insuficiente`)

## 6. Stack escolhida

- **Backend:** Node.js + Express
- **Banco:** MongoDB
- **Frontend:** HTML + JavaScript (fetch), podendo evoluir para React

## 7. Segurança e testes

- **JWT:** endpoints de registro e login; login devolve token com expiração configurada;
  rotas protegidas exigem `Authorization: Bearer <token>`.
- **RBAC:** dois perfis — `cliente` e `admin`. Criar/editar/remover produto e alterar
  status de pedido ficam restritos a `admin`; criar pedido fica disponível a `cliente`.
- **Testes:** cobertura da regra de negócio de criação de pedido — 2 cenários de sucesso
  (pedido criado com estoque suficiente, valor total calculado corretamente) e 2 de erro
  (produto inexistente, estoque insuficiente).
