using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcTorusMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcTorusProperties : QcBaseProperties
        {
            public float radius = 1;
            public float ringRadius = 0.2f;

            public bool sliceOn = false;
            public float sliceFrom = 0.0f;
            public float sliceTo = 0.0f;
            
            public int torusSegments = 24;
            public int ringSegments = 16;

            public void CopyFrom(QcTorusProperties source)
            {
                base.CopyFrom(source);

                this.radius = source.radius;
                this.ringRadius = source.ringRadius;

                this.sliceOn = source.sliceOn;
                this.sliceFrom = source.sliceFrom;
                this.sliceTo = source.sliceTo;

                this.torusSegments = source.torusSegments;
                this.ringSegments = source.ringSegments;
            }

            public bool Modified(QcTorusProperties source)
            {
                bool offsetChanged = (this.offset[0] != source.offset[0]) || (this.offset[1] != source.offset[1]) || (this.offset[2] != source.offset[2]);

                return ((this.radius != source.radius) ||
                    (this.ringRadius != source.ringRadius) ||
                    (this.torusSegments != source.torusSegments) ||
                    (this.sliceOn != source.sliceOn) ||
                    (this.sliceOn && ((this.sliceFrom != source.sliceFrom) || (this.sliceTo != source.sliceTo))) ||
                    (this.ringSegments != source.ringSegments) ||
                    (this.genTextureCoords != source.genTextureCoords) ||
                    (this.addCollider != source.addCollider) ||
                    offsetChanged);
            }
        }

        public QcTorusProperties properties = new QcTorusProperties();

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
            if (properties.radius <= 0)
                return;

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

            meshFilter.sharedMesh.Clear();

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
                collider.size = new Vector3((properties.ringRadius + properties.radius) * 2,
                                            properties.ringRadius * 2,
                                            (properties.ringRadius + properties.radius) * 2);
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
        //
        // adapted from http://wiki.unity3d.com/index.php/ProceduralPrimitives
        //
        private void GenerateVertices()
        {
            #region Vertices	
            bool sideCap = (properties.sliceOn) &&
                (properties.sliceFrom != properties.sliceTo) &&
                (Mathf.Abs(properties.sliceFrom - properties.sliceTo) < 360);

            float partAngle;
            float startAngle = 0;
            float endAngle = 0;
            if (!sideCap)
            {
                partAngle = (2f * Mathf.PI) / properties.torusSegments;
            }
            else
            {
                float sliceTo = properties.sliceTo;
                float sliceFrom = properties.sliceFrom;
                if (sliceFrom > sliceTo)
                {
                    sliceTo += 360;
                }
                startAngle = sliceFrom * Mathf.Deg2Rad;
                endAngle = sliceTo * Mathf.Deg2Rad;
                partAngle = (endAngle - startAngle) / properties.torusSegments;
            }

            for (int seg = 0; seg <= properties.torusSegments; ++seg)
            {
                //int currSeg = (seg == properties.torusSegments) ? 0 : seg;

                //float t1 = (float)currSeg / properties.torusSegments * twoPi;
                float t1 = seg * partAngle + startAngle;
                Vector3 r1 = new Vector3(Mathf.Cos(t1) * properties.radius, 0f, Mathf.Sin(t1) * properties.radius);

                for (int side = 0; side <= properties.ringSegments; ++side)
                {
                    int currSide = side == properties.ringSegments ? 0 : side;

                    float t2 = (float)currSide / properties.ringSegments * twoPi;
                    Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * 
                                 new Vector3(Mathf.Sin(t2) * properties.ringRadius, Mathf.Cos(t2) * properties.ringRadius);

                    AddVertex(r1 + r2);
                }
            }
            #endregion

            #region Normals
            for (int seg = 0; seg <= properties.torusSegments; ++seg)
            {
                float t1 = seg * partAngle + startAngle;
                Vector3 r1 = new Vector3(Mathf.Cos(t1) * properties.radius, 0f, Mathf.Sin(t1) * properties.radius);

                for (int side = 0; side <= properties.ringSegments; ++side)
                {
                    AddNormal((vertices[side + seg * (properties.ringSegments + 1)] - r1).normalized);
                }
            }
            #endregion

            #region UVs
            if (properties.genTextureCoords)
            {
                for (int seg = 0; seg <= properties.torusSegments; ++seg)
                {
                    for (int side = 0; side <= properties.ringSegments; ++side)
                    {
                        AddUV(new Vector2((float)seg / properties.torusSegments, 1 - (float)side / properties.ringSegments));
                    }
                }
            }

            if (sideCap)
            {
                // Add caps on both ends
                float t1 = startAngle;
                Vector3 r1 = new Vector3(Mathf.Cos(t1) * properties.radius, 0f, Mathf.Sin(t1) * properties.radius);

                AddVertex(r1);

                Vector3 circleNormal = Vector3.right;

                for (int side = 0; side <= properties.ringSegments; ++side)
                {
                    int currSide = side == properties.ringSegments ? 0 : side;

                    float t2 = (float)currSide / properties.ringSegments * twoPi;
                    Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) *
                                 new Vector3(Mathf.Sin(t2) * properties.ringRadius, Mathf.Cos(t2) * properties.ringRadius);

                    AddVertex(r1 + r2);
                    if (side == 0)
                    {
                        circleNormal = ComputeNormal(Vector3.zero, r2, r1);
                        AddNormal(circleNormal);     // for the vertex r1
                        AddUV(Vector2.zero);
                    }
                    AddNormal(circleNormal);
                    AddUV(Vector2.zero);
                }

                t1 = endAngle;
                r1 = new Vector3(Mathf.Cos(t1) * properties.radius, 0f, Mathf.Sin(t1) * properties.radius);

                AddVertex(r1);

                for (int side = 0; side <= properties.ringSegments; ++side)
                {
                    int currSide = side == properties.ringSegments ? 0 : side;

                    float t2 = (float)currSide / properties.ringSegments * twoPi;
                    Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) *
                                 new Vector3(Mathf.Sin(t2) * properties.ringRadius, Mathf.Cos(t2) * properties.ringRadius);

                    AddVertex(r1 + r2);
                    if (side == 0)
                    {
                        circleNormal = ComputeNormal(Vector3.zero, r1, r2);
                        AddNormal(circleNormal);        // for the vertex r1
                        AddUV(Vector2.zero);
                    }
                    AddNormal(circleNormal);
                    AddUV(Vector2.zero);
                }
            }
            #endregion

            #region Triangles
            faces.Clear();

            if (!sideCap)
            {
                for (int seg = 0; seg <= properties.torusSegments; seg++)
                {
                    for (int side = 0; side <= properties.ringSegments - 1; side++)
                    {
                        int current = side + seg * (properties.ringSegments + 1);
                        int next = side + (seg < (properties.torusSegments) ? (seg + 1) * (properties.ringSegments + 1) : 0);

                        faces.Add(new TriangleIndices(current, next, next + 1));
                        faces.Add(new TriangleIndices(current, next + 1, current + 1));
                    }
                }
            }
            else
            {
                for (int seg = 0; seg < properties.torusSegments; seg++)
                {
                    for (int side = 0; side <= properties.ringSegments - 1; side++)
                    {
                        int current = side + seg * (properties.ringSegments + 1);
                        int next = side + (seg + 1) * (properties.ringSegments + 1);

                        faces.Add(new TriangleIndices(current, next, next + 1));
                        faces.Add(new TriangleIndices(current, next + 1, current + 1));
                    }
                }

                int baseIndex = (properties.torusSegments + 1) * (properties.ringSegments + 1);
                // Add caps on both ends
                for (int side = 0; side <= properties.ringSegments; ++side)
                {
                    faces.Add(new TriangleIndices(baseIndex, baseIndex + side + 0, baseIndex + side + 1));
                }

                baseIndex += properties.ringSegments + 2;
                for (int side = 0; side <= properties.ringSegments; ++side)
                {
                    faces.Add(new TriangleIndices(baseIndex, baseIndex + side + 1, baseIndex + side));
                }
            }
            #endregion
        }
        #endregion
    }
}


