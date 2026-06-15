using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela de Configuração de Sala com duas abas:
///   - CRIAR  : escolhe disciplina + nº de inimigos e cria a sala (o back gera o code).
///   - ENTRAR : digita o código, busca a sala (GET /rooms/code) e mostra a
///              configuração dela (disciplina + nº de inimigos); pode entrar.
///
/// Tem ainda um "X" no canto que volta ao menu principal (via SceneLoader, igual
/// ao LoginScreenController).
///
/// Mesmo molde dos seus controllers: controller "puro" no Awake + async void.
/// </summary>
public class RoomConfigScreenController : MonoBehaviour
{
    private RoomController roomController;
    private SceneLoader sceneLoader;   // mesmo componente usado no LoginScreenController

    [Header("Navegação")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [Tooltip("Opcional: cena a carregar após entrar numa sala (deixe vazio para só exibir status).")]
    [SerializeField] private string lobbySceneAfterJoin = "";

    [Header("Abas")]
    [SerializeField] private Button tabCreateButton;
    [SerializeField] private Button tabJoinButton;
    [SerializeField] private GameObject createPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private Button exitButton;       // X -> menu principal

    [Header("Dono da sala")]
    [Tooltip("Vazio = usa Session.CurrentUserId (do login). Preencha só para testar.")]
    [SerializeField] private string ownerIdOverride = "";

    [Header("Aba CRIAR")]
    [SerializeField] private TMP_InputField titleInput;
    [SerializeField] private TMP_Dropdown disciplineDropdown;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private TMP_Text quantityValueLabel;
    [SerializeField] private Button createButton;
    [SerializeField] private TMP_Text codeText;

    [Header("Aba ENTRAR")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private Button findButton;
    [SerializeField] private TMP_Text joinDisciplineLabel;
    [SerializeField] private TMP_Text joinEnemiesLabel;
    [SerializeField] private Button joinButton;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;

    [Header("Quantidade de inimigos")]
    [SerializeField] private int minQuantity = 1;
    [SerializeField] private int maxQuantity = 20;
    [SerializeField] private int defaultQuantity = 10;

    private readonly List<TagModel> tags = new List<TagModel>();
    private int quantity;
    private RoomModel lastCreatedRoom;
    private RoomModel foundRoom;

    private static readonly Color ActiveTab = new Color32(233, 229, 216, 255);
    private static readonly Color InactiveTab = new Color32(33, 30, 23, 255);

    private void Awake()
    {
        roomController = new RoomController();
        sceneLoader = GetComponent<SceneLoader>();
    }

    private async void Start()
    {
        SetInteractable(false);
        SetCode("——");
        SetJoinInfo(null);
        if (joinButton) joinButton.interactable = false;
        SetStatus("Carregando disciplinas...");

        if (tabCreateButton) tabCreateButton.onClick.AddListener(() => SelectTab(0));
        if (tabJoinButton) tabJoinButton.onClick.AddListener(() => SelectTab(1));
        if (exitButton) exitButton.onClick.AddListener(Exit);
        if (minusButton) minusButton.onClick.AddListener(() => ChangeQuantity(-1));
        if (plusButton) plusButton.onClick.AddListener(() => ChangeQuantity(+1));
        if (createButton) createButton.onClick.AddListener(CreateRoom);
        if (findButton) findButton.onClick.AddListener(FindRoom);
        if (joinButton) joinButton.onClick.AddListener(JoinRoom);

        quantity = Mathf.Clamp(defaultQuantity, minQuantity, maxQuantity);
        UpdateQuantityLabel();

        var loaded = await roomController.GetTagsAsync();
        tags.Clear();
        tags.AddRange(loaded);
        BuildDisciplineOptions();

        SelectTab(0);
        SetInteractable(true);
        SetStatus("Pronto. Crie ou entre em uma sala.");
    }

    // ---- Abas ---------------------------------------------------------------

    private void SelectTab(int tab)
    {
        bool create = tab == 0;
        if (createPanel) createPanel.SetActive(create);
        if (joinPanel) joinPanel.SetActive(!create);
        SetTabVisual(tabCreateButton, create);
        SetTabVisual(tabJoinButton, !create);
    }

    private void SetTabVisual(Button button, bool active)
    {
        if (!button) return;
        var img = button.GetComponent<Image>();
        if (img) img.color = active ? ActiveTab : InactiveTab;
    }

    // ---- Disciplina / quantidade (aba criar) --------------------------------

    private void BuildDisciplineOptions()
    {
        if (!disciplineDropdown) return;
        disciplineDropdown.ClearOptions();
        var options = new List<string> { "Variado (todas)" };   // índice 0
        foreach (var t in tags) options.Add(t.label);
        disciplineDropdown.AddOptions(options);
        disciplineDropdown.value = 0;
    }

    private string SelectedTagTarget()
    {
        int idx = disciplineDropdown ? disciplineDropdown.value : 0;
        if (idx <= 0 || idx - 1 >= tags.Count) return "variado";
        return tags[idx - 1].label;   // troque para tags[idx-1].id se o back esperar o id
    }

    private void ChangeQuantity(int delta)
    {
        quantity = Mathf.Clamp(quantity + delta, minQuantity, maxQuantity);
        UpdateQuantityLabel();
    }

    private void UpdateQuantityLabel()
    {
        if (quantityValueLabel) quantityValueLabel.text = quantity.ToString();
    }

    // ---- Criar sala ---------------------------------------------------------

    public async void CreateRoom()
    {
        string ownerId = ResolveOwnerId();
        if (string.IsNullOrEmpty(ownerId)) { SetStatus("Faça login primeiro (dono da sala ausente)."); return; }

        string tagTarget = SelectedTagTarget();
        string title = titleInput ? titleInput.text?.Trim() : null;
        if (string.IsNullOrEmpty(title)) title = $"Sala de {tagTarget}";

        SetInteractable(false);
        SetStatus("Criando sala...");

        lastCreatedRoom = await roomController.CreateRoomAsync(title, ownerId, quantity, tagTarget);

        SetInteractable(true);

        if (lastCreatedRoom != null && !string.IsNullOrEmpty(lastCreatedRoom.code))
        {
            SetCode(lastCreatedRoom.code);
            SetStatus($"Sala criada! {quantity} inimigos · {tagTarget}.");
        }
        else
        {
            SetStatus("Falha ao criar a sala.");
        }
    }

    // ---- Entrar em sala -----------------------------------------------------

    public async void FindRoom()
    {
        string code = codeInput ? codeInput.text?.Trim() : null;
        if (string.IsNullOrEmpty(code)) { SetStatus("Digite o código da sala."); return; }

        if (findButton) findButton.interactable = false;
        SetStatus("Buscando sala...");

        foundRoom = await roomController.GetRoomByCodeAsync(code);

        if (findButton) findButton.interactable = true;

        if (foundRoom != null && !string.IsNullOrEmpty(foundRoom.id))
        {
            SetJoinInfo(foundRoom);
            if (joinButton) joinButton.interactable = true;
            SetStatus($"Sala \"{foundRoom.title}\" encontrada.");
        }
        else
        {
            foundRoom = null;
            SetJoinInfo(null);
            if (joinButton) joinButton.interactable = false;
            SetStatus("Sala não encontrada.");
        }
    }

    // Mostra a configuração da sala (disciplina + nº de inimigos).
    private void SetJoinInfo(RoomModel room)
    {
        if (joinDisciplineLabel)
            joinDisciplineLabel.text = "Disciplina: " + (room != null && !string.IsNullOrEmpty(room.tag_target) ? room.tag_target : "——");
        if (joinEnemiesLabel)
            joinEnemiesLabel.text = "Inimigos: " + (room != null ? room.level_quantity.ToString() : "——");
    }

    public async void JoinRoom()
    {
        if (foundRoom == null) { SetStatus("Busque uma sala primeiro."); return; }

        string ownerId = ResolveOwnerId();
        if (string.IsNullOrEmpty(ownerId)) { SetStatus("Faça login primeiro."); return; }

        if (joinButton) joinButton.interactable = false;
        SetStatus("Entrando na sala...");

        bool ok = await roomController.AddUserToRoomAsync(foundRoom.id, ownerId);

        if (joinButton) joinButton.interactable = true;

        if (ok)
        {
            SetStatus($"Entrou em \"{foundRoom.title}\"!");
            if (!string.IsNullOrEmpty(lobbySceneAfterJoin) && sceneLoader)
                sceneLoader.LoadScene(lobbySceneAfterJoin);
        }
        else
        {
            SetStatus("Falha ao entrar na sala.");
        }
    }

    // ---- Sair (X) -----------------------------------------------------------

    public void Exit()
    {
        if (sceneLoader) sceneLoader.LoadScene(mainMenuScene);
        else Debug.LogWarning("[RoomConfig] SceneLoader ausente: adicione-o ao GameObject para o X funcionar.");
    }

    // ---- Util ---------------------------------------------------------------

    private string ResolveOwnerId()
    {
        return !string.IsNullOrEmpty(ownerIdOverride) ? ownerIdOverride : PlayerPrefs.GetString("CurrentUserID");
    }

    private void SetCode(string code)
    {
        if (codeText) codeText.text = code;
    }

    private void SetInteractable(bool value)
    {
        if (minusButton) minusButton.interactable = value;
        if (plusButton) plusButton.interactable = value;
        if (createButton) createButton.interactable = value;
        if (findButton) findButton.interactable = value;
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
        Debug.Log("[RoomConfig] " + message);
    }
}
