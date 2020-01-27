using UnityEngine;
using UnityEditor;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcPyramidMesh))]
public class QcPyramidMeshEditor : Editor
{
    private QcPyramidMesh.QcPyramidProperties oldProp = new QcPyramidMesh.QcPyramidProperties();

    override public void OnInspectorGUI()
    {
        QcPyramidMesh mesh = target as QcPyramidMesh;

        mesh.properties.width = EditorGUILayout.Slider("Width", mesh.properties.width, 0.1f, 10);
        mesh.properties.depth = EditorGUILayout.Slider("Depth", mesh.properties.depth, 0.1f, 10);
        mesh.properties.height = EditorGUILayout.Slider("Height", mesh.properties.height, 0.1f, 10);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);

        mesh.properties.sides = EditorGUILayout.IntSlider("Sides", mesh.properties.sides, 3, 32);

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.sides != 3)))
        {
            if (group.visible == false)
            {
                mesh.properties.triangleIncline = EditorGUILayout.Slider("Triangle Incline", mesh.properties.triangleIncline, 0.0f, 1.0f);
            }
        }

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

    private void CheckValues(QcPyramidMesh pyramidMesh)
    {
    }

    private void ShowVertexCount(QcPyramidMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}
