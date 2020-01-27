using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcGridMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcGridProperties : QcBaseProperties
        {
            public float width = 1;
            public float depth = 1;
            public float height = 1;

            public int columnCount = 2;
            public int rowCount = 2;

            public float borderWidth;
            public float borderHeight;

            public bool textureWrapped;

            public void CopyFrom(QcGridProperties source)
            {
                base.CopyFrom(source);

                this.width = source.width;
                this.height = source.height;
                this.depth = source.depth;

                this.columnCount = source.columnCount;
                this.rowCount = source.rowCount;

                this.borderWidth = source.borderWidth;
                this.borderHeight = source.borderHeight;
            }

            public bool Modified(QcGridProperties source)
            {
                return ((this.width != source.width) ||
                        (this.height != source.height) ||
                        (this.depth != source.depth) ||
                        (this.columnCount != source.columnCount) ||
                        (this.rowCount != source.rowCount) ||
                        (this.borderWidth != source.borderWidth) ||
                        (this.borderHeight != source.borderHeight) ||
                        (this.offset[0] != source.offset[0]) ||
                        (this.offset[1] != source.offset[1]) ||
                        (this.offset[2] != source.offset[2]) ||
                        (this.genTextureCoords != source.genTextureCoords) ||
                        (this.textureWrapped != source.textureWrapped) ||
                        (this.addCollider != source.addCollider));
            }
        }

        public QcGridProperties properties = new QcGridProperties();

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
            
            GenerateVertices();
            GenerateTriangles();

            if (properties.offset != Vector3.zero)
            {
                AddOffset(properties.offset);
            }

            int[] triangles = new int[faces.Count * 3];
            int ti = 0;
            foreach (var tri in faces)
            {
                triangles[ti] = tri.v1;
                triangles[ti + 1] = tri.v2;
                triangles[ti + 2] = tri.v3;

                ti += 3;
            }

            Mesh mesh = new Mesh();

            meshFilter.sharedMesh = mesh;

            // Assign verts, norms, uvs and tris to mesh and calc normals
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            if (properties.genTextureCoords)
                meshFilter.sharedMesh.uv = uvs.ToArray();
            else
                meshFilter.sharedMesh.uv = null;
            mesh.triangles = triangles;

            SetCollider();

            mesh.RecalculateBounds();
        }
        #endregion

        #region RebuildGeometry
        public override void RebuildGeometry()
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            ClearVertices();

            GenerateVertices();
            GenerateTriangles();

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
                BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    boxCollider = gameObject.AddComponent<BoxCollider>();
                }

                // set collider bound
                boxCollider.enabled = true;
                boxCollider.center = properties.offset;
                boxCollider.size = new Vector3(properties.width, properties.height, properties.depth);
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

        public void ReassignMaterial()
        {
            if (!properties.genTextureCoords) return;

            uvs = new List<Vector2>();
            
            SetTextureCoords();

            gameObject.GetComponent<MeshFilter>().sharedMesh.uv = uvs.ToArray();
        }

        #region GenerateVertices
        private void GenerateVertices()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;

            int columnCount = properties.columnCount;
            int rowCount = properties.rowCount;

            float borderWidth = properties.borderWidth;
            float borderHeight = properties.borderHeight;

            float cellWidth = (width - borderWidth * (columnCount + 1)) / columnCount;
            float cellHegiht = (height - borderHeight * (rowCount + 1)) / rowCount;

            

            float w0 = -width * 0.5f;
            float w1 = width * 0.5f;
            float h0 = -height * 0.5f;
            float h1 = height * 0.5f;
            float d0 = -depth * 0.5f;
            float d1 = depth * 0.5f;

            Vector3[] vf = new Vector3[8];

            vf[0] = new Vector3(w0, h0, d0);
            vf[1] = new Vector3(w1, h0, d0);
            vf[2] = new Vector3(w1, h1, d0);
            vf[3] = new Vector3(w0, h1, d0);

            vf[4] = new Vector3(w0, h0, d1);
            vf[5] = new Vector3(w1, h0, d1);
            vf[6] = new Vector3(w1, h1, d1);
            vf[7] = new Vector3(w0, h1, d1);

            // top
            AddVertex(vf[3]);
            AddVertex(vf[2]);
            AddVertex(vf[6]);
            AddVertex(vf[7]);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            // bottom
            AddVertex(vf[4]);
            AddVertex(vf[5]);
            AddVertex(vf[1]);
            AddVertex(vf[0]);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            // left
            AddVertex(vf[4]);
            AddVertex(vf[0]);
            AddVertex(vf[3]);
            AddVertex(vf[7]);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            // right
            AddVertex(vf[1]);
            AddVertex(vf[5]);
            AddVertex(vf[6]);
            AddVertex(vf[2]);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            // horizontal bars
            for (int i = 0; i <= rowCount; ++i)
            {
                Vector3[] v = new Vector3[8];

                float width0 = -width * 0.5f;
                float width1 = width * 0.5f;
                float height0 = -height * 0.5f + i * (cellHegiht + borderHeight);
                float height1 = -height * 0.5f + i * (cellHegiht + borderHeight) + borderHeight;

                v[0] = new Vector3(width0, height0, -depth * 0.5f);
                v[1] = new Vector3(width1, height0, -depth * 0.5f);
                v[2] = new Vector3(width1, height1, -depth * 0.5f);
                v[3] = new Vector3(width0, height1, -depth * 0.5f);

                v[4] = new Vector3(width0, height0, depth * 0.5f);
                v[5] = new Vector3(width1, height0, depth * 0.5f);
                v[6] = new Vector3(width1, height1, depth * 0.5f);
                v[7] = new Vector3(width0, height1, depth * 0.5f);

                AddVertex(v[0]);
                AddVertex(v[1]);
                AddVertex(v[2]);
                AddVertex(v[3]);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);

                AddVertex(v[4]);
                AddVertex(v[5]);
                AddVertex(v[6]);
                AddVertex(v[7]);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);

                for (int j = 0; j < columnCount; ++j)
                {
                    width0 = -width * 0.5f + j * (cellWidth + borderWidth) + borderWidth;
                    width1 = -width * 0.5f + (j + 1) * (cellWidth + borderWidth);

                    height0 = -height * 0.5f + i * (cellHegiht + borderHeight) + borderHeight;
                    height1 = -height * 0.5f + i * (cellHegiht + borderHeight);

                    v[0] = new Vector3(width0, height0, -depth * 0.5f);
                    v[1] = new Vector3(width1, height0, -depth * 0.5f);
                    v[2] = new Vector3(width1, height0, depth * 0.5f);
                    v[3] = new Vector3(width0, height0, depth * 0.5f);

                    v[4] = new Vector3(width0, height1, -depth * 0.5f);
                    v[5] = new Vector3(width1, height1, -depth * 0.5f);
                    v[6] = new Vector3(width1, height1, depth * 0.5f);
                    v[7] = new Vector3(width0, height1, depth * 0.5f);

                    if (i != rowCount)
                    {
                        AddVertex(v[0]);
                        AddVertex(v[1]);
                        AddVertex(v[2]);
                        AddVertex(v[3]);
                        AddNormal(Vector3.up);
                        AddNormal(Vector3.up);
                        AddNormal(Vector3.up);
                        AddNormal(Vector3.up);
                    }

                    if (i != 0)
                    {
                        AddVertex(v[7]);
                        AddVertex(v[6]);
                        AddVertex(v[5]);
                        AddVertex(v[4]);
                        AddNormal(Vector3.down);
                        AddNormal(Vector3.down);
                        AddNormal(Vector3.down);
                        AddNormal(Vector3.down);
                    }
                }
            }

            // vertical bars
            for (int i = 0; i <= columnCount; ++i)
            {
                for (int j = 0; j < rowCount; ++j)
                {
                    Vector3[] v = new Vector3[8];

                    float width0 = -width * 0.5f + i * (width - borderWidth) / columnCount;
                    float width1 = -width * 0.5f + i * (width - borderWidth) / columnCount + borderWidth;
                    float height0 = -height * 0.5f + borderHeight + j * (cellHegiht + borderHeight);
                    float height1 = -height * 0.5f + (j + 1) * (cellHegiht + borderHeight);

                    v[0] = new Vector3(width0, height0, -depth * 0.5f);
                    v[1] = new Vector3(width1, height0, -depth * 0.5f);
                    v[2] = new Vector3(width1, height1, -depth * 0.5f);
                    v[3] = new Vector3(width0, height1, -depth * 0.5f);

                    v[4] = new Vector3(width0, height0, depth * 0.5f);
                    v[5] = new Vector3(width1, height0, depth * 0.5f);
                    v[6] = new Vector3(width1, height1, depth * 0.5f);
                    v[7] = new Vector3(width0, height1, depth * 0.5f);

                    AddVertex(v[0]);
                    AddVertex(v[1]);
                    AddVertex(v[2]);
                    AddVertex(v[3]);
                    AddNormal(Vector3.back);
                    AddNormal(Vector3.back);
                    AddNormal(Vector3.back);
                    AddNormal(Vector3.back);

                    AddVertex(v[4]);
                    AddVertex(v[5]);
                    AddVertex(v[6]);
                    AddVertex(v[7]);
                    AddNormal(Vector3.forward);
                    AddNormal(Vector3.forward);
                    AddNormal(Vector3.forward);
                    AddNormal(Vector3.forward);

                    if (i != 0)
                    {
                        AddVertex(v[4]);
                        AddVertex(v[0]);
                        AddVertex(v[3]);
                        AddVertex(v[7]);
                        AddNormal(Vector3.left);
                        AddNormal(Vector3.left);
                        AddNormal(Vector3.left);
                        AddNormal(Vector3.left);
                    }

                    if (i != columnCount)
                    {
                        AddVertex(v[1]);
                        AddVertex(v[5]);
                        AddVertex(v[6]);
                        AddVertex(v[2]);
                        AddNormal(Vector3.right);
                        AddNormal(Vector3.right);
                        AddNormal(Vector3.right);
                        AddNormal(Vector3.right);
                    }
                }
            }

            SetTextureCoords();
        }
        #endregion

        #region GenerateTriangles
        private void GenerateTriangles()
        {
            faces.Clear();

            faces.Add(new TriangleIndices(0, 3, 1));            // top
            faces.Add(new TriangleIndices(1, 3, 2));

            faces.Add(new TriangleIndices(4, 7, 5));            // bottom
            faces.Add(new TriangleIndices(5, 7, 6));

            faces.Add(new TriangleIndices(8, 11, 9));            // left
            faces.Add(new TriangleIndices(9, 11, 10));

            faces.Add(new TriangleIndices(13, 12, 14));            // right
            faces.Add(new TriangleIndices(14, 12, 15));

            int columnCount = properties.columnCount;
            int rowCount = properties.rowCount;

            // horizontal bars
            int index = 16;
            for (int i = 0; i <= rowCount; ++i)
            {
                faces.Add(new TriangleIndices(index + 1, index, index + 2));            // front
                faces.Add(new TriangleIndices(index + 2, index, index + 3));

                faces.Add(new TriangleIndices(index + 4, index + 5, index + 7));        // back
                faces.Add(new TriangleIndices(index + 7, index + 5, index + 6));

                index += 8;

                for (int j = 0; j < columnCount; ++j)
                {
                    if ((i == 0) || (i == rowCount))
                    {
                        faces.Add(new TriangleIndices(index + 1, index + 0, index + 2));       // top or bottom
                        faces.Add(new TriangleIndices(index + 2, index + 0, index + 3));

                        index += 4;
                    }
                    else
                    {
                        faces.Add(new TriangleIndices(index + 1, index + 0, index + 2));       // top
                        faces.Add(new TriangleIndices(index + 2, index + 0, index + 3));

                        faces.Add(new TriangleIndices(index + 5, index + 4, index + 6));     // bottom
                        faces.Add(new TriangleIndices(index + 6, index + 4, index + 7));

                        index += 8;
                    }
                }
            }

            // vertical bars
            for (int i = 0; i <= columnCount; ++i)
            {
                for (int j = 0; j < rowCount; ++j)
                {
                    faces.Add(new TriangleIndices(index + 1, index, index + 2));            // front
                    faces.Add(new TriangleIndices(index + 2, index, index + 3));

                    faces.Add(new TriangleIndices(index + 4, index + 5, index + 7));        // back
                    faces.Add(new TriangleIndices(index + 7, index + 5, index + 6));

                    if ((i == 0) || (i == columnCount))
                    {
                        if (i == 0)
                        {
                            faces.Add(new TriangleIndices(index + 9, index + 8, index + 10));       // right
                            faces.Add(new TriangleIndices(index + 10, index + 8, index + 11));
                        }
                        else if (i == columnCount)
                        {
                            faces.Add(new TriangleIndices(index + 9, index + 8, index + 10));     // left
                            faces.Add(new TriangleIndices(index + 10, index + 8, index + 11));
                        }

                        index += 12;
                    }
                    else
                    {
                        faces.Add(new TriangleIndices(index + 9, index + 8, index + 10));       // left
                        faces.Add(new TriangleIndices(index + 10, index + 8, index + 11));

                        faces.Add(new TriangleIndices(index + 13, index + 12, index + 14));     // right
                        faces.Add(new TriangleIndices(index + 14, index + 12, index + 15));

                        index += 16;
                    }
                }
            }
        }
        #endregion

        #region SetTextureCoords
        private void SetTextureCoords()
        {
            if (!properties.genTextureCoords) return;

            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;

            int columnCount = properties.columnCount;
            int rowCount = properties.rowCount;

            float borderWidth = properties.borderWidth;
            float borderHeight = properties.borderHeight;

            float cellWidth = (width - borderWidth * (columnCount + 1)) / columnCount;
            float cellHegiht = (height - borderHeight * (rowCount + 1)) / rowCount;

            float vf1 = depth / width;
            // top
            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, -vf1));
            AddUV(new Vector2(0, -vf1));

            // bottom
            AddUV(new Vector2(0, vf1));
            AddUV(new Vector2(1, vf1));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(0, 0));

            // left
            AddUV(new Vector2(vf1, 0));
            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(0, 1));
            AddUV(new Vector2(vf1, 1));

            // right
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1 - vf1, 0));
            AddUV(new Vector2(1 - vf1, 1));
            AddUV(new Vector2(1, 1));

            if (!properties.textureWrapped)
            {
                // for horizontal bars
                for (int i = 0; i <= rowCount; ++i)
                {
                    float v0 = (i * (cellHegiht + borderHeight)) / height;
                    float v1 = (i * (cellHegiht + borderHeight) + borderHeight) / height;

                    AddUV(new Vector2(0, v0));
                    AddUV(new Vector2(1, v0));
                    AddUV(new Vector2(1, v1));
                    AddUV(new Vector2(0, v1));

                    AddUV(new Vector2(0, v0));
                    AddUV(new Vector2(1, v0));
                    AddUV(new Vector2(1, v1));
                    AddUV(new Vector2(0, v1));

                    for (int j = 0; j < columnCount; ++j)
                    {
                        float u0 = (j * (cellWidth + borderWidth) + borderWidth) / width;
                        float u1 = ((j + 1) * (cellWidth + borderWidth)) / width;

                        if (i != rowCount)
                        {
                            float v00 = v1;
                            float v01 = v0 + depth / height;

                            AddUV(new Vector2(u0, v00));
                            AddUV(new Vector2(u1, v00));
                            AddUV(new Vector2(u1, v01));
                            AddUV(new Vector2(u0, v01));
                        }

                        if (i != 0)
                        {
                            float v00 = v0;
                            float v01 = v0 + depth / height;

                            AddUV(new Vector2(u0, v00));
                            AddUV(new Vector2(u1, v00));
                            AddUV(new Vector2(u1, v01));
                            AddUV(new Vector2(u0, v01));
                        }
                    }
                }

                // for vertical bars
                for (int i = 0; i <= columnCount; ++i)
                {
                    for (int j = 0; j < rowCount; ++j)
                    {
                        float u0 = (i * (cellWidth + borderWidth)) / width;
                        float u1 = (i * (cellWidth + borderWidth) + borderWidth) / width;

                        float v0 = (j * (cellHegiht + borderHeight) + borderHeight) / height;
                        float v1 = ((j + 1) * (cellHegiht + borderHeight)) / height;

                        AddUV(new Vector2(u0, v0));
                        AddUV(new Vector2(u1, v0));
                        AddUV(new Vector2(u1, v1));
                        AddUV(new Vector2(u0, v1));

                        AddUV(new Vector2(u0, v0));
                        AddUV(new Vector2(u1, v0));
                        AddUV(new Vector2(u1, v1));
                        AddUV(new Vector2(u0, v1));

                        if (i != 0)
                        {
                            float u01 = u0;
                            float u00 = u01 + depth / width;
                            AddUV(new Vector2(u00, v0));
                            AddUV(new Vector2(u01, v0));
                            AddUV(new Vector2(u01, v1));
                            AddUV(new Vector2(u00, v1));
                        }

                        if (i != columnCount)
                        {
                            float u00 = u1;
                            float u01 = u00 + depth / width;
                            AddUV(new Vector2(u00, v0));
                            AddUV(new Vector2(u01, v0));
                            AddUV(new Vector2(u01, v1));
                            AddUV(new Vector2(u00, v1));
                        }
                    }
                }
            }
            else
            {
                // for horizontal bars
                for (int i = 0; i <= rowCount; ++i)
                {
                    float v0 = (i * (cellHegiht + borderHeight)) / height;
                    float v1 = (i * (cellHegiht + borderHeight) + borderHeight) / height;

                    AddUV(new Vector2(0, v0));
                    AddUV(new Vector2(1, v0));
                    AddUV(new Vector2(1, v1));
                    AddUV(new Vector2(0, v1));

                    AddUV(new Vector2(0, v0));
                    AddUV(new Vector2(1, v0));
                    AddUV(new Vector2(1, v1));
                    AddUV(new Vector2(0, v1));

                    for (int j = 0; j < columnCount; ++j)
                    {
                        float u0 = (j * (cellWidth + borderWidth) + borderWidth) / width;
                        float u1 = ((j + 1) * (cellWidth + borderWidth)) / width;

                        if (i != rowCount)
                        {
                            float v00 = v1;
                            float v01 = v0 + depth / height;

                            AddUV(new Vector2(u0, v00));
                            AddUV(new Vector2(u1, v00));
                            AddUV(new Vector2(u1, v01));
                            AddUV(new Vector2(u0, v01));
                        }

                        if (i != 0)
                        {
                            float v00 = v0;
                            float v01 = v0 + depth / height;

                            AddUV(new Vector2(u0, v00));
                            AddUV(new Vector2(u1, v00));
                            AddUV(new Vector2(u1, v01));
                            AddUV(new Vector2(u0, v01));
                        }
                    }
                }

                // for vertical bars
                for (int i = 0; i <= columnCount; ++i)
                {
                    for (int j = 0; j < rowCount; ++j)
                    {
                        float u0 = (i * (cellWidth + borderWidth)) / width;
                        float u1 = (i * (cellWidth + borderWidth) + borderWidth) / width;

                        float v0 = (j * (cellHegiht + borderHeight) + borderHeight) / height;
                        float v1 = ((j + 1) * (cellHegiht + borderHeight)) / height;

                        AddUV(new Vector2(u0, v0));
                        AddUV(new Vector2(u1, v0));
                        AddUV(new Vector2(u1, v1));
                        AddUV(new Vector2(u0, v1));

                        AddUV(new Vector2(u0, v0));
                        AddUV(new Vector2(u1, v0));
                        AddUV(new Vector2(u1, v1));
                        AddUV(new Vector2(u0, v1));

                        if (i != 0)
                        {
                            float u01 = u0;
                            float u00 = u01 + depth / width;
                            AddUV(new Vector2(u00, v0));
                            AddUV(new Vector2(u01, v0));
                            AddUV(new Vector2(u01, v1));
                            AddUV(new Vector2(u00, v1));
                        }

                        if (i != columnCount)
                        {
                            float u00 = u1;
                            float u01 = u00 + depth / width;
                            AddUV(new Vector2(u00, v0));
                            AddUV(new Vector2(u01, v0));
                            AddUV(new Vector2(u01, v1));
                            AddUV(new Vector2(u00, v1));
                        }
                    }
                }
            }
        }
        #endregion
    }
}
