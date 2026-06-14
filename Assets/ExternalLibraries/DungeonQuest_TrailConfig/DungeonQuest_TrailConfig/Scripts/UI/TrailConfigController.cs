using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonQuest.Trails
{
    /// <summary>
    /// Trail configuration screen ("Configuração de Trilhas").
    ///
    /// Flow: load tags (live, /api/tags) -> seed default phases -> let the user
    /// add/remove/reorder phases and set a boss -> validate -> persist via the
    /// selected ITrailRepository.
    ///
    /// Built to match the existing UI controllers: a MonoBehaviour with
    /// serialized references wired in the inspector, coroutine-based calls,
    /// and a status label for user feedback.
    /// </summary>
    public class TrailConfigController : MonoBehaviour
    {
        [Header("API")]
        [Tooltip("OFF = LocalTrailRepository (works today). " +
                 "ON = RemoteTrailRepository (requires /api/trilhas on the backend).")]
        [SerializeField] private bool useRemoteApi = false;

        [Header("UI · Form")]
        [SerializeField] private TMP_InputField trailNameInput;
        [SerializeField] private Transform phaseListContainer;   // ScrollView "Content"
        [SerializeField] private GameObject phaseRowPrefab;      // TrailPhaseRow prefab
        [SerializeField] private Button addPhaseButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private TMP_Text statusText;

        [Header("Behaviour")]
        [Tooltip("Regular phases seeded when the screen first opens (a boss phase is added after).")]
        [SerializeField] private int seedPhaseCount = 3;

        private ITrailRepository _repository;
        private List<TagOption> _tags = new List<TagOption>();
        private Dictionary<string, int> _availableByTag = new Dictionary<string, int>();
        private readonly List<TrailPhaseRowController> _rows = new List<TrailPhaseRowController>();
        private bool _ready;

        private void Awake()
        {
            _repository = useRemoteApi
                ? (ITrailRepository)new RemoteTrailRepository()
                : new LocalTrailRepository();
        }

        private void Start()
        {
            SetInteractable(false);
            SetStatus("Carregando temas...");

            if (addPhaseButton) addPhaseButton.onClick.AddListener(() => AddPhase(null));
            if (saveButton) saveButton.onClick.AddListener(OnSaveClicked);

            StartCoroutine(LoadInitialData());
        }

        private IEnumerator LoadInitialData()
        {
            bool tagsDone = false;
            yield return StartCoroutine(TrailApiService.LoadTags(
                tags => { _tags = tags ?? new List<TagOption>(); tagsDone = true; },
                err => { SetStatus("Erro ao carregar temas: " + err); tagsDone = true; }));

            // Soft, non-blocking: how many questions exist per tag (for warnings).
            yield return StartCoroutine(TrailApiService.LoadQuestionCountsByTag(
                counts => _availableByTag = counts ?? new Dictionary<string, int>(),
                _ => { /* optional; ignore failures */ }));

            if (_tags.Count == 0)
            {
                SetStatus("Nenhum tema cadastrado. Cadastre tags antes de montar trilhas.");
                yield break;
            }

            SeedDefaultPhases();
            _ready = true;
            SetInteractable(true);
            SetStatus("Pronto. Monte a sua trilha.");
        }

        private void SeedDefaultPhases()
        {
            for (int i = 0; i < seedPhaseCount; i++) AddPhase(null);
            AddPhase(new TrailPhaseDto { is_boss = true, question_count = 8, enemy_name = "Dragão" });
        }

        private TrailPhaseRowController AddPhase(TrailPhaseDto data)
        {
            if (!phaseRowPrefab || !phaseListContainer)
            {
                SetStatus("phaseRowPrefab/phaseListContainer não atribuídos no inspector.");
                return null;
            }

            GameObject go = Instantiate(phaseRowPrefab, phaseListContainer);
            var row = go.GetComponent<TrailPhaseRowController>();
            row.Initialize(_tags, data, _rows.Count + 1);
            row.OnRemoveRequested += RemovePhase;
            _rows.Add(row);
            Renumber();
            return row;
        }

        private void RemovePhase(TrailPhaseRowController row)
        {
            _rows.Remove(row);
            Destroy(row.gameObject);
            Renumber();
        }

        private void Renumber()
        {
            for (int i = 0; i < _rows.Count; i++) _rows[i].SetOrder(i + 1);
        }

        private void OnSaveClicked()
        {
            if (!_ready) return;

            string name = trailNameInput ? trailNameInput.text?.Trim() : null;
            if (string.IsNullOrEmpty(name)) { SetStatus("Informe o nome da trilha."); return; }
            if (_rows.Count == 0) { SetStatus("Adicione ao menos uma fase."); return; }

            var phases = new List<TrailPhaseDto>(_rows.Count);
            for (int i = 0; i < _rows.Count; i++)
            {
                if (!_rows[i].Validate(out string err))
                {
                    SetStatus($"Fase {i + 1}: {err}.");
                    return;
                }
                phases.Add(_rows[i].ToModel(i + 1));
            }

            string warning = AvailabilityWarning(phases);
            var trail = new TrailDto { name = name, phases = phases };

            SetInteractable(false);
            SetStatus(string.IsNullOrEmpty(warning) ? "Salvando..." : warning + " Salvando...");

            StartCoroutine(_repository.SaveTrail(trail,
                saved =>
                {
                    SetInteractable(true);
                    int count = saved?.phases?.Count ?? phases.Count;
                    SetStatus($"Trilha \"{saved?.name ?? name}\" salva ({count} fases).");
                },
                error =>
                {
                    SetInteractable(true);
                    SetStatus("Falha ao salvar: " + error);
                }));
        }

        // Non-blocking heads-up if a phase asks for more questions than exist for its tag.
        private string AvailabilityWarning(List<TrailPhaseDto> phases)
        {
            if (_availableByTag == null || _availableByTag.Count == 0) return null;
            foreach (var p in phases)
            {
                int have = (p.tag_id != null && _availableByTag.TryGetValue(p.tag_id, out int c)) ? c : 0;
                if (p.question_count > have)
                    return $"Aviso: tema '{p.tag_label}' tem {have} pergunta(s), fase pede {p.question_count}.";
            }
            return null;
        }

        private void SetInteractable(bool value)
        {
            if (addPhaseButton) addPhaseButton.interactable = value;
            if (saveButton) saveButton.interactable = value;
        }

        private void SetStatus(string message)
        {
            if (statusText) statusText.text = message;
        }
    }
}
