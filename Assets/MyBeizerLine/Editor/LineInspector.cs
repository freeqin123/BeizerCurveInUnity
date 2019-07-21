using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Lines))]
public class LineInspector : Editor
{
    private void OnSceneGUI()
    {
        Lines line = target as Lines;
        Transform handleTransform = line.transform;
        Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
        Vector3 p0 = handleTransform.TransformPoint(line.p0);
        Vector3 p1 = handleTransform.TransformPoint(line.p1);

        Handles.color = Color.blue;
        Handles.DrawLine(p0,p1);
        Handles.DoPositionHandle(p0,handleRotation);
        Handles.DoPositionHandle(p1,handleRotation);

        EditorGUI.BeginChangeCheck();
        p0 = Handles.DoPositionHandle(p0,handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(line,"Move Point");
            EditorUtility.SetDirty(line);
            line.p0 = handleTransform.InverseTransformPoint(p0);
        }
        EditorGUI.BeginChangeCheck();
        p1 = Handles.DoPositionHandle(p1,handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(line, "Move Point");
            EditorUtility.SetDirty(line);
            line.p1 = handleTransform.InverseTransformPoint(p1);
        }
    }
}
