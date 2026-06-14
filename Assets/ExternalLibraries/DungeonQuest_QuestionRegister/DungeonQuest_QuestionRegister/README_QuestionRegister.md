# Dungeon Quest 2 — Cadastro de Perguntas

Lógica + UI para cadastrar perguntas (enunciado, explicação, temas e alternativas,
marcando a correta). No mesmo padrão dos seus controllers: `QuestionController`
"puro" (`HttpClient` + `async/await` + envelope `{response}` + Newtonsoft) e a tela
`QuestionRegisterScreenController` (MonoBehaviour, `async void` nos botões).

## A dependência circular (o ponto central)

A pergunta tem `answer_id` (alternativa correta), mas as alternativas têm
`question_id`. Não dá pra criar tudo de uma vez. O `QuestionController` resolve em
**3 passos**:

1. `POST /questions` com `answer_id` **placeholder** → obtém `question_id`
2. `POST /choices` (uma por alternativa) → obtém os ids
3. `PATCH /questions/{id}` com `answer_id` = id da alternativa correta

**Rollback:** se uma alternativa falhar no passo 2, a pergunta recém-criada é
apagada (`DELETE`) para não ficar pergunta órfã. Se só o passo 3 falhar, a pergunta
e as alternativas ficam; é só salvar de novo para marcar a correta.

> Se a API rejeitar o `answer_id` placeholder (`00000000-...`), troque por `null`
> ou omita o campo no passo 1 — está comentado no `QuestionController`.

## Estrutura

```
Scripts/
  Api/
    QuestionModels.cs               ApiResponse<T>, TagModel, QuestionModel, ChoiceModel
    QuestionController.cs           fluxo de 3 passos + rollback + helpers GET/POST/PATCH/DELETE
  UI/
    QuestionRegisterScreenController.cs  tela (async void), monta temas/alternativas e salva
    ChoiceRowController.cs          linha de alternativa (texto + "correta" + remover)
    TagToggleController.cs          tema selecionável (multi-seleção)
  Editor/
    QuestionRegisterUIBuilder.cs    gera Canvas + 3 prefabs (menu "Dungeon Quest")
```

## Setup

1. Copie `Scripts/` para `Assets/`.
2. Menu **Dungeon Quest ▸ Build Question Register UI** → cria Canvas + prefabs
   (`QuestionRegisterScreen`, `ChoiceRow`, `TagToggle`) já ligados.
3. **Play**: a tela carrega os temas da API, abre 4 alternativas em branco; preencha,
   marque a correta e um ou mais temas, e clique em **Cadastrar pergunta**.

## Regras de validação (na tela)

- Enunciado obrigatório.
- Mínimo de 2 alternativas preenchidas (vazias são ignoradas).
- Exatamente 1 marcada como correta (o "correta" funciona como rádio).
- Pelo menos 1 tema selecionado.

## Observações

- `ApiResponse<T>` e `TagModel` também existem no módulo de trilhas (que está de
  lado). Se for juntar os dois no mesmo projeto, mantenha só uma definição de cada.
- Estilo: troque o **TMP Font Asset** pela fonte pixel e atribua sprites 9-sliced aos
  `Image` para casar com o mockup.
- Próximo passo natural: tela de **edição/lista** de perguntas (a API já tem GET/PATCH/DELETE).
