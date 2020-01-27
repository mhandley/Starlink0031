using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcPlaneMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcPlaneProperties : QcBaseProperties
        {
            public float width = 1;
            public float height = 1;
            public int widthSegments = 1;
            public int heightSegments = 1;

            public bool doubleSided = false;
            public enum FaceDirection
            {
                Left,
                Right,
                Up,
                Down,
                Back,
                Forward
            }

            public FaceDirection direction = FaceDirection.Up;

            public void CopyFrom(QcPlaneProperties source)
            {
                base.CopyFrom(source);

                this.width = source.width;
                this.height = source.height;
                this.widthSegments = source.widthSegments;
                this.heightSegments = source.heightSegments;
                this.doubleSided = source.doubleSided;
                this.direction = source.direction;
            }

            public bool Modified(QcPlaneProperties source)
            {
                if ((this.width == source.width) && (this.height == source.height) &&
                    (this.widthSegments == source.widthSegments) && (this.heightSegments == source.heightSegments) &&
                    (this.direction == source.direction) &&
                    (this.doubleSided == source.doubleSided) &&
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

        public QcPlaneProperties properties = new QcPlaneProperties();

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
                mesh.uv = uvs.ToArray();
            else
                mesh.uv = null;

            mesh.triangles = triangles;

            // set collider bound
            SetBoxCollider();
            
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

                // set collider bound
                SetBoxCollider();

                meshFilter.sharedMesh.RecalculateBounds();
            }
        }
        #endregion

        #region GenerateVertices
        private void GenerateVertices()
        {
            float halfWidth = properties.width * 0.5f;
            float halfHeight = properties.height * 0.5f;

            if (properties.doubleSided)
            {
                switch (properties.direction)
                {
                    case QcPlaneProperties.FaceDirection.Left:
                    case QcPlaneProperties.FaceDirection.Right:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(0,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight,
                                                      -properties.width * (float)i / properties.widthSegments + halfWidth));
                                AddNormal(Vector3.left);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                    
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(0,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight,
                                                      properties.width * (float)i / properties.widthSegments - halfWidth));
                                AddNormal(Vector3.right);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;

                    case QcPlaneProperties.FaceDirection.Up:
                    case QcPlaneProperties.FaceDirection.Down:
                    default:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(properties.width * (float)i / properties.widthSegments - halfWidth,
                                          0,
                                          properties.height * (float)j / properties.heightSegments - halfHeight));
                                AddNormal(Vector3.up);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                    
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(properties.width * (float)i / properties.widthSegments - halfWidth,
                                                      0,
                                                      -properties.height * (float)j / properties.heightSegments + halfHeight));
                                AddNormal(Vector3.down);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;

                    case QcPlaneProperties.FaceDirection.Back:
                    case QcPlaneProperties.FaceDirection.Forward:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(-properties.width * (float)i / properties.widthSegments + halfWidth,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight, 0));
                                AddNormal(Vector3.forward);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                    
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(properties.width * (float)i / properties.widthSegments - halfWidth,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight, 0));
                                AddNormal(Vector3.back);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;
                }
            }
            else
            {
                switch (properties.direction)
                {
                    case QcPlaneProperties.FaceDirection.Left:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(0,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight,
                                                      -properties.width * (float)i / properties.widthSegments + halfWidth));
                                AddNormal(Vector3.left);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;

                    case QcPlaneProperties.FaceDirection.Right:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(0,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight,
                                                      properties.width * (float)i / properties.widthSegments - halfWidth));
                                AddNormal(Vector3.right);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;

                    case QcPlaneProperties.FaceDirection.Up:
                    default:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(properties.width * (float)i / properties.widthSegments - halfWidth,
                                          0,
                                          properties.height * (float)j / properties.heightSegments - halfHeight));
                                AddNormal(Vector3.up);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;

                    case QcPlaneProperties.FaceDirection.Down:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(properties.width * (float)i / properties.widthSegments - halfWidth,
                                                      0,
                                                      -properties.height * (float)j / properties.heightSegments + halfHeight));
                                AddNormal(Vector3.down);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;

                    case QcPlaneProperties.FaceDirection.Back:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(-properties.width * (float)i / properties.widthSegments + halfWidth,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight, 0));
                                AddNormal(Vector3.forward);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;

                    case QcPlaneProperties.FaceDirection.Forward:
                        for (int j = 0; j <= properties.heightSegments; ++j)
                        {
                            for (int i = 0; i <= properties.widthSegments; ++i)
                            {
                                AddVertex(new Vector3(properties.width * (float)i / properties.widthSegments - halfWidth,
                                                      properties.height * (float)j / properties.heightSegments - halfHeight, 0));
                                AddNormal(Vector3.back);
                                if (properties.genTextureCoords)
                                    AddUV(new Vector3((float)i / properties.widthSegments, (float)j / properties.heightSegments, 0));
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region GenerateTriangles
        private void GenerateTriangles()
        {
            if (properties.doubleSided)
            {
                for (int j = 0; j < properties.heightSegments; ++j)
                {
                    for (int i = 0; i < properties.widthSegments; ++i)
                    {
                        faces.Add(new TriangleIndices(j * (properties.widthSegments + 1) + i + 1,
                                                      j * (properties.widthSegments + 1) + i,
                                                      (j + 1) * (properties.widthSegments + 1) + i + 1));
                        faces.Add(new TriangleIndices((j + 1) * (properties.widthSegments + 1) + i + 1,
                                                      j * (properties.widthSegments + 1) + i,
                                                      (j + 1) * (properties.widthSegments + 1) + i));
                    }
                }

                int rowHeight = properties.heightSegments + 1;
                for (int j = 0; j < properties.heightSegments; ++j)
                {
                    for (int i = 0; i < properties.widthSegments; ++i)
                    {
                        int nj = rowHeight + j;
                        faces.Add(new TriangleIndices(nj * (properties.widthSegments + 1) + i + 1,
                                                      nj * (properties.widthSegments + 1) + i,
                                                      (nj + 1) * (properties.widthSegments + 1) + i + 1));
                        faces.Add(new TriangleIndices((nj + 1) * (properties.widthSegments + 1) + i + 1,
                                                      nj * (properties.widthSegments + 1) + i,
                                                      (nj + 1) * (properties.widthSegments + 1) + i));
                    }
                }
            }
            else
            {
                for (int j = 0; j < properties.heightSegments; ++j)
                {
                    for (int i = 0; i < properties.widthSegments; ++i)
                    {
                        faces.Add(new TriangleIndices(j * (properties.widthSegments + 1) + i + 1,
                                                      j * (properties.widthSegments + 1) + i,
                                                      (j + 1) * (properties.widthSegments + 1) + i + 1));
                        faces.Add(new TriangleIndices((j + 1) * (properties.widthSegments + 1) + i + 1,
                                                      j * (properties.widthSegments + 1) + i,
                                                      (j + 1) * (properties.widthSegments + 1) + i));
                    }
                }
            }
        }
        #endregion

        private void SetBoxCollider()
        {
            if (properties.addCollider)
            {
                BoxCollider collider = gameObject.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = gameObject.AddComponent<BoxCollider>();
                }

                const float thickness = 0.001f;

                collider.enabled = true;
                collider.center = properties.offset;

                switch (properties.direction)
                {
                    case QcPlaneProperties.FaceDirection.Left:
                    case QcPlaneProperties.FaceDirection.Right:
                        collider.size = new Vector3(thickness, properties.height, properties.width);
                        break;

                    case QcPlaneProperties.FaceDirection.Up:
                    case QcPlaneProperties.FaceDirection.Down:
                    default:
                        collider.size = new Vector3(properties.width, thickness, properties.height);
                        break;

                    case QcPlaneProperties.FaceDirection.Back:
                    case QcPlaneProperties.FaceDirection.Forward:
                        collider.size = new Vector3(properties.width, properties.height, thickness);
                        break;
                }
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
    }
 }
