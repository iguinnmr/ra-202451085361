# AAW - Aula 04: Auditoria de APIs REST (Café Newton vs. PetHouse)

Atividade prática referente à Aula 04 da disciplina Arquitetura de Aplicações Web.

## Relatório de Auditoria (PetHouse API - 12 Erros de Design)

| # | O que eu chamei | O que a resposta mostrou | Regra REST violada | Como eu redesenharia |
|---|---|---|---|---|
| **01** | `POST /api/v1/getPets` | Status 200 OK retornando a lista de pets. | Verbo na URI (RPC-style) e uso de POST para operação de leitura sem efeito colateral. | Método + Rota: `GET /api/v1/pets` <br> Status Code: `200 OK` |
| **02** | `GET /api/v1/deletarPet?id=7` | Status 200 OK com texto 'OK'. Pet removido do banco. | Uso de GET com efeito colateral (deletar), verbo na URI e ID via Query String. | Método + Rota: `DELETE /api/v1/pets/7` <br> Status Code: `204 No Content` |
| **03** | `GET /api/v1/pet/{id}` | Status 200 OK com dados do pet. | Inconsistência de convenção de nomes (uso do singular `/pet` em vez de `/pets`). | Método + Rota: `GET /api/v1/pets/{id}` <br> Status Code: `200 OK` |
| **04** | `GET /api/v1/banhosTosa`<br>`GET /api/v1/tutores_vip` | Status 200 OK. | Mistura de cases (*camelCase* e *snake_case*) e uso de adjetivo/filtro (`vip`) no caminho da rota. | Método + Rota: `GET /api/v1/banhos-e-tosas`<br>Método + Rota: `GET /api/v1/tutores?tipo=vip`<br>Status Code: `200 OK` |
| **05** | `POST /api/v1/pets` | Status 200 OK com texto 'Criado com sucesso'. | Status code de criação incorreto (deveria ser `201 Created`) e ausência do header `Location`. | Método + Rota: `POST /api/v1/pets`<br>Status Code: `201 Created`<br>Header: `Location: /api/v1/pets/{id}` |
| **06** | `GET /api/v1/pets/999999` | Status 200 OK com `{'erro': 'Pet nao encontrado'}`. | Status Code incoerente com a resposta (mascarou erro retornando `200 OK` para item inexistente). | Método + Rota: `GET /api/v1/pets/999999`<br>Status Code: `404 Not Found` |
| **07** | `GET /api/pets` | Status 200 OK com campos em *snake_case* (`data_nascimento`). | Falta de versionamento na URL (`/v1`) e quebra de contrato na nomenclatura do JSON. | Método + Rota: `GET /api/v1/pets`<br>Status Code: `200 OK` (com JSON padronizado em *camelCase*) |
| **08** | `GET /api/v1/petshops/1/clientes/5/pets/9/consultas/12/exames/6` | Status 200 OK retornando o exame 6. | Aninhamento e acoplamento excessivo de rotas (5 níveis de recursos encadeados). | Método + Rota: `GET /api/v1/exames/6`<br>Status Code: `200 OK` |
| **09** | `GET /api/v1/consultas` | Status 200 OK retornando milhares de registros. | Ausência de paginação em listagens com grande volume de dados. | Método + Rota: `GET /api/v1/consultas?page=1&size=20`<br>Status Code: `200 OK` |
| **10** | `PUT /api/v1/pets/12/vacinas` | Status 200 OK duplicando os registros a cada chamada. | Quebra da idempotência no método `PUT` (duplicou em vez de substituir a lista/recurso). | **Para Adicionar:** `POST /api/v1/pets/12/vacinas`<br>**Para Substituir:** `PUT /api/v1/pets/12/vacinas` |
| **11** | `POST /api/v1/sessao`<br>`GET /api/v1/meus-pets` | Status 200 OK usando sessão global no servidor. | Violação do princípio *Stateless* (guarda estado de sessão em vez de usar token por requisição). | Método + Rota: `GET /api/v1/meus-pets`<br>Status Code: `200 OK`<br>Header: `Authorization: Bearer <token>` |
| **12** | `GET /api/v1/tabela-de-precos` | Status 200 OK sem headers de cache. | Falta de estratégia de Caching para dados com baixíssima frequência de alteração. | Método + Rota: `GET /api/v1/tabela-de-precos`<br>Status Code: `200 OK`<br>Headers: `Cache-Control` e `ETag` |
