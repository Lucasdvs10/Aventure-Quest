#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builder da tela de Configuração de Sala (com abas Criar/Entrar + X de saída).
/// Menu:  Dungeon Quest > Build Room Config UI
/// </summary>
public static class RoomConfigUIBuilder
{
    static readonly Color Ink    = new Color32(11, 10, 9, 255);
    static readonly Color Panel  = new Color32(21, 20, 15, 255);
    static readonly Color Panel2 = new Color32(33, 30, 23, 255);
    static readonly Color Bone   = new Color32(233, 229, 216, 255);
    static readonly Color Dim    = new Color32(155, 150, 138, 255);
    static readonly Color Gold   = new Color32(214, 184, 99, 255);

    const string PrefabFolder = "Assets/DungeonQuest/Prefabs";
    static TMP_DefaultControls.Resources _res;

    [MenuItem("Dungeon Quest/Build Room Config UI")]
    public static void Build()
    {
        _res = new TMP_DefaultControls.Resources
        {
            standard   = S("UI/Skin/UISprite.psd"),
            background = S("UI/Skin/Background.psd"),
            inputField = S("UI/Skin/InputFieldBackground.psd"),
            knob       = S("UI/Skin/Knob.psd"),
            checkmark  = S("UI/Skin/Checkmark.psd"),
            dropdown   = S("UI/Skin/DropdownArrow.psd"),
            mask       = S("UI/Skin/UIMask.psd")
        };

        EnsureFolder(PrefabFolder);
        Canvas canvas = GetOrCreateCanvas();

        GameObject bg = NewRect("DQ_Background", canvas.transform);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = Ink; bgImg.raycastTarget = false;
        Stretch(bg);

        GameObject panel = NewRect("RoomConfigScreen", canvas.transform);
        var pImg = panel.AddComponent<Image>(); pImg.color = Panel; pImg.sprite = _res.standard; pImg.type = Image.Type.Sliced;
        var pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(560, 660);
        pRt.anchoredPosition = Vector2.zero;

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(26, 26, 22, 22);
        vlg.spacing = 10;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        // X de saída (flutua no canto superior direito, fora do layout)
        Button exitBtn = Btn("ExitButton", panel.transform, "X", Panel2, Bone, 18);
        var exitLE = exitBtn.gameObject.AddComponent<LayoutElement>(); exitLE.ignoreLayout = true;
        var exitRt = exitBtn.GetComponent<RectTransform>();
        exitRt.anchorMin = exitRt.anchorMax = exitRt.pivot = new Vector2(1f, 1f);
        exitRt.sizeDelta = new Vector2(34, 34);
        exitRt.anchoredPosition = new Vector2(-10, -10);

        LE(Label("Title", panel.transform, "CONFIGURAÇÃO DE SALA", 22, Bone, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 38);

        // Barra de abas
        GameObject tabBar = NewRect("TabBar", panel.transform);
        var tHlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        tHlg.spacing = 8; tHlg.childControlWidth = true; tHlg.childForceExpandWidth = true;
        tHlg.childControlHeight = true; tHlg.childForceExpandHeight = true;
        LE(tabBar, prefH: 44);
        Button tabCreate = Btn("TabCreate", tabBar.transform, "CRIAR", Bone, Ink, 15);
        Button tabJoin = Btn("TabJoin", tabBar.transform, "ENTRAR", Panel2, Bone, 15);

        // ---------- Painel CRIAR ----------
        GameObject createPanel = VPanel("CreatePanel", panel.transform);

        LE(Label("NameLabel", createPanel.transform, "NOME DA SALA (opcional)", 15, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 20);
        TMP_InputField titleInput = Input("TitleInput", createPanel.transform, "");
        ((TMP_Text)titleInput.placeholder).text = "Ex.: Quiz da Turma";
        LE(titleInput.gameObject, prefH: 44);

        LE(Label("DiscLabel", createPanel.transform, "DISCIPLINA", 15, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 20);
        TMP_Dropdown disc = Dropdown("DisciplineDropdown", createPanel.transform);
        LE(disc.gameObject, prefH: 44);

        LE(Label("EnemyLabel", createPanel.transform, "QUANTIDADE DE INIMIGOS", 15, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 20);
        GameObject stepper = NewRect("EnemyStepper", createPanel.transform);
        var sHlg = stepper.AddComponent<HorizontalLayoutGroup>();
        sHlg.spacing = 10; sHlg.childAlignment = TextAnchor.MiddleCenter;
        sHlg.childControlWidth = true; sHlg.childForceExpandWidth = false;
        sHlg.childControlHeight = true; sHlg.childForceExpandHeight = true;
        LE(stepper, prefH: 50);
        Button minus = Btn("MinusButton", stepper.transform, "-", Panel2, Bone, 22);
        LE(minus.gameObject, minW: 60, prefW: 60);
        TMP_Text qty = Label("QuantityValue", stepper.transform, "10", 26, Bone, FontStyles.Bold, TextAlignmentOptions.Center);
        LE(qty.gameObject, minW: 90, prefW: 90);
        Button plus = Btn("PlusButton", stepper.transform, "+", Panel2, Bone, 22);
        LE(plus.gameObject, minW: 60, prefW: 60);

        Button createBtn = Btn("CreateButton", createPanel.transform, "CRIAR SALA", Bone, Ink, 16);
        LE(createBtn.gameObject, prefH: 50);

        GameObject codeBox = NewRect("CodeBox", createPanel.transform);
        var cbImg = codeBox.AddComponent<Image>(); cbImg.color = Ink; cbImg.sprite = _res.standard; cbImg.type = Image.Type.Sliced;
        var cbVlg = codeBox.AddComponent<VerticalLayoutGroup>();
        cbVlg.padding = new RectOffset(10, 10, 6, 6); cbVlg.spacing = 2;
        cbVlg.childControlWidth = true; cbVlg.childForceExpandWidth = true;
        cbVlg.childControlHeight = true; cbVlg.childForceExpandHeight = false;
        cbVlg.childAlignment = TextAnchor.MiddleCenter;
        LE(codeBox, prefH: 76);
        LE(Label("CodeCaption", codeBox.transform, "CÓDIGO DA SALA", 13, Dim, FontStyles.Normal, TextAlignmentOptions.Center).gameObject, prefH: 18);
        TMP_Text code = Label("CodeText", codeBox.transform, "——", 32, Gold, FontStyles.Bold, TextAlignmentOptions.Center);
        LE(code.gameObject, prefH: 42);

        // ---------- Painel ENTRAR ----------
        GameObject joinPanel = VPanel("JoinPanel", panel.transform);

        LE(Label("CodeInLabel", joinPanel.transform, "CÓDIGO DA SALA", 15, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 20);
        TMP_InputField codeInput = Input("CodeInput", joinPanel.transform, "");
        ((TMP_Text)codeInput.placeholder).text = "Ex.: QUIZ123";
        LE(codeInput.gameObject, prefH: 44);

        Button findBtn = Btn("FindButton", joinPanel.transform, "BUSCAR SALA", Panel2, Bone, 15);
        LE(findBtn.gameObject, prefH: 46);

        GameObject infoBox = NewRect("InfoBox", joinPanel.transform);
        var ibImg = infoBox.AddComponent<Image>(); ibImg.color = Ink; ibImg.sprite = _res.standard; ibImg.type = Image.Type.Sliced;
        var ibVlg = infoBox.AddComponent<VerticalLayoutGroup>();
        ibVlg.padding = new RectOffset(14, 14, 10, 10); ibVlg.spacing = 6;
        ibVlg.childControlWidth = true; ibVlg.childForceExpandWidth = true;
        ibVlg.childControlHeight = true; ibVlg.childForceExpandHeight = false;
        ibVlg.childAlignment = TextAnchor.MiddleLeft;
        LE(infoBox, prefH: 86);
        LE(Label("ConfigCaption", infoBox.transform, "CONFIGURAÇÃO DA SALA", 13, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 18);
        TMP_Text discInfo = Label("JoinDiscipline", infoBox.transform, "Disciplina: ——", 18, Bone, FontStyles.Normal, TextAlignmentOptions.Left);
        LE(discInfo.gameObject, prefH: 24);
        TMP_Text enemyInfo = Label("JoinEnemies", infoBox.transform, "Inimigos: ——", 18, Bone, FontStyles.Normal, TextAlignmentOptions.Left);
        LE(enemyInfo.gameObject, prefH: 24);

        Button joinBtn = Btn("JoinButton", joinPanel.transform, "ENTRAR NA SALA", Bone, Ink, 16);
        LE(joinBtn.gameObject, prefH: 50);

        joinPanel.SetActive(false);   // começa na aba CRIAR

        // ---------- Status (compartilhado) ----------
        TMP_Text status = Label("StatusText", panel.transform, "", 15, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
        LE(status.gameObject, prefH: 26);

        // ---------- Liga ao controller ----------
        var ctrl = panel.AddComponent<RoomConfigScreenController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("tabCreateButton").objectReferenceValue = tabCreate;
        so.FindProperty("tabJoinButton").objectReferenceValue = tabJoin;
        so.FindProperty("createPanel").objectReferenceValue = createPanel;
        so.FindProperty("joinPanel").objectReferenceValue = joinPanel;
        so.FindProperty("exitButton").objectReferenceValue = exitBtn;
        so.FindProperty("titleInput").objectReferenceValue = titleInput;
        so.FindProperty("disciplineDropdown").objectReferenceValue = disc;
        so.FindProperty("minusButton").objectReferenceValue = minus;
        so.FindProperty("plusButton").objectReferenceValue = plus;
        so.FindProperty("quantityValueLabel").objectReferenceValue = qty;
        so.FindProperty("createButton").objectReferenceValue = createBtn;
        so.FindProperty("codeText").objectReferenceValue = code;
        so.FindProperty("codeInput").objectReferenceValue = codeInput;
        so.FindProperty("findButton").objectReferenceValue = findBtn;
        so.FindProperty("joinDisciplineLabel").objectReferenceValue = discInfo;
        so.FindProperty("joinEnemiesLabel").objectReferenceValue = enemyInfo;
        so.FindProperty("joinButton").objectReferenceValue = joinBtn;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(panel, PrefabFolder + "/RoomConfigScreen.prefab");
        Selection.activeGameObject = panel;
        EditorGUIUtility.PingObject(panel);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RoomConfig] UI gerada. Adicione o SceneLoader no GameObject RoomConfigScreen para o X funcionar. Defina Session.CurrentUserId no login (ou Owner Id Override p/ testar).");
    }

    // -- containers ------------------------------------------------------------

    static GameObject VPanel(string name, Transform parent)
    {
        GameObject go = NewRect(name, parent);
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;
        return go;
    }

    // -- helpers de elemento ---------------------------------------------------

    static TMP_Text Label(string name, Transform parent, string text, float size, Color color,
                          FontStyles style, TextAlignmentOptions align)
    {
        GameObject go = TMP_DefaultControls.CreateText(_res);
        go.name = name; go.transform.SetParent(parent, false);
        var t = go.GetComponent<TMP_Text>();
        t.text = text; t.fontSize = size; t.color = color; t.fontStyle = style; t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    static TMP_InputField Input(string name, Transform parent, string text)
    {
        GameObject go = TMP_DefaultControls.CreateInputField(_res);
        go.name = name; go.transform.SetParent(parent, false);
        var f = go.GetComponent<TMP_InputField>();
        f.text = text;
        var img = go.GetComponent<Image>(); if (img) img.color = Panel2;
        if (f.textComponent) { f.textComponent.color = Bone; f.textComponent.fontSize = 20; }
        if (f.placeholder is TMP_Text ph) { ph.color = Dim; ph.fontSize = 20; }
        return f;
    }

    static TMP_Dropdown Dropdown(string name, Transform parent)
    {
        GameObject go = TMP_DefaultControls.CreateDropdown(_res);
        go.name = name; go.transform.SetParent(parent, false);
        var d = go.GetComponent<TMP_Dropdown>();
        var img = go.GetComponent<Image>(); if (img) img.color = Panel2;
        if (d.captionText) { d.captionText.color = Bone; d.captionText.fontSize = 18; }
        if (d.itemText) { d.itemText.color = Ink; d.itemText.fontSize = 18; }
        return d;
    }

    static Button Btn(string name, Transform parent, string label, Color bgColor, Color textColor, float fontSize)
    {
        GameObject go = NewRect(name, parent);
        var img = go.AddComponent<Image>(); img.color = bgColor; img.sprite = _res.standard; img.type = Image.Type.Sliced;
        var b = go.AddComponent<Button>(); b.targetGraphic = img;
        TMP_Text t = Label(name + "_Label", go.transform, label, fontSize, textColor, FontStyles.Bold, TextAlignmentOptions.Center);
        var rt = t.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return b;
    }

    // -- baixo nível -----------------------------------------------------------

    static GameObject NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent) go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void LE(GameObject go, float minH = -1, float prefH = -1, float minW = -1, float prefW = -1, float flexW = -1, float flexH = -1)
    {
        var le = go.GetComponent<LayoutElement>(); if (!le) le = go.AddComponent<LayoutElement>();
        if (minH >= 0) le.minHeight = minH;
        if (prefH >= 0) le.preferredHeight = prefH;
        if (minW >= 0) le.minWidth = minW;
        if (prefW >= 0) le.preferredWidth = prefW;
        if (flexW >= 0) le.flexibleWidth = flexW;
        if (flexH >= 0) le.flexibleHeight = flexH;
    }

    static Canvas GetOrCreateCanvas()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (!canvas)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        if (!Object.FindObjectOfType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return canvas;
    }

    static Sprite S(string path) => AssetDatabase.GetBuiltinExtraResource<Sprite>(path);

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string leaf = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
