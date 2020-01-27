using UnityEditor;
using UnityEngine;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcStairMesh))]
public class QcStairMeshEditor : Editor
{
    private QcStairMesh.QcStairProperties oldProp = new QcStairMesh.QcStairProperties();

    override public void OnInspectorGUI()
    {
        QcStairMesh mesh = target as QcStairMesh;

        mesh.properties.spiral = EditorGUILayout.ToggleLeft("Spiral Stair", mesh.properties.spiral);
        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(!mesh.properties.spiral)))
        {
            if (group.visible == false)
            {
                mesh.properties.height = EditorGUILayout.Slider("Height", mesh.properties.height, 0.1f, 20);
                float minRadius = 0.0f;
                if (mesh.properties.conical) minRadius = 0.1f;
                mesh.properties.innerRadius = EditorGUILayout.Slider("Radius", mesh.properties.innerRadius, minRadius, 10.0f);
                
                mesh.properties.conical = EditorGUILayout.ToggleLeft("Conical", mesh.properties.conical);
                using (new EditorGUI.DisabledScope(!mesh.properties.conical))
                {
                    EditorGUI.indentLevel++;
                    mesh.properties.radius = EditorGUILayout.Slider("Top Radius", mesh.properties.radius, 0.1f, 10.0f); 
                    EditorGUI.indentLevel--;
                }

                if ((mesh.properties.type != QcStairMesh.QcStairProperties.Types.Box) || mesh.properties.conical)
                    mesh.properties.rotations = EditorGUILayout.Slider("Turns", mesh.properties.rotations, 0.1f, 30f);
                else
                    mesh.properties.rotations = EditorGUILayout.Slider("Turns", mesh.properties.rotations, 0.1f, 1f);
                mesh.properties.windingDirection = 
                    (QcStairMesh.QcStairProperties.WindingDirection)EditorGUILayout.EnumPopup("Winding Direction", mesh.properties.windingDirection);
               
            }
            else
            {
                mesh.properties.depth = EditorGUILayout.Slider("Depth", mesh.properties.depth, 0.01f, 20);
                mesh.properties.height = EditorGUILayout.Slider("Height", mesh.properties.height, 0.1f, 20);
            }
        }

        mesh.properties.type =
                   (QcStairMesh.QcStairProperties.Types)EditorGUILayout.EnumPopup("Type", mesh.properties.type);

        EditorGUI.indentLevel++;
        mesh.properties.width = EditorGUILayout.Slider("Step Width", mesh.properties.width, 0.1f, 10);
        EditorGUI.indentLevel--;

        using (var group2 =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.type !=
                                               QcStairMesh.QcStairProperties.Types.Open)))
        {
            if (group2.visible == false)
            {
                float stepHeight = mesh.properties.height / mesh.properties.steps;
                float stepDepth;
                if (!mesh.properties.spiral)
                    stepDepth = mesh.properties.depth / mesh.properties.steps;
                else
                    stepDepth = (mesh.properties.innerRadius + mesh.properties.width) * (float)Math.PI / mesh.properties.steps * mesh.properties.rotations;

                EditorGUI.indentLevel++;
                mesh.properties.treadDepth =
                    EditorGUILayout.Slider("Step Depth", mesh.properties.treadDepth, 0.01f, stepDepth * 4);
                mesh.properties.treadThickness =
                    EditorGUILayout.Slider("Step Thickness", mesh.properties.treadThickness, 0.01f, stepHeight * 2);
                EditorGUI.indentLevel--;
            }
        }

        mesh.properties.steps = EditorGUILayout.IntField("Number of Steps", mesh.properties.steps);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);
        
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

    private void CheckValues(QcStairMesh mesh)
    {
        if (!mesh.properties.spiral)
        {
            if (mesh.properties.steps < 2) mesh.properties.steps = 2;
        }
        else
        {
            if (mesh.properties.steps < 4) mesh.properties.steps = 4;
        }
    }

    private void ShowVertexCount(QcStairMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}