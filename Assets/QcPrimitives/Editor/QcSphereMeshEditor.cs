using UnityEngine;
using UnityEditor;
using System;
using QuickPrimitives;

[CustomEditor(typeof(QcSphereMesh))]
public class QcSphereMeshEditor : Editor
{
    private QcSphereMesh.QcSphereProperties oldProp = new QcSphereMesh.QcSphereProperties();

    override public void OnInspectorGUI()
    {
        QcSphereMesh mesh = target as QcSphereMesh;

        mesh.properties.radius = EditorGUILayout.Slider("Radius", mesh.properties.radius, 0.1f, 20);

        mesh.properties.offset =
                    EditorGUILayout.Vector3Field("Offset", mesh.properties.offset);

        EditorGUILayout.Space();
        mesh.properties.meshGenMethod =
            (QcSphereMesh.QcSphereProperties.MeshGenMethod)EditorGUILayout.EnumPopup("Mesh Gen Method", mesh.properties.meshGenMethod);

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.meshGenMethod !=
                                               QcSphereMesh.QcSphereProperties.MeshGenMethod.Icosphere)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                mesh.properties.icosphere.subdivisions =
                    EditorGUILayout.IntSlider("Subdivisions", mesh.properties.icosphere.subdivisions, 0, 5);
                EditorGUI.indentLevel--;
            }
        }

        using (var group =
            new EditorGUILayout.FadeGroupScope(Convert.ToSingle(mesh.properties.meshGenMethod !=
                                               QcSphereMesh.QcSphereProperties.MeshGenMethod.UVSphere)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                mesh.properties.uvSphere.segments =
                    EditorGUILayout.IntSlider("Segments", mesh.properties.uvSphere.segments, 1, 64);

                mesh.properties.uvSphere.hemisphere =
                    EditorGUILayout.Slider("Hemisphere", mesh.properties.uvSphere.hemisphere, 0.0f, 0.9f);

                mesh.properties.uvSphere.sliceOn = EditorGUILayout.Toggle("Slice On", mesh.properties.uvSphere.sliceOn);
                using (new EditorGUI.DisabledScope(!mesh.properties.uvSphere.sliceOn))
                {
                    EditorGUI.indentLevel++;
                    mesh.properties.uvSphere.sliceFrom = EditorGUILayout.Slider("Slice From", mesh.properties.uvSphere.sliceFrom, 0, 360);
                    mesh.properties.uvSphere.sliceTo = EditorGUILayout.Slider("Slice To", mesh.properties.uvSphere.sliceTo, 0, 360);
                    EditorGUI.indentLevel--;
                }
                
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

    private void CheckValues(QcSphereMesh sphereMesh)
    {
    }

    private void ShowVertexCount(QcSphereMesh mesh)
    {
        EditorGUILayout.HelpBox(mesh.vertices.Count + " vertices\r\n" + mesh.faces.Count + " triangles", MessageType.Info);
    }
}
