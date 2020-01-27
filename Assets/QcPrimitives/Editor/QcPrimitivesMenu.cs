using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QuickPrimitives
{
    public class QcPrimitivesMenu : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/Quick Primitives/Box", false, 10)]
        private static void CreateBox(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject box = new GameObject("QcBoxMesh");
            box.AddComponent<QcBoxMesh>();
            box.name = "QcBox";
            box.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(box, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(box, "Create " + box.name);
            Selection.activeObject = box;
        }

        [MenuItem("GameObject/Quick Primitives/Cylinder", false, 10)]
        private static void CreateCylinder(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject cylinder = new GameObject("QuickCylinder");
            cylinder.AddComponent<QcCylinderMesh>();
            cylinder.name = "QcCylinder";
            cylinder.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(cylinder, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(cylinder, "Create " + cylinder.name);
            Selection.activeObject = cylinder;
        }

        [MenuItem("GameObject/Quick Primitives/Sphere", false, 10)]
        private static void CreateSphere(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject sphere = new GameObject("QcSphereMesh");
            sphere.AddComponent<QcSphereMesh>();
            sphere.name = "QcSphere";
            sphere.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(sphere, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(sphere, "Create " + sphere.name);
            Selection.activeObject = sphere;
        }

        [MenuItem("GameObject/Quick Primitives/Torus", false, 10)]
        private static void CreateTorus(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject torus = new GameObject("QcTorusMesh"); 
            torus.AddComponent<QcTorusMesh>();
            torus.name = "QcTorus";
            torus.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(torus, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(torus, "Create " + torus.name);
            Selection.activeObject = torus;
        }

        [MenuItem("GameObject/Quick Primitives/Pyramid", false, 10)]
        private static void CreatePyramid(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject pyramid = new GameObject("QcPyramidMesh");
            pyramid.AddComponent<QcPyramidMesh>();
            pyramid.name = "QcPyramid";
            pyramid.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(pyramid, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(pyramid, "Create " + pyramid.name);
            Selection.activeObject = pyramid;
        }

        [MenuItem("GameObject/Quick Primitives/Column", false, 10)]
        private static void CreateColumn(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject plane = new GameObject("QcColumnMesh");
            plane.AddComponent<QcColumnMesh>();
            plane.name = "QcColumn";
            plane.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(plane, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(plane, "Create " + plane.name);
            Selection.activeObject = plane;
        }

        [MenuItem("GameObject/Quick Primitives/Stair", false, 10)]
        private static void CreateStair(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject stair = new GameObject("QcStairMesh");
            stair.AddComponent<QcStairMesh>();
            stair.name = "QcStair";
            stair.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(stair, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(stair, "Create " + stair.name);
            Selection.activeObject = stair;
        }

        [MenuItem("GameObject/Quick Primitives/Section", false, 10)]
        private static void CreateBeam(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject section = new GameObject("QcSectionMesh");
            section.AddComponent<QcSectionMesh>();
            section.name = "QcSection";
            section.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(section, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(section, "Create " + section.name);
            Selection.activeObject = section;
        }


        [MenuItem("GameObject/Quick Primitives/Plane", false, 10)]
        private static void CreatePlane(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject plane = new GameObject("QcPlaneMesh");
            plane.AddComponent<QcPlaneMesh>();
            plane.name = "QcPlane";
            plane.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(plane, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(plane, "Create " + plane.name);
            Selection.activeObject = plane;
        }


        [MenuItem("GameObject/Quick Primitives/Circle", false, 10)]
        private static void CreateCircle(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject plane = new GameObject("QcCircleMesh");
            plane.AddComponent<QcCircleMesh>();
            plane.name = "QcCircle";
            plane.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(plane, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(plane, "Create " + plane.name);
            Selection.activeObject = plane;
        }

        [MenuItem("GameObject/Quick Primitives/Grid", false, 10)]
        private static void CreateFrame(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject grid = new GameObject("QcGridMesh");
            grid.AddComponent<QcGridMesh>();
            grid.name = "QcGrid";
            grid.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(grid, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(grid, "Create " + grid.name);
            Selection.activeObject = grid;
        }

#endif
    }
}
