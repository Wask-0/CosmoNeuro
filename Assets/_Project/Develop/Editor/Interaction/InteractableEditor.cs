using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Develop.Runtime.Gameplay.Interaction.Actions;
using _Project.Develop.Runtime.Gameplay.Interaction.Core;
using _Project.Develop.Runtime.Gameplay.Interaction.Requirements;
using UnityEditor;
using UnityEngine;

namespace _Project.Develop.Editor.Interaction
{
    [CustomEditor(typeof(Interactable), true)]
    public sealed class InteractableEditor : UnityEditor.Editor
    {
        private readonly Dictionary<UnityEngine.Object, bool> _expanded = new();
        private bool _advanced;
        private Interactable Owner => (Interactable)target;
        private bool CanEditStructure => !EditorApplication.isPlaying && !PrefabUtility.IsPartOfPrefabAsset(Owner);

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                DrawPropertiesExcluding(serializedObject, "m_Script", "_successActions", "_failedActions");
                serializedObject.ApplyModifiedProperties();
                if (PrefabUtility.IsPartOfPrefabAsset(Owner))
                {
                    EditorGUILayout.HelpBox("Для добавления и удаления actions откройте префаб в Prefab Mode.", MessageType.Info);
                    if (GUILayout.Button("Открыть префаб")) AssetDatabase.OpenAsset(Owner.gameObject);
                }
                using (new EditorGUI.DisabledScope(!CanEditStructure))
                {
                    var pointProperty = serializedObject.FindProperty("_interactionPoint");
                    if (pointProperty != null && pointProperty.objectReferenceValue == null
                        && GUILayout.Button("Создать точку взаимодействия"))
                        InteractionAuthoring.CreateInteractionPoint(Owner);
                    if (Owner.GetComponentsInChildren<Collider2D>(true).Length == 0)
                    {
                        EditorGUILayout.HelpBox("Для поиска в радиусе нужен Collider2D на объекте или его дочернем объекте.", MessageType.Info);
                        if (GUILayout.Button("Добавить Collider2D для взаимодействия"))
                        {
                            var collider = Undo.AddComponent<BoxCollider2D>(Owner.gameObject);
                            collider.isTrigger = true;
                        }
                    }
                }

                DrawRequirements();
                DrawBranch("При успешном взаимодействии", "_successActions");
                DrawBranch("Если условия не выполнены", "_failedActions");

                _advanced = EditorGUILayout.Foldout(_advanced, "Дополнительно: корни цепочек", true);
                if (_advanced)
                {
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_successActions"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_failedActions"));
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.HelpBox("Actions выполняются последовательно по иерархии. Parallel Interaction Action запускает перечисленные в нём действия одновременно.", MessageType.Info);
                }
            }
        }



        private void DrawRequirements()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Условия взаимодействия", EditorStyles.boldLabel);
            var requirements = Owner.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(component => component is IInteractionRequirement).ToArray();
            if (requirements.Length == 0) EditorGUILayout.LabelField("Без дополнительных условий", EditorStyles.miniLabel);
            foreach (MonoBehaviour requirement in requirements)
                DrawCard(requirement, null, null, new HashSet<MonoBehaviour>(), false);
            AddButton("Добавить условие…", typeof(IInteractionRequirement), type => InteractionAuthoring.AddRequirement(Owner, type));
        }

        private void DrawBranch(string label, string field)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            serializedObject.Update();
            var root = serializedObject.FindProperty(field).objectReferenceValue as Transform;
            if (root != null)
            {
                if (root != Owner.transform && !root.IsChildOf(Owner.transform))
                    EditorGUILayout.HelpBox("Корень цепочки находится вне этого объекта. Ниже показаны его общие настройки.", MessageType.Warning);
                var actions = root.GetComponentsInChildren<MonoBehaviour>(true)
                    .Where(component => component is IInteractionAction).ToArray();
                var referenced = new HashSet<MonoBehaviour>();
                foreach (var group in actions.OfType<ParallelInteractionAction>())
                    CollectMembers(group, referenced, new HashSet<ParallelInteractionAction>());
                foreach (MonoBehaviour action in actions)
                    if (!referenced.Contains(action)) DrawCard(action, root, field, new HashSet<MonoBehaviour>(), true);
                if (actions.Length == 0) EditorGUILayout.LabelField("Нет действий", EditorStyles.miniLabel);
                if (actions.Length > 0 && actions.All(referenced.Contains))
                    EditorGUILayout.HelpBox("Все действия ссылаются друг на друга: проверьте циклы в Parallel-группах.", MessageType.Error);
            }
            else EditorGUILayout.LabelField("Нет действий", EditorStyles.miniLabel);
            AddButton("Добавить action…", typeof(IInteractionAction), type => InteractionAuthoring.AddAction(Owner, field, type));
        }

        private void DrawCard(MonoBehaviour component, Transform root, string branch, HashSet<MonoBehaviour> path, bool action)
        {
            if (component == null) return;
            if (!path.Add(component))
            { EditorGUILayout.HelpBox("Циклическая ссылка на action: " + component.name, MessageType.Error); return; }
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            bool open = Foldout(component, ObjectNames.NicifyVariableName(component.GetType().Name));
            bool own = component.transform == Owner.transform || component.transform.IsChildOf(Owner.transform);
            using (new EditorGUI.DisabledScope(!CanEditStructure || !own))
            {
                if (action)
                {
                    using (new EditorGUI.DisabledScope(!InteractionAuthoring.CanMove(component, root)))
                    {
                        if (GUILayout.Button("↑", GUILayout.Width(24))) { InteractionAuthoring.Move(component, -1); GUIUtility.ExitGUI(); }
                        if (GUILayout.Button("↓", GUILayout.Width(24))) { InteractionAuthoring.Move(component, 1); GUIUtility.ExitGUI(); }
                    }
                }
                if (GUILayout.Button("×", GUILayout.Width(24)))
                { InteractionAuthoring.Remove(Owner, component); GUIUtility.ExitGUI(); }
            }
            EditorGUILayout.EndHorizontal();
            if (open)
            {
                if (!own) EditorGUILayout.HelpBox("Action с другого объекта: изменения применяются к исходному компоненту.", MessageType.Info);
                DrawFields(component);
                if (component is ParallelInteractionAction group)
                {
                    var data = new SerializedObject(group);
                    var members = data.FindProperty("_actions");
                    for (int i = 0; i < members.arraySize; i++)
                    {
                        var member = members.GetArrayElementAtIndex(i).objectReferenceValue as MonoBehaviour;
                        if (member == null || member is not IInteractionAction)
                            EditorGUILayout.HelpBox("В Parallel-группе есть пустая ссылка или компонент без IInteractionAction.", MessageType.Warning);
                        else DrawCard(member, null, branch, path, true);
                    }
                    using (new EditorGUI.DisabledScope(!own))
                        AddButton("Добавить в параллельную группу…", typeof(IInteractionAction),
                            type => InteractionAuthoring.AddAction(Owner, branch, type, group));
                }
                
            }
            EditorGUILayout.EndVertical();
            path.Remove(component);
        }



        private bool Foldout(UnityEngine.Object value, string label)
        {
            bool open = !_expanded.TryGetValue(value, out bool stored) || stored;
            open = EditorGUILayout.Foldout(open, label, true);
            _expanded[value] = open;
            return open;
        }

        private static void DrawFields(UnityEngine.Object value)
        {
            var data = new SerializedObject(value);
            data.Update();
            DrawPropertiesExcluding(data, "m_Script");
            data.ApplyModifiedProperties();
        }

        private void AddButton(string label, Type contract, Action<Type> selected)
        {
            using (new EditorGUI.DisabledScope(!CanEditStructure))
                if (GUILayout.Button(label))
                    new InteractionComponentDropdown(contract, type =>
                    {
                        if (target == null) return;
                        selected(type); serializedObject.Update(); Repaint();
                    }).Show(GUILayoutUtility.GetLastRect());
        }

        private static void CollectMembers(ParallelInteractionAction group, HashSet<MonoBehaviour> members, HashSet<ParallelInteractionAction> visited)
        {
            if (!visited.Add(group)) return;
            var data = new SerializedObject(group);
            var list = data.FindProperty("_actions");
            for (int i = 0; i < list.arraySize; i++)
            {
                var member = list.GetArrayElementAtIndex(i).objectReferenceValue as MonoBehaviour;
                if (member == null) continue;
                members.Add(member);
                if (member is ParallelInteractionAction nested) CollectMembers(nested, members, visited);
            }
        }
    }
}
