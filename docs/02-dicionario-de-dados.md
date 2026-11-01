# Dicionário de dados

SGBD: SQLite (via Entity Framework Core). O esquema é criado automaticamente na primeira execução.

## Tabela `Chamados`

| Coluna | Tipo | Nulo | Chave | Domínio / regra |
|---|---|---|---|---|
| Id | INTEGER | Não | PK, autoincremento | Identificador interno |
| Tipo | INTEGER | Não | | 0 PlacaCaida, 1 PlacaVirada, 2 FaixaApagada, 3 SemaforoComDefeito, 4 Outro |
| Descricao | TEXT(500) | Não | | Texto livre, até 500 caracteres |
| Endereco | TEXT(200) | Não | | Texto livre, até 200 caracteres |
| Latitude | REAL | Sim | | -90 a 90 |
| Longitude | REAL | Sim | | -180 a 180 |
| FotoPath | TEXT | Sim | | Caminho relativo em `/uploads`; JPG ou PNG até 5 MB |
| Solicitante | TEXT(80) | Não | | Nome de quem abriu |
| Status | INTEGER | Não | | 0 Solicitado, 1 Aceito, 2 Programado, 3 EmAndamento, 4 Encerrado, 5 Recusado |
| NumeroOs | INTEGER | Sim | Índice único | Preenchido ao aceitar; sequencial |
| CriadoEm | TEXT (UTC) | Não | | Data e hora de abertura |
| DataProgramada | TEXT (data) | Sim | | Preenchida ao programar |
| MotivoRecusa | TEXT(300) | Sim | | Obrigatório ao recusar |

## Tabela `Historicos`

| Coluna | Tipo | Nulo | Chave | Domínio / regra |
|---|---|---|---|---|
| Id | INTEGER | Não | PK, autoincremento | |
| ChamadoId | INTEGER | Não | FK → Chamados.Id (exclusão em cascata) | |
| Status | INTEGER | Não | | Mesmo domínio de `Chamados.Status` |
| Observacao | TEXT | Não | | Descrição da mudança |
| Em | TEXT (UTC) | Não | | Data e hora da mudança |

## Relacionamento

`Chamados` 1 — N `Historicos`. Todo chamado nasce com um registro de histórico.

## Modelo lógico (normalização)

O modelo está em 3FN: o histórico fica em tabela própria (sem grupo repetitivo em `Chamados`), e os atributos de `Chamados` dependem apenas da chave `Id`. Tipo e status são enumerações fixas armazenadas como inteiros.
