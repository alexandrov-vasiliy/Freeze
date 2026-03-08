using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterChaseRetreat))]
public class MonsterChaseRetreatEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying)
            return;

        MonsterChaseRetreat monster = (MonsterChaseRetreat)target;
        if (monster == null)
            return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug (Play Mode)", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.EnumPopup("Состояние", monster.CurrentState);

        Vector3? targetPosition = monster.GetDebugTargetPosition();
        if (targetPosition.HasValue)
        {
            EditorGUILayout.Vector3Field("Куда идёт", targetPosition.Value);
            EditorGUILayout.FloatField("Дистанция до цели", Vector3.Distance(monster.transform.position, targetPosition.Value));
        }
        else
        {
            EditorGUILayout.LabelField("Куда идёт", "—");
        }

        EditorGUILayout.IntField("Индекс точки отхода", monster.DebugRetreatPointIndex);
        EditorGUILayout.FloatField("Таймер неподвижности (сек)", monster.DebugStillTimer);
        EditorGUILayout.Toggle("Отход запрошен", monster.DebugRetreatRequested);

        EditorGUI.EndDisabledGroup();
    }

    private void OnSceneGUI()
    {
        MonsterChaseRetreat monster = (MonsterChaseRetreat)target;
        if (monster == null || !Application.isPlaying)
            return;

        Vector3? targetPosition = monster.GetDebugTargetPosition();
        if (!targetPosition.HasValue)
            return;

        Vector3 from = monster.transform.position;
        Vector3 to = targetPosition.Value;

        Handles.color = monster.CurrentState == MonsterChaseRetreat.MonsterState.Chase ? Color.red : Color.yellow;
        Handles.DrawLine(from, to);
        Handles.Label(to + Vector3.up * 0.5f, monster.CurrentState == MonsterChaseRetreat.MonsterState.Chase ? "Игрок" : "Точка отхода");
    }
}
