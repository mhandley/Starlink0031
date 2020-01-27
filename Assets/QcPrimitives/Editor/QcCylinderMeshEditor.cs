using UnityEngine;
using UnityEditor;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcCylinderMesh))]
public class QcCylinderMeshEditor : Editor
{
    private QcCylinderMesh.QcCylinderProperties oldProp = new QcCylinderMesh.QcCylinderProperties();

    override public void OnInspectorGUI()
    {
        QcCylinderMesh mesh = target as QcCylinderMesh;

        mesh.properties.radius = EditorGUILayout.Slider("Radius", mesh.properties.radius, 0.01f, 10);
        mesh.properties.topRadius = EditorGUILayout.Slider("Top Radius", mesh.properties.topRadius, 0.0f, 10);
        mesh.properties.height = EditorGUILayout.Slider("Height", mesh.properties.height, 0.1f, 10);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);

        mesh.properties.sides =
                    EditorGUILayout.IntSlider("Sides", mesh.properties.sides, 8, 64);

        mesh.properties.sliceOn = EditorGUILayout.Toggle("Slice On", mesh.properties.sliceOn);
        mesh.properties.sliceFrom = EditorGUILayout.Slider("Slice From", mesh.properties.sliceFrom, 0.0f, 360);
        mesh.properties.sliceTo = EditorGUILayout.Slider("Slice To", mesh.properties.sliceTo, 0.0f, 360);


        EditorGUILayout.Space();
        mesh.properties.option =
            (QcCylinderMesh.QcCylinderProperties.Options)EditorGUILayout.EnumPopup("Option", mesh.properties.option);

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.option !=
                                               QcCylinderMesh.QcCylinderProperties.Options.BeveledEdge)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                mesh.properties.beveledEdge.width =
                    EditorGUILayout.Slider("Width", mesh.properties.beveledEdge.width, 0.001f,
                    (mesh.properties.height * 0.5f < mesh.properties.radius) ? mesh.properties.height * 0.5f : mesh.properties.radius);
                EditorGUI.indentLevel--;
            }
        }

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.option !=
                                               QcCylinderMesh.QcCylinderProperties.Options.Hollow)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                mesh.properties.hollow.thickness =
                    EditorGUILayout.Slider("Thickness", mesh.properties.hollow.thickness, 0.001f, mesh.properties.radius);

                mesh.properties.hollow.height =
                    EditorGUILayout.Slider("Height", mesh.properties.hollow.height, 0.1f, mesh.properties.height);
                EditorGUI.indentLevel--;
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

    private void CheckValues(QcCylinderMesh cylinderMesh)
    {
        if (cylinderMesh.properties.beveledEdge.width < 0) cylinderMesh.properties.beveledEdge.width = 0;
        if (cylinderMesh.properties.hollow.thickness < 0) cylinderMesh.properties.hollow.thickness = 0;
        if (cylinderMesh.properties.hollow.height < 0) cylinderMesh.properties.hollow.height = 0;

        if (cylinderMesh.properties.beveledEdge.width >= cylinderMesh.properties.radius) cylinderMesh.properties.beveledEdge.width = 0;
        if (cylinderMesh.properties.beveledEdge.width >= cylinderMesh.properties.height * 0.5f) cylinderMesh.properties.beveledEdge.width = 0;
        if (cylinderMesh.properties.hollow.thickness >= cylinderMesh.properties.radius) cylinderMesh.properties.hollow.thickness = cylinderMesh.properties.radius - 0.001f;
        if (cylinderMesh.properties.hollow.height > cylinderMesh.properties.height) cylinderMesh.properties.hollow.height = cylinderMesh.properties.height;
        //if (cylinderMesh.properties.sliceFrom > cylinderMesh.properties.sliceTo) cylinderMesh.properties.sliceFrom = cylinderMesh.properties.sliceTo;
    }

    private void ShowVertexCount(QcCylinderMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}
