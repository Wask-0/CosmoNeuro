using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Project.Develop.Runtime.Comics;
using _Project.Develop.Runtime.Configs;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Characters.Core;
using _Project.Develop.Runtime.Gameplay.GameTime;
using _Project.Develop.Runtime.Gameplay.Hero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace _Project.Develop.Editor.Content
{
    // Собирает контент по диз-доку: импорт арта, блоки, конфиги, HUD, комиксы и уровень.
    // Шаги можно запускать по отдельности из меню Tools/The Lost Cosmonaut/Content.
    public static partial class LostCosmonautBuilder
    {
        private const string Art = "Assets/_Project/Art";
        private const string Sprites = Art + "/Sprite";
        private const string Env = Sprites + "/Enviroments";
        private const string Blocks = Env + "/Blocks";
        private const string Decor = Env + "/Decor";
        private const string Dust = Env + "/Dust";
        private const string Items = Sprites + "/Items";
        private const string HeroArt = Sprites + "/Hero";
        private const string Ui = Sprites + "/UI";
        private const string Comics = Art + "/Comics";
        private const string Audio = Art + "/Audio/Generated";
        private const string Tiles = Art + "/Tiles";
        private const string Configs = "Assets/_Project/Resources/Configs";
        private const string GameplayConfigs = Configs + "/Gameplay";
        private const string GameplayPrefabs = "Assets/_Project/Resources/Prefab/Gameplay";
        private const string GameplayScene = "Assets/_Project/Scenes/GameplayScene.unity";
        private const string ComicScene = "Assets/_Project/Scenes/ComicScene.unity";
        private const string MainScene = "Assets/_Project/Scenes/MainScene.unity";

        private static readonly Vector2 Center = new(0.5f, 0.5f);
        private static readonly Vector2 Bottom = new(0.5f, 0f);

        [MenuItem("Tools/The Lost Cosmonaut/Content/Build Everything")]
        public static void BuildEverything()
        {
            RequireEditMode();
            ImportArt();
            BuildTiles();
            BuildConfigs();
            BuildPrefabs();
            BuildHud();
            BuildComicScene();
            BuildLevel();
            EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
            Debug.Log("CONTENT_BUILD_OK");
        }

        [MenuItem("Tools/The Lost Cosmonaut/Content/1. Import Art")]
        public static void ImportArt()
        {
            RequireEditMode();
            foreach (string path in Pngs(Blocks))
                ImportSprite(path, 32f, Path.GetFileName(path).StartsWith("ceiling") ? new Vector2(0.5f, 64f / 96f) : Center);

            foreach (string path in Pngs(Decor))
            {
                string name = Path.GetFileName(path);
                if (name.StartsWith("ridge")) ImportSprite(path, 16f, Bottom, true);
                else if (name.StartsWith("earth") || name.StartsWith("mars")) ImportSprite(path, 16f, Center);
                else if (name.StartsWith("sparks")) ImportSprite(path, 32f, Center);
                else ImportSprite(path, 32f, Bottom);
            }

            foreach (string path in Pngs(Dust))
            {
                if (Path.GetFileName(path).StartsWith("warm")) ImportSprite(path, 24f, Bottom);
                else ImportSprite(path, 20f, Center);
            }

            foreach (string path in Pngs(Items)) ImportSprite(path, 36f, Bottom);
            ImportSprite(HeroArt + "/bag.png", 36f, Center);
            ImportSprite(HeroArt + "/vacuum_1.png", 100f, Center);
            ImportSprite(HeroArt + "/vacuum_2.png", 100f, Center);
            foreach (string name in new[] { "morale_excellent", "morale_good", "morale_bad", "morale_arrow", "direction_arrow" })
                ImportSprite($"{Ui}/{name}.png", 100f, Center);
            foreach (string path in Pngs(Comics)) ImportSprite(path, 100f, Center);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/The Lost Cosmonaut/Content/2. Build Tiles")]
        public static void BuildTiles()
        {
            RequireEditMode();
            EnsureFolder(Tiles);
            foreach (string path in Pngs(Blocks))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                string tilePath = $"{Tiles}/{name}.asset";
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, tilePath);
                }

                tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                tile.colliderType = Tile.ColliderType.Grid;
                tile.color = Color.white;
                EditorUtility.SetDirty(tile);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/The Lost Cosmonaut/Content/3. Build Configs")]
        public static void BuildConfigs()
        {
            RequireEditMode();

            // Часы: 04-08 утро, 08-16 жара, 16-20 вечер, 20-04 холод; один переход длится 2.5 реальные минуты.
            Set(AssetDatabase.LoadAssetAtPath<GameTimeConfig>($"{GameplayConfigs}/GameTimeConfig.asset"),
                ("_hour", 5), ("_minute", 0), ("_gameSecondsPerRealSecond", 96f));

            var schedule = AssetDatabase.LoadAssetAtPath<DayScheduleConfig>($"{GameplayConfigs}/DayScheduleConfig.asset");
            WriteSchedule(schedule);

            var survival = AssetDatabase.LoadAssetAtPath<HeroSurvivalConfig>($"{GameplayConfigs}/HeroSurvivalConfig.asset");
            survival.InitialFood = 0;
            survival.MaximumFood = 5;
            survival.InitialEnergy = 0f;
            survival.InitialOxygen = 35f;
            survival.FoodPickup = 5;
            survival.OxygenPickup = 65f;
            survival.OxygenPerHour = 7f;
            survival.RestMinutes = 120f;
            EditorUtility.SetDirty(survival);

            LoadOrCreate<HeroMobilityConfig>($"{GameplayConfigs}/HeroMobilityConfig.asset");

            StoryConfig story = LoadOrCreate<StoryConfig>($"{GameplayConfigs}/StoryConfig.asset");
            if (story.Monologues == null || story.Monologues.Length == 0)
                FillStory(story);

            FillAudio(LoadOrCreate<GameplayAudioConfig>($"{GameplayConfigs}/GameplayAudioConfig.asset"));
            FillComics(LoadOrCreate<ComicConfig>($"{Configs}/ComicConfig.asset"));
            AssetDatabase.SaveAssets();
        }

        private static void WriteSchedule(DayScheduleConfig schedule)
        {
            var so = new SerializedObject(schedule);
            SerializedProperty periods = so.FindProperty("_periods");
            (int hour, DayPeriod period, DangerCause cause, bool warm)[] data =
            {
                (4, DayPeriod.Safe, DangerCause.Cold, false),
                (8, DayPeriod.Dangerous, DangerCause.Heat, true),
                (16, DayPeriod.Safe, DangerCause.Heat, true),
                (20, DayPeriod.Dangerous, DangerCause.Cold, false),
            };
            periods.arraySize = data.Length;
            for (int i = 0; i < data.Length; i++)
            {
                SerializedProperty item = periods.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("_hour").intValue = data[i].hour;
                item.FindPropertyRelative("_minute").intValue = 0;
                item.FindPropertyRelative("_period").enumValueIndex = (int)data[i].period;
                item.FindPropertyRelative("_dangerCause").enumValueIndex = (int)data[i].cause;
                item.FindPropertyRelative("_survivalGameMinutes").floatValue = 30f;
                item.FindPropertyRelative("_groundIsWarm").boolValue = data[i].warm;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void FillAudio(GameplayAudioConfig audio)
        {
            audio.Ambient = Clip("amb_moon");
            audio.Music = Clip("music_calm");
            audio.Step = Clip("sfx_step");
            audio.JumpBreath = Clip("sfx_breath_in");
            audio.Exhale = Clip("sfx_exhale");
            audio.ClimbSighs = new[] { Clip("sfx_climb_sigh_1"), Clip("sfx_climb_sigh_2") };
            audio.HardLanding = Clip("sfx_land_hard");
            audio.BagThrow = Clip("sfx_bag_throw");
            audio.BagLand = Clip("sfx_bag_land");
            audio.Pickup = Clip("sfx_pickup");
            audio.Tent = Clip("sfx_tent");
            audio.DeviceLoop = Clip("sfx_magnet_loop");
            audio.Warning = Clip("sfx_warning");
            audio.Victory = Clip("sfx_victory");
            EditorUtility.SetDirty(audio);
        }

        private static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>($"{Audio}/{name}.wav");

        [MenuItem("Tools/The Lost Cosmonaut/Content/4. Build Prefabs")]
        public static void BuildPrefabs()
        {
            RequireEditMode();

            var bag = new GameObject("HeroBag");
            SpriteRenderer renderer = bag.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeroArt + "/bag.png");
            renderer.sortingOrder = -2;
            bag.AddComponent<BagView>();
            PrefabUtility.SaveAsPrefabAsset(bag, $"{GameplayPrefabs}/HeroBag.prefab");
            Object.DestroyImmediate(bag);

            // Герой: кадры с МУ и вид со спины при подъёме.
            const string heroPath = "Assets/_Project/Prefabs/Characters/RigidbodyCharacter.prefab";
            GameObject hero = PrefabUtility.LoadPrefabContents(heroPath);
            CharacterView view = hero.GetComponentInChildren<CharacterView>(true);
            Sprite[] heroSprites = AssetDatabase.LoadAllAssetsAtPath(HeroArt + "/cosmo_Sprite Sheet.png").OfType<Sprite>().ToArray();
            Set(view,
                ("_deviceSprites", new Object[]
                {
                    AssetDatabase.LoadAssetAtPath<Sprite>(HeroArt + "/vacuum_1.png"),
                    AssetDatabase.LoadAssetAtPath<Sprite>(HeroArt + "/vacuum_2.png")
                }),
                ("_climbSprite", heroSprites.First(sprite => sprite.name == "cosmo_Sprite Sheet_6")));
            PrefabUtility.SaveAsPrefabAsset(hero, heroPath);
            PrefabUtility.UnloadPrefabContents(hero);

            // Камера чуть шире: блоки по 2 единицы, а перепады высот должны читаться заранее.
            const string cameraPath = GameplayPrefabs + "/GameplayCamera.prefab";
            GameObject camera = PrefabUtility.LoadPrefabContents(cameraPath);
            camera.GetComponent<Camera>().orthographicSize = 7f;
            PrefabUtility.SaveAsPrefabAsset(camera, cameraPath);
            PrefabUtility.UnloadPrefabContents(camera);

            AssetDatabase.SaveAssets();
        }

        private static IEnumerable<string> Pngs(string folder)
            => Directory.GetFiles(folder, "*.png").Select(path => path.Replace('\\', '/'));

        private static void ImportSprite(string path, float pixelsPerUnit, Vector2 pivot, bool repeat = false)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
                throw new InvalidOperationException("Not imported: " + path);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteExtrude = 1;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Stop Play Mode before building content.");
        }

        private static void Set(Object target, params (string name, object value)[] values)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var so = new SerializedObject(target);
            foreach ((string name, object value) in values)
            {
                SerializedProperty property = so.FindProperty(name);
                if (property == null)
                    throw new InvalidOperationException($"{target.GetType().Name} has no field {name}.");

                switch (value)
                {
                    case null:
                        property.objectReferenceValue = null;
                        break;
                    case Object[] array:
                        property.arraySize = array.Length;
                        for (int i = 0; i < array.Length; i++)
                            property.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
                        break;
                    case Object reference:
                        property.objectReferenceValue = reference;
                        break;
                    case bool flag:
                        property.boolValue = flag;
                        break;
                    case int number when property.propertyType == SerializedPropertyType.Enum:
                        property.enumValueIndex = number;
                        break;
                    case int number:
                        property.intValue = number;
                        break;
                    case float number:
                        property.floatValue = number;
                        break;
                    case string text:
                        property.stringValue = text;
                        break;
                    case Vector2 vector:
                        property.vector2Value = vector;
                        break;
                    case Color color:
                        property.colorValue = color;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported value for {name}.");
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
