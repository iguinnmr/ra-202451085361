# HANDOUT — AULA 03

## Consultoria de Design: a API da EscolaTech

*Identifique os anti-padrões e proponha o redesenho — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

A EscolaTech contratou a consultoria de vocês para auditar a API do sistema escolar. Todos os endpoints abaixo FUNCIONAM e estão em produção — mas o time novo se recusa a mexer neles. Para CADA endpoint:


> **Nomes:** Igor Nóbrega Moreira e Gabriel Luiz Sá Silva  **Turma:** Arquitetura de Aplicações Web  **Data:** 09 / 09 / 2026

## ENDPOINT 01 — POST /api/getAlunos

**Documentação atual (extraída da wiki da EscolaTech):**

```text
POST /api/getAlunos
Retorna TODOS os alunos cadastrados (hoje: 12.482 registros).
Resposta: 200 OK + array JSON completo (~9 MB).
Obs. da wiki: "usar POST porque GET não estava funcionando".
```

1. Qual(is) problema(s) de design vocês identificam?
    - Uso de `POST` para leitura/consulta (deveria ser `GET`).
    - Verbo no caminho da URL (`getAlunos`) .
    - Ausência de paginação para um volume grande de dados (12.482 registros / ~9 MB).

2. Seu redesenho (método + rota + status codes):
    `GET /api/alunos?page=1&limit=20`
    `200 OK`

## ENDPOINT 02 — GET /deletarAluno?id=7

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /deletarAluno?id=7
Remove o aluno do banco de dados.
Resposta: 200 OK + "OK" (mesmo se o aluno não existir).
Obs. da wiki: "dá pra deletar pelo navegador, bem prático".
```

1. Qual(is) problema(s) de design vocês identificam?
    - Uso de `GET` para operação de exclusão.
    - Verbo e id expostos na URL (`deletarAluno?id=7`).
    - Retorno fixo de `200 OK` mesmo se o recurso não existir.

2. Seu redesenho (método + rota + status codes):
    `DELETE /api/alunos/7`
    `204 No Content`

## ENDPOINT 03 — POST /api/alunos (criação)

**Documentação atual (extraída da wiki da EscolaTech):**

```text
POST /api/alunos
Body: { "nome": "...", "curso": "..." }
Cria o aluno e responde: 200 OK + body "OK".
O app precisa buscar a lista inteira de novo para descobrir o ID gerado.
```

1. Qual(is) problema(s) de design vocês identificam?
    - Status code `200 OK` para criação de recurso.
    - Resposta textual sem o ID do recurso gerado, obrigando o cliente a realizar uma nova busca.

2. Seu redesenho (método + rota + status codes):
    `POST /api/alunos`
    `201 Created`

## ENDPOINT 04 — GET /escolas/1/turmas/3/alunos/25/matriculas/88/disciplinas/12

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /escolas/1/turmas/3/alunos/25/matriculas/88/disciplinas/12
Retorna os dados da disciplina 12 da matrícula 88.
Para montar a URL o app precisa conhecer 5 IDs diferentes.
Resposta: 200 OK + JSON da disciplina.
```

1. Qual(is) problema(s) de design vocês identificam?
    - Muitas rotas em apenas uma solicitação.
    
2. Seu redesenho (método + rota + status codes):
    `GET /api/matriculas/88/disciplinas/12`
    `200 OK` (se encontrado) ou `404 Not Found`.

## ENDPOINT 05 — GET /api/alunos/7/matriculas (erro)

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /api/alunos/7/matriculas
Se o aluno 7 não existe, responde:
200 OK + "<html><b>Erro: aluno nao existe!</b></html>"
O app mobile quebra tentando fazer parse do JSON.
```

1. Qual(is) problema(s) de design vocês identificam?
    - Incompatibilidade de formato (retorna HTML em uma API que opera em JSON).
    - Status `200 OK` retornado em uma situação de erro, quebrando o app mobile do cliente.

2. Seu redesenho (método + rota + status codes):
    `GET /api/alunos/7/matriculas`
    `404 Not Found`
    
## ENDPOINT 06 — PUT /api/atualizarNotaParcial?aluno=7&disc=12&nota=8.5

**Documentação atual (extraída da wiki da EscolaTech):**

```text
PUT /api/atualizarNotaParcial?aluno=7&disc=12&nota=8.5
Atualiza SÓ a nota parcial da disciplina, sem body.
Todos os dados vão na query string.
Resposta: 200 OK + "OK".
```

1. Qual(is) problema(s) de design vocês identificam?
    - Verbo na URL.
    - Uso do `PUT` (substituição total) para uma alteração parcial de campo (o adequado é `PATCH`).

2. Seu redesenho (método + rota + status codes):
     `PATCH /api/alunos/7/disciplinas/12`
     `{"notaParcial": 8.5}`
     `200 OK`

## DESAFIO

1. A EscolaTech quer lançar mudanças na API sem quebrar o app mobile antigo, que não recebe atualização há 2 anos. Que decisão de design — que falta na API INTEIRA — resolve esse problema? Como ficariam as rotas?
1. **Decisão de design:**
   - Implementar um versionamento de API

2. **Como ficam as rotas:**
   - **app antigo:** `GET /api/v1/getAlunos`, `GET /api/v1/deletarAluno?id=7`
   - **novas rotas:** `GET /api/v2/alunos`, `DELETE /api/v2/alunos/7`
