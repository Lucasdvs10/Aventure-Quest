#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builder da tela de Configuração de Sala. Gera Canvas + prefab com tudo
/// ligado ao RoomConfigScreenController.
///
/// Menu:  Dungeon Quest > Build Room Config UI
///
/// Scaffold visual: troque o TMP Font Asset pela fonte pixel e os sprites
/// 9-sliced para casar com o mockup.
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
        pRt.sizeDelta = new Vector2(560, 680);
        pRt.anchoredPosition = Vector2.zero;

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 24, 24);
        vlg.spacing = 12;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        LE(Label("Title", panel.transform, "CONFIGURAÇÃO DE SALA", 24, Bone, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 40);

        LE(Label("NameLabel", panel.transform, "NOME DA SALA (opcional)", 16, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);
        TMP_InputField titleInput = Input("TitleInput", panel.transform, "");
        ((TMP_Text)titleInput.placeholder).text = "Ex.: Quiz da Turma";
        LE(titleInput.gameObject, prefH: 46);

        LE(Label("DiscLabel", panel.transform, "DISCIPLINA", 16, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);
        TMP_Dropdown disc = Dropdown("DisciplineDropdown", panel.transform);
        LE(disc.gameObject, prefH: 46);

        LE(Label("EnemyLabel", panel.transform, "QUANTIDADE DE INIMIGOS", 16, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);

        // Stepper: [ - ] [ valor ] [ + ]
        GameObject stepper = NewRect("EnemyStepper", panel.transform);
        var sHlg = stepper.AddComponent<HorizontalLayoutGroup>();
        sHlg.spacing = 10; sHlg.childAlignment = TextAnchor.MiddleCenter;
        sHlg.childControlWidth = true; sHlg.childForceExpandWidth = false;
        sHlg.childControlHeight = true; sHlg.childForceExpandHeight = true;
        LE(stepper, prefH: 52);

        Button minus = Btn("MinusButton", stepper.transform, "−", Panel2, Bone, 22);
        LE(minus.gameObject, minW: 64, prefW: 64);
        TMP_Text qty = Label("QuantityValue", stepper.transform, "10", 26, Bone, FontStyles.Bold, TextAlignmentOptions.Center);
        LE(qty.gameObject, minW: 90, prefW: 90);
        Button plus = Btn("PlusButton", stepper.transform, "+", Panel2, Bone, 22);
        LE(plus.gameObject, minW: 64, prefW: 64);

        Button createBtn = Btn("CreateButton", panel.transform, "CRIAR SALA", Bone, Ink, 16);
        LE(createBtn.gameObject, prefH: 52);

        // Bloco do código gerado.
        GameObject codeBox = NewRect("CodeBox", panel.transform);
        var cbImg = codeBox.AddComponent<Image>(); cbImg.color = Ink; cbImg.sprite = _res.standard; cbImg.type = Image.Type.Sliced;
        var cbVlg = codeBox.AddComponent<VerticalLayoutGroup>();
        cbVlg.padding = new RectOffset(10, 10, 8, 8); cbVlg.spacing = 2;
        cbVlg.childControlWidth = true; cbVlg.childForceExpandWidth = true;
        cbVlg.childControlHeight = true; cbVlg.childForceExpandHeight = false;
        cbVlg.childAlignment = TextAnchor.MiddleCenter;
        LE(codeBox, prefH: 84);
        LE(Label("CodeCaption", codeBox.transform, "CÓDIGO DA SALA", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Center).gameObject, prefH: 20);
        TMP_Text code = Label("CodeText", codeBox.transform, "——", 34, Gold, FontStyles.Bold, TextAlignmentOptions.Center);
        LE(code.gameObject, prefH: 44);

        TMP_Text status = Label("StatusText", panel.transform, "", 16, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
        LE(status.gameObject, prefH: 28);

        var ctrl = panel.AddComponent<RoomConfigScreenController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("titleInput").objectReferenceValue = titleInput;
        so.FindProperty("disciplineDropdown").objectReferenceValue = disc;
        so.FindProperty("minusButton").objectReferenceValue = minus;
        so.FindProperty("plusButton").objectReferenceValue = plus;
        so.FindProperty("quantityValueLabel").objectReferenceValue = qty;
        so.FindProperty("createButton").objectReferenceValue = createBtn;
        so.FindProperty("codeText").objectReferenceValue = code;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(panel, PrefabFolder + "/RoomConfigScreen.prefab");
        Selection.activeGameObject = panel;
        EditorGUIUtility.PingObject(panel);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RoomConfig] UI gerada em " + PrefabFolder + ". Defina Session.CurrentUserId no login (ou use Owner Id Override para testar).");
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
