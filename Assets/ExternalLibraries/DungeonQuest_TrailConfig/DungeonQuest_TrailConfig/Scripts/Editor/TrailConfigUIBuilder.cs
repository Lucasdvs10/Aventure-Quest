#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DungeonQuest.Trails;

namespace DungeonQuest.Trails.EditorTools
{
    /// <summary>
    /// One-click builder for the Trail Config screen. Run via the menu:
    ///   Dungeon Quest > Build Trail Config UI
    ///
    /// It creates (or reuses) a Canvas + EventSystem, builds the screen panel
    /// and the reusable phase-row, wires every serialized reference, and saves
    /// both as prefabs under Assets/DungeonQuest/Prefabs/.
    ///
    /// This is intentionally a scaffold: colours approximate the dark-fantasy
    /// look, but swap in your pixel TMP Font Asset and 9-sliced frame sprites
    /// to match the mockup exactly. Generating prefabs in-editor (instead of
    /// shipping raw .prefab YAML) guarantees the script/TMP references link.
    /// </summary>
    public static class TrailConfigUIBuilder
    {
        // Dark-fantasy monochrome palette (see mockup).
        static readonly Color Ink    = new Color32(11, 10, 9, 255);
        static readonly Color Panel  = new Color32(21, 20, 15, 255);
        static readonly Color Panel2 = new Color32(33, 30, 23, 255);
        static readonly Color Bone   = new Color32(233, 229, 216, 255);
        static readonly Color Dim    = new Color32(155, 150, 138, 255);

        const string PrefabFolder = "Assets/DungeonQuest/Prefabs";
        static TMP_DefaultControls.Resources _res;

        [MenuItem("Dungeon Quest/Build Trail Config UI")]
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
            GameObject rowPrefab = BuildRowPrefab();
            BuildScreen(rowPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TrailConfig] UI gerada. Prefabs em " + PrefabFolder + ". Ajuste fonte/sprites para o estilo pixel.");
        }

        // -- screen ------------------------------------------------------------

        static void BuildScreen(GameObject rowPrefab)
        {
            Canvas canvas = GetOrCreateCanvas();

            GameObject bg = NewRect("DQ_Background", canvas.transform);
            var bgImg = bg.AddComponent<Image>(); bgImg.color = Ink; bgImg.raycastTarget = false;
            Stretch(bg);

            GameObject panel = NewRect("TrailConfigScreen", canvas.transform);
            var pImg = panel.AddComponent<Image>(); pImg.color = Panel; pImg.sprite = _res.standard; pImg.type = Image.Type.Sliced;
            var pRt = panel.GetComponent<RectTransform>();
            pRt.anchorMin = pRt.anchorMax = pRt.pivot = new Vector2(0.5f, 0.5f);
            pRt.sizeDelta = new Vector2(720, 860);
            pRt.anchoredPosition = Vector2.zero;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 24, 24);
            vlg.spacing = 12;
            vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            LE(Label("Title", panel.transform, "MAPA DA JORNADA", 26, Bone, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 42);
            LE(Label("NameLabel", panel.transform, "NOME DA TRILHA", 18, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 24);

            TMP_InputField nameInput = Input("TrailNameInput", panel.transform, "Fundamentos do Saber");
            LE(nameInput.gameObject, prefH: 46);

            LE(Label("PhasesLabel", panel.transform, "FASES  ·  tema · nº · inimigo", 18, Bone, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 24);

            ScrollRect scroll; RectTransform content;
            BuildScrollView("PhasesScroll", panel.transform, out scroll, out content);
            LE(scroll.gameObject, minH: 280, flexH: 1f);

            Button addBtn = Btn("AddPhaseButton", panel.transform, "+ ADICIONAR FASE", Panel2, Bone, 16);
            LE(addBtn.gameObject, prefH: 40);

            Button saveBtn = Btn("SaveButton", panel.transform, "SALVAR TRILHA", Bone, Ink, 16);
            LE(saveBtn.gameObject, prefH: 54);

            TMP_Text status = Label("StatusText", panel.transform, "", 16, Dim, FontStyles.Normal, TextAlignmentOptions.Center);
            LE(status.gameObject, prefH: 30);

            var ctrl = panel.AddComponent<TrailConfigController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("trailNameInput").objectReferenceValue = nameInput;
            so.FindProperty("phaseListContainer").objectReferenceValue = content;
            so.FindProperty("phaseRowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("addPhaseButton").objectReferenceValue = addBtn;
            so.FindProperty("saveButton").objectReferenceValue = saveBtn;
            so.FindProperty("statusText").objectReferenceValue = status;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(panel, PrefabFolder + "/TrailConfigScreen.prefab");
            Selection.activeGameObject = panel;
            EditorGUIUtility.PingObject(panel);
        }

        // -- reusable phase row ------------------------------------------------

        static GameObject BuildRowPrefab()
        {
            GameObject row = NewRect("TrailPhaseRow", null);
            var le = row.AddComponent<LayoutElement>(); le.minHeight = 46; le.preferredHeight = 46;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8; hlg.padding = new RectOffset(6, 6, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true; hlg.childForceExpandWidth = false;
            hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
            var bg = row.AddComponent<Image>(); bg.color = Panel; bg.sprite = _res.standard; bg.type = Image.Type.Sliced;

            TMP_Text order = Label("Order", row.transform, "1", 20, Bone, FontStyles.Bold, TextAlignmentOptions.Center);
            LE(order.gameObject, minW: 30, prefW: 30);

            TMP_Dropdown theme = Dropdown("Theme", row.transform);
            LE(theme.gameObject, minW: 120, flexW: 1f, minH: 34);

            TMP_InputField count = Input("Count", row.transform, "5");
            count.contentType = TMP_InputField.ContentType.IntegerNumber;
            count.textComponent.alignment = TextAlignmentOptions.Center;
            LE(count.gameObject, minW: 56, prefW: 56, minH: 34);

            TMP_InputField enemy = Input("Enemy", row.transform, "");
            ((TMP_Text)enemy.placeholder).text = "Inimigo";
            LE(enemy.gameObject, minW: 90, flexW: 1f, minH: 34);

            Toggle boss = Tgl("Boss", row.transform, "BOSS");
            LE(boss.gameObject, minW: 64, prefW: 64);

            Button remove = Btn("Remove", row.transform, "X", Panel2, Bone, 16);
            LE(remove.gameObject, minW: 36, prefW: 36, minH: 34);

            var ctrl = row.AddComponent<TrailPhaseRowController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("orderLabel").objectReferenceValue = order;
            so.FindProperty("themeDropdown").objectReferenceValue = theme;
            so.FindProperty("countInput").objectReferenceValue = count;
            so.FindProperty("enemyInput").objectReferenceValue = enemy;
            so.FindProperty("bossToggle").objectReferenceValue = boss;
            so.FindProperty("removeButton").objectReferenceValue = remove;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, PrefabFolder + "/TrailPhaseRow.prefab");
            Object.DestroyImmediate(row);
            return prefab;
        }

        // -- element helpers ---------------------------------------------------

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

        static Toggle Tgl(string name, Transform parent, string label)
        {
            GameObject go = NewRect(name, parent);

            GameObject bgGo = NewRect("Background", go.transform);
            var bgImg = bgGo.AddComponent<Image>(); bgImg.color = Panel2; bgImg.sprite = _res.standard; bgImg.type = Image.Type.Sliced;
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = bgRt.anchorMax = bgRt.pivot = new Vector2(0f, 0.5f);
            bgRt.sizeDelta = new Vector2(22, 22); bgRt.anchoredPosition = Vector2.zero;

            GameObject chkGo = NewRect("Checkmark", bgGo.transform);
            var chkImg = chkGo.AddComponent<Image>(); chkImg.color = Bone; chkImg.sprite = _res.checkmark;
            var chkRt = chkGo.GetComponent<RectTransform>();
            chkRt.anchorMin = Vector2.zero; chkRt.anchorMax = Vector2.one;
            chkRt.offsetMin = new Vector2(2, 2); chkRt.offsetMax = new Vector2(-2, -2);

            TMP_Text t = Label("Label", go.transform, label, 14, Dim, FontStyles.Normal, TextAlignmentOptions.Left);
            var tRt = t.rectTransform;
            tRt.anchorMin = tRt.anchorMax = tRt.pivot = new Vector2(0f, 0.5f);
            tRt.anchoredPosition = new Vector2(26, 0); tRt.sizeDelta = new Vector2(40, 22);

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
            clg.spacing = 8; clg.padding = new RectOffset(6, 6, 6, 6);
            clg.childControlWidth = true; clg.childForceExpandWidth = true;
            clg.childControlHeight = true; clg.childForceExpandHeight = false;
            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.viewport = vRt; scroll.content = content;
        }

        // -- low-level helpers -------------------------------------------------

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
}
#endif
