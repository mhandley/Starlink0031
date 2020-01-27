using UnityEngine;
using UnityEditor;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcCircleMesh))]
public class QcCircleMeshEditor : Editor
{
    private QcCircleMesh.QcCircleProperties oldProp = new QcCircleMesh.QcCircleProperties();

    override public void OnInspectorGUI()
    {
        QcCircleMesh mesh = target as QcCircleMesh;

        mesh.properties.radius = EditorGUILayout.Slider("Radius", mesh.properties.radius, 0.1f, 10);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);

        mesh.properties.segments = EditorGUILayout.IntSlider("Segments", mesh.properties.segments, 3, 64);

        mesh.properties.doubleSided = EditorGUILayout.Toggle("Double Sided", mesh.properties.doubleSided);

        mesh.properties.direction = (QcCircleMesh.QcCircleProperties.FaceDirection)EditorGUILayout.EnumPopup("Direction", mesh.properties.direction);

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

    private void CheckValues(QcCircleMesh planeMesh)
    {
    }

    private void ShowVertexCount(QcCircleMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}