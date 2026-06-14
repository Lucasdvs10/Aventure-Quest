#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builder da tela de Login/Cadastro. Gera o Canvas + prefab e liga TUDO ao
/// seu LoginScreenController já existente (campos serializados + cliques dos
/// botões -> Login()/Signup()).
///
/// Rode pelo menu:  Dungeon Quest > Build Login UI
///
/// IMPORTANTE: o LoginScreenController faz GetComponent&lt;SceneLoader&gt;() no
/// Awake. Depois de gerar, adicione o seu componente SceneLoader no MESMO
/// GameObject "LoginScreen" (senão o login dá erro ao navegar p/ MainMenu).
///
/// É um scaffold visual: troque o TMP Font Asset pela fonte pixel e os sprites
/// 9-sliced para ficar igual ao mockup.
/// </summary>
public static class LoginUIBuilder
{
    static readonly Color Ink    = new Color32(11, 10, 9, 255);
    static readonly Color Panel  = new Color32(21, 20, 15, 255);
    static readonly Color Panel2 = new Color32(33, 30, 23, 255);
    static readonly Color Bone   = new Color32(233, 229, 216, 255);
    static readonly Color Dim    = new Color32(155, 150, 138, 255);

    const string PrefabFolder = "Assets/DungeonQuest/Prefabs";
    static TMP_DefaultControls.Resources _res;

    [MenuItem("Dungeon Quest/Build Login UI")]
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

        // Fundo escuro.
        GameObject bg = NewRect("DQ_Background", canvas.transform);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = Ink; bgImg.raycastTarget = false;
        Stretch(bg);

        // Painel raiz (recebe o LoginScreenController).
        GameObject screen = NewRect("LoginScreen", canvas.transform);
        var sImg = screen.AddComponent<Image>(); sImg.color = Panel; sImg.sprite = _res.standard; sImg.type = Image.Type.Sliced;
        var sRt = screen.GetComponent<RectTransform>();
        sRt.anchorMin = sRt.anchorMax = sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.sizeDelta = new Vector2(520, 640);
        sRt.anchoredPosition = Vector2.zero;

        var vlg = screen.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 26, 26);
        vlg.spacing = 14;
        vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        LE(Label("Title", screen.transform, "DUNGEON QUEST II", 24, Bone, FontStyles.Bold, TextAlignmentOptions.Center).gameObject, prefH: 40);

        // ---- Bloco LOGIN ----
        LE(Label("LoginHeader", screen.transform, "ENTRAR", 18, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 24);
        TMP_InputField loginUser = Input("LoginInputField", screen.transform, "", "Usuário", false);
        LE(loginUser.gameObject, prefH: 46);
        TMP_InputField loginPass = Input("PasswordInputField", screen.transform, "", "Senha", true);
        LE(loginPass.gameObject, prefH: 46);
        Button loginBtn = Btn("LoginButton", screen.transform, "ENTRAR", Bone, Ink, 16);
        LE(loginBtn.gameObject, prefH: 50);

        // separador
        LE(Label("Sep", screen.transform, "— ou crie uma conta —", 14, Dim, FontStyles.Italic, TextAlignmentOptions.Center).gameObject, prefH: 26);

        // ---- Bloco CADASTRO ----
        LE(Label("SignupHeader", screen.transform, "CADASTRO", 18, Dim, FontStyles.Normal, TextAlignmentOptions.Left).gameObject, prefH: 24);
        TMP_InputField signupUser = Input("SignupUsernameInputField", screen.transform, "", "Novo usuário", false);
        LE(signupUser.gameObject, prefH: 46);
        TMP_InputField signupPass = Input("SignupPasswordInputField", screen.transform, "", "Nova senha", true);
        LE(signupPass.gameObject, prefH: 46);
        Button signupBtn = Btn("SignupButton", screen.transform, "CRIAR CONTA", Panel2, Bone, 16);
        LE(signupBtn.gameObject, prefH: 50);

        // ---- Liga ao LoginScreenController existente ----
        var ctrl = screen.AddComponent<LoginScreenController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("loginInputField").objectReferenceValue = loginUser;
        so.FindProperty("passwordInputField").objectReferenceValue = loginPass;
        so.FindProperty("signupUsernameInputField").objectReferenceValue = signupUser;
        so.FindProperty("signupPasswordInputField").objectReferenceValue = signupPass;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Cliques -> métodos públicos do controller (listener persistente, igual
        // a ligar pelo OnClick do inspector).
        UnityEventTools.AddPersistentListener(loginBtn.onClick, new UnityAction(ctrl.Login));
        UnityEventTools.AddPersistentListener(signupBtn.onClick, new UnityAction(ctrl.Signup));

        PrefabUtility.SaveAsPrefabAsset(screen, PrefabFolder + "/LoginScreen.prefab");
        Selection.activeGameObject = screen;
        EditorGUIUtility.PingObject(screen);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.LogWarning("[Login] UI gerada. AÇÃO NECESSÁRIA: adicione o seu componente 'SceneLoader' no GameObject 'LoginScreen' (o LoginScreenController usa GetComponent<SceneLoader>()).");
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

    static TMP_InputField Input(string name, Transform parent, string text, string placeholder, bool password)
    {
        GameObject go = TMP_DefaultControls.CreateInputField(_res);
        go.name = name; go.transform.SetParent(parent, false);
        var f = go.GetComponent<TMP_InputField>();
        f.text = text;
        if (password) f.contentType = TMP_InputField.ContentType.Password;
        var img = go.GetComponent<Image>(); if (img) img.color = Panel2;
        if (f.textComponent) { f.textComponent.color = Bone; f.textComponent.fontSize = 20; }
        if (f.placeholder is TMP_Text ph) { ph.color = Dim; ph.fontSize = 20; ph.text = placeholder; }
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
