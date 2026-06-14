#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builder da tela de Cadastro de Perguntas. Gera o Canvas + 3 prefabs
/// (tela, ChoiceRow, TagToggle) com todas as referências ligadas ao
/// QuestionRegisterScreenController.
///
/// Rode pelo menu:  Dungeon Quest > Build Question Register UI
///
/// Scaffold visual: troque o TMP Font Asset pela fonte pixel e os sprites
/// 9-sliced para casar com o mockup.
/// </summary>
public static class QuestionRegisterUIBuilder
{
    static readonly Color Ink    = new Color32(11, 10, 9, 255);
    static readonly Color Panel  = new Color32(21, 20, 15, 255);
    static readonly Color Panel2 = new Color32(33, 30, 23, 255);
    static readonly Color Bone   = new Color32(233, 229, 216, 255);
    static readonly Color Dim    = new Color32(155, 150, 138, 255);

    const string PrefabFolder = "Assets/DungeonQuest/Prefabs";
    static TMP_DefaultControls.Resources _res;

    [MenuItem("Dungeon Quest/Build Question Register UI")]
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
        GameObject choicePrefab = BuildChoiceRowPrefab();
        GameObject tagPrefab = BuildTagTogglePrefab();
        BuildScreen(choicePrefab, tagPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[QuestionRegister] UI gerada. Prefabs em " + PrefabFolder + ".");
    }

    // -- tela ------------------------------------------------------------------

    static void BuildScreen(GameObject choicePrefab, GameObject tagPrefab)
    {
        Canvas canvas = GetOrCreateCanvas();

        GameObject bg = NewRect("DQ_Background", canvas.transform);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = Ink; bgImg.raycastTarget = false;
        Stretch(bg);

        GameObject panel = NewRect("QuestionRegisterScreen", canvas.transform);
        var pImg = panel.AddComponent<Image>(); pImg.color = Panel; pImg.sprite = _res.standard; pImg.type = Image.Type.Sliced;
        var pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(760, 920);
        pRt.anchoredPosition = Vector2.zero;

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 24, 24);
        vlg.spacing = 10;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        LE(Label("Title", panel.transform, "CADASTRO DE PERGUNTAS", 24, Bone, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 40);

        LE(Label("PromptLabel", panel.transform, "ENUNCIADO", 18, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);
        TMP_InputField prompt = MultilineInput("PromptInput", panel.transform, "Digite a pergunta...");
        LE(prompt.gameObject, prefH: 92);

        LE(Label("ExplLabel", panel.transform, "EXPLICAÇÃO DA RESPOSTA", 18, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);
        TMP_InputField explanation = MultilineInput("ExplanationInput", panel.transform, "Por que essa é a resposta correta? (opcional)");
        LE(explanation.gameObject, prefH: 72);

        LE(Label("TagsLabel", panel.transform, "TEMAS (marque um ou mais)", 18, Bone, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);
        ScrollRect tagScroll; RectTransform tagContent;
        BuildScrollView("TagsScroll", panel.transform, out tagScroll, out tagContent);
        LE(tagScroll.gameObject, minH: 96, flexH: 0.6f);

        LE(Label("ChoicesLabel", panel.transform, "ALTERNATIVAS (marque a correta)", 18, Bone, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);
        ScrollRect choiceScroll; RectTransform choiceContent;
        BuildScrollView("ChoicesScroll", panel.transform, out choiceScroll, out choiceContent);
        LE(choiceScroll.gameObject, minH: 170, flexH: 1f);

        Button addBtn = Btn("AddChoiceButton", panel.transform, "+ ADICIONAR ALTERNATIVA", Panel2, Bone, 15);
        LE(addBtn.gameObject, prefH: 38);

        Button saveBtn = Btn("SaveButton", panel.transform, "CADASTRAR PERGUNTA", Bone, Ink, 16);
        LE(saveBtn.gameObject, prefH: 52);

        TMP_Text status = Label("StatusText", panel.transform, "", 16, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
        LE(status.gameObject, prefH: 28);

        var ctrl = panel.AddComponent<QuestionRegisterScreenController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("promptInput").objectReferenceValue = prompt;
        so.FindProperty("explanationInput").objectReferenceValue = explanation;
        so.FindProperty("tagListContainer").objectReferenceValue = tagContent;
        so.FindProperty("tagTogglePrefab").objectReferenceValue = tagPrefab;
        so.FindProperty("choiceListContainer").objectReferenceValue = choiceContent;
        so.FindProperty("choiceRowPrefab").objectReferenceValue = choicePrefab;
        so.FindProperty("addChoiceButton").objectReferenceValue = addBtn;
        so.FindProperty("saveButton").objectReferenceValue = saveBtn;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(panel, PrefabFolder + "/QuestionRegisterScreen.prefab");
        Selection.activeGameObject = panel;
        EditorGUIUtility.PingObject(panel);
    }

    // -- prefab: linha de alternativa -----------------------------------------

    static GameObject BuildChoiceRowPrefab()
    {
        GameObject row = NewRect("ChoiceRow", null);
        var le = row.AddComponent<LayoutElement>(); le.minHeight = 44; le.preferredHeight = 44;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8; hlg.padding = new RectOffset(6, 6, 4, 4);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true; hlg.childForceExpandWidth = false;
        hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
        var bg = row.AddComponent<Image>(); bg.color = Panel; bg.sprite = _res.standard; bg.type = Image.Type.Sliced;

        Toggle correct = Checkbox("Correct", row.transform);
        LE(correct.gameObject, minW: 30, prefW: 30);

        TMP_InputField label = Input("Label", row.transform, "");
        ((TMP_Text)label.placeholder).text = "Texto da alternativa";
        LE(label.gameObject, minW: 120, flexW: 1f, minH: 34);

        Button remove = Btn("Remove", row.transform, "X", Panel2, Bone, 16);
        LE(remove.gameObject, minW: 36, prefW: 36, minH: 34);

        var ctrl = row.AddComponent<ChoiceRowController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("labelInput").objectReferenceValue = label;
        so.FindProperty("correctToggle").objectReferenceValue = correct;
        so.FindProperty("removeButton").objectReferenceValue = remove;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, PrefabFolder + "/ChoiceRow.prefab");
        Object.DestroyImmediate(row);
        return prefab;
    }

    // -- prefab: tema selecionável --------------------------------------------

    static GameObject BuildTagTogglePrefab()
    {
        GameObject row = NewRect("TagToggle", null);
        var le = row.AddComponent<LayoutElement>(); le.minHeight = 30; le.preferredHeight = 30;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8; hlg.padding = new RectOffset(6, 6, 2, 2);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true; hlg.childForceExpandWidth = false;
        hlg.childControlHeight = true; hlg.childForceExpandHeight = true;

        Toggle toggle = Checkbox("Toggle", row.transform);
        LE(toggle.gameObject, minW: 26, prefW: 26);

        TMP_Text label = Label("Label", row.transform, "tema", 18, Bone, FontStyles.Normal, TextAlignmentOptions.Left);
        LE(label.gameObject, minW: 100, flexW: 1f);

        var ctrl = row.AddComponent<TagToggleController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("toggle").objectReferenceValue = toggle;
        so.FindProperty("label").objectReferenceValue = label;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, PrefabFolder + "/TagToggle.prefab");
        Object.DestroyImmediate(row);
        return prefab;
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
        if (f.textComponent) { f.textComponent.color = Bone; f.textComponent.fontSize = 18; }
        if (f.placeholder is TMP_Text ph) { ph.color = Dim; ph.fontSize = 18; }
        return f;
    }

    static TMP_InputField MultilineInput(string name, Transform parent, string placeholder)
    {
        var f = Input(name, parent, "");
        f.lineType = TMP_InputField.LineType.MultiLineNewline;
        if (f.textComponent) f.textComponent.alignment = TextAlignmentOptions.TopLeft;
        if (f.placeholder is TMP_Text ph) { ph.text = placeholder; ph.alignment = TextAlignmentOptions.TopLeft; }
        return f;
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

    // Caixa de seleção quadrada (sem rótulo embutido).
    static Toggle Checkbox(string name, Transform parent)
    {
        GameObject go = NewRect(name, parent);
        var bgImg = go.AddComponent<Image>(); bgImg.color = Panel2; bgImg.sprite = _res.standard; bgImg.type = Image.Type.Sliced;

        GameObject chkGo = NewRect("Checkmark", go.transform);
        var chkImg = chkGo.AddComponent<Image>(); chkImg.color = Bone; chkImg.sprite = _res.checkmark;
        var chkRt = chkGo.GetComponent<RectTransform>();
        chkRt.anchorMin = Vector2.zero; chkRt.anchorMax = Vector2.one;
        chkRt.offsetMin = new Vector2(3, 3); chkRt.offsetMax = new Vector2(-3, -3);

        var tog = go.AddComponent<Toggle>();
        tog.targetGraphic = bgImg; tog.graphic = chkImg; tog.isOn = false;
        return tog;
    }

    static void BuildScrollView(string name, Transform parent, out ScrollRect scroll, out RectTransform content)
    {
        GameObject go = NewRect(name, parent);
        var img = go.AddComponent<Image>(); img.color = Ink; img.sprite = _res.standard; img.type = Image.Type.Sliced;
        scroll = go.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 24;

        GameObject viewport = NewRect("Viewport", go.transform);
        var vImg = viewport.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0.001f);
        viewport.AddComponent<RectMask2D>();
        var vRt = viewport.GetComponent<RectTransform>();
        vRt.anchorMin = Vector2.zero; vRt.anchorMax = Vector2.one;
        vRt.offsetMin = new Vector2(4, 4); vRt.offsetMax = new Vector2(-4, -4); vRt.pivot = new Vector2(0f, 1f);

        GameObject contentGo = NewRect("Content", viewport.transform);
        content = contentGo.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f); content.anchoredPosition = Vector2.zero; content.sizeDelta = Vector2.zero;
        var clg = contentGo.AddComponent<VerticalLayoutGroup>();
        clg.spacing = 6; clg.padding = new RectOffset(6, 6, 6, 6);
        clg.childControlWidth = true; clg.childForceExpandWidth = true;
        clg.childControlHeight = true; clg.childForceExpandHeight = false;
        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scroll.viewport = vRt; scroll.content = content;
    }

    // -- helpers de baixo nível ------------------------------------------------

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
