using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcCircleMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcCircleProperties : QcBaseProperties
        {
            public float radius = 1.0f;
            public int segments = 8;

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

            public void CopyFrom(QcCircleProperties source)
            {
                base.CopyFrom(source);

                this.radius = source.radius;
                this.segments = source.segments;
                this.doubleSided = source.doubleSided;
                this.direction = source.direction;
            }

            public bool Modified(QcCircleProperties source)
            {
                if ((this.radius == source.radius) && (this.segments == source.segments) &&
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

        public QcCircleProperties properties = new QcCircleProperties();

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
            int segments = properties.segments;
            float radius = properties.radius;

            if (properties.doubleSided)
            {
                switch (properties.direction)
                {
                    case QcCircleProperties.FaceDirection.Forward:
                    case QcCircleProperties.FaceDirection.Back:
                    default:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.back);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, radius * sin1, 0));
                            AddNormal(Vector3.back);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                    
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.forward);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = -twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, radius * sin1, 0));
                            AddNormal(Vector3.forward);
                            AddUV(new Vector2((-cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                        break;

                    case QcCircleProperties.FaceDirection.Left:
                    case QcCircleProperties.FaceDirection.Right:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.left);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(0, radius * sin1, -radius * cos1));
                            AddNormal(Vector3.left);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }

                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.right);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = -twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(0, radius * sin1, -radius * cos1));
                            AddNormal(Vector3.right);
                            AddUV(new Vector2((-cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                        break;

                    case QcCircleProperties.FaceDirection.Up:
                    case QcCircleProperties.FaceDirection.Down:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.up);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = -twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, 0, -radius * sin1));
                            AddNormal(Vector3.up);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (-sin1 + 1f) * 0.5f));
                        }
                    
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.down);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, 0, -radius * sin1));
                            AddNormal(Vector3.down);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                        break;
                }
            }
            else
            {
                switch (properties.direction)
                {
                    case QcCircleProperties.FaceDirection.Forward:
                    default:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.back);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, radius * sin1, 0));
                            AddNormal(Vector3.back);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                        break;

                    case QcCircleProperties.FaceDirection.Back:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.forward);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = -twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, radius * sin1, 0));
                            AddNormal(Vector3.forward);
                            AddUV(new Vector2((-cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                        break;

                    case QcCircleProperties.FaceDirection.Left:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.left);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(0, radius * sin1, -radius * cos1));
                            AddNormal(Vector3.left);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                        break;

                    case QcCircleProperties.FaceDirection.Right:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.right);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = -twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(0, radius * sin1, -radius * cos1));
                            AddNormal(Vector3.right);
                            AddUV(new Vector2((-cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
                        }
                        break;

                    case QcCircleProperties.FaceDirection.Up:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.up);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = -twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, 0, -radius * sin1));
                            AddNormal(Vector3.up);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (-sin1 + 1f) * 0.5f));
                        }
                        break;

                    case QcCircleProperties.FaceDirection.Down:
                        AddVertex(Vector3.zero);      // center vertex
                        AddNormal(Vector3.down);
                        AddUV(new Vector2(0.5f, 0.5f));

                        for (int i = 0; i < segments; ++i)
                        {
                            float a1 = twoPi * (float)i / segments;
                            float sin1 = Mathf.Sin(a1);
                            float cos1 = Mathf.Cos(a1);

                            AddVertex(new Vector3(radius * cos1, 0, -radius * sin1));
                            AddNormal(Vector3.down);
                            AddUV(new Vector2((cos1 + 1f) * 0.5f, (sin1 + 1f) * 0.5f));
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
                for (int i = 0; i < properties.segments; ++i)
                {
                    int baseIndex = i + 1;
                    faces.Add(new TriangleIndices(baseIndex, 0, (i + 1) % properties.segments + 1));
                }

                for (int i = 0; i < properties.segments; ++i)
                {
                    int baseIndex = i + properties.segments + 2;
                    faces.Add(new TriangleIndices(baseIndex, properties.segments + 1, (i + 1) % properties.segments + properties.segments + 2));
                }
            }
            else
            {
                for (int i = 0; i < properties.segments; ++i)
                {
                    int baseIndex = i + 1;
                    faces.Add(new TriangleIndices(baseIndex, 0, baseIndex % properties.segments + 1));
                }
            }
        }
        #endregion

        private void SetBoxCollider()
        {
            //if (properties.addCollider)
            //{
            //    SphereCollider collider = gameObject.GetComponent<SphereCollider>();
            //    if (collider == null)
            //    {
            //        collider = gameObject.AddComponent<SphereCollider>();
            //    }

            //    collider.enabled = true;
            //    collider.center = properties.offset;
            //    collider.radius = properties.radius;
            //}
            //else
            //{
            //    SphereCollider collider = gameObject.GetComponent<SphereCollider>();
            //    if (collider != null)
            //    {
            //        collider.enabled = false;
            //    }
            //}

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
                    case QcCircleProperties.FaceDirection.Left:
                    case QcCircleProperties.FaceDirection.Right:
                        collider.size = new Vector3(thickness, properties.radius * 2, properties.radius * 2);
                        break;

                    case QcCircleProperties.FaceDirection.Up:
                    case QcCircleProperties.FaceDirection.Down:
                    default:
                        collider.size = new Vector3(properties.radius * 2, thickness, properties.radius * 2);
                        break;

                    case QcCircleProperties.FaceDirection.Back:
                    case QcCircleProperties.FaceDirection.Forward:
                        collider.size = new Vector3(properties.radius * 2, properties.radius * 2, thickness);
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
