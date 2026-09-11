# HANDOUT — AULA 02

## Dissecando o HTTP

*6 requisições sob o microscópio — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

> **Nomes:** Igor Nóbrega Moreira e Gabriel Luiz Sá Silva  **Turma:** Arquitetura de Aplicações Web  **Data:** 14 / 09 / 2026

## REQUISIÇÃO 01 — A prateleira inteira

```text
→ REQUISIÇÃO
GET /api/livros HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

[ { "id": 1, "titulo": "Clean Code", "autor": "Robert C. Martin" },
  { "id": 7, "titulo": "O Programador Pragmático", "autor": "Hunt & Thomas" } ]
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)? 
   - O cliente utilizou o verbo GET para solicitar o recurso /api/livros (ou seja, pediu a lista de todos os livros cadastrados na biblioteca). 

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
   - O status code retornado foi 200 OK, indicando que a requisição deu totalmente certo e o servidor devolveu o array com os livros em formato JSON.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
   - O estado do servidor não muda, pois o GET é utilizado apenas para leitura, então a resposta continuará a mesma.

## REQUISIÇÃO 02 — O livro fantasma

```text
→ REQUISIÇÃO
GET /api/livros/99 HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{ "title": "Not Found", "status": 404 }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
    - O cliente usou o verbo GET para buscar o recurso específico /api/livros/99 (tentou encontrar o livro de ID 99).

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
    - O status 404 Not Found indindica que o recurso solicitado não existe no servidor, então a requisição não deu certo. A culpa é do cliente que fez a requisição, pois ele pediu um ID que não está cadastrado na base de dados.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
    - O estado do servidor não muda. A resposta se mantém a mesma: 404 Not Found.

## REQUISIÇÃO 03 — Livro novo na estante

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

```text
← RESPOSTA
HTTP/1.1 201 Created
Location: /api/livros/8
Content-Type: application/json

{ "id": 8, "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
    - O cliente usou o verbo POST no recurso /api/livros, enviando no corpo da requisição os dados de um livro novo ("Domain-Driven Design") para ser cadastrado.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
    - O status 201 Created informa que a requisição deu certo e que um novo recurso foi criado com sucesso.

3. Enviando este POST 3 vezes seguidas, o que acontece na estante? Para que serve o header Location?
    - Se você enviar o mesmo POST 3 vezes, o servidor criará três livros idênticos, cada um com um ID diferente. O header Location serve para informar o endereço exato (/api/livros/8) onde o recurso recém-criado pode ser acessado ou consultado a partir de agora.

## REQUISIÇÃO 04 — Corrigindo a ficha completa

```text
→ REQUISIÇÃO
PUT /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
    - O cliente utilizou o verbo PUT no recurso /api/livros/7 para substituir os dados completos do livro de ID 7 pelos novos dados informados na requisição.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
    - O status 200 OK informa que a requisição deu certo e que o registro foi atualizado corretamente pelo servidor.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
    - Na primeira vez, o servidor atualiza o registro conforme solicitado. Já na segunda e terceira vezes, o estado do servidor não muda mais, pois os dados enviados são exatamente os mesmos que já estão lá (isso torna o PUT idempotente). A resposta continua sendo 200 OK confirmando a alteração.

## REQUISIÇÃO 05 — Fora do catálogo

```text
→ REQUISIÇÃO
DELETE /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
```

```text
← RESPOSTA
HTTP/1.1 204 No Content
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
    - O cliente utilizou o verbo DELETE para remover o recurso /api/livros/7, apagando o livro de ID 7 do sistema.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
    - O status 204 No Content informa que a requisição deu certo, o livro foi apagado, mas o servidor não retornou nenhum corpo na resposta (já que o recurso não existe mais).

3. Repetindo o DELETE, o estado do servidor muda? Que resposta você ESPERA na segunda vez?
    - Na primeira vez o livro é apagado. Se repetir, o estado do servidor não muda porque o livro já não está lá. Na segunda vez, o servidor retorne um 404 Not Found para indicar que o recurso já não existe mais.

## REQUISIÇÃO 06 — O cadastro capenga

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "autor": "Anônimo" }
```

```text
← RESPOSTA
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{ "title": "Bad Request", "status": 400,
  "errors": { "Titulo": [ "O campo Titulo é obrigatório" ] } }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
    - O cliente usou o verbo POST em /api/livros tentando cadastrar um livro mandando apenas o autor, sem informar o título.
    
2. O que o status code informa? Deu certo? Culpa de quem se não deu?
    - O status 400 Bad Request informa que a requisição está incompleta. A culpa é do cliente, pois ele enviou um JSON faltando um campo obrigatório.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
    - O estado do servidor não muda, pois a requisição é rejeitada, então não ocorre o cadastro. A resposta se repete idêntica nas 3 tentativas, apontando o erro de validação do campo "Titulo".

## TABELA-SÍNTESE — Os verbos do HTTP

*Preencham com base nos 6 cards. “Seguro” = não altera nada no servidor. “Idempotente” = repetir N vezes deixa o servidor no mesmo estado que 1 vez.*

| **Verbo**  |  **Para que serve**   | **Seguro?** | **Idempotente?** | **Status típicos** |
|    ---     |           ---         |      ---    |        ---       |         ---        |
| **`GET`**  |   Consultar recursos  |     Sim     |        Sim       |     200,404,400    |
| **`POST`** |  Criar novos recursos |     Não     |        Não       |     201,400,422    |
| **`PUT`**  | Atualiza por completo |     Não     |        Sim       |   200,204,400,404  |
|**`PATCH`** | Atualiza parcialmente |     Não     |        Não       |   200,204,400,404  |
|**`DELETE`**|    Remove recursos    |     Não     |        Sim       |     200,204,400    |

## DESAFIO

1. O verbo PATCH não apareceu em nenhum card. Qual a diferença entre PATCH e PUT? Um app de banco quer alterar SÓ o apelido do usuário, entre dezenas de campos do perfil — qual dos dois você usaria e por quê?
    - O PUT exige que você envie o recurso inteiro atualizado, pois ele substitui o registro por completo. Já o PATCH serve para alterar apenas os campos desejados, sem a necessidade de enviar todos os campos. Para   alterar apenas o apelido do usuário entre dezenas de campos do perfil, eu usaria o PATCH. Por conta da eficiência, você trafega na rede apenas o campo alterado, em vez de ter que resgatar e reenviar o objeto inteiro do usuário como o PUT exigiria.
