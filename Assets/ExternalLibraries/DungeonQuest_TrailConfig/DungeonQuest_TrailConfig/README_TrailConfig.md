# Dungeon Quest 2 — Tela de Configuração de Trilhas

Módulo de UI + integração para montar **trilhas** (sequências de fases, cada fase
com um tema, nº de perguntas e inimigo, mais um boss final). Segue o mesmo padrão
dos controllers existentes: `MonoBehaviour` com referências serializadas, chamadas
em coroutine, `UnityWebRequest` + `Newtonsoft.Json` e o envelope `{ "response": ... }`.

---

## ⚠️ Decisão de arquitetura: persistência da trilha

A API **não possui endpoint de trilhas** — só existem `Usuários`, `Tags`,
`Perguntas` e `Alternativas`. Para destravar a tela sem depender do backend, a
persistência é abstraída em `ITrailRepository` com duas implementações:

| Repositório | Estado | Uso |
|---|---|---|
| `LocalTrailRepository` | **Ativo (padrão)** | Salva em `Application.persistentDataPath/trails.json`. Funciona hoje, sem mexer no backend. |
| `RemoteTrailRepository` | Pronto, inativo | Chama `/api/trilhas` (mesmo envelope/coroutine). Basta o backend implementar a rota. |

Para alternar: no inspector do `TrailConfigController`, marque **`Use Remote Api`**.

O que **já é integração viva com a API** (independente da decisão acima):
- **Temas das fases** → `GET /api/tags` popula os dropdowns.
- **Validação de disponibilidade** → `GET /api/questions` conta perguntas por tag e
  avisa (sem bloquear) se uma fase pedir mais perguntas do que existem para aquele tema.

---

## Estrutura de arquivos

```
Scripts/
  Api/
    TrailModels.cs           DTOs (TrailDto, TrailPhaseDto) + models de leitura
    TrailApiService.cs       HTTP em coroutine + LoadTags / LoadQuestionCountsByTag
    ITrailRepository.cs      contrato de persistência
    LocalTrailRepository.cs  persistência local em JSON (padrão)
    RemoteTrailRepository.cs  persistência via /api/trilhas (quando existir)
  UI/
    TrailConfigController.cs     controller da tela
    TrailPhaseRowController.cs   controller da linha de fase (reutilizável)
  Editor/
    TrailConfigUIBuilder.cs  gera Canvas + prefabs (menu "Dungeon Quest")
```

Dependências do projeto já presentes: **Newtonsoft.Json** e **TextMeshPro**.

---

## Setup (3 passos)

1. Copie a pasta `Scripts/` para dentro de `Assets/` do projeto (mescla com
   `Scripts/Api` e `Scripts/UI` existentes).
2. No menu do Unity: **Dungeon Quest ▸ Build Trail Config UI**. Isso cria/reutiliza
   `Canvas` + `EventSystem`, monta a tela e salva dois prefabs em
   `Assets/DungeonQuest/Prefabs/` (`TrailConfigScreen` e `TrailPhaseRow`), com todas
   as referências já ligadas.
3. Dê **Play**. A tela carrega os temas da API, semeia 3 fases + 1 boss e já permite
   adicionar/remover fases e salvar.

> Por que um builder em vez de um `.prefab` pronto? Um `.prefab` em YAML escrito à mão
> não consegue casar os GUIDs dos seus scripts/TMP no import e quebra as referências.
> Gerar em-editor garante que tudo fica ligado.

---

## Fluxo da tela

`Start` → `LoadTags` (API) → `LoadQuestionCountsByTag` (API, opcional) → semeia fases →
usuário edita → **Salvar**: valida (nome, ≥1 fase, nº de perguntas) → aviso de
disponibilidade → `ITrailRepository.SaveTrail`.

Cada `TrailPhaseRowController` expõe `ToModel(order)` e `Validate(out err)`; o
controller renumera as fases ao adicionar/remover e marca a linha de boss como `BOSS`.

---

## Ativando a API remota (contrato sugerido para o backend)

Quando quiser persistir no servidor, implemente no FastAPI (mantendo o envelope
`{ "response": ... }` de todas as respostas):

```python
# rotas
GET    /api/trilhas?limit=&offset=     -> { "response": [ Trilha, ... ] }
POST   /api/trilhas      (body: Trilha) -> { "response": Trilha }   # 201
GET    /api/trilhas/{id}                -> { "response": Trilha }
PATCH  /api/trilhas/{id} (body: Trilha) -> { "response": Trilha }
DELETE /api/trilhas/{id}                -> { "response": {...} }

# schema (Pydantic)
class Fase(BaseModel):
    order: int
    tag_id: str            # FK -> tags.id
    tag_label: str | None  # opcional (denormalizado p/ UI)
    question_count: int
    enemy_name: str | None
    is_boss: bool = False

class Trilha(BaseModel):
    id: str | None = None
    name: str
    phases: list[Fase]
```

Modelo relacional sugerido: tabela `trilhas (id, name)` + tabela `fases
(id, trilha_id FK, order, tag_id FK, question_count, enemy_name, is_boss)`.

Feito isso: marque `Use Remote Api` no controller. Nenhuma outra mudança no cliente.

---

## Wiring manual (caso não use o builder)

`TrailConfigScreen` (com `TrailConfigController`):
`trailNameInput` → TMP_InputField · `phaseListContainer` → o `Content` do ScrollView ·
`phaseRowPrefab` → prefab `TrailPhaseRow` · `addPhaseButton`, `saveButton` → Buttons ·
`statusText` → TMP_Text.

`TrailPhaseRow` (com `TrailPhaseRowController`):
`orderLabel` → TMP_Text · `themeDropdown` → TMP_Dropdown · `countInput` → TMP_InputField
(IntegerNumber) · `enemyInput` → TMP_InputField · `bossToggle` → Toggle ·
`removeButton` → Button.

---

## Estilo (deixar igual ao mockup)

O builder usa cores monocromáticas e a fonte padrão do TMP. Para o visual pixel
dark-fantasy: troque o **TMP Font Asset** dos textos pela sua fonte pixel (ex.: gerar
um asset a partir da `Press Start 2P` / `VT323`) e atribua **sprites 9-sliced** de
moldura aos `Image` dos painéis/inputs (`Image.Type = Sliced`).

---

## Pontos em aberto / próximos passos

- **Endpoint `/api/trilhas`**: decisão entre criar no backend (recomendado p/ ranking
  e multiplataforma) ou manter local.
- **Editar trilha existente**: `LoadTrails` já existe; falta a UI de seleção/edição.
- **Boss único**: hoje o boss é um toggle por linha (permite 0 ou vários). Se quiser
  forçar exatamente um, adicione a regra no `OnSaveClicked`.
- **Vincular trilha ao gameplay**: consumir a trilha salva na cena de jogo (sorteio de
  perguntas por `tag_id`, ordem das fases, inimigos).
- **Rollback / sessão**: alinhar com as decisões em aberto das outras telas.
