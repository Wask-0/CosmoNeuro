using System;
using _Project.Develop.Runtime.Configs.Gameplay;
using _Project.Develop.Runtime.Gameplay.Dialogue;
using _Project.Develop.Runtime.Gameplay.Interaction;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.Interaction.Scanning;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Develop.Editor.Interaction
{
    public static class InteractionVerification
    {
        [MenuItem("Tools/The Lost Cosmonaut/Verify Interactions (Play Mode)")]
        public static void Verify()
        {
            if (!Application.isPlaying)
            { Debug.LogWarning("Запусти Play Mode, затем повтори проверку взаимодействий."); return; }
            GameObject root = new GameObject("Temporary Interaction Verification");
            root.transform.position = new Vector3(10000f, 10000f, 0f);
            InteractionConfig config = ScriptableObject.CreateInstance<InteractionConfig>();
            config.DetectionMask = 1 << 30;
            config.Radius = 2.5f;
            var dialogue = new DialogueState();
            try
            {
                Transform hero = Child(root.transform, "Hero", Vector3.zero).transform;
                InteractableObject first = Target(root.transform, "First", new Vector3(0.7f, 0f));
                InteractableObject second = Target(root.transform, "Second", new Vector3(1.4f, 0f));
                // Multiple colliders on a single interactable must not produce duplicate targets.
                Child(first.transform, "Extra collider", Vector3.zero).AddComponent<CircleCollider2D>().isTrigger = true;
                first.transform.GetChild(0).gameObject.layer = 30;
                var scanner = new CircleInteractionScanner(hero, config);
                var context = new InteractionContext(new CharacterInteractor(hero, dialogue, null));
                Physics2D.SyncTransforms();
                Require(scanner.FindNearest(context) == first, "Выбор ближайшего из двух объектов");
                hero.localPosition = new Vector3(1.2f, 0f);
                Physics2D.SyncTransforms();
                Require(scanner.FindNearest(context) == second, "Переключение на ближайший объект при движении");
                second.enabled = false;
                Require(scanner.FindNearest(context) == first, "Исключение выключенного интерактивного объекта");
                first.gameObject.SetActive(false);
                Physics2D.SyncTransforms();
                Require(scanner.FindNearest(context) == null, "Отсутствие доступных объектов");
                first.gameObject.SetActive(true);
                hero.localPosition = Vector3.zero;
                for (int i = 0; i < 70; i++)
                    Target(root.transform, "Dense " + i, new Vector3(1.8f + i * 0.001f, 0f));
                Physics2D.SyncTransforms();
                Require(scanner.FindNearest(context) == first, "Поиск без потери ближайшего объекта при большом числе коллайдеров");
                Debug.Log("INTERACTION_SCANNER_OK: ближайший объект, переключение, выключение, пустой радиус, 70+ объектов.");
            }
            finally
            {
                dialogue.Dispose();
                Object.Destroy(root);
                Object.Destroy(config);
            }
        }

        private static GameObject Child(Transform parent, string name, Vector3 position)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            return item;
        }

        private static InteractableObject Target(Transform parent, string name, Vector3 position)
        {
            GameObject item = Child(parent, name, position);
            item.layer = 30;
            item.AddComponent<BoxCollider2D>().isTrigger = true;
            return item.AddComponent<InteractableObject>();
        }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Проверка взаимодействий: " + message); }
    }
}
