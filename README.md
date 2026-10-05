# Undercraft

Undercraft é uma conversão total **Undertale × Minecraft**. O objetivo é integrar as mecânicas de Minecraft aos sistemas de Undertale, em vez de apenas trocar sprites.

## Objetivos

- Steve como personagem jogável no overworld, cutscenes e batalhas.
- Item equipado visível na mão fora da batalha.
- Hotbar, inventário, equipamentos e crafting persistentes.
- Blocos colocáveis/quebráveis integrados às rooms e ao save.
- Ferramentas e itens com funções reais.
- FIGHT / ACT / ITEM / MERCY integrados ao estado do Steve.
- Diálogos, puzzles, encontros e bosses reagindo às mecânicas novas.
- Patch reproduzível aplicado sobre uma cópia legítima de Undertale.

## Estrutura inicial

- `tools/`: ferramentas de análise e build.
- `mod/`: manifesto e fontes próprias do mod.
- `docs/`: arquitetura e decisões técnicas.

## Regra de distribuição

Este repositório **não inclui `data.win`, executáveis, áudio ou outros arquivos originais de Undertale/Minecraft**. O build deve operar sobre arquivos fornecidos localmente pelo usuário e gerar somente o necessário para o mod/patch.
