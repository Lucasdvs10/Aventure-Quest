#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Builder do Ranking global.  Menu: Dungeon Quest > Build Ranking UI</summary>
public static class RankingUIBuilder
{
    static readonly Color Ink = new Color32(11, 10, 9, 255);
    static readonly Color Panel = new Color32(21, 20, 15, 255);
    static readonly Color Panel2 = new Color32(33, 30, 23, 255);
    static readonly Color Bone = new Color32(233, 229, 216, 255);
    static readonly Color Dim = new Color32(155, 150, 138, 255);
    static readonly Color Gold = new Color32(214, 184, 99, 255);

    const string PrefabFolder = "Assets/DungeonQuest/Prefabs";
    static TMP_DefaultControls.Resources _res;

    [MenuItem("Dungeon Quest/Build Ranking UI")]
    public static void Build()
    {
        _res = Res();
        EnsureFolder(PrefabFolder);

        GameObject rowPrefab = BuildRowPrefab();

        Canvas canvas = GetOrCreateCanvas();
        GameObject bg = NewRect("DQ_Background", canvas.transform);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = Ink; bgImg.raycastTarget = false; Stretch(bg);

        GameObject panel = NewRect("RankingScreen", canvas.transform);
        var pImg = panel.AddComponent<Image>(); pImg.color = Panel; pImg.sprite = _res.standard; pImg.type = Image.Type.Sliced;
        var pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(600, 760); pRt.anchoredPosition = Vector2.zero;
        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 22, 22); vlg.spacing = 10;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        Button exitBtn = Btn("ExitButton", panel.transform, "X", Panel2, Bone, 18);
        var exLE = exitBtn.gameObject.AddComponent<LayoutElement>(); exLE.ignoreLayout = true;
        var exRt = exitBtn.GetComponent<RectTransform>();
        exRt.anchorMin = exRt.anchorMax = exRt.pivot = new Vector2(1f, 1f);
        exRt.sizeDelta = new Vector2(34, 34); exRt.anchoredPosition = new Vector2(-10, -10);

        LE(Label("Title", panel.transform, "RANKING GLOBAL", 24, Gold, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 42);

        // Cabeçalho da lista
        GameObject header = NewRect("Header", panel.transform);
        var hHlg = header.AddComponent<HorizontalLayoutGroup>();
        hHlg.spacing = 8; hHlg.padding = new RectOffset(10, 10, 0, 0);
        hHlg.childControlWidth = true; hHlg.childForceExpandWidth = false; hHlg.childControlHeight = true; hHlg.childForceExpandHeight = true;
        LE(header, prefH: 22);
        LE(Label("H1", header.transform, "#", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, minW: 50, prefW: 50);
        LE(Label("H2", header.transform, "JOGADOR", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, flexW: 1f);
        LE(Label("H3", header.transform, "PONTOS", 14, Dim, FontStyles.Normal, TextAlignmentOptions.Right).gameObject, minW: 110, prefW: 110);

        // ScrollView com a lista
        ScrollRect scroll; RectTransform content;
        BuildScrollView("RankingScroll", panel.transform, out scroll, out content);
        LE(scroll.gameObject, minH: 520, flexH: 1f);

        TMP_Text status = Label("StatusText", panel.transform, "", 15, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
        LE(status.gameObject, prefH: 26);

        var ctrl = panel.AddComponent<RankingScreenController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("listContainer").objectReferenceValue = content;
        so.FindProperty("rowPrefab").objectReferenceValue = rowPrefab;
        so.FindProperty("exitButton").objectReferenceValue = exitBtn;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(panel, PrefabFolder + "/RankingScreen.prefab");
        Selection.activeGameObject = panel; EditorGUIUtility.PingObject(panel);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("[Ranking] UI gerada. Adicione o SceneLoader no GameObject RankingScreen para o X funcionar.");
    }

    static GameObject BuildRowPrefab()
    {
        GameObject row = NewRect("RankingRow", null);
        var le = row.AddComponent<LayoutElement>(); le.minHeight = 40; le.preferredHeight = 40;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8; hlg.padding = new RectOffset(10, 10, 4, 4); hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true; hlg.childForceExpandWidth = false; hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
        var bg = row.AddComponent<Image>(); bg.color = Panel2; bg.sprite = _res.standard; bg.type = Image.Type.Sliced;

        TMP_Text rank = Label("Rank", row.transform, "1º", 18, Gold, FontStyles.Bold, TextAlignmentOptions.Left);
        LE(rank.gameObject, minW: 50, prefW: 50);
        TMP_Text name = Label("Name", row.transform, "jogador", 18, Bone, FontStyles.Normal, TextAlignmentOptions.Left);
        LE(name.gameObject, flexW: 1f);
        TMP_Text score = Label("Score", row.transform, "0", 18, Bone, FontStyles.Bold, TextAlignmentOptions.Right);
        LE(score.gameObject, minW: 110, prefW: 110);

        var ctrl = row.AddComponent<RankingRowController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("rankLabel").objectReferenceValue = rank;
        so.FindProperty("nameLabel").objectReferenceValue = name;
        so.FindProperty("scoreLabel").objectReferenceValue = score;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, PrefabFolder + "/RankingRow.prefab");
        Object.DestroyImmediate(row);
        return prefab;
    }

    static void BuildScrollView(string name, Transform parent, out ScrollRect scroll, out RectTransform content)
    {
        GameObject go = NewRect(name, parent);
        var img = go.AddComponent<Image>(); img.color = Ink; img.sprite = _res.standard; img.type = Image.Type.Sliced;
        scroll = go.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 28;

        GameObject viewport = NewRect("Viewport", go.transform);
        var vImg = viewport.AddComponent<Image>(); vImg.color = new Color(0, 0, 0, 0.001f);
        viewport.AddComponent<RectMask2D>();
        var vRt = viewport.GetComponent<RectTransform>();
        vRt.anchorMin = Vector2.zero; vRt.anchorMax = Vector2.one; vRt.offsetMin = new Vector2(4, 4); vRt.offsetMax = new Vector2(-4, -4); vRt.pivot = new Vector2(0f, 1f);

        GameObject contentGo = NewRect("Content", viewport.transform);
        content = contentGo.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f); content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero; content.sizeDelta = Vector2.zero;
        var clg = contentGo.AddComponent<VerticalLayoutGroup>();
        clg.spacing = 4; clg.padding = new RectOffset(4, 4, 4, 4);
        clg.childControlWidth = true; clg.childForceExpandWidth = true; clg.childControlHeight = true; clg.childForceExpandHeight = false;
        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        scroll.viewport = vRt; scroll.content = content;
    }

    // helpers
    static TMP_Text Label(string n, Transform p, string t, float s, Color c, FontStyles st, TextAlignmentOptions a)
    { var go = TMP_DefaultControls.CreateText(_res); go.name = n; go.transform.SetParent(p, false); var x = go.GetComponent<TMP_Text>(); x.text = t; x.fontSize = s; x.color = c; x.fontStyle = st; x.alignment = a; x.raycastTarget = false; return x; }
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
