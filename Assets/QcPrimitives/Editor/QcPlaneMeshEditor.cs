using UnityEngine;
using UnityEditor;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcPlaneMesh))]
public class QcPlaneMeshEditor : Editor
{
    private QcPlaneMesh.QcPlaneProperties oldProp = new QcPlaneMesh.QcPlaneProperties();

    override public void OnInspectorGUI()
    {
        QcPlaneMesh mesh = target as QcPlaneMesh;

        mesh.properties.width = EditorGUILayout.Slider("Width", mesh.properties.width, 0.1f, 10);
        mesh.properties.height = EditorGUILayout.Slider("Height", mesh.properties.height, 0.1f, 10);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);

        mesh.properties.widthSegments = EditorGUILayout.IntSlider("Width Segments", mesh.properties.widthSegments, 1, 20);
        mesh.properties.heightSegments = EditorGUILayout.IntSlider("Height Segments", mesh.properties.heightSegments, 1, 20);

        mesh.properties.doubleSided = EditorGUILayout.Toggle("Double Sided", mesh.properties.doubleSided);

        mesh.properties.direction = (QcPlaneMesh.QcPlaneProperties.FaceDirection)EditorGUILayout.EnumPopup("Direction", mesh.properties.direction);

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

    private void CheckValues(QcPlaneMesh planeMesh)
    {
    }

    private void ShowVertexCount(QcPlaneMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}
