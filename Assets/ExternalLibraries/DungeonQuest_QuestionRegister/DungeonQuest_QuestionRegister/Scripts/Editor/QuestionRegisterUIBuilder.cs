#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builder da tela de Cadastro de Perguntas. Agora inclui a linha "criar tema"
/// (input + botão "+ TEMA") acima da lista de temas.
/// Menu:  Dungeon Quest > Build Question Register UI
/// Gera também os prefabs TagToggle e ChoiceRow.
/// </summary>
public static class QuestionRegisterUIBuilder
{
    static readonly Color Ink    = new Color32(11, 10, 9, 255);
    static readonly Color Panel  = new Color32(21, 20, 15, 255);
    static readonly Color Panel2 = new Color32(33, 30, 23, 255);
    static readonly Color Bone   = new Color32(233, 229, 216, 255);
    static readonly Color Dim    = new Color32(155, 150, 138, 255);
    static readonly Color Gold   = new Color32(214, 184, 99, 255);

    const string PrefabFolder = "Assets/DungeonQuest/Prefabs";
    static TMP_DefaultControls.Resources _res;

    [MenuItem("Dungeon Quest/Build Question Register UI")]
    public static void Build()
    {
        _res = Res();
        EnsureFolder(PrefabFolder);

        GameObject tagPrefab = BuildTagTogglePrefab();
        GameObject choicePrefab = BuildChoiceRowPrefab();

        Canvas canvas = GetOrCreateCanvas();

        GameObject bg = NewRect("DQ_Background", canvas.transform);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = Ink; bgImg.raycastTarget = false; Stretch(bg);

        GameObject panel = NewRect("QuestionRegisterScreen", canvas.transform);
        var pImg = panel.AddComponent<Image>(); pImg.color = Panel; pImg.sprite = _res.standard; pImg.type = Image.Type.Sliced;
        var pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(760, 940); pRt.anchoredPosition = Vector2.zero;
        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 22, 22); vlg.spacing = 8;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        LE(Label("Title", panel.transform, "CADASTRO DE PERGUNTAS", 22, Bone, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 38);

        // Enunciado
        LE(Label("PromptLabel", panel.transform, "ENUNCIADO", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 18);
        TMP_InputField prompt = Input("PromptInput", panel.transform, "", multiline: true);
        ((TMP_Text)prompt.placeholder).text = "Escreva a pergunta...";
        LE(prompt.gameObject, prefH: 72);

        // Explicação
        LE(Label("ExplLabel", panel.transform, "EXPLICAÇÃO (opcional)", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 18);
        TMP_InputField expl = Input("ExplanationInput", panel.transform, "", multiline: true);
        ((TMP_Text)expl.placeholder).text = "Por que a resposta correta está certa...";
        LE(expl.gameObject, prefH: 56);

        // Temas
        LE(Label("TagsLabel", panel.transform, "TEMAS (marque um ou mais)", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 18);

        // Linha de criar tema novo
        GameObject newTagRow = NewRect("NewTagRow", panel.transform);
        var ntHlg = newTagRow.AddComponent<HorizontalLayoutGroup>();
        ntHlg.spacing = 8; ntHlg.childControlWidth = true; ntHlg.childForceExpandWidth = false;
        ntHlg.childControlHeight = true; ntHlg.childForceExpandHeight = true;
        ntHlg.childAlignment = TextAnchor.MiddleLeft;
        LE(newTagRow, prefH: 44);
        TMP_InputField newTagInput = Input("NewTagInput", newTagRow.transform, "");
        ((TMP_Text)newTagInput.placeholder).text = "Novo tema...";
        LE(newTagInput.gameObject, flexW: 1f, minW: 140);
        Button addTagBtn = Btn("AddTagButton", newTagRow.transform, "+ TEMA", Panel2, Bone, 14);
        LE(addTagBtn.gameObject, minW: 120, prefW: 120);

        // Lista (scroll) de temas
        ScrollRect tagScroll; RectTransform tagContent;
        BuildScrollView("TagsScroll", panel.transform, out tagScroll, out tagContent, horizontalFit: false);
        LE(tagScroll.gameObject, minH: 90, flexH: 0.55f);

        // Alternativas
        GameObject choiceHeader = NewRect("ChoiceHeader", panel.transform);
        var chHlg = choiceHeader.AddComponent<HorizontalLayoutGroup>();
        chHlg.spacing = 8; chHlg.childControlWidth = true; chHlg.childForceExpandWidth = false;
        chHlg.childControlHeight = true; chHlg.childForceExpandHeight = true;
        chHlg.childAlignment = TextAnchor.MiddleLeft;
        LE(choiceHeader, prefH: 26);
        var chLbl = Label("ChoicesLabel", choiceHeader.transform, "ALTERNATIVAS (marque a correta)", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Left);
        LE(chLbl.gameObject, flexW: 1f);
        Button addChoiceBtn = Btn("AddChoiceButton", choiceHeader.transform, "+ ALTERNATIVA", Panel2, Bone, 13);
        LE(addChoiceBtn.gameObject, minW: 150, prefW: 150);

        ScrollRect choiceScroll; RectTransform choiceContent;
        BuildScrollView("ChoicesScroll", panel.transform, out choiceScroll, out choiceContent, horizontalFit: false);
        LE(choiceScroll.gameObject, minH: 150, flexH: 1f);

        // Ações
        Button saveBtn = Btn("SaveButton", panel.transform, "CADASTRAR PERGUNTA", Bone, Ink, 16);
        LE(saveBtn.gameObject, prefH: 52);

        TMP_Text status = Label("StatusText", panel.transform, "", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
        LE(status.gameObject, prefH: 24);

        // Liga ao controller
        var ctrl = panel.AddComponent<QuestionRegisterScreenController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("promptInput").objectReferenceValue = prompt;
        so.FindProperty("explanationInput").objectReferenceValue = expl;
        so.FindProperty("tagListContainer").objectReferenceValue = tagContent;
        so.FindProperty("tagTogglePrefab").objectReferenceValue = tagPrefab;
        so.FindProperty("newTagInput").objectReferenceValue = newTagInput;
        so.FindProperty("addTagButton").objectReferenceValue = addTagBtn;
        so.FindProperty("choiceListContainer").objectReferenceValue = choiceContent;
        so.FindProperty("choiceRowPrefab").objectReferenceValue = choicePrefab;
        so.FindProperty("addChoiceButton").objectReferenceValue = addChoiceBtn;
        so.FindProperty("saveButton").objectReferenceValue = saveBtn;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(panel, PrefabFolder + "/QuestionRegisterScreen.prefab");
        Selection.activeGameObject = panel; EditorGUIUtility.PingObject(panel);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("[QuestionRegister] UI gerada (com criar tema).");
    }

    // ---- Prefabs -------------------------------------------------------------

    static GameObject BuildTagTogglePrefab()
    {
        GameObject root = NewRect("TagToggle", null);
        var img = root.AddComponent<Image>(); img.color = Panel2; img.sprite = _res.standard; img.type = Image.Type.Sliced;
        var le = root.AddComponent<LayoutElement>(); le.minHeight = 40; le.preferredHeight = 40;
        var hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 4, 4); hlg.spacing = 8;
        hlg.childControlWidth = true; hlg.childForceExpandWidth = false;
        hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        GameObject tg = NewRect("Toggle", root.transform);
        var tImg = tg.AddComponent<Image>(); tImg.color = Ink; tImg.sprite = _res.standard; tImg.type = Image.Type.Sliced;
        var tle = tg.AddComponent<LayoutElement>(); tle.minWidth = 28; tle.preferredWidth = 28;
        var toggle = tg.AddComponent<Toggle>(); toggle.targetGraphic = tImg;
        GameObject check = NewRect("Checkmark", tg.transform);
        var cImg = check.AddComponent<Image>(); cImg.color = Gold; cImg.sprite = _res.checkmark;
        var cRt = check.GetComponent<RectTransform>(); cRt.anchorMin = new Vector2(0.5f, 0.5f); cRt.anchorMax = new Vector2(0.5f, 0.5f); cRt.sizeDelta = new Vector2(22, 22); cRt.anchoredPosition = Vector2.zero;
        toggle.graphic = cImg; toggle.isOn = false;

        TMP_Text lbl = Label("Label", root.transform, "Tema", 16, Bone, FontStyles.Normal, TextAlignmentOptions.Left);
        var lle = lbl.gameObject.AddComponent<LayoutElement>(); lle.flexibleWidth = 1f;

        var tagCtrl = root.AddComponent<TagToggleController>();
        var so = new SerializedObject(tagCtrl);
        so.FindProperty("toggle").objectReferenceValue = toggle;
        so.FindProperty("label").objectReferenceValue = lbl;
        so.ApplyModifiedPropertiesWithoutUndo();

        string path = PrefabFolder + "/TagToggle.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject BuildChoiceRowPrefab()
    {
        GameObject root = NewRect("ChoiceRow", null);
        var img = root.AddComponent<Image>(); img.color = Panel2; img.sprite = _res.standard; img.type = Image.Type.Sliced;
        var le = root.AddComponent<LayoutElement>(); le.minHeight = 48; le.preferredHeight = 48;
        var hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 6, 6); hlg.spacing = 8;
        hlg.childControlWidth = true; hlg.childForceExpandWidth = false;
        hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        GameObject tg = NewRect("CorrectToggle", root.transform);
        var tImg = tg.AddComponent<Image>(); tImg.color = Ink; tImg.sprite = _res.standard; tImg.type = Image.Type.Sliced;
        var tle = tg.AddComponent<LayoutElement>(); tle.minWidth = 28; tle.preferredWidth = 28;
        var toggle = tg.AddComponent<Toggle>(); toggle.targetGraphic = tImg;
        GameObject check = NewRect("Checkmark", tg.transform);
        var cImg = check.AddComponent<Image>(); cImg.color = Gold; cImg.sprite = _res.checkmark;
        var cRt = check.GetComponent<RectTransform>(); cRt.anchorMin = new Vector2(0.5f, 0.5f); cRt.anchorMax = new Vector2(0.5f, 0.5f); cRt.sizeDelta = new Vector2(22, 22); cRt.anchoredPosition = Vector2.zero;
        toggle.graphic = cImg; toggle.isOn = false;

        TMP_InputField input = Input("LabelInput", root.transform, "");
        ((TMP_Text)input.placeholder).text = "Texto da alternativa...";
        var ile = input.gameObject.AddComponent<LayoutElement>(); ile.flexibleWidth = 1f;

        Button remove = Btn("RemoveButton", root.transform, "x", Ink, Dim, 18);
        var rle = remove.gameObject.AddComponent<LayoutElement>(); rle.minWidth = 36; rle.preferredWidth = 36;

        var rowCtrl = root.AddComponent<ChoiceRowController>();
        var so = new SerializedObject(rowCtrl);
        so.FindProperty("labelInput").objectReferenceValue = input;
        so.FindProperty("correctToggle").objectReferenceValue = toggle;
        so.FindProperty("removeButton").objectReferenceValue = remove;
        so.ApplyModifiedPropertiesWithoutUndo();

        string path = PrefabFolder + "/ChoiceRow.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // ---- Scroll --------------------------------------------------------------

    static void BuildScrollView(string name, Transform parent, out ScrollRect scroll, out RectTransform content, bool horizontalFit)
    {
        GameObject sv = NewRect(name, parent);
        var svImg = sv.AddComponent<Image>(); svImg.color = Ink; svImg.sprite = _res.standard; svImg.type = Image.Type.Sliced;
        scroll = sv.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
        var mask = sv.AddComponent<RectMask2D>();

        GameObject viewport = NewRect("Viewport", sv.transform);
        Stretch(viewport);
        GameObject contentGo = NewRect("Content", viewport.transform);
        var cRt = contentGo.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 1); cRt.anchorMax = new Vector2(1, 1); cRt.pivot = new Vector2(0.5f, 1f);
        cRt.anchoredPosition = Vector2.zero; cRt.sizeDelta = new Vector2(0, 0);
        var cVlg = contentGo.AddComponent<VerticalLayoutGroup>();
        cVlg.padding = new RectOffset(6, 6, 6, 6); cVlg.spacing = 6;
        cVlg.childControlWidth = true; cVlg.childForceExpandWidth = true;
        cVlg.childControlHeight = true; cVlg.childForceExpandHeight = false;
        cVlg.childAlignment = TextAnchor.UpperCenter;
        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = cRt;
        content = cRt;
    }

    // ---- helpers -------------------------------------------------------------

    static TMP_Text Label(string n, Transform p, string t, float s, Color c, FontStyles st, TextAlignmentOptions a)
    { var go = TMP_DefaultControls.CreateText(_res); go.name = n; if (p) go.transform.SetParent(p, false); var x = go.GetComponent<TMP_Text>(); x.text = t; x.fontSize = s; x.color = c; x.fontStyle = st; x.alignment = a; x.raycastTarget = false; return x; }
    static TMP_InputField Input(string n, Transform p, string t, bool multiline = false)
    { var go = TMP_DefaultControls.CreateInputField(_res); go.name = n; if (p) go.transform.SetParent(p, false); var f = go.GetComponent<TMP_InputField>(); f.text = t; if (multiline) f.lineType = TMP_InputField.LineType.MultiLineNewline; var i = go.GetComponent<Image>(); if (i) i.color = Panel2; if (f.textComponent) { f.textComponent.color = Bone; f.textComponent.fontSize = 18; } if (f.placeholder is TMP_Text ph) { ph.color = Dim; ph.fontSize = 18; } return f; }
    static Button Btn(string n, Transform p, string l, Color bgc, Color tc, float fs)
    { var go = NewRect(n, p); var img = go.AddComponent<Image>(); img.color = bgc; img.sprite = _res.standard; img.type = Image.Type.Sliced; var b = go.AddComponent<Button>(); b.targetGraphic = img; var t = Label(n + "_Label", go.transform, l, fs, tc, FontStyles.Bold, TextAlignmentOptions.Center); var rt = t.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; return b; }
    static GameObject NewRect(string n, Transform p) { var go = new GameObject(n, typeof(RectTransform)); if (p) go.transform.SetParent(p, false); return go; }
    static void Stretch(GameObject go) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
    static void LE(GameObject go, float minH = -1, float prefH = -1, float minW = -1, float prefW = -1, float flexW = -1, float flexH = -1)
    { var le = go.GetComponent<LayoutElement>(); if (!le) le = go.AddComponent<LayoutElement>(); if (minH >= 0) le.minHeight = minH; if (prefH >= 0) le.preferredHeight = prefH; if (minW >= 0) le.minWidth = minW; if (prefW >= 0) le.preferredWidth = prefW; if (flexW >= 0) le.flexibleWidth = flexW; if (flexH >= 0) le.flexibleHeight = flexH; }
    static Canvas GetOrCreateCanvas()
    { var c = Object.FindObjectOfType<Canvas>(); if (!c) { var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; var sc = go.GetComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080); sc.matchWidthOrHeight = 0.5f; } if (!Object.FindObjectOfType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); return c; }
    static TMP_DefaultControls.Resources Res() => new TMP_DefaultControls.Resources { standard = S("UI/Skin/UISprite.psd"), background = S("UI/Skin/Background.psd"), inputField = S("UI/Skin/InputFieldBackground.psd"), knob = S("UI/Skin/Knob.psd"), checkmark = S("UI/Skin/Checkmark.psd"), dropdown = S("UI/Skin/DropdownArrow.psd"), mask = S("UI/Skin/UIMask.psd") };
    static Sprite S(string path) => AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
    static void EnsureFolder(string path) { if (AssetDatabase.IsValidFolder(path)) return; string par = Path.GetDirectoryName(path).Replace("\\", "/"); string leaf = Path.GetFileName(path); if (!AssetDatabase.IsValidFolder(par)) EnsureFolder(par); AssetDatabase.CreateFolder(par, leaf); }
}
#endif
