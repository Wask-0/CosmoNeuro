using System.Collections.Generic;
using System.Linq;
using _Project.Develop.Runtime.Comics;
using _Project.Develop.Runtime.UI.Gameplay.Clock;
using _Project.Develop.Runtime.UI.Gameplay.Environment;
using _Project.Develop.Runtime.UI.Gameplay.Morale;
using _Project.Develop.Runtime.UI.Gameplay.Resources;
using _Project.Develop.Runtime.UI.Gameplay.Subtitles;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _Project.Develop.Editor.Content
{
    public static partial class LostCosmonautBuilder
    {
        private const string UiPrefabs = "Assets/_Project/Resources/UI/Gameplay";

        private static readonly Color Ink = new(0.05f, 0.06f, 0.11f, 1f);
        private static readonly Color Glass = new(0.84f, 0.89f, 0.92f, 0.9f);
        private static readonly Color Panel = new(0.04f, 0.05f, 0.1f, 0.78f);

        [MenuItem("Tools/The Lost Cosmonaut/Content/5. Build HUD")]
        public static void BuildHud()
        {
            RequireEditMode();
            BuildResourceGauges();
            BuildClock();
            BuildRootHud();
            AssetDatabase.SaveAssets();
        }

        // Макет интерфейса: пища под часами, энергия и кислород внизу справа.
        private static void BuildResourceGauges()
        {
            string path = UiPrefabs + "/Resources/HeroResources.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            foreach (Transform child in root.transform.Cast<Transform>().ToArray())
                Object.DestroyImmediate(child.gameObject);

            ResourceGaugeView food = Gauge(root.transform, "Food", true, new Vector2(1f, 1f), new Vector2(-32f, -264f),
                new Vector2(200f, 28f), new Color(0.56f, 0.83f, 0.92f));
            ResourceGaugeView energy = Gauge(root.transform, "Energy", false, new Vector2(1f, 0f), new Vector2(-224f, 58f),
                new Vector2(50f, 176f), new Color(0.27f, 0.84f, 1f));
            ResourceGaugeView oxygen = Gauge(root.transform, "Oxygen", false, new Vector2(1f, 0f), new Vector2(-128f, 58f),
                new Vector2(50f, 176f), new Color(0.13f, 0.52f, 0.98f));
            ResourceGaugeView warm = Gauge(root.transform, "WarmDust", false, new Vector2(1f, 0f), new Vector2(-32f, 58f),
                new Vector2(50f, 176f), new Color(1f, 0.6f, 0.22f));

            Set(root.GetComponent<HeroResourcesView>(),
                ("_food", food), ("_energy", energy), ("_oxygen", oxygen), ("_warmDust", warm));

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static ResourceGaugeView Gauge(Transform parent, string name, bool horizontal, Vector2 anchor,
            Vector2 position, Vector2 size, Color liquidColor)
        {
            RectTransform root = Rect(parent, name, anchor, anchor, new Vector2(anchor.x, anchor.y), position, size);
            Graphic(root, "Frame", Ink, stretch: true);
            Image glass = Graphic(root, "Glass", Glass, stretch: true);
            glass.rectTransform.offsetMin = new Vector2(4f, 4f);
            glass.rectTransform.offsetMax = new Vector2(-4f, -4f);

            RectTransform liquidRect = Rect(root, "Liquid", Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero);
            liquidRect.offsetMin = new Vector2(5f, 5f);
            liquidRect.offsetMax = new Vector2(-5f, -5f);
            var liquid = liquidRect.gameObject.AddComponent<LiquidGaugeGraphic>();
            liquid.color = liquidColor;
            liquid.raycastTarget = false;
            Set(liquid, ("_horizontal", horizontal),
                ("_surfaceColor", Color.Lerp(liquidColor, Color.white, 0.55f)));

            TextMeshProUGUI label = Text(root, "Label", "", 17f, Color.white, TextAlignmentOptions.Center);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -3f);
            labelRect.sizeDelta = new Vector2(Mathf.Max(size.x, 110f), 24f);

            ResourceGaugeView view = root.gameObject.AddComponent<ResourceGaugeView>();
            Set(view, ("_liquid", liquid), ("_label", label));
            return view;
        }

        // Циферблат справа сверху, дата по центру экрана.
        private static void BuildClock()
        {
            string path = UiPrefabs + "/Clock/GameClock.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            Stretch(root.GetComponent<RectTransform>());

            RectTransform dial = root.transform.Find("Dial").GetComponent<RectTransform>();
            dial.anchorMin = dial.anchorMax = new Vector2(1f, 1f);
            dial.pivot = Center;
            dial.anchoredPosition = new Vector2(-132f, -128f);
            dial.sizeDelta = new Vector2(208f, 208f);

            Transform oldFrame = root.transform.Find("DateFrame");
            if (oldFrame != null) Object.DestroyImmediate(oldFrame.gameObject);

            RectTransform frame = Rect(root.transform, "DateFrame", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(280f, 66f));
            Graphic(frame, "Border", Ink, stretch: true);
            Image inside = Graphic(frame, "Inside", new Color(0.92f, 0.93f, 0.92f, 0.95f), stretch: true);
            inside.rectTransform.offsetMin = new Vector2(4f, 4f);
            inside.rectTransform.offsetMax = new Vector2(-4f, -4f);
            frame.SetAsFirstSibling();

            var date = root.transform.Find("DateText").GetComponent<TMP_Text>();
            RectTransform dateRect = date.rectTransform;
            dateRect.anchorMin = dateRect.anchorMax = new Vector2(0.5f, 1f);
            dateRect.pivot = new Vector2(0.5f, 1f);
            dateRect.anchoredPosition = new Vector2(0f, -38f);
            dateRect.sizeDelta = new Vector2(280f, 50f);
            date.fontSize = 36f;
            date.alignment = TextAlignmentOptions.Center;
            date.color = Ink;

            Set(root.GetComponent<GameClockView>(), ("_midnightAngle", 90f));

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void BuildRootHud()
        {
            string path = UiPrefabs + "/GameplayUIRoot.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            Transform hud = root.transform.Find("HUDLayer");

            foreach (string name in new[] { "ScreenTint", "VisionHole", "RestOverlay", "Morale", "Subtitles", "Hints", "DirectionHint" })
            {
                Transform old = hud.Find(name);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            Stretch(hud.Find("GameClock").GetComponent<RectTransform>());

            // Экранные эффекты идут первыми, чтобы часы и шкалы рисовались поверх них.
            Image tintImage = Graphic(hud, "ScreenTint", new Color(0f, 0f, 0f, 0f), stretch: true);
            ScreenTintView tint = tintImage.gameObject.AddComponent<ScreenTintView>();
            Set(tint, ("_image", tintImage));

            RectTransform visionRect = Rect(hud, "VisionHole", Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero);
            var vision = visionRect.gameObject.AddComponent<VisionHoleGraphic>();
            vision.color = Color.black;
            vision.raycastTarget = false;

            Image restImage = Graphic(hud, "RestOverlay", new Color(0.015f, 0.02f, 0.05f, 0.93f), stretch: true);
            CanvasGroup restGroup = restImage.gameObject.AddComponent<CanvasGroup>();
            restGroup.alpha = 0f;
            restGroup.blocksRaycasts = false;
            restGroup.interactable = false;
            TextMeshProUGUI restText = Text(restImage.rectTransform, "Text", "", 34f, Color.white, TextAlignmentOptions.Center);
            Stretch(restText.rectTransform);
            RestOverlayView rest = restImage.gameObject.AddComponent<RestOverlayView>();
            Set(rest, ("_group", restGroup), ("_text", restText));

            tintImage.transform.SetSiblingIndex(0);
            visionRect.SetSiblingIndex(1);
            restImage.transform.SetSiblingIndex(2);

            BuildMorale(hud);

            RectTransform subtitles = Rect(hud, "Subtitles", Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero);
            RectTransform subtitlePanel = Rect(subtitles, "Panel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(-60f, 108f), new Vector2(1180f, 96f));
            Graphic(subtitlePanel, "Background", Panel, stretch: true);
            TextMeshProUGUI subtitleText = Text(subtitlePanel, "Text", "", 28f, Color.white, TextAlignmentOptions.Center);
            Stretch(subtitleText.rectTransform);
            subtitleText.rectTransform.offsetMin = new Vector2(24f, 6f);
            subtitleText.rectTransform.offsetMax = new Vector2(-24f, -6f);
            subtitlePanel.gameObject.SetActive(false);
            SubtitleView subtitleView = subtitles.gameObject.AddComponent<SubtitleView>();
            Set(subtitleView, ("_panel", subtitlePanel.gameObject), ("_text", subtitleText));

            RectTransform hints = Rect(hud, "Hints", Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero);
            RectTransform hintPanel = Rect(hints, "Panel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(-60f, 44f), new Vector2(760f, 50f));
            Graphic(hintPanel, "Background", new Color(0.9f, 0.76f, 0.2f, 0.92f), stretch: true);
            TextMeshProUGUI hintText = Text(hintPanel, "Text", "", 22f, Ink, TextAlignmentOptions.Center);
            Stretch(hintText.rectTransform);
            hintPanel.gameObject.SetActive(false);
            HintView hintView = hints.gameObject.AddComponent<HintView>();
            Set(hintView, ("_panel", hintPanel.gameObject), ("_text", hintText));

            RectTransform arrow = Rect(hud, "DirectionHint", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(280f, 44f));
            Image arrowImage = arrow.gameObject.AddComponent<Image>();
            arrowImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Ui + "/direction_arrow.png");
            arrowImage.raycastTarget = false;
            CanvasGroup arrowGroup = arrow.gameObject.AddComponent<CanvasGroup>();
            arrowGroup.alpha = 0f;
            arrowGroup.blocksRaycasts = false;
            DirectionHintView direction = arrow.gameObject.AddComponent<DirectionHintView>();
            Set(direction, ("_group", arrowGroup));

            // Предупреждение об опасности уходит из-под индикатора морали под дату.
            RectTransform warning = root.transform.Find("PopupsLayer/DeathScreen/DangerWarning").GetComponent<RectTransform>();
            warning.anchorMin = warning.anchorMax = new Vector2(0.5f, 1f);
            warning.pivot = new Vector2(0.5f, 1f);
            warning.anchoredPosition = new Vector2(0f, -106f);
            warning.sizeDelta = new Vector2(760f, 76f);
            TMP_Text warningText = warning.GetComponent<TMP_Text>();
            warningText.alignment = TextAlignmentOptions.Center;
            warningText.color = new Color(1f, 0.74f, 0.4f, 1f);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void BuildMorale(Transform hud)
        {
            RectTransform root = Rect(hud, "Morale", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30f, -30f), new Vector2(148f, 112f));
            Graphic(root, "Border", Ink, stretch: true);
            Image inside = Graphic(root, "Inside", new Color(0.92f, 0.93f, 0.92f, 0.95f), stretch: true);
            inside.rectTransform.offsetMin = new Vector2(4f, 4f);
            inside.rectTransform.offsetMax = new Vector2(-4f, -4f);

            RectTransform faceRect = Rect(root, "Face", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, 0f), new Vector2(88f, 88f));
            Image face = faceRect.gameObject.AddComponent<Image>();
            face.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Ui + "/morale_excellent.png");
            face.raycastTarget = false;

            RectTransform arrowRect = Rect(root, "Arrow", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Center,
                new Vector2(-24f, 0f), new Vector2(26f, 70f));
            Image arrow = arrowRect.gameObject.AddComponent<Image>();
            arrow.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Ui + "/morale_arrow.png");
            arrow.raycastTarget = false;

            MoraleView view = root.gameObject.AddComponent<MoraleView>();
            Set(view, ("_face", face), ("_arrow", arrowRect),
                ("_excellent", AssetDatabase.LoadAssetAtPath<Sprite>(Ui + "/morale_excellent.png")),
                ("_good", AssetDatabase.LoadAssetAtPath<Sprite>(Ui + "/morale_good.png")),
                ("_bad", AssetDatabase.LoadAssetAtPath<Sprite>(Ui + "/morale_bad.png")));
        }

        [MenuItem("Tools/The Lost Cosmonaut/Content/6. Build Comic Scene")]
        public static void BuildComicScene()
        {
            RequireEditMode();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();

            var canvasObject = new GameObject("Comic Page", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Graphic(canvasObject.transform, "Paper", new Color(0.93f, 0.9f, 0.82f, 1f), stretch: true);

            var panels = new List<ComicPanelView>();
            Vector2 size = new(820f, 462f);
            Vector2[] positions =
            {
                new(-430f, 248f), new(430f, 248f),
                new(-430f, -232f), new(430f, -232f),
            };
            for (int i = 0; i < positions.Length; i++)
                panels.Add(ComicPanel(canvasObject.transform, i, positions[i], size));

            TextMeshProUGUI hint = Text(canvasObject.transform, "Hint", "", 22f, Ink, TextAlignmentOptions.Center);
            RectTransform hintRect = hint.rectTransform;
            hintRect.anchorMin = hintRect.anchorMax = new Vector2(0.5f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.anchoredPosition = new Vector2(0f, 8f);
            hintRect.sizeDelta = new Vector2(900f, 30f);

            var bootstrapObject = new GameObject("SceneBootstrap");
            ComicSceneBootstrap bootstrap = bootstrapObject.AddComponent<ComicSceneBootstrap>();
            Set(bootstrap, ("_panels", panels.Cast<Object>().ToArray()), ("_hint", hint));

            EditorSceneManager.SaveScene(scene, ComicScene);

            List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
            if (buildScenes.All(item => item.path != ComicScene))
            {
                buildScenes.Add(new EditorBuildSettingsScene(ComicScene, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }
        }

        private static ComicPanelView ComicPanel(Transform parent, int index, Vector2 position, Vector2 size)
        {
            RectTransform root = Rect(parent, "Panel " + (index + 1), Center, Center, Center, position, size);
            CanvasGroup group = root.gameObject.AddComponent<CanvasGroup>();
            Graphic(root, "Border", Ink, stretch: true);

            RectTransform imageRect = Rect(root, "Image", Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero);
            imageRect.offsetMin = new Vector2(6f, 6f);
            imageRect.offsetMax = new Vector2(-6f, -6f);
            Image image = imageRect.gameObject.AddComponent<Image>();
            image.raycastTarget = false;

            // Подпись сверху, как в комиксах: внизу кадра обычно важный для сюжета грунт.
            RectTransform band = Rect(root, "Caption", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -6f), new Vector2(-12f, 76f));
            Graphic(band, "Background", new Color(0.99f, 0.95f, 0.72f, 0.96f), stretch: true);
            TextMeshProUGUI caption = Text(band, "Text", "", 23f, Ink, TextAlignmentOptions.MidlineLeft);
            Stretch(caption.rectTransform);
            caption.rectTransform.offsetMin = new Vector2(16f, 4f);
            caption.rectTransform.offsetMax = new Vector2(-16f, -4f);

            ComicPanelView view = root.gameObject.AddComponent<ComicPanelView>();
            Set(view, ("_group", group), ("_image", image), ("_caption", caption));
            return view;
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform));
            item.layer = 5;
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = Center;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static Image Graphic(Transform parent, string name, Color color, bool stretch)
        {
            RectTransform rect = Rect(parent, name, Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero);
            if (stretch) Stretch(rect);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI Text(Transform parent, string name, string value, float size, Color color,
            TextAlignmentOptions alignment)
        {
            RectTransform rect = Rect(parent, name, Center, Center, Center, Vector2.zero, new Vector2(200f, 40f));
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }
    }
}
