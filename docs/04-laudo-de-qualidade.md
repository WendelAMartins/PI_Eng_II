# Laudo de qualidade

## Resumo

O sistema foi testado por 5 colegas (Carlos Eduardo da Silva, Mariana Souza Ramos, Lucas Ferreira dos Santos, Juliana Costa Ribeiro e Rodrigo Alves Martins), entre 05/10/2026 e 01/11/2026. Além disso, o sistema tem 31 testes automáticos das regras de negócio (`dotnet test`), e todos passam.

## Erros encontrados e o que foi feito

| # | Problema | Situação |
|---|---|---|
| 1 | A OS não era explicada ao solicitante. | Corrigido: o chamado em análise mostra um aviso dizendo que o número da OS só é gerado quando a chefia aceita. |
| 2 | A busca só achava pelo nome exato e não aceitava o número da OS. | Corrigido: a busca ignora maiúsculas e minúsculas e aceita o número da OS (ex.: "OS 4"). |
| 3 | Coordenadas digitadas com ponto davam erro. | Corrigido: aceita ponto ou vírgula e avisa se o valor for inválido. |
| 4 | A tela não avisava os formatos de foto aceitos. | Corrigido: o formulário mostra "JPG ou PNG, até 5 MB". |
| 5 | "Av." e "Avenida" eram tratados como endereços diferentes. | Corrigido: o sistema ignora acentos, pontuação e abreviações comuns ao comparar endereços. |
| 6 | Era possível programar uma data que já passou. | Corrigido: o campo bloqueia dias anteriores e o sistema também recusa a data. |
| 7 | Ao recusar sem motivo, o aviso aparecia no topo da página. | Corrigido: o aviso aparece no próprio cartão do chamado. |
| 8 | Não havia como preencher a localização pelo celular. | Corrigido: botão "Usar minha localização". |
| 9 | A chefia não via foto, descrição e histórico da OS no painel. | Corrigido: link "Ver detalhes, foto e histórico" em cada cartão. Também corrigi o identificador dos cartões. |

## Não implementado nesta versão

Aviso por e-mail ou WhatsApp quando o serviço termina, e login com senha e perfis de acesso. Ficaram de fora por estarem além do escopo da disciplina.

## Evidências

Os prints estão na pasta [evidencias](evidencias).

## Conclusão

Os 8 pontos levantados pelos colegas foram corrigidos, e mais 1 defeito que achei ao preparar as evidências também. O sistema cumpre o fluxo principal: abrir o chamado, a chefia aceitar e gerar a OS, programar, iniciar, encerrar e acompanhar o histórico.
