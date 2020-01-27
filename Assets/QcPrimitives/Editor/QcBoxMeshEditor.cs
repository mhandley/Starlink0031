using UnityEngine;
using UnityEditor;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcBoxMesh))]
public class QcBoxMeshEditor : Editor
{
    private QcBoxMesh.QcBoxProperties oldProp = new QcBoxMesh.QcBoxProperties();

    override public void OnInspectorGUI()
    {
        QcBoxMesh mesh = target as QcBoxMesh;

        mesh.properties.width = EditorGUILayout.Slider("Width", mesh.properties.width, 0.01f, 10);
        mesh.properties.depth = EditorGUILayout.Slider("Depth", mesh.properties.depth, 0.01f, 10);
        mesh.properties.height = EditorGUILayout.Slider("Height", mesh.properties.height, 0.01f, 10);

        mesh.properties.widthSegments = EditorGUILayout.IntSlider("Width Segments", mesh.properties.widthSegments, 1, 20);
        mesh.properties.depthSegments = EditorGUILayout.IntSlider("Depth Segments", mesh.properties.depthSegments, 1, 20);
        mesh.properties.heightSegments = EditorGUILayout.IntSlider("Height Segments", mesh.properties.heightSegments, 1, 20);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);

        EditorGUILayout.Space();
        mesh.properties.option =
            (QcBoxMesh.QcBoxProperties.Options)EditorGUILayout.EnumPopup("Option", mesh.properties.option);

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.option !=
                                               QcBoxMesh.QcBoxProperties.Options.BeveledEdge)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                mesh.properties.beveledEdge.width =
                    EditorGUILayout.Slider("Width", mesh.properties.beveledEdge.width, 0.001f,
                                           mesh.properties.width < mesh.properties.depth ?
                                           mesh.properties.width * 0.5f : mesh.properties.depth * 0.5f);
                EditorGUI.indentLevel--;
            }
        }

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.option !=
                                               QcBoxMesh.QcBoxProperties.Options.Hollow)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                mesh.properties.hollow.thickness =
                    EditorGUILayout.Slider("Wall Depth", mesh.properties.hollow.thickness, 0.001f,
                                           mesh.properties.width < mesh.properties.depth ?
                                           mesh.properties.width * 0.5f : mesh.properties.depth * 0.5f);

                mesh.properties.hollow.height =
                    EditorGUILayout.Slider("Height", mesh.properties.hollow.height, 0.1f, mesh.properties.height);
                EditorGUI.indentLevel--;
            }
        }

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.option !=
                                               QcBoxMesh.QcBoxProperties.Options.SlantedSides)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                mesh.properties.slantedSides.size =
                    EditorGUILayout.Vector2Field("Size", mesh.properties.slantedSides.size);
                EditorGUI.indentLevel--;
            }
        }

        mesh.properties.genTextureCoords = EditorGUILayout.Toggle("Gen Texture Coords", mesh.properties.genTextureCoords);
        using (new EditorGUI.DisabledScope(!mesh.properties.genTextureCoords))
        {
            mesh.properties.textureWrapped = EditorGUILayout.Toggle("Wrap Texture", mesh.properties.textureWrapped);
        }
        
        mesh.properties.addCollider = EditorGUILayout.Toggle("Add Collider", mesh.properties.addCollider);

        ShowVertexCount(mesh);

        CheckValues(mesh);

        if (oldProp.Modified(mesh.properties))
        {
            mesh.RebuildGeometry();
            oldProp.CopyFrom(mesh.properties);
        }

        if (oldProp.textureWrapped != mesh.properties.textureWrapped)
        {
            mesh.ReassignMaterial();
            oldProp.textureWrapped = mesh.properties.textureWrapped;
        }
    }

    private void CheckValues(QcBoxMesh boxMesh)
    {
        if (boxMesh.properties.beveledEdge.width < 0) boxMesh.properties.beveledEdge.width = 0;
        if (boxMesh.properties.slantedSides.size[0] < 0) boxMesh.properties.slantedSides.size[0] = 0;
        if (boxMesh.properties.slantedSides.size[1] < 0) boxMesh.properties.slantedSides.size[1] = 0;
        if (boxMesh.properties.hollow.thickness < 0) boxMesh.properties.hollow.thickness = 0;
        if (boxMesh.properties.hollow.height < 0) boxMesh.properties.hollow.height = 0;

        if (boxMesh.properties.beveledEdge.width >= boxMesh.properties.width * 0.5f) boxMesh.properties.beveledEdge.width = 0;
        if (boxMesh.properties.beveledEdge.width >= boxMesh.properties.depth * 0.5f) boxMesh.properties.beveledEdge.width = 0;
        if (boxMesh.properties.beveledEdge.width >= boxMesh.properties.height * 0.5f) boxMesh.properties.beveledEdge.width = 0;
        if (boxMesh.properties.slantedSides.size[0] >= boxMesh.properties.width * 0.5f) boxMesh.properties.slantedSides.size[0] = boxMesh.properties.width * 0.5f - 0.001f;
        if (boxMesh.properties.slantedSides.size[1] >= boxMesh.properties.height * 0.5f) boxMesh.properties.slantedSides.size[1] = boxMesh.properties.height * 0.5f - 0.001f;
        if (boxMesh.properties.hollow.thickness >= boxMesh.properties.depth * 0.5f) boxMesh.properties.hollow.thickness = boxMesh.properties.depth * 0.5f;
        if (boxMesh.properties.hollow.thickness >= boxMesh.properties.width * 0.5f) boxMesh.properties.hollow.thickness = boxMesh.properties.width * 0.5f;
        if (boxMesh.properties.hollow.height > boxMesh.properties.height) boxMesh.properties.hollow.height = boxMesh.properties.height;
    }
    private void ShowVertexCount(QcBoxMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}