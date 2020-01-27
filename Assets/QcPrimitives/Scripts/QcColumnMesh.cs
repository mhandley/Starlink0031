using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcColumnMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcColumnProperties : QcBaseProperties
        {
            [System.Serializable]
            public class Hollow
            {
                public bool enabled;
                public float ratio = 0;
            }
            
            public float width = 1;
            public float depth = 1;
            public float height = 1;

            public int sides = 5;
            public float triangleIncline = 0.5f;

            public Hollow hollow = new Hollow();

            public void CopyFrom(QcColumnProperties source)
            {
                base.CopyFrom(source);

                this.width = source.width;
                this.depth = source.depth;
                this.height = source.height;
                this.sides = source.sides;
                this.triangleIncline = source.triangleIncline;

                this.hollow.enabled = source.hollow.enabled;
                this.hollow.ratio = source.hollow.ratio;
            }

            public bool Modified(QcColumnProperties source)
            {
                if ((this.width == source.width) && (this.depth == source.depth) && (this.height == source.height) &&
                    (this.sides == source.sides) &&
                    (this.triangleIncline == source.triangleIncline) &&
                    (this.hollow.enabled == source.hollow.enabled) &&
                    (this.hollow.ratio == source.hollow.ratio) &&
                    (this.genTextureCoords == source.genTextureCoords) &&
                    (this.addCollider == source.addCollider) &&
                    (this.offset[0] == source.offset[0]) && (this.offset[1] == source.offset[1]) && (this.offset[2] == source.offset[2]))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public QcColumnProperties properties = new QcColumnProperties();

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
            if ((properties.width <= 0) || (properties.height <= 0) || (properties.depth <= 0)) return;

            
            xc = new float[properties.sides + 1];
            yc = new float[properties.sides + 1];

            if (properties.sides == 3)
            {
                xc[0] = -1;
                yc[0] = -1;
                xc[1] = 1;
                yc[1] = properties.triangleIncline * 2 - 1;
                xc[2] = -1;
                yc[2] = 1;
                xc[3] = -1;
                yc[3] = -1;
            }
            else if (properties.sides == 4)
            {
                xc[0] = -1;
                yc[0] = -1;
                xc[1] = 1;
                yc[1] = -1;
                xc[2] = 1;
                yc[2] = 1;
                xc[3] = -1;
                yc[3] = 1;
                xc[4] = -1;
                yc[4] = -1;
            }
            else
            {
                float partAngle = twoPi / properties.sides;
                for (int i = 0; i <= properties.sides; ++i)
                {
                    float angle = i * partAngle;
                    if (properties.sides % 2 == 1) angle += Mathf.PI * 0.5f;
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

            GenerateVertices(properties.height, properties.sides, xc, yc);
            GenerateTriangles(properties.sides);

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

            if (properties.sides == 3)
            {
                xc[0] = -1;
                yc[0] = -1;
                xc[1] = 1;
                yc[1] = properties.triangleIncline * 2 - 1;
                xc[2] = -1;
                yc[2] = 1;
                xc[3] = -1;
                yc[3] = -1;
            }
            else if (properties.sides == 4)
            {
                xc[0] = -1;
                yc[0] = -1;
                xc[1] = 1;
                yc[1] = -1;
                xc[2] = 1;
                yc[2] = 1;
                xc[3] = -1;
                yc[3] = 1;
                xc[4] = -1;
                yc[4] = -1;
            }
            else
            {
                float partAngle = twoPi / properties.sides;
                for (int i = 0; i <= properties.sides; ++i)
                {
                    float angle = i * partAngle;
                    if (properties.sides % 2 == 1) angle += Mathf.PI * 0.5f;
                    xc[i] = Mathf.Cos(angle);
                    yc[i] = Mathf.Sin(angle);
                }
            }

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            ClearVertices();

            GenerateVertices(properties.height, properties.sides, xc, yc);
            GenerateTriangles(properties.sides);

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
                BoxCollider collider = gameObject.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = gameObject.AddComponent<BoxCollider>();
                }

                collider.enabled = true;
                collider.center = properties.offset;
                collider.size = new Vector3(properties.width, properties.height, properties.depth);
            }
            else
            {
                BoxCollider collider = gameObject.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        #region GenerateVertices
        private void GenerateVertices(float height, int numCircleSegments, float[] xc, float[] yc)
        {
            if (!properties.hollow.enabled) 
                GenerateVerticesFilled(height, numCircleSegments, xc, yc);
            else
                GenerateVerticesHollow(height, numCircleSegments, xc, yc);
        }
        #endregion

        #region GenerateVerticesFilled
        private void GenerateVerticesFilled(float height, int numCircleSegments, float[] xc, float[] yc)
        {
            float[] x = new float[numCircleSegments + 1];
            float[] y = new float[numCircleSegments + 1];

            float halfWidth = properties.width * 0.5f;
            float halfDepth = properties.depth * 0.5f;
            for (int i = 0; i <= numCircleSegments; ++i)
            {
                x[i] = xc[i] * halfWidth;
                y[i] = yc[i] * halfDepth;
            }

            float[] xn;
            float[] yn;
            ComputeNormals(numCircleSegments, halfWidth, halfDepth, out xn, out yn);

            Vector3 bottomNormal = Vector3.down;
            Vector3 topNormal = Vector3.up;

            AddVertex(new Vector3(0f, -0.5f * height, 0f));       // bottom center 

            AddNormal(bottomNormal);

            AddUV(new Vector2(0.5f, 0.5f));

            AddVertex(new Vector3(0f, 0.5f * height, 0f));       // top center 

            AddNormal(topNormal);

            AddUV(new Vector2(0.5f, 0.5f));

            for (int i = 0; i <= numCircleSegments; i++)
            {
                // for bottom face
                AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                AddNormal(bottomNormal);
                AddUV(new Vector2((x[i] + 1.0f) * 0.5f, (y[i] + 1.0f) * 0.5f));

                // for top face
                AddVertex(new Vector3(x[i], 0.5f * height, y[i]));
                AddNormal(topNormal);
                AddUV(new Vector2((x[i] + 1.0f) * 0.5f, (y[i] + 1.0f) * 0.5f));

                // for front faces
                AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                AddVertex(new Vector3(x[i], 0.5f * height, y[i]));
                AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));
                AddUV(new Vector2((2 * i + 1) / (float)numCircleSegments, 1));

                if ((i > 0) && (i < numCircleSegments))
                {
                    AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                    AddVertex(new Vector3(x[i], 0.5f * height, y[i]));
                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));
                    AddUV(new Vector2((2 * i + 1) / (float)numCircleSegments, 1));
                }

                if (i == 0)
                {
                    Vector3 normal1 = new Vector3(xn[i], 0, yn[i]);
                    normal1.Normalize();

                    AddNormal(normal1);
                    AddNormal(normal1);

                }
                else if (i == numCircleSegments)
                {
                    Vector3 normal1 = new Vector3(xn[i - 1], 0, yn[i - 1]);
                    normal1.Normalize();

                    AddNormal(normal1);
                    AddNormal(normal1);
                }
                else
                {
                    Vector3 normal1 = new Vector3(xn[i - 1], 0, yn[i - 1]);
                    normal1.Normalize();

                    AddNormal(normal1);
                    AddNormal(normal1);

                    Vector3 normal2 = new Vector3(xn[i], 0, yn[i]);
                    normal2.Normalize();

                    AddNormal(normal2);
                    AddNormal(normal2);
                }
            }
        }
        #endregion

        #region GenerateVerticesHollow
        private void GenerateVerticesHollow(float height, int numCircleSegments, float[] xc, float[] yc)
        {
            float[] x = new float[numCircleSegments + 1];
            float[] y = new float[numCircleSegments + 1];

            float halfWidth = properties.width * 0.5f;
            float halfDepth = properties.depth * 0.5f;
            for (int i = 0; i <= numCircleSegments; ++i)
            {
                x[i] = xc[i] * halfWidth;
                y[i] = yc[i] * halfDepth;
            }

            float[] x0 = new float[numCircleSegments + 1];
            float[] y0 = new float[numCircleSegments + 1];

            if (numCircleSegments != 3)
            {
                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x0[i] = xc[i] * halfWidth * properties.hollow.ratio;
                    y0[i] = yc[i] * halfDepth * properties.hollow.ratio;
                }
            }
            else
            {
                float xo = (x[0] + x[1] + x[2]) / 3.0f;
                float yo = (y[0] + y[1] + y[2]) / 3.0f;

                float ratio = properties.hollow.ratio;
                for (int i = 0; i <= numCircleSegments; ++i)
                {
                    x0[i] = x[i] * ratio + xo * (1 - ratio);
                    y0[i] = y[i] * ratio + yo * (1 - ratio);
                }
            }

            float[] xn;
            float[] yn;
            ComputeNormals(numCircleSegments, halfWidth, halfDepth, out xn, out yn);
            //float partAngle = (2f * Mathf.PI) / numCircleSegments;
            //for (int i = 0; i < numCircleSegments; ++i)
            //{
            //    float angle = i * partAngle + partAngle * 0.5f + Mathf.PI * 0.5f;
            //    xn[i] = Mathf.Cos(angle) * halfWidth;
            //    yn[i] = Mathf.Sin(angle) * halfDepth;
            //}

            Vector3 bottomNormal = Vector3.down;
            Vector3 topNormal = Vector3.up;

            for (int i = 0; i <= numCircleSegments; i++)
            {
                // for bottom face
                AddVertex(new Vector3(x0[i], -0.5f * height, y0[i]));
                AddNormal(bottomNormal);
                AddUV(new Vector2((xc[i] * properties.hollow.ratio + 1.0f) * 0.5f, (yc[i] * properties.hollow.ratio + 1.0f) * 0.5f));

                AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                AddNormal(bottomNormal);
                AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                // for top face
                AddVertex(new Vector3(x[i], 0.5f * height, y[i]));
                AddNormal(topNormal);
                AddUV(new Vector2((xc[i] + 1.0f) * 0.5f, (yc[i] + 1.0f) * 0.5f));

                AddVertex(new Vector3(x0[i], 0.5f * height, y0[i]));
                AddNormal(topNormal);
                AddUV(new Vector2((xc[i] * properties.hollow.ratio + 1.0f) * 0.5f, (yc[i] * properties.hollow.ratio + 1.0f) * 0.5f));

                // for front faces
                AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                AddVertex(new Vector3(x[i], 0.5f * height, y[i]));
                AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));
                AddUV(new Vector2((2 * i + 1) / (float)numCircleSegments, 1));

                // for inner faces
                AddVertex(new Vector3(x0[i], 0.5f * height, y0[i]));
                AddVertex(new Vector3(x0[i], -0.5f * height, y0[i]));
                AddUV(new Vector2((2 * i + 1) / (float)numCircleSegments, 1));
                AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));

                if ((i > 0) && (i < numCircleSegments))
                {
                    AddVertex(new Vector3(x[i], -0.5f * height, y[i]));
                    AddVertex(new Vector3(x[i], 0.5f * height, y[i]));
                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));
                    AddUV(new Vector2((2 * i + 1) / (float)numCircleSegments, 1));

                    AddVertex(new Vector3(x0[i], 0.5f * height, y0[i]));
                    AddVertex(new Vector3(x0[i], -0.5f * height, y0[i]));
                    AddUV(new Vector2((2 * i + 1) / (float)numCircleSegments, 1));
                    AddUV(new Vector2(2 * i / (float)numCircleSegments, 0));
                }

                if (i == 0)
                {
                    Vector3 normal1 = new Vector3(xn[i], 0, yn[i]);
                    normal1.Normalize();

                    AddNormal(normal1);
                    AddNormal(normal1);

                    Vector3 normal2 = new Vector3(-xn[i], 0, -yn[i]);
                    normal2.Normalize();
                    AddNormal(normal2);
                    AddNormal(normal2);
                }
                else if (i == numCircleSegments)
                {
                    Vector3 normal1 = new Vector3(xn[i - 1], 0, yn[i - 1]);
                    normal1.Normalize();

                    AddNormal(normal1);
                    AddNormal(normal1);

                    Vector3 normal2 = new Vector3(-xn[i - 1], 0, -yn[i - 1]);
                    normal2.Normalize();

                    AddNormal(normal2);
                    AddNormal(normal2);
                }
                else
                {
                    Vector3 normal1 = new Vector3(xn[i - 1], 0, yn[i - 1]);
                    normal1.Normalize();

                    AddNormal(normal1);
                    AddNormal(normal1);

                    Vector3 normal3 = new Vector3(-xn[i - 1], 0, -yn[i - 1]);
                    normal3.Normalize();

                    AddNormal(normal3);
                    AddNormal(normal3);

                    Vector3 normal2 = new Vector3(xn[i], 0, yn[i]);
                    normal2.Normalize();

                    AddNormal(normal2);
                    AddNormal(normal2);
                    
                    Vector3 normal4 = new Vector3(-xn[i], 0, -yn[i]);
                    normal4.Normalize();

                    AddNormal(normal4);
                    AddNormal(normal4);
                }
            }
        }
        #endregion

        private void ComputeNormals(int numCircleSegments, float halfWidth, float halfDepth, out float[] xn, out float[] yn)
        {
            xn = new float[numCircleSegments + 1];
            yn = new float[numCircleSegments + 1];

            if (properties.sides == 3)
            {
                xn[0] = properties.triangleIncline;
                yn[0] = -halfWidth / halfDepth;
                xn[1] = 1 - properties.triangleIncline;
                yn[1] = halfWidth / halfDepth;
                xn[2] = -1;
                yn[2] = 0;
                xn[3] = properties.triangleIncline * 2 - 1;
                yn[3] = halfWidth / halfDepth;
            }
            else if (properties.sides == 4)
            {
                xn[0] = 0;
                yn[0] = -1;
                xn[1] = 1;
                yn[1] = 0;
                xn[2] = 0;
                yn[2] = 1;
                xn[3] = -1;
                yn[3] = 0;
                xn[4] = 0;
                yn[4] = -1;
            }
            else
            {
                float partAngle = (2f * Mathf.PI) / numCircleSegments;
                for (int i = 0; i < numCircleSegments; ++i)
                {
                    float angle = i * partAngle + partAngle * 0.5f;
                    if (properties.sides % 2 == 1) angle += Mathf.PI * 0.5f;
                    xn[i] = Mathf.Cos(angle) * halfWidth;
                    yn[i] = Mathf.Sin(angle) * halfDepth;
                }
            }
        }

        #region GenerateTriangles
        private void GenerateTriangles(int numCircleSegments)
        {
            if (!properties.hollow.enabled) 
                GenerateTrianglesFilled(numCircleSegments);
            else
                GenerateTrianglesHollow(numCircleSegments);
        }
        #endregion

        #region GenerateTrianglesFilled
        private void GenerateTrianglesFilled(int numCircleSegments)
        {
            for (int i = 0; i < numCircleSegments; ++i)
            { 
                if (i == 0)
                {
                    int base1 = 2;
                    int base2 = 6;

                    // bottom triangles
                    faces.Add(new TriangleIndices(base2, 0, base1));

                    // top triangles
                    faces.Add(new TriangleIndices(base1 + 1, 1, base2 + 1));

                    // side triangles
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));
                }
                else if (i == numCircleSegments - 1)
                {
                    int base1 = 6 + 6 * (i - 1);
                    int base2 = 6 + 6 * i;

                    // bottom triangles
                    faces.Add(new TriangleIndices(base2, 0, base1));

                    // top triangles
                    faces.Add(new TriangleIndices(base1 + 1, 1, base2 + 1));

                    // side triangles
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 4, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 4, base1 + 5));
                }
                else
                {
                    int base1 = 6 + 6 * (i - 1);
                    int base2 = 6 + 6 * i;

                    // bottom triangles
                    faces.Add(new TriangleIndices(base2, 0, base1));

                    // top triangles
                    faces.Add(new TriangleIndices(base1 + 1, 1, base2 + 1));

                    // side triangles
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 4, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 4, base1 + 5));
                }
            }
        }
        #endregion

        #region GenerateTrianglesHollow
        private void GenerateTrianglesHollow(int numCircleSegments)
        {
            for (int i = 0; i < numCircleSegments; ++i)
            {
                if (i == 0)
                {
                    int base1 = 0;
                    int base2 = 8;

                    // bottom face
                    faces.Add(new TriangleIndices(base2, base1, base2 + 1));
                    faces.Add(new TriangleIndices(base2 + 1, base1, base1 + 1));

                    // top face
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));

                    // side face
                    faces.Add(new TriangleIndices(base2 + 4, base1 + 4, base2 + 5));
                    faces.Add(new TriangleIndices(base2 + 5, base1 + 4, base1 + 5));

                    // inner face
                    faces.Add(new TriangleIndices(base2 + 6, base1 + 6, base2 + 7));
                    faces.Add(new TriangleIndices(base2 + 7, base1 + 6, base1 + 7));
                }
                else if (i == numCircleSegments - 1)
                {
                    int base1 = 8 + 12 * (i - 1);
                    int base2 = 8 + 12 * i;

                    // bottom face
                    faces.Add(new TriangleIndices(base2, base1, base2 + 1));
                    faces.Add(new TriangleIndices(base2 + 1, base1, base1 + 1));

                    // top face
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));

                    // side face
                    faces.Add(new TriangleIndices(base2 + 4, base1 + 8, base2 + 5));
                    faces.Add(new TriangleIndices(base2 + 5, base1 + 8, base1 + 9));

                    // inner face
                    faces.Add(new TriangleIndices(base2 + 6, base1 + 10, base2 + 7));
                    faces.Add(new TriangleIndices(base2 + 7, base1 + 10, base1 + 11));
                }
                else
                {
                    int base1 = 8 + 12 * (i - 1);
                    int base2 = 8 + 12 * i;

                    // bottom face
                    faces.Add(new TriangleIndices(base2, base1, base2 + 1));
                    faces.Add(new TriangleIndices(base2 + 1, base1, base1 + 1));

                    // top face
                    faces.Add(new TriangleIndices(base2 + 2, base1 + 2, base2 + 3));
                    faces.Add(new TriangleIndices(base2 + 3, base1 + 2, base1 + 3));

                    // side face
                    faces.Add(new TriangleIndices(base2 + 4, base1 + 8, base2 + 5));
                    faces.Add(new TriangleIndices(base2 + 5, base1 + 8, base1 + 9));

                    // inner face
                    faces.Add(new TriangleIndices(base2 + 6, base1 + 10, base2 + 7));
                    faces.Add(new TriangleIndices(base2 + 7, base1 + 10, base1 + 11));
                }
            }
        }
        #endregion
    }
}
