#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DreamBayAppInstaller
{
    private const string ScenePath = "Assets/Scenes/4.mobile.unity";
    private const string LogoPath = "Assets/Sprites/Applogos/dreambay-app-button.png";
    private const string HomeButtonsPath = "Meet And Talk/Menu Canvas/Aspect Ratio Panel/_BG/Home Screen/Buttons";
    private const string SourceButtonPath = HomeButtonsPath + "/Social Media Button";
    private const string DreamBayButtonPath = HomeButtonsPath + "/DreamBay Button";
    private const string BottomContainerPath = "Meet And Talk/Menu Canvas/Aspect Ratio Panel/_BG/Bottom Buttons/Container";
    private const string BottomPanelPath = BottomContainerPath + "/Panel";
    private const string OtherButtonsPath = BottomContainerPath + "/Other Buttons";
    private const string ReplayButtonPath = BottomPanelPath + "/Replay Button";
    private const string GalleryButtonPath = BottomPanelPath + "/Gallery Button";
    private const string SettingsButtonPath = BottomPanelPath + "/Settings Button";
    private const string PowerButtonPath = OtherButtonsPath + "/Power Button";
    private const string SettingsCanvasPath = "Meet And Talk/Settings Canvas";
    private const string DreamBayCanvasPath = "Meet And Talk/DreamBay Canvas";

    private static readonly Color DeepSea = new Color32(6, 10, 8, 255);
    private static readonly Color Surface = new Color32(19, 28, 23, 255);
    private static readonly Color Surface2 = new Color32(28, 40, 33, 255);
    private static readonly Color Text = new Color32(234, 245, 238, 255);
    private static readonly Color Muted = new Color32(143, 168, 152, 255);
    private static readonly Color Green = new Color32(34, 224, 107, 255);
    private static readonly Color Teal = new Color32(28, 211, 192, 255);

    [MenuItem("Tools/Codex/Install DreamBay App")]
    public static void Install()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        CreateLogoAsset();

        DestroyExisting(DreamBayButtonPath);
        DestroyExisting(BottomPanelPath + "/DreamBay Button");
        DestroyExisting(DreamBayCanvasPath);

        var logo = AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);
        if (logo == null)
            throw new InvalidOperationException("DreamBay logo could not be imported as a sprite.");

        var dreamCanvas = CreateDreamBayCanvas(logo);
        var dreamButton = CreateDreamBayButton(logo, dreamCanvas);
        ConfigureBottomDock(dreamButton);

        Validate(dreamButton, dreamCanvas, logo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = dreamButton;

        Debug.Log("DreamBay app installed and validated: home icon, in-game interface, promo code, and https://dreambay.ai button are ready.");
    }

    private static DreamBayCanvas CreateDreamBayCanvas(Sprite logo)
    {
        var template = GameObject.Find(SettingsCanvasPath);
        if (template == null)
            throw new InvalidOperationException("Settings Canvas template was not found in the mobile scene.");

        var root = UnityEngine.Object.Instantiate(template, template.transform.parent);
        Undo.RegisterCreatedObjectUndo(root, "Create DreamBay Canvas");
        root.name = "DreamBay Canvas";
        root.tag = "Untagged";
        root.transform.SetAsLastSibling();

        var settings = root.GetComponent<SettingsCanvas>();
        if (settings != null)
            Undo.DestroyObjectImmediate(settings);

        var content = root.transform.Find("Aspect Ratio Panel") as RectTransform;
        if (content == null)
            throw new InvalidOperationException("DreamBay canvas template has no Aspect Ratio Panel.");

        for (var index = content.childCount - 1; index >= 0; index--)
            Undo.DestroyObjectImmediate(content.GetChild(index).gameObject);

        var canvas = root.GetComponent<Canvas>();
        canvas.enabled = false;
        canvas.sortingOrder = -1;

        var dreamCanvas = Undo.AddComponent<DreamBayCanvas>(root);
        BuildInterface(content, logo);
        return dreamCanvas;
    }

    private static GameObject CreateDreamBayButton(Sprite logo, DreamBayCanvas dreamCanvas)
    {
        var source = GameObject.Find(SourceButtonPath);
        if (source == null)
            throw new InvalidOperationException("Social Media home button template was not found.");

        var clone = UnityEngine.Object.Instantiate(source, source.transform.parent);
        Undo.RegisterCreatedObjectUndo(clone, "Create DreamBay App Button");
        clone.name = "DreamBay Button";
        clone.transform.SetAsLastSibling();

        var label = clone.transform.Find("Label");
        if (label != null && label.TryGetComponent<TMP_Text>(out var labelText))
            labelText.text = "DreamBay";

        var buttonTransform = clone.transform.Find("Button");
        if (buttonTransform == null)
            throw new InvalidOperationException("DreamBay button template has no Button child.");

        var image = buttonTransform.GetComponent<Image>();
        image.sprite = logo;
        image.preserveAspect = true;

        var button = buttonTransform.GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        var launcher = buttonTransform.gameObject.AddComponent<DreamBayAppLauncher>();
        launcher.Configure(dreamCanvas);
        return clone;
    }

    private static void ConfigureBottomDock(GameObject dreamButton)
    {
        var container = GameObject.Find(BottomContainerPath);
        if (container == null)
            throw new InvalidOperationException("The existing bottom dock hierarchy is incomplete.");

        var panelTransform = container.transform.Find("Panel");
        var otherButtonsTransform = container.transform.Find("Other Buttons");
        var replay = panelTransform?.Find("Replay Button")?.gameObject;
        var gallery = panelTransform?.Find("Gallery Button")?.gameObject;
        var settings = panelTransform?.Find("Settings Button")?.gameObject;
        var powerTemplate = otherButtonsTransform?.Find("Power Button")?.gameObject;

        if (panelTransform == null || otherButtonsTransform == null || replay == null || gallery == null || settings == null || powerTemplate == null)
            throw new InvalidOperationException("The existing bottom dock hierarchy is incomplete.");

        var existingDockPower = panelTransform.Find("Power Button");
        if (existingDockPower != null)
            Undo.DestroyObjectImmediate(existingDockPower.gameObject);

        var power = UnityEngine.Object.Instantiate(powerTemplate, panelTransform);
        Undo.RegisterCreatedObjectUndo(power, "Create Dock Power Button");
        power.name = "Power Button";

        var panel = panelTransform.gameObject;
        var otherButtons = otherButtonsTransform.gameObject;
        dreamButton.transform.SetParent(panel.transform, false);

        replay.transform.SetSiblingIndex(0);
        gallery.transform.SetSiblingIndex(1);
        settings.transform.SetSiblingIndex(2);
        dreamButton.transform.SetSiblingIndex(3);
        power.transform.SetSiblingIndex(4);

        otherButtons.SetActive(false);

        var containerRect = (RectTransform)container.transform;
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, 100f);

        var dockLayout = panel.GetComponent<HorizontalLayoutGroup>();
        dockLayout.padding = new RectOffset(12, 12, 0, 0);
        dockLayout.spacing = 8f;
        dockLayout.childAlignment = TextAnchor.MiddleCenter;
        dockLayout.childControlWidth = true;
        dockLayout.childControlHeight = false;
        dockLayout.childForceExpandWidth = false;
        dockLayout.childForceExpandHeight = false;

        var orderedButtons = new[] { replay, gallery, settings, dreamButton, power };
        foreach (var button in orderedButtons)
        {
            var rect = (RectTransform)button.transform;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.sizeDelta = new Vector2(64f, 64f);

            var layoutElement = button.GetComponent<LayoutElement>();
            layoutElement.minWidth = 64f;
            layoutElement.minHeight = 64f;
            layoutElement.preferredWidth = 64f;
            layoutElement.preferredHeight = 64f;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
    }

    private static void BuildInterface(RectTransform root, Sprite logo)
    {
        var background = CreateImage("Deep Sea Background", root, DeepSea);
        Stretch(background.rectTransform);

        var glow = CreateImage("Bay Glow", background.transform, new Color32(10, 46, 39, 255));
        SetRect(glow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -215f), new Vector2(520f, 360f));

        var header = CreateImage("Header", background.transform, new Color(Surface.r, Surface.g, Surface.b, 0.96f));
        SetRect(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(405f, 84f));

        var headerLogo = CreateImage("DreamBay Mark", header.transform, Color.white, logo);
        SetRect(headerLogo.rectTransform, new Vector2(0f, 0.5f), new Vector2(38f, 0f), new Vector2(46f, 46f));

        var wordmark = CreateText("DreamBay Wordmark", header.transform, "Dream<color=#22E06B>Bay</color><size=65%>.ai</size>", 30f, Text, TextAlignmentOptions.Left);
        SetRect(wordmark.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 48f));

        var heroLogo = CreateImage("DreamBay App Logo", background.transform, Color.white, logo);
        SetRect(heroLogo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(132f, 132f));

        var title = CreateText("Headline", background.transform, "Meet your next AI companion", 24f, Text, TextAlignmentOptions.Center);
        title.fontStyle = FontStyles.Bold;
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -292f), new Vector2(385f, 50f));

        var body = CreateText("Promo Message", background.transform, "Go to DreamBay.ai\nfor the best AI companion website.", 19f, Muted, TextAlignmentOptions.Center);
        body.textWrappingMode = TextWrappingModes.Normal;
        SetRect(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -354f), new Vector2(340f, 72f));

        var codeCard = CreateImage("Promo Code Card", background.transform, Surface2);
        codeCard.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        codeCard.type = Image.Type.Sliced;
        SetRect(codeCard.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -444f), new Vector2(292f, 62f));

        var codeLabel = CreateText("Promo Code", codeCard.transform, "USE CODE   <b><color=#22E06B>Tastysoap</color></b>", 21f, Text, TextAlignmentOptions.Center);
        Stretch(codeLabel.rectTransform, 12f, 8f, 12f, 8f);

        var visitObject = new GameObject("Visit DreamBay Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        visitObject.layer = LayerMask.NameToLayer("UI");
        visitObject.transform.SetParent(background.transform, false);
        var visitImage = visitObject.GetComponent<Image>();
        visitImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        visitImage.type = Image.Type.Sliced;
        visitImage.color = Green;
        var visitButton = visitObject.GetComponent<Button>();
        visitButton.targetGraphic = visitImage;
        var colors = visitButton.colors;
        colors.normalColor = Green;
        colors.highlightedColor = Teal;
        colors.pressedColor = new Color32(19, 185, 90, 255);
        colors.selectedColor = Green;
        visitButton.colors = colors;
        var hyperlink = visitObject.AddComponent<HyperlinkButton>();
        hyperlink.Link = DreamBayCanvas.WebsiteUrl;
        SetRect((RectTransform)visitObject.transform, new Vector2(0.5f, 1f), new Vector2(0f, -535f), new Vector2(304f, 66f));

        var visitLabel = CreateText("Button Label", visitObject.transform, "Visit dreambay.ai", 22f, DeepSea, TextAlignmentOptions.Center);
        visitLabel.fontStyle = FontStyles.Bold;
        Stretch(visitLabel.rectTransform, 12f, 6f, 12f, 6f);

        var footer = CreateText("Tagline", background.transform, "Your bay of AI companions — chat, create and dream.", 15f, Muted, TextAlignmentOptions.Center);
        SetRect(footer.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -608f), new Vector2(350f, 44f));
    }

    private static void CreateLogoAsset()
    {
        const int size = 256;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        var clear = new Color32(0, 0, 0, 0);
        for (var index = 0; index < pixels.Length; index++)
            pixels[index] = clear;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                if (!InsideRoundedRect(x, y, 8, 8, 240, 240, 56))
                    continue;

                var color = DeepSea;
                var onBorder = !InsideRoundedRect(x, y, 14, 14, 228, 228, 50);
                if (onBorder)
                    color = Color.Lerp(Green, Teal, (x + y) / 510f) * new Color(1f, 1f, 1f, 0.68f);

                var outerD = x >= 66 && x <= 194 && y >= 55 && y <= 201 &&
                             (x <= 105 || Ellipse(x, y, 108, 128, 86, 73));
                var innerD = x >= 96 && Ellipse(x, y, 108, 128, 50, 43);
                if (outerD && !innerD)
                    color = Color.Lerp(Green, Teal, Mathf.InverseLerp(66f, 194f, x));

                var pearlDistance = Vector2.Distance(new Vector2(x, y), new Vector2(188f, 188f));
                if (pearlDistance <= 24f)
                    color = Color.Lerp(Color.white, Teal, Mathf.Clamp01(pearlDistance / 24f));

                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        var absolutePath = Path.GetFullPath(LogoPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
        File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(LogoPath, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(LogoPath) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 256;
        importer.SaveAndReimport();
    }

    private static void Validate(GameObject buttonRoot, DreamBayCanvas dreamCanvas, Sprite logo)
    {
        var launcher = buttonRoot.GetComponentInChildren<DreamBayAppLauncher>(true);
        var icon = buttonRoot.transform.Find("Button")?.GetComponent<Image>();
        var visit = dreamCanvas.transform.Find("Aspect Ratio Panel/Deep Sea Background/Visit DreamBay Button")?.GetComponent<HyperlinkButton>();
        var promo = dreamCanvas.transform.Find("Aspect Ratio Panel/Deep Sea Background/Promo Code Card/Promo Code")?.GetComponent<TMP_Text>();

        if (launcher == null || launcher.Canvas != dreamCanvas)
            throw new InvalidOperationException("DreamBay home button is not connected to the DreamBay canvas.");
        if (icon == null || icon.sprite != logo)
            throw new InvalidOperationException("DreamBay home button logo is missing.");
        if (visit == null || visit.Link != DreamBayCanvas.WebsiteUrl)
            throw new InvalidOperationException("DreamBay website button is not configured correctly.");
        if (promo == null || !promo.text.Contains(DreamBayCanvas.PromoCode))
            throw new InvalidOperationException("DreamBay promo code is missing from the interface.");

        ValidateBottomDock();
    }

    private static void ValidateBottomDock()
    {
        var panel = GameObject.Find(BottomPanelPath);
        var otherButtons = GameObject.Find(OtherButtonsPath);
        var expectedOrder = new[] { "Replay Button", "Gallery Button", "Settings Button", "DreamBay Button", "Power Button" };

        if (panel == null || panel.transform.childCount != expectedOrder.Length)
            throw new InvalidOperationException("The bottom dock must contain exactly five apps.");
        if (otherButtons == null || otherButtons.activeSelf)
            throw new InvalidOperationException("The old secondary bottom row should be hidden.");

        for (var index = 0; index < expectedOrder.Length; index++)
        {
            var child = panel.transform.GetChild(index);
            var rect = (RectTransform)child;
            if (child.name != expectedOrder[index])
                throw new InvalidOperationException("Bottom dock order is incorrect at position " + index + ".");
            if (!Mathf.Approximately(rect.rect.width, 64f) || !Mathf.Approximately(rect.rect.height, 64f))
                throw new InvalidOperationException(child.name + " is not 64 by 64 pixels.");
        }
    }

    private static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.sprite = sprite;
        image.preserveAspect = sprite != null;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static bool InsideRoundedRect(float x, float y, float left, float bottom, float width, float height, float radius)
    {
        var clampedX = Mathf.Clamp(x, left + radius, left + width - radius);
        var clampedY = Mathf.Clamp(y, bottom + radius, bottom + height - radius);
        var dx = x - clampedX;
        var dy = y - clampedY;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static bool Ellipse(float x, float y, float centerX, float centerY, float radiusX, float radiusY)
    {
        var dx = (x - centerX) / radiusX;
        var dy = (y - centerY) / radiusY;
        return dx * dx + dy * dy <= 1f;
    }

    private static void DestroyExisting(string path)
    {
        var existing = GameObject.Find(path);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);
    }
}
#endif
