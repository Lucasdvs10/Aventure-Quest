#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Builder da tela Jogar.  Menu: Dungeon Quest > Build Play Setup UI</summary>
public static class PlaySetupUIBuilder
{
    static readonly Color Ink = new Color32(11, 10, 9, 255);
    static readonly Color Panel = new Color32(21, 20, 15, 255);
    static readonly Color Panel2 = new Color32(33, 30, 23, 255);
    static readonly Color Bone = new Color32(233, 229, 216, 255);
    static readonly Color Dim = new Color32(155, 150, 138, 255);

    const string PrefabFolder = "Assets/DungeonQuest/Prefabs";
    static TMP_DefaultControls.Resources _res;

    [MenuItem("Dungeon Quest/Build Play Setup UI")]
    public static void Build()
    {
        _res = Res();
        EnsureFolder(PrefabFolder);
        Canvas canvas = GetOrCreateCanvas();

        GameObject bg = NewRect("DQ_Background", canvas.transform);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = Ink; bgImg.raycastTarget = false; Stretch(bg);

        GameObject panel = NewRect("PlaySetupScreen", canvas.transform);
        var pImg = panel.AddComponent<Image>(); pImg.color = Panel; pImg.sprite = _res.standard; pImg.type = Image.Type.Sliced;
        var pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(520, 420); pRt.anchoredPosition = Vector2.zero;
        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 26, 26); vlg.spacing = 16;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        Button exitBtn = Btn("ExitButton", panel.transform, "X", Panel2, Bone, 18);
        var exLE = exitBtn.gameObject.AddComponent<LayoutElement>(); exLE.ignoreLayout = true;
        var exRt = exitBtn.GetComponent<RectTransform>();
        exRt.anchorMin = exRt.anchorMax = exRt.pivot = new Vector2(1f, 1f);
        exRt.sizeDelta = new Vector2(34, 34); exRt.anchoredPosition = new Vector2(-10, -10);

        LE(Label("Title", panel.transform, "JOGAR", 28, Bone, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 46);
        LE(Label("DiscLabel", panel.transform, "DISCIPLINA", 16, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 22);
        TMP_Dropdown disc = Dropdown("DisciplineDropdown", panel.transform);
        LE(disc.gameObject, prefH: 48);

        Button playBtn = Btn("PlayButton", panel.transform, "JOGAR  >", Bone, Ink, 18);
        LE(playBtn.gameObject, prefH: 64);

        TMP_Text status = Label("StatusText", panel.transform, "", 15, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
        LE(status.gameObject, prefH: 26);

        var ctrl = panel.AddComponent<PlaySetupScreenController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("disciplineDropdown").objectReferenceValue = disc;
        so.FindProperty("playButton").objectReferenceValue = playBtn;
        so.FindProperty("exitButton").objectReferenceValue = exitBtn;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(panel, PrefabFolder + "/PlaySetupScreen.prefab");
        Selection.activeGameObject = panel; EditorGUIUtility.PingObject(panel);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("[Play] UI gerada. Adicione o SceneLoader no GameObject PlaySetupScreen e ajuste combatScene/mainMenuScene.");
    }

    // helpers
    static TMP_Text Label(string n, Transform p, string t, float s, Color c, FontStyles st, TextAlignmentOptions a)
    { var go = TMP_DefaultControls.CreateText(_res); go.name = n; go.transform.SetParent(p, false); var x = go.GetComponent<TMP_Text>(); x.text = t; x.fontSize = s; x.color = c; x.fontStyle = st; x.alignment = a; x.raycastTarget = false; return x; }
    static TMP_Dropdown Dropdown(string n, Transform p)
    { var go = TMP_DefaultControls.CreateDropdown(_res); go.name = n; go.transform.SetParent(p, false); var d = go.GetComponent<TMP_Dropdown>(); var i = go.GetComponent<Image>(); if (i) i.color = Panel2; if (d.captionText) { d.captionText.color = Bone; d.captionText.fontSize = 18; } if (d.itemText) { d.itemText.color = Ink; d.itemText.fontSize = 18; } return d; }
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
