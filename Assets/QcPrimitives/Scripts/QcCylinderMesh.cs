using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcCylinderMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcCylinderProperties : QcBaseProperties
        {
            [System.Serializable]
            public class BeveledEdge
            {
                public float width;
            }

            [System.Serializable]
            public class Hollow
            {
                public float thickness;
                public float height;
            }

            public enum Options { None, BeveledEdge, Hollow }
            
            public float radius = 0.5f;
            public float topRadius = 0.5f;
            public float height = 1;

            public int sides = 16;

            public bool sliceOn = false;
            public float sliceFrom = 0.0f;
            public float sliceTo = 0.0f;

            public Options option = Options.None;
            public BeveledEdge beveledEdge = new BeveledEdge();
            public Hollow hollow = new Hollow();

            public void CopyFrom(QcCylinderProperties source)
            {
                base.CopyFrom(source);

                this.radius = source.radius;
                this.topRadius = source.topRadius;
                this.height = source.height;
                this.sides = source.sides;

                this.sliceOn = source.sliceOn;
                this.sliceFrom = source.sliceFrom;
                this.sliceTo = source.sliceTo;

                this.beveledEdge.width = source.beveledEdge.width;              

                this.hollow.thickness = source.hollow.thickness;
                this.hollow.height = source.hollow.height;

                this.option = source.option;
            }

            public bool Modified(QcCylinderProperties source)
            { 
                return (this.radius != source.radius) || (this.height != source.height) || (this.sides != source.sides) ||
                       (this.topRadius != source.topRadius) ||
                       (this.sliceOn != source.sliceOn) ||
                       (this.sliceOn && ((this.sliceFrom != source.sliceFrom) || (this.sliceTo != source.sliceTo))) ||
                       (this.option != source.option) ||
                       ((source.option == QcCylinderProperties.Options.BeveledEdge) && (this.beveledEdge.width != source.beveledEdge.width)) ||
                       ((source.option == QcCylinderProperties.Options.Hollow) && ((this.hollow.thickness == source.hollow.thickness) || (this.hollow.height == source.hollow.height))) ||
                       (this.offset[0] != source.offset[0]) && (this.offset[1] != source.offset[1]) && (this.offset[2] != source.offset[2]) ||
                       (this.genTextureCoords != source.genTextureCoords) ||
                       (this.addCollider != source.addCollider) ||
                       (this.option != source.option);
            }
        }

        public QcCylinderProperties properties = new QcCylinderProperties();

        private float[] xc;
        private float[] yc;

        public void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            BuildGeometry();
        }

        #region BuildGeometry
        protected override void BuildGeometry()
        {
            if ((properties.radius <= 0) || (properties.height <= 0)) return;

            xc = new float[properties.sides + 1];
            yc = new float[properties.sides + 1];

            bool sideCap = (properties.sliceOn) &&
                (properties.sliceFrom != properties.sliceTo) &&
                (Mathf.Abs(properties.sliceFrom - properties.sliceTo) < 360);
            if (!sideCap)
            {
                float partAngle = (2f * Mathf.PI) / properties.sides;
                for (int i = 0; i <= properties.sides; ++i)
                {
                    float angle = i * partAngle;
                    xc[i] = Mathf.Cos(angle);
                    yc[i] = Mathf.Sin(angle);
                }
            }
            else
            {
                float startAngle = 0;
                float endAngle = 0;

                float sliceTo = properties.sliceTo;
                float sliceFrom = properties.sliceFrom;
                if (sliceFrom > sliceTo)
                {
                    sliceTo += 360;
                }
                startAngle = sliceFrom * Mathf.Deg2Rad;
                endAngle = sliceTo * Mathf.Deg2Rad;

                float partAngle = (endAngle - startAngle) / properties.sides;
                for (int i = 0; i <= properties.sides; ++i)
                {
                    float angle = startAngle + i * partAngle;
                    xc[i] = Mathf.Cos(angle);
                    yc[i] = Mathf.Sin(angle);
                }
            }

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            ClearVertices();

            if ((properties.option == QcCylinderProperties.Options.Hollow) &&
                (properties.hollow.thickness > 0) && (properties.hollow.height > 0))
            {
                GenerateCylinderVerticesHollowed(properties.height, properties.sides, xc, yc, sideCap);
                GenerateCylinderTrianglesHollowed(properties.height, properties.sides, sideCap);
            }
            else if ((properties.option == QcCylinderProperties.Options.BeveledEdge) &&
                     (properties.beveledEdge.width > 0))
            {
                GenerateCylinderVerticesBeveled(properties.height, properties.sides, xc, yc, sideCap);
                GenerateCylinderTrianglesBeveled(properties.height, properties.sides, sideCap);
            }
            else
            {
                if (properties.radius == properties.topRadius)
                {
                    GenerateCylinderVertices(properties.height, properties.sides, xc, yc, sideCap);
                    GenerateCylinderTriangles(properties.height, properties.sides, sideCap);
                }
                else
                {
                    GenerateConeVertices(properties.height, properties.sides, xc, yc, sideCap);
                    GenerateConeTriangles(properties.height, properties.sides, sideCap);
                }
            }

            if (properties.offset != Vector3.zero)
            {
                AddOffset(properties.offset);
            }

            int[] triangles = new int[faces.Count * 3];
            int index = 0;
            foreach (var tri in faces)
            {
                triangles[index] = tri.v1;
                triangles[index + 1] = tri.v2;
                triangles[index + 2] = tri.v3;

                index += 3;
            }

            Mesh mesh = new Mesh();

            meshFilter.sharedMesh = mesh;

            // Assign verts, norms, uvs and tris to mesh and calc normals
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            if (properties.genTextureCoords)
                mesh.uv = uvs.ToArray();
            else
                mesh.uv = null;

            mesh.triangles = triangles;

            SetCollider();

            mesh.RecalculateBounds();
        }
        #endregion

        #region RebuildGeometry
        public override void RebuildGeometry()
        {
            xc = new float[properties.sides + 1];
            yc = new float[properties.sides + 1];

            bool sideCap = (properties.sliceOn) &&
                (properties.sliceFrom != properties.sliceTo) &&
                (Mathf.Abs(properties.sliceFrom - properties.sliceTo) < 360);
            if (!sideCap)
            {
                float partAngle = (2f * Mathf.PI) / properties.sides;
                for (int i = 0; i <= properties.sides; ++i)
                {
                    float angle = i * partAngle;
                    xc[i] = Mathf.Cos(angle);
                    yc[i] = Mathf.Sin(angle);
                }
            }
            else
            {
                float startAngle = 0;
                float endAngle = 0;

                float sliceTo = properties.sliceTo;
                float sliceFrom = properties.sliceFrom;
                if (sliceFrom > sliceTo)
                {
                    sliceTo += 360;
                }
                startAngle = sliceFrom * Mathf.Deg2Rad;
                endAngle = sliceTo * Mathf.Deg2Rad;

                float partAngle = (endAngle - startAngle) / properties.sides;
                for (int i = 0; i <= properties.sides; ++i)
                {
                    float angle = startAngle + i * partAngle;
                    xc[i] = Mathf.Cos(angle);
                    yc[i] = Mathf.Sin(angle);
                }
            }

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            ClearVertices();

            if ((properties.option == QcCylinderProperties.Options.Hollow) && 
                (properties.hollow.thickness > 0) && (properties.hollow.height > 0))
            {
                GenerateCylinderVerticesHollowed(properties.height, properties.sides, xc, yc, sideCap);
                GenerateCylinderTrianglesHollowed(properties.height, properties.sides, sideCap);
            }
            else if ((properties.option == QcCylinderProperties.Options.BeveledEdge) && 
                     (properties.beveledEdge.width > 0))
            {
                GenerateCylinderVerticesBeveled(properties.height, properties.sides, xc, yc, sideCap);
                GenerateCylinderTrianglesBeveled(properties.height, properties.sides, sideCap);
            }
            else
            {
                if (properties.radius == properties.topRadius)
                {
                    GenerateCylinderVertices(properties.height, properties.sides, xc, yc, sideCap);
                    GenerateCylinderTriangles(properties.height, properties.sides, sideCap);
                }
                else
                {
                    GenerateConeVertices(properties.height, properties.sides, xc, yc, sideCap);
                    GenerateConeTriangles(properties.height, properties.sides, sideCap);
                }
            }

            if (properties.offset != Vector3.zero)
            {
                AddOffset(properties.offset);
            }

            int[] triangles = new int[faces.Count * 3];
            int index = 0;
            foreach (var tri in faces)
            {
                triangles[index] = tri.v1;
                triangles[index + 1] = tri.v2;
                triangles[index + 2] = tri.v3;

                index += 3;
            }

            // Assign verts, norms, uvs and tris to mesh and calc normals
            if (meshFilter.sharedMesh != null)
            {
                meshFilter.sharedMesh.Clear();

                meshFilter.sharedMesh.vertices = vertices.ToArray();
                meshFilter.sharedMesh.normals = normals.ToArray();
                if (properties.genTextureCoords)
                    meshFilter.sharedMesh.uv = uvs.ToArray();
                else
                    meshFilter.sharedMesh.uv = null;

                meshFilter.sharedMesh.triangles = triangles;

                SetCollider();

                meshFilter.sharedMesh.RecalculateBounds();
            }
        }
        #endregion

        private void SetCollider()
        {
            if (properties.addCollider)
            {
                // set collider bound
                CapsuleCollider collider = gameObject.GetComponent<CapsuleCollider>();
                if (collider == null)
                {
                    collider = gameObject.AddComponent<CapsuleCollider>();
                }

                collider.enabled = true;
                collider.center = properties.offset;
                collider.radius = properties.radius;
                collider.height = properties.height;
            }
            else
            {
                CapsuleCollider collider = gameObject.GetComponent<CapsuleCollider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        #region GenerateCylinderVertices
        private void GenerateCylinderVertices(float height, int numCircleSegments, float[] xc, float[] yc, bool sideCap)
        {
            Vector3 topNormal = Vector3.up;
            Vector3 bottomNormal = Vector3.down;

            AddVertex(new Vector3(0f, -0.5f * height, 0f));      // bottom center    
            AddVertex(new Vector3(0f, 0.5f * height, 0f));       // top center

            AddNormal(bottomNormal);
            AddNormal(topNormal);

            AddUV(new Vector2(0.5f, 0.5f));
            AddUV(new Vector2(0.5f, 0.5f));

            float[] x = new float[numCircleSegments + 1];
            float[] y = new float[numCircleSegments + 1];

            for (int i = 0; i <= numCircleSegments; ++i)
            {
                x[i] = xc[i] * properties.radius;
                y[i] = yc[i] * properties.radius;
            }

            for (int i = 0; i <= numCircleSegments; i++)
            {
                // for bottom face
                AddVertex(new Vector3(x[i], -0.5f * height, y[i]));

                // for top face
                AddVertex(new Vector3(x[i], 0.5f * height, y[i]));

                // for front faces
                AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                AddVertex(new Vector3(x[i], 0.5f * height, y[i]));

                AddNormal(bottomNormal);
                AddNormal(topNormal);

                Vector3 normal1 = new Vector3(x[i], 0f, y[i]);
                normal1.Normalize();
                AddNormal(normal1);
                AddNormal(normal1);

                AddUV(new Vector2((xc[numCircleSegments - i] + 1.0f) * 0.5f, (yc[numCircleSegments - i] + 1.0f) * 0.5f));
                AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));
                AddUV(new Vector2(2 * i / (float)numCircleSegments, 1));
            }

            if (sideCap)
            {
                AddVertex(new Vector3(x[0], -0.5f * height, y[0]));
                AddVertex(new Vector3(x[0], 0.5f * height, y[0]));

                AddVertex(new Vector3(0, -0.5f * height, 0));
                AddVertex(new Vector3(0, 0.5f * height, 0));

                Vector3 capNormal1 = ComputeNormal(new Vector3(x[0], -0.5f * height, y[0]), 
                                                   new Vector3(0, -0.5f * height, 0),
                                                   new Vector3(0, 0.5f * height, 0));

                AddNormal(capNormal1);
                AddNormal(capNormal1);
                AddNormal(capNormal1);
                AddNormal(capNormal1);

                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0.5f, 0));
                AddUV(new Vector2(0.5f, 1));

                AddVertex(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]));
                AddVertex(new Vector3(x[numCircleSegments], 0.5f * height, y[numCircleSegments]));

                AddVertex(new Vector3(0, -0.5f * height, 0));
                AddVertex(new Vector3(0, 0.5f * height, 0));

                Vector3 capNormal2 = ComputeNormal(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]),
                                                   new Vector3(0, 0.5f * height, 0),
                                                   new Vector3(0, -0.5f * height, 0));

                AddNormal(capNormal2);
                AddNormal(capNormal2);
                AddNormal(capNormal2);
                AddNormal(capNormal2);

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(0, 1));
                AddUV(new Vector2(0.5f, 0));
                AddUV(new Vector2(0.5f, 1));
            }
        }
        #endregion

        #region GenerateConeVertices
        private void GenerateConeVertices(float height, int numCircleSegments, float[] xc, float[] yc, bool sideCap)
        {
            float[] x = new float[numCircleSegments + 1];
            float[] y = new float[numCircleSegments + 1];

            for (int i = 0; i <= numCircleSegments; ++i)
            {
                x[i] = xc[i] * properties.radius;
                y[i] = yc[i] * properties.radius;
            }

            if (properties.topRadius == 0)
            {
                Vector3 bottomNormal = Vector3.down;

                AddVertex(new Vector3(0f, -0.5f * height, 0f));       // bottom center 

                AddNormal(bottomNormal);

                AddUV(new Vector2(0.5f, 0.5f));

                float coneAngle = Mathf.Atan(properties.radius / height);

                for (int i = 0; i <= numCircleSegments; i++)
                {
                    // for bottom face
                    AddVertex(new Vector3(x[i], -0.5f * height, y[i]));

                    // for front faces
                    AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                    AddVertex(new Vector3(0, 0.5f * height, 0));

                    AddNormal(bottomNormal);

                    Vector3 normal1 = new Vector3(xc[i] * Mathf.Cos(coneAngle), Mathf.Sin(coneAngle), yc[i] * Mathf.Cos(coneAngle));
                    normal1.Normalize();

                    AddNormal(normal1);
                    AddNormal(normal1);

                    AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));
                    AddUV(new Vector2((2 * i + 1) / (float)numCircleSegments, 1));
                }

                if (sideCap)
                {
                    AddVertex(new Vector3(x[0], -0.5f * height, y[0]));

                    AddVertex(new Vector3(0, -0.5f * height, 0));
                    AddVertex(new Vector3(0, 0.5f * height, 0));

                    Vector3 capNormal1 = ComputeNormal(new Vector3(x[0], -0.5f * height, y[0]),
                                                       new Vector3(0, -0.5f * height, 0),
                                                       new Vector3(0, 0.5f * height, 0));

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    AddUV(new Vector2(1, 0));
                    AddUV(new Vector2(0.5f, 0));
                    AddUV(new Vector2(0.5f, 1));

                    AddVertex(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]));

                    AddVertex(new Vector3(0, -0.5f * height, 0));
                    AddVertex(new Vector3(0, 0.5f * height, 0));

                    Vector3 capNormal2 = ComputeNormal(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]),
                                                       new Vector3(0, 0.5f * height, 0),
                                                       new Vector3(0, -0.5f * height, 0));

                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);

                    AddUV(new Vector2(0, 0));
                    AddUV(new Vector2(0.5f, 0));
                    AddUV(new Vector2(0.5f, 1));
                }
            }
            else
            {
                Vector3 topNormal = Vector3.up;
                Vector3 bottomNormal = Vector3.down;

                float[] x1 = new float[numCircleSegments + 1];      // top outside
                float[] y1 = new float[numCircleSegments + 1];

                float radius1 = properties.topRadius;
                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x1[i] = xc[i] * radius1;
                    y1[i] = yc[i] * radius1;
                }

                AddVertex(new Vector3(0f, -0.5f * height, 0f));       // top center    
                AddVertex(new Vector3(0f, 0.5f * height, 0f));   // bottom center

                AddNormal(bottomNormal);
                AddNormal(topNormal);

                AddUV(new Vector2(0.5f, 0.5f));
                AddUV(new Vector2(0.5f, 0.5f));

                for (int i = 0; i <= numCircleSegments; i++)
                {
                    // for bottom outer face
                    AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                    // for top face
                    AddVertex(new Vector3(x1[i], 0.5f * height, y1[i]));

                    // for outer faces
                    AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                    AddVertex(new Vector3(x1[i], 0.5f * height, y1[i]));

                    AddNormal(bottomNormal);
                    AddNormal(topNormal);

                    float coneAngle = Mathf.Atan((properties.radius - properties.topRadius) / height);
                    Vector3 normal1 = new Vector3(xc[i] * Mathf.Cos(coneAngle), Mathf.Sin(coneAngle), yc[i] * Mathf.Cos(coneAngle));
                    normal1.Normalize();
                    AddNormal(normal1);
                    AddNormal(normal1);

                    AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));
                    AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));
                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));
                }

                if (sideCap)
                {
                    AddVertex(new Vector3(x[0], -0.5f * height, y[0]));
                    AddVertex(new Vector3(x1[0], 0.5f * height, y1[0]));

                    AddVertex(new Vector3(0, -0.5f * height, 0));
                    AddVertex(new Vector3(0, 0.5f * height, 0));

                    Vector3 capNormal1 = ComputeNormal(new Vector3(x[0], -0.5f * height, y[0]),
                                                       new Vector3(0, -0.5f * height, 0),
                                                       new Vector3(0, 0.5f * height, 0));

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    AddUV(new Vector2(1, 0));
                    AddUV(new Vector2(1, 1));
                    AddUV(new Vector2(0.5f, 0));
                    AddUV(new Vector2(0.5f, 1));

                    AddVertex(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]));
                    AddVertex(new Vector3(x1[numCircleSegments], 0.5f * height, y1[numCircleSegments]));

                    AddVertex(new Vector3(0, -0.5f * height, 0));
                    AddVertex(new Vector3(0, 0.5f * height, 0));

                    Vector3 capNormal2 = ComputeNormal(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]),
                                                       new Vector3(0, 0.5f * height, 0),
                                                       new Vector3(0, -0.5f * height, 0));

                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);

                    AddUV(new Vector2(0, 0));
                    AddUV(new Vector2(0, 1));
                    AddUV(new Vector2(0.5f, 0));
                    AddUV(new Vector2(0.5f, 1));
                }
            }
        }
        #endregion

        #region GenerateCylinderVerticesBeveled
        private void GenerateCylinderVerticesBeveled(float height, int numCircleSegments,
                                                     float[] xc, float[] yc, bool sideCap)
        {
            Vector3 topNormal = Vector3.up;
            Vector3 bottomNormal = Vector3.down;

            AddVertex(new Vector3(0f, -0.5f * height, 0f));       // bottom center    
            AddVertex(new Vector3(0f, 0.5f * height, 0f));   // top center

            AddNormal(bottomNormal);
            AddNormal(topNormal);

            AddUV(new Vector2(0.5f, 0.5f));
            AddUV(new Vector2(0.5f, 0.5f));

            float[] x = new float[numCircleSegments + 1];
            float[] y = new float[numCircleSegments + 1];

            for (int i = 0; i <= numCircleSegments; ++i)
            {
                x[i] = xc[i] * properties.radius;
                y[i] = yc[i] * properties.radius;
            }

            float[] x0 = new float[numCircleSegments + 1];
            float[] y0 = new float[numCircleSegments + 1];

            float radius0 = properties.radius - properties.beveledEdge.width;

            for (int i = 0; i <= numCircleSegments; ++i)
            {
                x0[i] = xc[i] * radius0;
                y0[i] = yc[i] * radius0;
            }

            for (int i = 0; i <= numCircleSegments; i++)
            {
                // for bottom face
                AddVertex(new Vector3(x0[i], -0.5f * height, y0[i]));

                // for top face
                AddVertex(new Vector3(x0[i], 0.5f * height, y0[i]));

                // for front faces
                AddVertex(new Vector3(x[i], -0.5f * height + properties.beveledEdge.width, y[i]));
                AddVertex(new Vector3(x[i], 0.5f * height - properties.beveledEdge.width, y[i]));

                // for bottom bevel
                AddVertex(new Vector3(x0[i], -0.5f * height, y0[i]));
                AddVertex(new Vector3(x[i], -0.5f * height + properties.beveledEdge.width, y[i]));

                // for top bevel
                AddVertex(new Vector3(x[i], 0.5f * height - properties.beveledEdge.width, y[i]));
                AddVertex(new Vector3(x0[i], 0.5f * height, y0[i]));

                AddNormal(-topNormal);
                AddNormal(topNormal);

                Vector3 normal1 = new Vector3(x[i], 0f, y[i]);
                normal1.Normalize();
                AddNormal(normal1);
                AddNormal(normal1);

                Vector3 normalBottomBevel = new Vector3(x[i], -properties.radius, y[i]);
                normalBottomBevel.Normalize();
                AddNormal(normalBottomBevel);
                AddNormal(normalBottomBevel);

                Vector3 normalTopBevel = new Vector3(x[i], properties.radius, y[i]);
                normalTopBevel.Normalize();
                AddNormal(normalTopBevel);
                AddNormal(normalTopBevel);

                AddUV(new Vector2((xc[numCircleSegments - i] + 1.0f) * 0.5f, (yc[numCircleSegments - i] + 1.0f) * 0.5f));
                AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                AddUV(new Vector2(2 * i / (float)numCircleSegments, properties.beveledEdge.width / height));
                AddUV(new Vector2(2 * i / (float)numCircleSegments, 1 - properties.beveledEdge.width / height));

                AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));
                AddUV(new Vector2(2 * i / (float)numCircleSegments, properties.beveledEdge.width / height));

                AddUV(new Vector2(2 * i / (float)numCircleSegments, 1 - properties.beveledEdge.width / height));
                AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));
            }

            if (sideCap)
            {
                // for front faces
                AddVertex(new Vector3(x[0], -0.5f * height + properties.beveledEdge.width, y[0]));
                AddVertex(new Vector3(x[0], 0.5f * height - properties.beveledEdge.width, y[0]));

                // for bottom bevel
                AddVertex(new Vector3(x0[0], -0.5f * height, y0[0]));

                // for top bevel
                AddVertex(new Vector3(x0[0], 0.5f * height, y0[0]));

                AddVertex(new Vector3(0, -0.5f * height, 0));
                AddVertex(new Vector3(0, 0.5f * height, 0));

                Vector3 capNormal1 = ComputeNormal(new Vector3(x[0], -0.5f * height, y[0]),
                                                   new Vector3(0, -0.5f * height, 0),
                                                   new Vector3(0, 0.5f * height, 0));

                AddNormal(capNormal1);
                AddNormal(capNormal1);
                AddNormal(capNormal1);
                AddNormal(capNormal1);
                AddNormal(capNormal1);
                AddNormal(capNormal1);

                float ub = properties.beveledEdge.width / properties.radius * 0.5f;
                float vb = properties.beveledEdge.width / height;
                AddUV(new Vector2(1, vb));
                AddUV(new Vector2(1, 1 - vb));

                AddUV(new Vector2(1 - ub, 0));
                AddUV(new Vector2(1 - ub, 1));

                AddUV(new Vector2(0.5f, 0));
                AddUV(new Vector2(0.5f, 1));

                // for front faces
                AddVertex(new Vector3(x[numCircleSegments], -0.5f * height + properties.beveledEdge.width, y[numCircleSegments]));
                AddVertex(new Vector3(x[numCircleSegments], 0.5f * height - properties.beveledEdge.width, y[numCircleSegments]));

                // for bottom bevel
                AddVertex(new Vector3(x0[numCircleSegments], -0.5f * height, y0[numCircleSegments]));

                // for top bevel);
                AddVertex(new Vector3(x0[numCircleSegments], 0.5f * height, y0[numCircleSegments]));

                AddVertex(new Vector3(0, -0.5f * height, 0));
                AddVertex(new Vector3(0, 0.5f * height, 0));

                Vector3 capNormal2 = ComputeNormal(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]),
                                                   new Vector3(0, 0.5f * height, 0),
                                                   new Vector3(0, -0.5f * height, 0));

                AddNormal(capNormal2);
                AddNormal(capNormal2);
                AddNormal(capNormal2);
                AddNormal(capNormal2);
                AddNormal(capNormal2);
                AddNormal(capNormal2);

                AddUV(new Vector2(0, vb));
                AddUV(new Vector2(0, 1 - vb));

                AddUV(new Vector2(ub, 0));
                AddUV(new Vector2(ub, 1));

                AddUV(new Vector2(0.5f, 0));
                AddUV(new Vector2(0.5f, 1));
            }
        }
        #endregion

        #region GenerateCylinderVertices
        private void GenerateCylinderVerticesHollowed(float height, int numCircleSegments,
                                                      float[] xc, float[] yc, bool sideCap)
        {
            Vector3 topNormal = Vector3.up;
            Vector3 bottomNormal = Vector3.down;

            float[] x0;
            float[] y0;
            float[] x1;
            float[] y1;
            float[] x2;
            float[] y2;

            float[] x = new float[numCircleSegments + 1];
            float[] y = new float[numCircleSegments + 1];

            for (int i = 0; i <= numCircleSegments; ++i)
            {
                x[i] = xc[i] * properties.radius;
                y[i] = yc[i] * properties.radius;
            }

            float[] xb = null;
            float[] yb = null;
            bool beveled = (properties.option == QcCylinderProperties.Options.BeveledEdge) && 
                           (properties.beveledEdge.width > 0);
            if (beveled)
            {
                xb = new float[numCircleSegments + 1];
                yb = new float[numCircleSegments + 1];

                float radiusB = properties.radius - properties.beveledEdge.width;

                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    xb[i] = xc[i] * radiusB;
                    yb[i] = yc[i] * radiusB;
                }
            }

            if (properties.topRadius != properties.radius)
            {
                float radius0 = (height - properties.hollow.height) / height * (properties.topRadius - properties.radius) + properties.radius - properties.hollow.thickness;

                x0 = new float[numCircleSegments + 1];      // inner points
                y0 = new float[numCircleSegments + 1];

                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x0[i] = xc[i] * radius0;
                    y0[i] = yc[i] * radius0;
                }

                x1 = new float[numCircleSegments + 1];      // top outside
                y1 = new float[numCircleSegments + 1];

                float radius1 = properties.topRadius;
                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x1[i] = xc[i] * radius1;
                    y1[i] = yc[i] * radius1;
                }

                x2 = new float[numCircleSegments + 1];      // top inside
                y2 = new float[numCircleSegments + 1];

                float radius2 = properties.topRadius - properties.hollow.thickness;
                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x2[i] = xc[i] * radius2;
                    y2[i] = yc[i] * radius2;
                }
            }
            else
            {
                float radius0 = properties.radius - properties.hollow.thickness;

                x0 = new float[numCircleSegments + 1];      // inner points
                y0 = new float[numCircleSegments + 1];

                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x0[i] = xc[i] * radius0;
                    y0[i] = yc[i] * radius0;
                }

                x1 = new float[numCircleSegments + 1];      // top outside
                y1 = new float[numCircleSegments + 1];

                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x1[i] = x[i];
                    y1[i] = y[i];
                }

                x2 = new float[numCircleSegments + 1];      // top inside 
                y2 = new float[numCircleSegments + 1];

                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x2[i] = x0[i];
                    y2[i] = y0[i];
                }
            }

            if (height == properties.hollow.height)
            {
                for (int i = 0; i <= numCircleSegments; i++)
                {
                    // for bottom face
                    AddVertex(new Vector3(x0[i], -0.5f * height, y0[i]));
                    if (beveled)
                        AddVertex(new Vector3(xb[i], -0.5f * height, yb[i]));
                    else
                        AddVertex(new Vector3(x[i], -0.5f * height, y[i]));

                    // for top face
                    AddVertex(new Vector3(x1[i], 0.5f * height, y1[i]));
                    AddVertex(new Vector3(x2[i], 0.5f * height, y2[i]));

                    // for outer wall
                    if (beveled)
                        AddVertex(new Vector3(x[i], -0.5f * height + properties.beveledEdge.width, y[i]));
                    else
                        AddVertex(new Vector3(x[i], -0.5f * height, y[i]));

                    AddVertex(new Vector3(x1[i], 0.5f * height, y1[i]));

                    // for inner wall
                    AddVertex(new Vector3(x2[i], 0.5f * height, y2[i]));
                    AddVertex(new Vector3(x0[i], -0.5f * height, y0[i]));

                    if (beveled)
                    {
                        AddVertex(new Vector3(xb[i], -0.5f * height, yb[i]));
                        AddVertex(new Vector3(x[i], -0.5f * height + properties.beveledEdge.width, y[i]));
                    }

                    AddNormal(bottomNormal);
                    AddNormal(bottomNormal);

                    AddNormal(topNormal);
                    AddNormal(topNormal);

                    Vector3 normal1 = new Vector3(x[i], 0f, y[i]);
                    normal1.Normalize();
                    AddNormal(normal1);
                    AddNormal(normal1);

                    Vector3 normal2 = new Vector3(-x[i], 0f, -y[i]);
                    normal2.Normalize();
                    AddNormal(normal2);
                    AddNormal(normal2);

                    if (beveled)
                    {
                        Vector3 normalBottomBevel = new Vector3(x[i], -properties.radius, y[i]);
                        normalBottomBevel.Normalize();
                        AddNormal(normalBottomBevel);
                        AddNormal(normalBottomBevel);
                    }

                    //uvs[baseIndex + 0] = new Vector2(x0[i], y0[i]);
                    //uvs[baseIndex + 1] = new Vector2(x[i], y[i]);

                    float r0 = (properties.radius - properties.hollow.thickness) / properties.radius;
                    AddUV(new Vector2((xc[numCircleSegments - i] * r0 + 1.0f) * 0.5f, (yc[numCircleSegments - i] + 1.0f) * 0.5f));
                    AddUV(new Vector2((xc[numCircleSegments - i] + 1.0f) * 0.5f, (yc[numCircleSegments - i] + 1.0f) * 0.5f));

                    float r1 = (properties.topRadius - properties.hollow.thickness) / properties.topRadius;
                    AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));
                    AddUV(new Vector2((xc[i] * r1 + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                    //uvs[baseIndex + 2] = new Vector2(x[i], y[i]);
                    //uvs[baseIndex + 3] = new Vector2(x0[i], y0[i]);

                    //uvs[baseIndex + 4] = new Vector2(2 * i / (float)numCircleSegments, 0f);
                    //uvs[baseIndex + 5] = new Vector2(2 * i / (float)numCircleSegments, 1f);

                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));
                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));

                    if (beveled)
                    {
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, properties.beveledEdge.width / height));
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));

                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, properties.beveledEdge.width / height));
                    }
                    else
                    {
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));
                    }
                }

                if (sideCap)
                {
                    // Ignore bevel because it is not used

                    // for outer wall
                    AddVertex(new Vector3(x[0], -0.5f * height, y[0]));
                    AddVertex(new Vector3(x1[0], 0.5f * height, y1[0]));

                    // for inner wall
                    AddVertex(new Vector3(x2[0], 0.5f * height, y2[0]));
                    AddVertex(new Vector3(x0[0], -0.5f * height, y0[0]));

                    Vector3 capNormal1 = ComputeNormal(new Vector3(x[0], -0.5f * height, y[0]),
                                                       new Vector3(0, -0.5f * height, 0),
                                                       new Vector3(0, 0.5f * height, 0));

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    float ub = properties.hollow.thickness / properties.radius * 0.5f;

                    AddUV(new Vector2(1, 0));
                    AddUV(new Vector2(1, 1));
                    AddUV(new Vector2(1 - ub, 1));
                    AddUV(new Vector2(1 - ub, 0));

                    // for outer wall
                    AddVertex(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]));
                    AddVertex(new Vector3(x1[numCircleSegments], 0.5f * height, y1[numCircleSegments]));

                    // for inner wall
                    AddVertex(new Vector3(x2[numCircleSegments], 0.5f * height, y2[numCircleSegments]));
                    AddVertex(new Vector3(x0[numCircleSegments], -0.5f * height, y0[numCircleSegments]));
                    
                    Vector3 capNormal2 = ComputeNormal(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]),
                                                       new Vector3(0, 0.5f * height, 0),
                                                       new Vector3(0, -0.5f * height, 0));

                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);

                    AddUV(new Vector2(0, 0));
                    AddUV(new Vector2(0, 1));
                    AddUV(new Vector2(ub, 1));
                    AddUV(new Vector2(ub, 0));
                }
            }
            else
            {
                AddVertex(new Vector3(0f, -0.5f * height, 0f));       // top center    
                AddVertex(new Vector3(0f, 0.5f * height - properties.hollow.height, 0f));   // bottom center

                AddNormal(bottomNormal);
                AddNormal(topNormal);

                AddUV(new Vector2(0.5f, 0.5f));
                AddUV(new Vector2(0.5f, 0.5f));

                for (int i = 0; i <= numCircleSegments; i++)
                {
                    // for bottom outer face

                    if (beveled)
                        AddVertex(new Vector3(xb[i], -0.5f * height, yb[i]));
                    else
                        AddVertex(new Vector3(x[i], -0.5f * height, y[i]));

                    // for bottom inner face
                    AddVertex(new Vector3(x0[i], 0.5f * height - properties.hollow.height, y0[i]));

                    // for top face
                    AddVertex(new Vector3(x1[i], 0.5f * height, y1[i]));
                    AddVertex(new Vector3(x2[i], 0.5f * height, y2[i]));

                    // for outer faces
                    if (beveled)
                        AddVertex(new Vector3(x[i], -0.5f * height + properties.beveledEdge.width, y[i]));
                    else
                        AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                    AddVertex(new Vector3(x1[i], 0.5f * height, y1[i]));

                    // for inner faces
                    AddVertex(new Vector3(x2[i], 0.5f * height, y2[i]));
                    AddVertex(new Vector3(x0[i], 0.5f * height - properties.hollow.height, y0[i]));

                    if (beveled)
                    {
                        AddVertex(new Vector3(xb[i], -0.5f * height, yb[i]));
                        AddVertex(new Vector3(x[i], -0.5f * height + properties.beveledEdge.width, y[i]));
                    }

                    AddNormal(bottomNormal);
                    AddNormal(topNormal);

                    AddNormal(topNormal);
                    AddNormal(topNormal);

                    Vector3 normal1;
                    if (properties.radius == properties.topRadius)
                    {
                        normal1 = new Vector3(xc[i], 0f, yc[i]);
                    }
                    else
                    {
                        float coneAngle = Mathf.Atan((properties.radius - properties.topRadius) / height);
                        normal1 = new Vector3(xc[i] * Mathf.Cos(coneAngle), Mathf.Sin(coneAngle), yc[i] * Mathf.Cos(coneAngle));
                    }
                    normal1.Normalize();
                    AddNormal(normal1);
                    AddNormal(normal1);

                    Vector3 normal2 = new Vector3(-x[i], 0f, -y[i]);
                    normal2.Normalize();
                    AddNormal(normal2);
                    AddNormal(normal2);

                    if (beveled)
                    {
                        Vector3 normalBottomBevel = new Vector3(x[i], -properties.radius, y[i]);
                        normalBottomBevel.Normalize();
                        AddNormal(normalBottomBevel);
                        AddNormal(normalBottomBevel);
                    }

                    AddUV(new Vector2((xc[numCircleSegments - i] + 1.0f) * 0.5f, (yc[numCircleSegments - i] + 1.0f) * 0.5f));
                    AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                    float r1 = (properties.topRadius - properties.hollow.thickness) / properties.topRadius;
                    AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));
                    AddUV(new Vector2((xc[i] * r1 + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));
                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));

                    if (beveled)
                    {
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, properties.beveledEdge.width / height));
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));

                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, properties.beveledEdge.width / height));
                    }
                    else
                    {
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 0f));
                        AddUV(new Vector2(2 * i / (float)numCircleSegments, 1f));
                    }
                }

                if (sideCap)
                {
                    // for outer faces
                    AddVertex(new Vector3(x[0], -0.5f * height, y[0]));
                    AddVertex(new Vector3(x1[0], 0.5f * height, y1[0]));

                    // for inner faces
                    AddVertex(new Vector3(x2[0], 0.5f * height, y2[0]));
                    AddVertex(new Vector3(x0[0], 0.5f * height - properties.hollow.height, y0[0]));
                    
                    AddVertex(new Vector3(0, 0.5f * height - properties.hollow.height, 0));
                    AddVertex(new Vector3(0, -0.5f * height, 0));

                    Vector3 capNormal1 = ComputeNormal(new Vector3(x[0], -0.5f * height, y[0]),
                                                       new Vector3(0, -0.5f * height, 0),
                                                       new Vector3(0, 0.5f * height, 0));

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    float ub = properties.hollow.thickness / properties.radius * 0.5f;
                    float vb = 1 - properties.hollow.height / properties.height;

                    AddUV(new Vector2(1, 0));
                    AddUV(new Vector2(1, 1));
                    AddUV(new Vector2(1 - ub, 1));
                    AddUV(new Vector2(1 - ub, vb));
                    AddUV(new Vector2(0.5f, vb));
                    AddUV(new Vector2(0.5f, 0));

                    // for outer faces
                    AddVertex(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]));
                    AddVertex(new Vector3(x1[numCircleSegments], 0.5f * height, y1[numCircleSegments]));

                    // for inner faces
                    AddVertex(new Vector3(x2[numCircleSegments], 0.5f * height, y2[numCircleSegments]));
                    AddVertex(new Vector3(x0[numCircleSegments], 0.5f * height - properties.hollow.height, y0[numCircleSegments]));
                    
                    AddVertex(new Vector3(0, 0.5f * height - properties.hollow.height, 0));
                    AddVertex(new Vector3(0, -0.5f * height, 0));

                    Vector3 capNormal2 = ComputeNormal(new Vector3(x[numCircleSegments], -0.5f * height, y[numCircleSegments]),
                                                       new Vector3(0, 0.5f * height, 0),
                                                       new Vector3(0, -0.5f * height, 0));

                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);
                    AddNormal(capNormal2);

                    AddUV(new Vector2(0, 0));
                    AddUV(new Vector2(0, 1));
                    AddUV(new Vector2(ub, 1));
                    AddUV(new Vector2(ub, vb));
                    AddUV(new Vector2(0.5f, vb));
                    AddUV(new Vector2(0.5f, 0));
                }
            }
        }
        #endregion

        #region GenerateCylinderTriangles
        private void GenerateCylinderTriangles(float height, int numCircleSegments, bool sideCap)
        {
            int ti = 0;
            for (int i = 0; i < numCircleSegments; ++i)
            {
                int base1 = 2 + 4 * i;
                int base2 = 2 + 4 * (i + 1);

                // bottom triangles
                faces.Add(new TriangleIndices(base2, 0, base1));

                // top triangles
                faces.Add(new TriangleIndices(base1 + 1, 1, base2 + 1));

                // side triangles
                faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));

                ti += 12;       // 4 triangles * 3 vertices per triangle
            }

            int bi = (numCircleSegments + 1) * 4 + 2;
            if (sideCap)
            {
                faces.Add(new TriangleIndices(bi + 0, bi + 2, bi + 1));
                faces.Add(new TriangleIndices(bi + 1, bi + 2, bi + 3));

                bi += 4;

                faces.Add(new TriangleIndices(bi + 1, bi + 3, bi + 0));
                faces.Add(new TriangleIndices(bi + 0, bi + 3, bi + 2));
            }
        }
        #endregion

        #region GenerateConeTriangles
        private void GenerateConeTriangles(float height, int numCircleSegments, bool sideCap)
        {
            if (properties.topRadius == 0)
            {
                for (int i = 0; i < numCircleSegments; ++i)
                {
                    int base1 = 1 + 3 * i;
                    int base2 = 1 + 3 * (i + 1);

                    // bottom triangles
                    faces.Add(new TriangleIndices(base2, 0, base1));

                    // side triangles
                    faces.Add(new TriangleIndices(base2 + 1, base1 + 1, base1 + 2));
                }

                int bi = (numCircleSegments + 1) * 3 + 1;
                if (sideCap)
                {
                    faces.Add(new TriangleIndices(bi + 0, bi + 1, bi + 2));

                    bi += 3;
                    
                    faces.Add(new TriangleIndices(bi + 0, bi + 2, bi + 1));
                }
            }
            else
            {
                for (int i = 0; i < numCircleSegments; ++i)
                {
                    int base1 = 2 + 4 * i;
                    int base2 = 2 + 4 * (i + 1);

                    // bottom triangles
                    faces.Add(new TriangleIndices(base2, 0, base1));

                    // top triangles
                    faces.Add(new TriangleIndices(base1 + 1, 1, base2 + 1));

                    // side triangles
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));
                }

                int bi = (numCircleSegments + 1) * 4 + 2;
                if (sideCap)
                {
                    faces.Add(new TriangleIndices(bi + 0, bi + 2, bi + 1));
                    faces.Add(new TriangleIndices(bi + 1, bi + 2, bi + 3));

                    bi += 4;

                    faces.Add(new TriangleIndices(bi + 1, bi + 3, bi + 0));
                    faces.Add(new TriangleIndices(bi + 0, bi + 3, bi + 2));
                }
            }
        }
        #endregion

        #region GenerateCylinderTrianglesBeveled
        private void GenerateCylinderTrianglesBeveled(float height, int numCircleSegments, bool sideCap)
        {
            for (int i = 0; i < numCircleSegments; ++i)
            {
                int base1 = 2 + 8 * i;
                int base2 = 2 + 8 * (i + 1);

                // bottom triangles
                faces.Add(new TriangleIndices(base2, 0, base1));

                // top triangles
                faces.Add(new TriangleIndices(base1 + 1, 1, base2 + 1));

                // side triangles
                faces.Add(new TriangleIndices(base1 + 2, base1 + 3, base2 + 2));

                faces.Add(new TriangleIndices(base2 + 2, base1 + 3, base2 + 3));

                // bottom bevel
                faces.Add(new TriangleIndices(base1 + 4, base1 + 5, base2 + 4));
                faces.Add(new TriangleIndices(base2 + 4, base1 + 5, base2 + 5));

                // top bevel
                faces.Add(new TriangleIndices(base1 + 6, base1 + 7, base2 + 6));
                faces.Add(new TriangleIndices(base2 + 6, base1 + 7, base2 + 7));
            }

            int bi = (numCircleSegments + 1) * 8 + 2;
            if (sideCap)
            {
                faces.Add(new TriangleIndices(bi + 5, bi + 3, bi + 4));
                faces.Add(new TriangleIndices(bi + 4, bi + 3, bi + 2));
                faces.Add(new TriangleIndices(bi + 2, bi + 3, bi + 0));
                faces.Add(new TriangleIndices(bi + 0, bi + 3, bi + 1));

                bi += 6;

                faces.Add(new TriangleIndices(bi + 5, bi + 4, bi + 3));
                faces.Add(new TriangleIndices(bi + 3, bi + 4, bi + 2));
                faces.Add(new TriangleIndices(bi + 2, bi + 0, bi + 3));
                faces.Add(new TriangleIndices(bi + 3, bi + 0, bi + 1));
            }
        }
        #endregion

        #region GenerateCylinderTrianglesHollowed
        private void GenerateCylinderTrianglesHollowed(float height, int numCircleSegments, bool sideCap)
        {
            bool beveled = (properties.option == QcCylinderProperties.Options.BeveledEdge) && 
                           (properties.beveledEdge.width > 0);
            if (height == properties.hollow.height)
            {
                for (int i = 0; i < numCircleSegments; ++i)
                {
                    int base1;
                    int base2;
                    if (beveled)
                    {
                        base1 = 10 * i;
                        base2 = 10 * (i + 1);
                    }
                    else
                    {
                        base1 = 8 * i;
                        base2 = 8 * (i + 1);
                    }

                    // bottom face
                    faces.Add(new TriangleIndices(base2, base1, base2 + 1));
                    faces.Add(new TriangleIndices(base2 + 1, base1, base1 + 1));

                    // top face
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));

                    // outer triangles
                    faces.Add(new TriangleIndices(base2 + 4, base1 + 4, base2 + 5));
                    faces.Add(new TriangleIndices(base2 + 5, base1 + 4, base1 + 5));

                    // inner bevel
                    faces.Add(new TriangleIndices(base2 + 6, base1 + 6, base2 + 7));
                    faces.Add(new TriangleIndices(base2 + 7, base1 + 6, base1 + 7));

                    if (beveled)
                    {
                        // bottom bevel
                        faces.Add(new TriangleIndices(base2 + 8, base1 + 8, base2 + 9));
                        faces.Add(new TriangleIndices(base2 + 9, base1 + 8, base1 + 9));
                    }
                }

                int bi = (numCircleSegments + 1) * 8;
                if (sideCap)
                {
                    faces.Add(new TriangleIndices(bi + 2, bi + 1, bi + 3));
                    faces.Add(new TriangleIndices(bi + 3, bi + 1, bi + 0));

                    bi += 4;

                    faces.Add(new TriangleIndices(bi + 2, bi + 3, bi + 1));
                    faces.Add(new TriangleIndices(bi + 1, bi + 3, bi + 0));
                }
            }
            else
            {
                for (int i = 0; i < numCircleSegments; ++i)
                {
                    int base1;
                    int base2;
                    if (beveled)
                    {
                        base1 = 2 + 10 * i;
                        base2 = 2 + 10 * (i + 1);
                    }
                    else
                    {
                        base1 = 2 + 8 * i;
                        base2 = 2 + 8 * (i + 1);
                    }

                    // bottom face
                    faces.Add(new TriangleIndices(base2, 0, base1));
                    faces.Add(new TriangleIndices(base1 + 1, 1, base2 + 1));

                    // top face
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));

                    // outer face
                    faces.Add(new TriangleIndices(base2 + 4, base1 + 4, base2 + 5));
                    faces.Add(new TriangleIndices(base2 + 5, base1 + 4, base1 + 5));

                    // inner bevel
                    faces.Add(new TriangleIndices(base2 + 6, base1 + 6, base2 + 7));
                    faces.Add(new TriangleIndices(base2 + 7, base1 + 6, base1 + 7));

                    if (beveled)
                    {
                        // bottom bevel
                        faces.Add(new TriangleIndices(base2 + 8, base1 + 8, base2 + 9));
                        faces.Add(new TriangleIndices(base2 + 9, base1 + 8, base1 + 9));
                    }
                }

                int bi = (numCircleSegments + 1) * 8 + 2;
                if (sideCap)
                {
                    faces.Add(new TriangleIndices(bi + 2, bi + 1, bi + 3));
                    faces.Add(new TriangleIndices(bi + 3, bi + 1, bi + 0));
                    faces.Add(new TriangleIndices(bi + 0, bi + 5, bi + 3));
                    faces.Add(new TriangleIndices(bi + 3, bi + 5, bi + 4));

                    bi += 6;

                    faces.Add(new TriangleIndices(bi + 2, bi + 3, bi + 1));
                    faces.Add(new TriangleIndices(bi + 1, bi + 3, bi + 0));
                    faces.Add(new TriangleIndices(bi + 0, bi + 3, bi + 5));
                    faces.Add(new TriangleIndices(bi + 5, bi + 3, bi + 4));
                }
            }
        }
        #endregion
    }
}
