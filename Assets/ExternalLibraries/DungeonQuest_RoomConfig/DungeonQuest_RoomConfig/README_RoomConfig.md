# Dungeon Quest 2 — Configuração de Sala

Substitui a antiga tela de "trilha". O usuário escolhe uma **disciplina** (tag) e a
**quantidade de inimigos**; ao criar, o **backend gera o código da sala**, que é
exibido na tela. Mesmo padrão dos seus controllers (controller async tipo
`UserController` + tela MonoBehaviour tipo `LoginScreenController`).

## Mapeamento com a API (`/api/rooms`)

`POST /api/rooms` body `{ title, owner, level_quantity, tag_target }` → resposta com
`code` gerado. Na tela:

| Campo da API | Origem na tela |
|---|---|
| `tag_target` | disciplina escolhida (label da tag) ou `"variado"` |
| `level_quantity` | quantidade de inimigos (stepper − / +) |
| `owner` | `Session.CurrentUserId` (do login) |
| `title` | opcional; se vazio, vira `"Sala de {disciplina}"` |
| `code` | **vem do backend** e é exibido no bloco "CÓDIGO DA SALA" |

> `tag_target` vai como **label** da tag (ex.: `historia`) ou `variado`. Se o backend
> esperar o **id** da tag, troque em `RoomConfigScreenController.SelectedTagTarget()`.

## Estrutura

```
Scripts/
  Api/
    RoomModels.cs            RoomModel (reusa ApiResponse<T>/TagModel do módulo de perguntas)
    RoomController.cs        GetTags / CreateRoom / GetRoomByCode (async + envelope)
  Core/
    Session.cs              guarda o usuário logado (CurrentUserId)
  UI/
    RoomConfigScreenController.cs  a tela
  Editor/
    RoomConfigUIBuilder.cs   gera Canvas + prefab (menu "Dungeon Quest")
```

## Setup

1. Copie `Scripts/` para `Assets/`.
2. Menu **Dungeon Quest ▸ Build Room Config UI** → cria Canvas + prefab
   `RoomConfigScreen` já ligado.
3. **Owner**: defina `Session.CurrentUserId` após o login (o `/api/users/login` agora
   retorna o `user` — veja abaixo). Para testar sem login, preencha
   **Owner Id Override** no inspector.
4. **Play**: escolhe disciplina, ajusta inimigos, clica em **Criar sala** → o código
   aparece.

## Definindo o owner no login

O login agora devolve `{ valid, user: { id, ... } }`. No seu fluxo de login, depois
do sucesso, guarde o id:

```csharp
// dentro do seu LoginScreenController, após login bem-sucedido:
Session.CurrentUserId = userId;   // id vindo de response.user.id
```

(Hoje o seu `UserController.LoginAsync` só retorna `bool`. Para ter o id, faça o
parse de `response.user.id` no login e repasse para a tela/Session.)

## Dependências entre módulos

`ApiResponse<T>` e `TagModel` são **reaproveitados** do módulo de Cadastro de
Perguntas (`QuestionModels.cs`). Se for usar este módulo **sozinho**, descomente o
bloco no fim de `RoomModels.cs`.

## ⚠️ Sobre o módulo de Perguntas (mudou na API)

O `POST /api/questions` agora aceita criar a pergunta **com `choices` inline**
(`{label, correct:true}`) + `tags` + `created_by` em **um único request** — o
`answer_id` é definido automaticamente. Ou seja, o fluxo de 3 passos com `tag_ids` que
entreguei antes ficou **desatualizado** (inclusive o campo virou `tags`, não
`tag_ids`). Recomendo simplificar o `QuestionController` para 1 request — posso refazer.

## Próximos passos

- Entrar em sala por código (`GET /api/rooms/code/{code}` já está no controller).
- Gerenciar jogadores na sala (`/api/rooms/{id}/users`) e placar (`score`).

---

## Atualização: abas Criar/Entrar + saída

A tela agora tem **duas abas**:

- **CRIAR**: disciplina + nº de inimigos → cria a sala (o back gera o `code`, exibido).
- **ENTRAR**: digita o código → `GET /rooms/code/{code}` → mostra a **configuração da
  sala** (disciplina = `tag_target`, inimigos = `level_quantity`) e permite **entrar**
  (`POST /rooms/{id}/users` com `Session.CurrentUserId`).

E um **X** no canto superior direito que volta ao menu principal.

**Navegação (X):** o controller usa `GetComponent<SceneLoader>()` (igual ao
`LoginScreenController`). Depois de gerar a UI, **adicione o seu `SceneLoader` no
GameObject `RoomConfigScreen`** e ajuste `mainMenuScene` (padrão `"MainMenu"`). Para ir
direto a um lobby após entrar, preencha `lobbySceneAfterJoin`.
