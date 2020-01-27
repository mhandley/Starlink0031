using UnityEngine;
using UnityEditor;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcTorusMesh))]
public class QcTorusMeshEditor : Editor
{
    private QcTorusMesh.QcTorusProperties oldProp = new QcTorusMesh.QcTorusProperties();

    override public void OnInspectorGUI()
    {
        QcTorusMesh mesh = target as QcTorusMesh;

        mesh.properties.radius = EditorGUILayout.Slider("Radius", mesh.properties.radius, 0.1f, 20);

        mesh.properties.ringRadius = EditorGUILayout.Slider("Ring Radius", mesh.properties.ringRadius, 0.01f, 20);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);

        mesh.properties.sliceOn = EditorGUILayout.Toggle("Slice On", mesh.properties.sliceOn);
        mesh.properties.sliceFrom = EditorGUILayout.Slider("Slice From", mesh.properties.sliceFrom, 0.0f, 360);
        mesh.properties.sliceTo = EditorGUILayout.Slider("Slice To", mesh.properties.sliceTo, 0.0f, 360);

        mesh.properties.torusSegments =
            EditorGUILayout.IntSlider("Segments", mesh.properties.torusSegments, 8, 128);

        mesh.properties.ringSegments =
            EditorGUILayout.IntSlider("Ring Segments", mesh.properties.ringSegments, 8, 128);

        mesh.properties.genTextureCoords = EditorGUILayout.Toggle("Gen Texture Coords", mesh.properties.genTextureCoords);
        mesh.properties.addCollider = EditorGUILayout.Toggle("Add Collider", mesh.properties.addCollider);

        ShowVertexCount(mesh);

        CheckValues(mesh);

        if (oldProp.Modified(mesh.properties))
        {
            mesh.RebuildGeometry();

            oldProp.CopyFrom(mesh.properties);
        }
    }

    private void CheckValues(QcTorusMesh torusMesh)
    {
    }

    private void ShowVertexCount(QcTorusMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}
