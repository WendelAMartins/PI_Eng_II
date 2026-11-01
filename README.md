# Chamados de Campo

Projeto Integrador Transdisciplinar em Engenharia de Software II.

Este é o meu projeto do PIT II: um sistema web para abrir e acompanhar solicitações de manutenção feitas por quem está em campo.

## De onde veio a ideia (e por que este repositório é diferente do sistema original)

Eu construí o **GSV (Gestão de Sinalização Viária)**, o sistema de gestão da **MCTRANS**, que já está no ar e em uso real: https://gsv.mctransonline.com.br/. Entre os módulos dele existe um para receber pedidos de manutenção vindos da rua, e foi esse módulo que me serviu de ponto de partida.

O problema que ele resolve é bem comum. Antes, os pedidos chegavam soltos, em conversas de mensagens. Eles se perdiam, ninguém sabia quem ia atender nem quando, e quem pediu não tinha como saber o andamento. Sem resposta, a pessoa acabava pedindo de novo, e o mesmo problema virava dois ou três pedidos.

Como o GSV é de uso interno da MCTRANS e o código não pode ser público, eu **não publiquei o repositório dele**. Em vez disso, extraí só essa ideia (abrir chamado, virar OS numerada, acompanhar o status e evitar duplicidade) e **reescrevi do zero como um projeto novo e independente**, com tema genérico e dados fictícios. Neste repositório não há código, dados, telas, chaves nem histórico do GSV. O que aproveitei foi a ideia do fluxo e o que aprendi construindo aquele sistema, e essa adaptação foi feita por mim, como autor do original, para esta disciplina.

## O que o sistema faz

- **Quem pede** (o solicitante) abre um chamado informando o tipo de problema, o endereço, uma descrição e, se quiser, uma foto e as coordenadas.
- **A chefia** analisa no painel. Se aceitar, o chamado vira uma **OS com número** (só é atendido o que tem número). Se recusar, precisa informar o motivo.
- Depois de aceita, a OS pode ser **programada** para uma data, **iniciada** e **encerrada**.
- **Quem pediu** consulta os próprios chamados e vê o status, o número da OS, a data programada e o histórico de tudo que aconteceu.
- **Sem duplicidade:** se já existe um chamado aberto do mesmo tipo no mesmo endereço, o sistema mostra o que já existe em vez de criar outro.

## Tecnologias

| Item | Escolha |
|---|---|
| Linguagem do back-end | C# (.NET 10) |
| Padrão | MVC (ASP.NET Core MVC), com as regras de negócio numa camada de serviço |
| Banco de dados | SQLite (arquivo local, criado sozinho) via Entity Framework Core |
| Front-end | Razor Views, HTML, CSS, Bootstrap e JavaScript |
| Testes | xUnit |
| Modo de codificação | Tradicional (sem low-code nem no-code) |

## Estrutura

```
src/ChamadosCampo.Web/
  Models/        entidades (Chamado, HistoricoChamado)
  Data/          contexto do banco e dados iniciais fictícios
  Services/      regras de negócio (ChamadoService)
  Controllers/   ChamadosController (solicitante) e PainelController (chefia)
  Views/         telas Razor
tests/ChamadosCampo.Tests/   testes automatizados
docs/                        documentação, formulário de teste e modelo de laudo
```

## Como rodar

Precisa do [.NET SDK 10](https://dotnet.microsoft.com/download). Não é preciso instalar banco de dados.

```bash
dotnet run --project src/ChamadosCampo.Web --urls http://localhost:5099
```

Abra http://localhost:5099. Na primeira execução o arquivo `chamados.db` é criado com 2 chamados de exemplo. Para zerar os dados, pare o sistema e apague esse arquivo.

Telas principais:

- `/Chamados/Create`: abrir chamado
- `/Chamados`: consultar chamados pelo nome do solicitante (o exemplo usa "Usuário Exemplo" ou "Rodrigo Alves Martins")
- `/Painel`: painel da chefia

## Testes

```bash
dotnet test
```

São 31 testes automatizados sobre as regras de negócio: abertura, campos obrigatórios, duplicidade (inclusive com abreviações no endereço), geração do número da OS, busca por nome ou número da OS, recusa com motivo, programação de data, fluxo completo de status, transições inválidas, coordenadas e validação de foto.

## Como o sistema está hospedado

Por ser um projeto acadêmico baseado em algo interno, ele **não está publicado na internet**. Ele roda localmente, como explicado acima, e a demonstração está nos vídeos da entrega.

## Limitações conhecidas

Deixo aqui o que não está pronto, de propósito, para ficar claro:

- Não há login. O solicitante é identificado pelo nome digitado e o painel da chefia abre direto. Em um sistema real seriam necessários autenticação e perfis de acesso.
- Não há aviso automático quando o status muda; o solicitante precisa consultar a tela.
- Os testes automatizados cobrem a camada de serviço, mas não os controllers nem as telas. As telas foram testadas manualmente (veja o laudo).

## Documentação

Tudo está na pasta [docs](docs):

1. [Requisitos e modelagem](docs/01-requisitos-e-modelagem.md): requisitos, regras de negócio, casos de uso, diagramas de classes, estados e sequência, e decisões de interface.
2. [Dicionário de dados](docs/02-dicionario-de-dados.md)
3. [Formulário de teste](docs/03-formulario-de-teste.md): usado pelos colegas que testaram o sistema.
4. [Laudo de qualidade](docs/04-laudo-de-qualidade.md): erros encontrados, correções e evidências.

## Aviso

Todos os dados, nomes e endereços usados aqui são fictícios.
