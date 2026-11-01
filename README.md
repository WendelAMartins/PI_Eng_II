# Chamados de Campo

Projeto Integrador Transdisciplinar em Engenharia de Software II.

Sistema web para abrir e acompanhar solicitações de manutenção em campo. O solicitante abre um chamado com tipo, endereço, descrição e foto. A chefia aceita (e o chamado vira **OS com número**), programa, inicia e encerra. O solicitante acompanha o status. Se já existe chamado aberto do mesmo tipo no mesmo endereço, o sistema mostra o existente em vez de duplicar.

Todos os dados são fictícios.

## Tecnologias

| Item | Escolha |
|---|---|
| Linguagem do back-end | C# (.NET 10) |
| Padrão | MVC (ASP.NET Core MVC) |
| Banco de dados | SQLite (arquivo local, criado automaticamente) via Entity Framework Core |
| Front-end | Razor Views, HTML, CSS, Bootstrap, JavaScript |
| Testes | xUnit |
| Modo de codificação | Tradicional |

## Estrutura

```
src/ChamadosCampo.Web/
  Models/        entidades (Chamado, HistoricoChamado)
  Data/          AppDbContext e dados iniciais fictícios
  Services/      regras de negócio (ChamadoService)
  Controllers/   ChamadosController (solicitante), PainelController (chefia)
  Views/         telas Razor
tests/ChamadosCampo.Tests/   testes automatizados
docs/                        documentação, formulário de teste e modelo de laudo
```

## Como rodar

Requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/ChamadosCampo.Web --urls http://localhost:5099
```

Abra http://localhost:5099. O banco `chamados.db` é criado na primeira execução, com 2 chamados de exemplo.

## Como testar

```bash
dotnet test
```

## Telas

- `/Chamados/Create`: abrir chamado.
- `/Chamados`: consultar chamados pelo nome do solicitante.
- `/Painel`: painel da chefia (aceitar, recusar, programar, iniciar, encerrar).

## Limitações conhecidas

- Não há login: o solicitante é identificado pelo nome digitado e o painel é aberto. Em produção seriam necessários autenticação e perfis.
- Não há notificação automática de mudança de status; o solicitante consulta a tela.

## Documentação

Veja a pasta [docs](docs): requisitos, modelagem UML, dicionário de dados, formulário de teste e modelo de laudo.
