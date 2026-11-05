using System;
using System.Linq;
using System.Reflection;
using _Project.Develop.Runtime.Gameplay.Interaction.Actions;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.Interaction.Requirements;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Develop.Editor.Interaction
{
    // Editor-only composition. Runtime still consumes ordinary components and action roots.
    public static class InteractionAuthoring
    {
        public static Type[] GetTypes(Type contract) => TypeCache.GetTypesDerivedFrom(contract)
            .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && !type.IsAbstract
                && !type.ContainsGenericParameters && !type.Assembly.GetName().Name.Contains(".Tests")
                && !type.Assembly.GetName().Name.EndsWith(".Editor", StringComparison.Ordinal))
            .OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();

        public static MonoBehaviour AddAction(Interactable owner, string branch, Type type,
            ParallelInteractionAction group = null)
        {
            Validate(owner, type, typeof(IInteractionAction));
            if (branch != "_successActions" && branch != "_failedActions")
                throw new ArgumentException("Unknown interaction branch.", nameof(branch));
            if (group != null && !group.transform.IsChildOf(owner.transform))
                throw new ArgumentException("New group members must belong to this interactable.", nameof(group));
            int undo = StartUndo("Добавить действие взаимодействия");
            try
            {
                var data = new SerializedObject(owner);
                SerializedProperty property = data.FindProperty(branch);
                var root = property.objectReferenceValue as Transform;
                if (root == null)
                {
                    root = CreateChild(owner.transform, branch == "_successActions" ? "SuccessActions" : "FailedActions");
                    property.objectReferenceValue = root;
                    data.ApplyModifiedProperties();
                }
                var node = CreateChild(group != null ? group.transform : root, type.Name);
                var action = (MonoBehaviour)Undo.AddComponent(node.gameObject, type);
                ConfigureDefaults(owner, action);
                if (group != null)
                {
                    var groupData = new SerializedObject(group);
                    var members = groupData.FindProperty("_actions");
                    members.arraySize++;
                    members.GetArrayElementAtIndex(members.arraySize - 1).objectReferenceValue = action;
                    groupData.ApplyModifiedProperties();
                }
                return action;
            }
            finally { Undo.CollapseUndoOperations(undo); }
        }

        public static MonoBehaviour AddRequirement(Interactable owner, Type type)
        {
            Validate(owner, type, typeof(IInteractionRequirement));
            int undo = StartUndo("Добавить условие взаимодействия");
            try
            {
                Transform root = owner.transform.Find("InteractionRequirements")
                    ?? CreateChild(owner.transform, "InteractionRequirements");
                var node = CreateChild(root, type.Name);
                return (MonoBehaviour)Undo.AddComponent(node.gameObject, type);
            }
            finally { Undo.CollapseUndoOperations(undo); }
        }

        public static void CreateInteractionPoint(Interactable owner)
        {
            if (EditorApplication.isPlaying || PrefabUtility.IsPartOfPrefabAsset(owner)) return;
            var data = new SerializedObject(owner);
            var property = data.FindProperty("_interactionPoint");
            if (property == null || property.objectReferenceValue != null) return;
            int undo = StartUndo("Создать точку взаимодействия");
            property.objectReferenceValue = CreateChild(owner.transform, "InteractionPoint");
            data.ApplyModifiedProperties();
            Undo.CollapseUndoOperations(undo);
        }



        public static void Remove(Interactable owner, MonoBehaviour component)
        {
            if (component == null) return;
            int undo = StartUndo("Удалить действие или условие");
            try
            {
                bool dedicated = component.transform != owner.transform && component.transform.IsChildOf(owner.transform)
                    && IsActionTree(component.transform);
                var removed = dedicated ? component.GetComponentsInChildren<MonoBehaviour>(true)
                    : new[] { component };
                // Remove references before destruction; existing external group members are kept.
                foreach (var group in owner.GetComponentsInChildren<ParallelInteractionAction>(true))
                {
                    var data = new SerializedObject(group);
                    var members = data.FindProperty("_actions");
                    for (int i = members.arraySize - 1; i >= 0; i--)
                        if (removed.Contains(members.GetArrayElementAtIndex(i).objectReferenceValue as MonoBehaviour))
                        { members.GetArrayElementAtIndex(i).objectReferenceValue = null; members.DeleteArrayElementAtIndex(i); }
                    data.ApplyModifiedProperties();
                }
                // Only dedicated action nodes can be removed as a whole. Other scene components survive.
                if (dedicated) Undo.DestroyObjectImmediate(component.gameObject);
                else Undo.DestroyObjectImmediate(component);
            }
            finally { Undo.CollapseUndoOperations(undo); }
        }

        public static bool CanMove(MonoBehaviour component, Transform branch)
            => component != null && component.transform.parent == branch
                && component.GetComponents<IInteractionAction>().Length == 1;

        public static void Move(MonoBehaviour component, int direction)
        {
            int index = component.transform.GetSiblingIndex();
            Undo.RegisterCompleteObjectUndo(component.transform.parent, "Изменить порядок действий");
            Undo.RecordObject(component.transform, "Изменить порядок действий");
            component.transform.SetSiblingIndex(index + direction);
            PrefabUtility.RecordPrefabInstancePropertyModifications(component.transform);
        }

        private static bool IsActionTree(Transform root)
        {
            foreach (var component in root.GetComponents<Component>())
                if (component is not Transform && component is not IInteractionAction && component is not IInteractionRequirement)
                    return false;
            foreach (Transform child in root)
                if (!IsActionTree(child)) return false;
            return true;
        }

        private static void ConfigureDefaults(Interactable owner, MonoBehaviour component)
        {
            var data = new SerializedObject(component);
            var target = data.FindProperty("_target");
            var field = component.GetType().GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (target != null && target.propertyType == SerializedPropertyType.ObjectReference && target.objectReferenceValue == null)
            {
                if (field?.FieldType == typeof(Transform)) target.objectReferenceValue = owner.transform;
                else if (field?.FieldType == typeof(GameObject)) target.objectReferenceValue = owner.gameObject;
            }
            var interactable = data.FindProperty("_interactable");
            if (interactable != null && interactable.propertyType == SerializedPropertyType.ObjectReference
                && interactable.objectReferenceValue == null) interactable.objectReferenceValue = owner;

            data.ApplyModifiedProperties();
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var node = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(node, "Создать настройку взаимодействия");
            Undo.SetTransformParent(node.transform, parent, "Назначить объект взаимодействия");
            node.transform.localPosition = Vector3.zero;
            node.transform.localRotation = Quaternion.identity;
            node.transform.localScale = Vector3.one;
            return node.transform;
        }

        private static int StartUndo(string label)
        { Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName(label); return group; }

        private static void Validate(Interactable owner, Type type, Type contract)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (EditorApplication.isPlaying || PrefabUtility.IsPartOfPrefabAsset(owner))
                throw new InvalidOperationException("Edit hierarchy in the scene or Prefab Mode, outside Play Mode.");
            if (type == null || type.IsAbstract || type.ContainsGenericParameters
                || !typeof(MonoBehaviour).IsAssignableFrom(type) || !contract.IsAssignableFrom(type))
                throw new ArgumentException("Type must be a concrete interaction component.", nameof(type));
        }
    }
}
