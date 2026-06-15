# Dungeon Quest 2 — Tela Jogar + Ranking Global

Duas telas, no mesmo padrão dos seus controllers (controller "puro" async +
MonoBehaviour de tela + builder de UI).

## 1) Tela Jogar (`PlaySetupScreen`)

Escolhe a **disciplina** (dropdown com "Variado" + tags do `GET /tags`), botão
**JOGAR** e um **X** que volta ao menu.

- Ao dar Play: grava `MatchConfig.TagTarget` e carrega a cena de combate
  (`combatScene`, padrão `"Combat"`).
- O **X** volta para `mainMenuScene` (padrão `"MainMenu"`).
- Navegação usa `GetComponent<SceneLoader>()` — **adicione o seu `SceneLoader`** no
  GameObject `PlaySetupScreen`.

Como o PvP local só depende da disciplina, esta tela substitui a antiga
"configuração" para o fluxo local: disciplina + play.

## 2) Tela Ranking Global (`RankingScreen`)

Lista os usuários por `high_score` (maior primeiro), **rolável**, até 100.

- `GET /api/users?limit=100` (teto da API) → ordena desc por `high_score`.
- Cada linha (prefab `RankingRow`): posição, nome e pontos.
- **X** volta ao menu.

## Setup

1. Copie `Scripts/` para `Assets/`.
2. Menu **Dungeon Quest ▸ Build Play Setup UI** e **▸ Build Ranking UI**.
3. Em cada tela gerada, **adicione o `SceneLoader`** no GameObject raiz e ajuste os
   nomes de cena (`mainMenuScene`, `combatScene`).

## Arquivos

```
Scripts/
  Core/  MatchConfig.cs               (disciplina escolhida p/ o combate)
  Api/   PlaySetupController.cs        (GET /tags)
         RankingController.cs          (GET /users + ordena)
         RankingModels.cs              (UserRankModel)
  UI/    PlaySetupScreenController.cs
         RankingScreenController.cs
         RankingRowController.cs
  Editor/ PlaySetupUIBuilder.cs
          RankingUIBuilder.cs
```

## Observações

- `ApiResponse<T>` e `TagModel` são reaproveitados dos módulos anteriores
  (perguntas/sala). Se faltar, defina-os uma vez.
- `MatchConfig` também aparece no módulo de combate — mantenha **uma** cópia.
- O ranking mostra todos os usuários retornados; se quiser esconder contas
  inativas, filtre por `active` no `RankingController`.
- Acima de 100 jogadores a API exige paginação (offset) — hoje o pedido é "até 100".
