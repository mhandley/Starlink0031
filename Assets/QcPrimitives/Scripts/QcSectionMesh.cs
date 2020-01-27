using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcSectionMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcSectionProperties : QcBaseProperties
        {
            [System.Serializable]
            public class SlantedSides
            {
                public Vector2 size;
            }

            public enum Types { LType, IType, CType, TType }
            public enum Options { None, SlantedSides }

            public float width = 1;
            public float depth = 1;
            public float height = 1;
            public float frontThickness = 0.2f;
            public float backThickness = 0.2f;
            public float sideThickness = 0.2f;

            public bool capThickness = false;
            public float frontCap = 0.2f;
            public float backCap = 0.2f;
            public float sideCap = 0.2f;

            public Types type = new Types();
            public Options option = Options.None;
            public SlantedSides slantedSides = new SlantedSides();
            public bool textureWrapped;

            public void CopyFrom(QcSectionProperties source)
            {
                base.CopyFrom(source);

                this.width = source.width;
                this.height = source.height;
                this.depth = source.depth;
                this.frontThickness = source.frontThickness;
                this.backThickness = source.backThickness;
                this.sideThickness = source.sideThickness;

                this.capThickness = source.capThickness;
                this.frontCap = source.frontCap;
                this.backCap = source.backCap;
                this.sideCap = source.sideCap;

                this.type = source.type;

                this.option = source.option;
                this.slantedSides.size = source.slantedSides.size;

                this.textureWrapped = source.textureWrapped;
            }

            public bool Modified(QcSectionProperties source)
            {
                return ((this.width != source.width) ||
                        (this.height != source.height) ||
                        (this.depth != source.depth) ||
                        (this.frontThickness != source.frontThickness) ||
                        (this.backThickness != source.backThickness) ||
                        (this.sideThickness != source.sideThickness) ||
                        (this.capThickness != source.capThickness) ||
                        (this.capThickness && 
                         ((this.frontCap != source.frontCap) || 
                          (this.backCap != source.backCap) || 
                          (this.sideCap != source.sideCap))) ||
                        (this.offset[0] != source.offset[0]) ||
                        (this.offset[1] != source.offset[1]) ||
                        (this.offset[2] != source.offset[2]) ||
                        (this.genTextureCoords != source.genTextureCoords) ||
                        (this.textureWrapped != source.textureWrapped) ||
                        (this.addCollider != source.addCollider) ||
                        (this.type != source.type) ||
                        (this.option != source.option) ||
                        ((source.option == QcSectionProperties.Options.SlantedSides) &&
                         ((this.slantedSides.size[0] != source.slantedSides.size[0]) ||
                          (this.slantedSides.size[1] != source.slantedSides.size[1]))));
            }
        }

        public QcSectionProperties properties = new QcSectionProperties();

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

            GenerateGeometry();

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
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            ClearVertices();

            GenerateGeometry();

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
                collider.center = properties.offset + new Vector3(0, 
                                                                  properties.height * 0.5f, 
                                                                  0);
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

        private void GenerateGeometry()
        {
            switch (properties.type)
            {
                case QcSectionProperties.Types.LType:
                default:
                    GenerateVerticesLType();
                    GenerateTrianglesLType();
                    break;

                case QcSectionProperties.Types.IType:
                    GenerateVerticesIType();
                    GenerateTrianglesIType();
                    break;

                case QcSectionProperties.Types.CType:
                    GenerateVerticesCType();
                    GenerateTrianglesCType();
                    break;

                case QcSectionProperties.Types.TType:
                    GenerateVerticesTType();
                    GenerateTrianglesTType();
                    break;
            }
        }

        #region GenerateVerticesLType
        private void GenerateVerticesLType()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;
            float backThicknees = properties.backThickness;
            float sideThickness = properties.sideThickness;

            Vector3[] pts = new Vector3[16];
            if (!properties.capThickness)
            {
                pts[0] = new Vector3(-width * 0.5f, 0, -depth * 0.5f);
                pts[1] = new Vector3(-width * 0.5f + sideThickness, 0, -depth * 0.5f);
                pts[2] = new Vector3(-width * 0.5f + sideThickness, 0, depth * 0.5f - backThicknees);
                pts[3] = new Vector3(width * 0.5f, 0, depth * 0.5f - backThicknees);

                pts[4] = new Vector3(width * 0.5f, 0, depth * 0.5f);
                pts[5] = new Vector3(-width * 0.5f + sideThickness, 0, depth * 0.5f);
                pts[6] = new Vector3(-width * 0.5f, 0, depth * 0.5f);
                pts[7] = new Vector3(-width * 0.5f, 0, depth * 0.5f - backThicknees);

                pts[8] = new Vector3(-width * 0.5f, height, -depth * 0.5f);
                pts[9] = new Vector3(-width * 0.5f + sideThickness, height, -depth * 0.5f);
                pts[10] = new Vector3(-width * 0.5f + sideThickness, height, depth * 0.5f - backThicknees);
                pts[11] = new Vector3(width * 0.5f, height, depth * 0.5f - backThicknees);

                pts[12] = new Vector3(width * 0.5f, height, depth * 0.5f);
                pts[13] = new Vector3(-width * 0.5f + sideThickness, height, depth * 0.5f);
                pts[14] = new Vector3(-width * 0.5f, height, depth * 0.5f);
                pts[15] = new Vector3(-width * 0.5f, height, depth * 0.5f - backThicknees);
            }
            else
            {
                pts[0] = new Vector3(-width * 0.5f, 0, -depth * 0.5f);
                pts[1] = new Vector3(-width * 0.5f + properties.sideCap, 0, -depth * 0.5f);
                pts[2] = new Vector3(-width * 0.5f + sideThickness, 0, depth * 0.5f - backThicknees);
                pts[3] = new Vector3(width * 0.5f, 0, depth * 0.5f - properties.backCap);

                pts[4] = new Vector3(width * 0.5f, 0, depth * 0.5f);
                pts[5] = new Vector3(-width * 0.5f + sideThickness, 0, depth * 0.5f);
                pts[6] = new Vector3(-width * 0.5f, 0, depth * 0.5f);
                pts[7] = new Vector3(-width * 0.5f, 0, depth * 0.5f - backThicknees);

                pts[8] = new Vector3(-width * 0.5f, height, -depth * 0.5f);
                pts[9] = new Vector3(-width * 0.5f + properties.sideCap, height, -depth * 0.5f);
                pts[10] = new Vector3(-width * 0.5f + sideThickness, height, depth * 0.5f - backThicknees);
                pts[11] = new Vector3(width * 0.5f, height, depth * 0.5f - properties.backCap);

                pts[12] = new Vector3(width * 0.5f, height, depth * 0.5f);
                pts[13] = new Vector3(-width * 0.5f + sideThickness, height, depth * 0.5f);
                pts[14] = new Vector3(-width * 0.5f, height, depth * 0.5f);
                pts[15] = new Vector3(-width * 0.5f, height, depth * 0.5f - backThicknees);
            }

            // bottom face
            AddVertex(pts[0]);
            AddVertex(pts[1]);
            AddVertex(pts[2]);
            AddVertex(pts[3]);

            AddVertex(pts[4]);
            AddVertex(pts[5]);
            AddVertex(pts[6]);
            AddVertex(pts[7]);

            // top face
            AddVertex(pts[8]);
            AddVertex(pts[9]);
            AddVertex(pts[10]);
            AddVertex(pts[11]);

            AddVertex(pts[12]);
            AddVertex(pts[13]);
            AddVertex(pts[14]);
            AddVertex(pts[15]);

            // front facing
            AddVertex(pts[0]);  
            AddVertex(pts[1]);
            AddVertex(pts[9]);
            AddVertex(pts[8]);

            AddVertex(pts[2]);
            AddVertex(pts[3]);
            AddVertex(pts[11]);
            AddVertex(pts[10]);

            // back facing
            AddVertex(pts[4]);
            AddVertex(pts[6]);
            AddVertex(pts[14]);
            AddVertex(pts[12]);

            // left
            AddVertex(pts[6]);
            AddVertex(pts[0]);
            AddVertex(pts[8]);
            AddVertex(pts[14]);

            // right
            AddVertex(pts[1]);
            AddVertex(pts[2]);
            AddVertex(pts[10]);
            AddVertex(pts[9]);

            AddVertex(pts[3]);
            AddVertex(pts[4]);
            AddVertex(pts[12]);
            AddVertex(pts[11]);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);

            if (!properties.capThickness)
            {
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
            }
            else
            {
                Vector3 frontNormal = new Vector3(properties.backThickness - properties.backCap, 
                                                  0, 
                                                  -(properties.width - properties.sideThickness));
                frontNormal.Normalize();
                AddNormal(frontNormal);
                AddNormal(frontNormal);
                AddNormal(frontNormal);
                AddNormal(frontNormal);
            }

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            if (!properties.capThickness)
            {
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
            }
            else
            {
                Vector3 rightNormal = new Vector3(properties.depth - properties.backThickness, 
                                                  0, 
                                                  -(properties.sideThickness - properties.sideCap));
                rightNormal.Normalize();
                AddNormal(rightNormal);
                AddNormal(rightNormal);
                AddNormal(rightNormal);
                AddNormal(rightNormal);
            }

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            AddUV(new Vector2(0, 1));   // bottom
            if (!properties.capThickness)
                AddUV(new Vector2(sideThickness / width, 1));
            else
                AddUV(new Vector2(properties.sideCap / width, 1));

            AddUV(new Vector2(sideThickness / width, backThicknees / depth));

            if (!properties.capThickness)
                AddUV(new Vector2(1, backThicknees / depth));
            else
                AddUV(new Vector2(1, properties.backCap / depth));

            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(sideThickness / width, 0));
            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(0, backThicknees / depth));

            AddUV(new Vector2(0, 0));   // top

            if (!properties.capThickness)
                AddUV(new Vector2(sideThickness / width, 0));
            else
                AddUV(new Vector2(properties.sideCap / width, 0));

            AddUV(new Vector2(sideThickness / width, 1 - backThicknees / depth));

            if (!properties.capThickness)
                AddUV(new Vector2(1,  1- backThicknees / depth));
            else
                AddUV(new Vector2(1, 1 - properties.backCap / depth));

            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(sideThickness / width, 1));
            AddUV(new Vector2(0, 1));
            AddUV(new Vector2(0, 1 - backThicknees / depth));

            AddUV(new Vector2(0, 0));   // front

            if (!properties.capThickness)
            {
                AddUV(new Vector2(sideThickness / width, 0));
                AddUV(new Vector2(sideThickness / width, 1));
            }
            else
            {
                AddUV(new Vector2(properties.sideCap / (width - sideThickness + properties.sideCap), 0));
                AddUV(new Vector2(properties.sideCap / (width - sideThickness + properties.sideCap), 1));
            }

            AddUV(new Vector2(0, 1));

            if (!properties.capThickness)
            {
                AddUV(new Vector2(sideThickness / width, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(sideThickness / width, 1));
            }
            else
            {
                AddUV(new Vector2(properties.sideCap / (width - sideThickness + properties.sideCap), 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(properties.sideCap / (width - sideThickness + properties.sideCap), 1));
            }

            AddUV(new Vector2(0, 0));   // back
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0, 0));   // left
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0, 0));   // right

            if (!properties.capThickness)
            {
                AddUV(new Vector2(1 - backThicknees / depth, 0));
                AddUV(new Vector2(1 - backThicknees / depth, 1));
            }
            else
            {
                AddUV(new Vector2(1 - properties.backCap / (depth - backThicknees + properties.backCap), 0));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThicknees + properties.backCap), 1));
            }

            AddUV(new Vector2(0, 1));

            if (!properties.capThickness)
            {
                AddUV(new Vector2(1 - backThicknees / depth, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - backThicknees / depth, 1));
            }
            else
            {
                AddUV(new Vector2(1 - properties.backCap / (depth - backThicknees + properties.backCap), 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThicknees + properties.backCap), 1));
            }
        }
        #endregion

        #region GenerateVerticesIType
        private void GenerateVerticesIType()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;
            float frontThickness = properties.frontThickness;
            float backThickness = properties.backThickness;
            float sideThickness = properties.sideThickness;

            Vector3[] pts = new Vector3[32];
            if (!properties.capThickness)
            {
                pts[0] = new Vector3(-width * 0.5f, 0, -depth * 0.5f);
                pts[1] = new Vector3(-sideThickness * 0.5f, 0, -depth * 0.5f);
                pts[2] = new Vector3(sideThickness * 0.5f, 0, -depth * 0.5f);
                pts[3] = new Vector3(width * 0.5f, 0, -depth * 0.5f);
                pts[4] = new Vector3(width * 0.5f, 0, -depth * 0.5f + frontThickness);
                pts[5] = new Vector3(sideThickness * 0.5f, 0, -depth * 0.5f + frontThickness);
                pts[6] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f - backThickness);
                pts[7] = new Vector3(width * 0.5f, 0, depth * 0.5f - backThickness);

                pts[8] = new Vector3(width * 0.5f, 0, depth * 0.5f);
                pts[9] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f);
                pts[10] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f);
                pts[11] = new Vector3(-width * 0.5f, 0, depth * 0.5f);
                pts[12] = new Vector3(-width * 0.5f, 0, depth * 0.5f - backThickness);
                pts[13] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f - backThickness);
                pts[14] = new Vector3(-sideThickness * 0.5f, 0, -depth * 0.5f + frontThickness);
                pts[15] = new Vector3(-width * 0.5f, 0, -depth * 0.5f + frontThickness);

                pts[16] = new Vector3(-width * 0.5f, height, -depth * 0.5f);
                pts[17] = new Vector3(-sideThickness * 0.5f, height, -depth * 0.5f);
                pts[18] = new Vector3(sideThickness * 0.5f, height, -depth * 0.5f);
                pts[19] = new Vector3(width * 0.5f, height, -depth * 0.5f);
                pts[20] = new Vector3(width * 0.5f, height, -depth * 0.5f + frontThickness);
                pts[21] = new Vector3(sideThickness * 0.5f, height, -depth * 0.5f + frontThickness);
                pts[22] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f - backThickness);
                pts[23] = new Vector3(width * 0.5f, height, depth * 0.5f - backThickness);

                pts[24] = new Vector3(width * 0.5f, height, depth * 0.5f);
                pts[25] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f);
                pts[26] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f);
                pts[27] = new Vector3(-width * 0.5f, height, depth * 0.5f);
                pts[28] = new Vector3(-width * 0.5f, height, depth * 0.5f - backThickness);
                pts[29] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f - backThickness);
                pts[30] = new Vector3(-sideThickness * 0.5f, height, -depth * 0.5f + frontThickness);
                pts[31] = new Vector3(-width * 0.5f, height, -depth * 0.5f + frontThickness);
            }
            else
            {
                pts[0] = new Vector3(-width * 0.5f, 0, -depth * 0.5f);
                pts[1] = new Vector3(-sideThickness * 0.5f, 0, -depth * 0.5f);
                pts[2] = new Vector3(sideThickness * 0.5f, 0, -depth * 0.5f);
                pts[3] = new Vector3(width * 0.5f, 0, -depth * 0.5f);
                pts[4] = new Vector3(width * 0.5f, 0, -depth * 0.5f + properties.frontCap);
                pts[5] = new Vector3(sideThickness * 0.5f, 0, -depth * 0.5f + frontThickness);
                pts[6] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f - backThickness);
                pts[7] = new Vector3(width * 0.5f, 0, depth * 0.5f - properties.backCap);

                pts[8] = new Vector3(width * 0.5f, 0, depth * 0.5f);
                pts[9] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f);
                pts[10] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f);
                pts[11] = new Vector3(-width * 0.5f, 0, depth * 0.5f);
                pts[12] = new Vector3(-width * 0.5f, 0, depth * 0.5f - properties.backCap);
                pts[13] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f - backThickness);
                pts[14] = new Vector3(-sideThickness * 0.5f, 0, -depth * 0.5f + frontThickness);
                pts[15] = new Vector3(-width * 0.5f, 0, -depth * 0.5f + properties.frontCap);

                pts[16] = new Vector3(-width * 0.5f, height, -depth * 0.5f);
                pts[17] = new Vector3(-sideThickness * 0.5f, height, -depth * 0.5f);
                pts[18] = new Vector3(sideThickness * 0.5f, height, -depth * 0.5f);
                pts[19] = new Vector3(width * 0.5f, height, -depth * 0.5f);
                pts[20] = new Vector3(width * 0.5f, height, -depth * 0.5f + properties.frontCap);
                pts[21] = new Vector3(sideThickness * 0.5f, height, -depth * 0.5f + frontThickness);
                pts[22] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f - backThickness);
                pts[23] = new Vector3(width * 0.5f, height, depth * 0.5f - properties.backCap);

                pts[24] = new Vector3(width * 0.5f, height, depth * 0.5f);
                pts[25] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f);
                pts[26] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f);
                pts[27] = new Vector3(-width * 0.5f, height, depth * 0.5f);
                pts[28] = new Vector3(-width * 0.5f, height, depth * 0.5f - properties.backCap);
                pts[29] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f - backThickness);
                pts[30] = new Vector3(-sideThickness * 0.5f, height, -depth * 0.5f + frontThickness);
                pts[31] = new Vector3(-width * 0.5f, height, -depth * 0.5f + properties.frontCap);
            }

            // bottom face
            AddVertex(pts[0]);
            AddVertex(pts[1]);
            AddVertex(pts[2]);
            AddVertex(pts[3]);

            AddVertex(pts[4]);
            AddVertex(pts[5]);
            AddVertex(pts[6]);
            AddVertex(pts[7]);

            AddVertex(pts[8]);
            AddVertex(pts[9]);
            AddVertex(pts[10]);
            AddVertex(pts[11]);

            AddVertex(pts[12]);
            AddVertex(pts[13]);
            AddVertex(pts[14]);
            AddVertex(pts[15]);

            // top face
            AddVertex(pts[16]);
            AddVertex(pts[17]);
            AddVertex(pts[18]);
            AddVertex(pts[19]);

            AddVertex(pts[20]);
            AddVertex(pts[21]);
            AddVertex(pts[22]);
            AddVertex(pts[23]);

            AddVertex(pts[24]);
            AddVertex(pts[25]);
            AddVertex(pts[26]);
            AddVertex(pts[27]);

            AddVertex(pts[28]);
            AddVertex(pts[29]);
            AddVertex(pts[30]);
            AddVertex(pts[31]);

            // front facing
            AddVertex(pts[0]);      // 32
            AddVertex(pts[1]);
            AddVertex(pts[17]);
            AddVertex(pts[16]);

            AddVertex(pts[1]);      // 36
            AddVertex(pts[2]);
            AddVertex(pts[18]);
            AddVertex(pts[17]);

            AddVertex(pts[2]);      // 40
            AddVertex(pts[3]);
            AddVertex(pts[19]);
            AddVertex(pts[18]);

            AddVertex(pts[12]);      // 44
            AddVertex(pts[13]);
            AddVertex(pts[29]);
            AddVertex(pts[28]);

            AddVertex(pts[6]);      // 48
            AddVertex(pts[7]);
            AddVertex(pts[23]);
            AddVertex(pts[22]);

            // back facing
            AddVertex(pts[8]);      // 52
            AddVertex(pts[9]);
            AddVertex(pts[25]);
            AddVertex(pts[24]);

            AddVertex(pts[9]);      // 56
            AddVertex(pts[10]);
            AddVertex(pts[26]);
            AddVertex(pts[25]);

            AddVertex(pts[10]);     // 60
            AddVertex(pts[11]);
            AddVertex(pts[27]);
            AddVertex(pts[26]);

            AddVertex(pts[4]);     // 64
            AddVertex(pts[5]);
            AddVertex(pts[21]);
            AddVertex(pts[20]);

            AddVertex(pts[14]);     // 68
            AddVertex(pts[15]);
            AddVertex(pts[31]);
            AddVertex(pts[30]);

            // left
            AddVertex(pts[11]);      // 72
            AddVertex(pts[12]);
            AddVertex(pts[28]);
            AddVertex(pts[27]);

            AddVertex(pts[13]);      // 76
            AddVertex(pts[14]);
            AddVertex(pts[30]);
            AddVertex(pts[29]);

            AddVertex(pts[15]);      // 80
            AddVertex(pts[0]);
            AddVertex(pts[16]);
            AddVertex(pts[31]);

            // right
            AddVertex(pts[3]);      // 84
            AddVertex(pts[4]);
            AddVertex(pts[20]);
            AddVertex(pts[19]);

            AddVertex(pts[5]);      // 88
            AddVertex(pts[6]);
            AddVertex(pts[22]);
            AddVertex(pts[21]);

            AddVertex(pts[7]);      // 92
            AddVertex(pts[8]);
            AddVertex(pts[24]);
            AddVertex(pts[23]);


            #region normals
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);

            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);

            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);

            if(!properties.capThickness)
            {
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);

                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
            }
            else
            {
                Vector3 forwardNormal0 = new Vector3(-(properties.backThickness - properties.backCap), 
                                                     0, 
                                                     -(properties.width - properties.sideThickness) * 0.5f);
                forwardNormal0.Normalize();

                AddNormal(forwardNormal0);
                AddNormal(forwardNormal0);
                AddNormal(forwardNormal0);
                AddNormal(forwardNormal0);

                Vector3 forwardNormal1 = new Vector3((properties.backThickness - properties.backCap),
                                                     0,
                                                     -(properties.width - properties.sideThickness) * 0.5f);
                forwardNormal1.Normalize();
                AddNormal(forwardNormal1);
                AddNormal(forwardNormal1);
                AddNormal(forwardNormal1);
                AddNormal(forwardNormal1);
            }

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            if (!properties.capThickness)
            {
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);

                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
            }
            else
            {
                Vector3 backNormal0 = new Vector3(properties.frontThickness - properties.frontCap,
                                                  0,
                                                  (properties.width - properties.sideThickness) * 0.5f);
                backNormal0.Normalize();

                AddNormal(backNormal0);
                AddNormal(backNormal0);
                AddNormal(backNormal0);
                AddNormal(backNormal0);

                Vector3 backNormal1 = new Vector3(-(properties.frontThickness - properties.frontCap),
                                                  0,
                                                  (properties.width - properties.sideThickness) * 0.5f);
                backNormal1.Normalize();
                AddNormal(backNormal1);
                AddNormal(backNormal1);
                AddNormal(backNormal1);
                AddNormal(backNormal1);
            }

            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            #endregion

            #region uv
            float side0 = sideThickness / width * 0.5f;

            AddUV(new Vector2(0, 1));   // bottom
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0.5f + side0, 1));
            AddUV(new Vector2(1, 1));

            if (!properties.capThickness)
                AddUV(new Vector2(1, 1 - frontThickness / depth));
            else
                AddUV(new Vector2(1, 1 - properties.frontCap / depth));

            AddUV(new Vector2(0.5f + side0, 1 - frontThickness / depth));
            AddUV(new Vector2(0.5f + side0, backThickness / depth));

            if (!properties.capThickness)
                AddUV(new Vector2(1, backThickness / depth));
            else
                AddUV(new Vector2(1, properties.backCap / depth));


            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0, 0));

            if (!properties.capThickness)
                AddUV(new Vector2(0, backThickness / depth));
            else
                AddUV(new Vector2(0, properties.backCap / depth));

            AddUV(new Vector2(0.5f - side0, backThickness / depth));
            AddUV(new Vector2(0.5f - side0, 1 - frontThickness / depth));

            if (!properties.capThickness)
                AddUV(new Vector2(0, 1 - frontThickness / depth));
            else
                AddUV(new Vector2(0, 1 - properties.frontCap / depth));

            AddUV(new Vector2(0, 0));   // top
            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(1, 0));

            if (!properties.capThickness)
                AddUV(new Vector2(1, frontThickness / depth));
            else
                AddUV(new Vector2(1, properties.frontCap / depth));
            AddUV(new Vector2(0.5f + side0, frontThickness / depth));
            AddUV(new Vector2(0.5f + side0, 1 - backThickness / depth));
            if (!properties.capThickness)
                AddUV(new Vector2(1, 1 - backThickness / depth));
            else
                AddUV(new Vector2(1, 1 - properties.backCap / depth));

            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0.5f + side0, 1));
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0, 1));

            if (!properties.capThickness)
                AddUV(new Vector2(0, 1 - backThickness / depth));
            else
                AddUV(new Vector2(0, 1 - properties.backCap / depth));

            AddUV(new Vector2(0.5f - side0, 1 - backThickness / depth));
            AddUV(new Vector2(0.5f - side0, frontThickness / depth));

            if (!properties.capThickness)
                AddUV(new Vector2(0, frontThickness / depth));
            else
                AddUV(new Vector2(0, properties.frontCap / depth));


            AddUV(new Vector2(0, 0));   // front
            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(0.5f + side0, 1));
            AddUV(new Vector2(0.5f - side0, 1));

            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0.5f + side0, 1));

            AddUV(new Vector2(0, 0));   
            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0.5f + side0, 1));

            AddUV(new Vector2(0, 0));   // back
            AddUV(new Vector2(0.5f - side0, 0));    
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(0.5f + side0, 1));
            AddUV(new Vector2(0.5f - side0, 1));

            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0.5f + side0, 1));

            AddUV(new Vector2(0, 0));   
            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0.5f + side0, 1));

            AddUV(new Vector2(0, 0));   // left
            if (!properties.capThickness)
            {
                AddUV(new Vector2(backThickness / depth, 0));
                AddUV(new Vector2(backThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(properties.backCap / (depth - backThickness + properties.backCap), 0));
                AddUV(new Vector2(properties.backCap / (depth - backThickness + properties.backCap), 1));
            }
            AddUV(new Vector2(0, 1));

            if (!properties.capThickness)
            {
                AddUV(new Vector2(backThickness / depth, 0));
                AddUV(new Vector2(1 - frontThickness / depth, 0));
                AddUV(new Vector2(1 - frontThickness / depth, 1));
                AddUV(new Vector2(backThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(properties.backCap / (depth - backThickness + properties.backCap), 0));
                AddUV(new Vector2(1 - properties.frontCap / (depth - frontThickness + properties.frontCap), 0));
                AddUV(new Vector2(1 - properties.frontCap / (depth - frontThickness + properties.frontCap), 1));
                AddUV(new Vector2(properties.backCap / (depth - backThickness + properties.backCap), 1));
            }

            if (!properties.capThickness)
            {
                AddUV(new Vector2(1 - frontThickness / depth, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - frontThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(1 - properties.frontCap / (depth - frontThickness + properties.frontCap), 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - properties.frontCap / (depth - frontThickness + properties.frontCap), 1));
            }

            AddUV(new Vector2(0, 0));   // right
            if (!properties.capThickness)
            {
                AddUV(new Vector2(frontThickness / depth, 0));
                AddUV(new Vector2(frontThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 0));
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 1));
            }
            AddUV(new Vector2(0, 1));

            if (!properties.capThickness)
            {
                AddUV(new Vector2(frontThickness / depth, 0));
                AddUV(new Vector2(1 - backThickness / depth, 0));
                AddUV(new Vector2(1 - backThickness / depth, 1));
                AddUV(new Vector2(frontThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 0));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 0));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 1));
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 1));
            }

            if (!properties.capThickness)
            {
                AddUV(new Vector2(1 - backThickness / depth, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - backThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 1));
            }
            #endregion                                                                                                                                                                                                                                                                                                                  
        }
        #endregion

        #region GenerateVerticesCType
        private void GenerateVerticesCType()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;
            float frontThickness = properties.frontThickness;
            float backThickness = properties.backThickness;
            float sideThickness = properties.sideThickness;

            float halfWidth = 0.5f * width;
            float halfDepth = 0.5f * depth;

            Vector3[] pts = new Vector3[24];
            if (!properties.capThickness)
            {
                pts[0] = new Vector3(-halfWidth, 0, -halfDepth);
                pts[1] = new Vector3(-halfWidth + sideThickness, 0, -halfDepth);
                pts[2] = new Vector3(halfWidth, 0, -halfDepth);
                pts[3] = new Vector3(halfWidth, 0, -halfDepth + frontThickness);
                pts[4] = new Vector3(-halfWidth + sideThickness, 0, -halfDepth + frontThickness);
                pts[5] = new Vector3(-halfWidth + sideThickness, 0, halfDepth - backThickness);
                pts[6] = new Vector3(halfWidth, 0, halfDepth - backThickness);
                pts[7] = new Vector3(halfWidth, 0, halfDepth);
                pts[8] = new Vector3(-halfWidth + sideThickness, 0, halfDepth);
                pts[9] = new Vector3(-halfWidth, 0, halfDepth);
                pts[10] = new Vector3(-halfWidth, 0, halfDepth - backThickness);
                pts[11] = new Vector3(-halfWidth, 0, -halfDepth + frontThickness);

                pts[12] = new Vector3(-halfWidth, height, -halfDepth);
                pts[13] = new Vector3(-halfWidth + sideThickness, height, -halfDepth);
                pts[14] = new Vector3(halfWidth, height, -halfDepth);
                pts[15] = new Vector3(halfWidth, height, -halfDepth + frontThickness);
                pts[16] = new Vector3(-halfWidth + sideThickness, height, -halfDepth + frontThickness);
                pts[17] = new Vector3(-halfWidth + sideThickness, height, halfDepth - backThickness);
                pts[18] = new Vector3(halfWidth, height, halfDepth - backThickness);
                pts[19] = new Vector3(halfWidth, height, halfDepth);
                pts[20] = new Vector3(-halfWidth + sideThickness, height, halfDepth);
                pts[21] = new Vector3(-halfWidth, height, halfDepth);
                pts[22] = new Vector3(-halfWidth, height, halfDepth - backThickness);
                pts[23] = new Vector3(-halfWidth, height, -halfDepth + frontThickness);
            }
            else
            {
                pts[0] = new Vector3(-halfWidth, 0, -halfDepth);
                pts[1] = new Vector3(-halfWidth + sideThickness, 0, -halfDepth);
                pts[2] = new Vector3(halfWidth, 0, -halfDepth);
                pts[3] = new Vector3(halfWidth, 0, -halfDepth + properties.frontCap);
                pts[4] = new Vector3(-halfWidth + sideThickness, 0, -halfDepth + frontThickness);
                pts[5] = new Vector3(-halfWidth + sideThickness, 0, halfDepth - backThickness);
                pts[6] = new Vector3(halfWidth, 0, halfDepth - properties.backCap);
                pts[7] = new Vector3(halfWidth, 0, halfDepth);
                pts[8] = new Vector3(-halfWidth + sideThickness, 0, halfDepth);
                pts[9] = new Vector3(-halfWidth, 0, halfDepth);
                pts[10] = new Vector3(-halfWidth, 0, halfDepth - backThickness);
                pts[11] = new Vector3(-halfWidth, 0, -halfDepth + frontThickness);

                pts[12] = new Vector3(-halfWidth, height, -halfDepth);
                pts[13] = new Vector3(-halfWidth + sideThickness, height, -halfDepth);
                pts[14] = new Vector3(halfWidth, height, -halfDepth);
                pts[15] = new Vector3(halfWidth, height, -halfDepth + +properties.frontCap);
                pts[16] = new Vector3(-halfWidth + sideThickness, height, -halfDepth + frontThickness);
                pts[17] = new Vector3(-halfWidth + sideThickness, height, halfDepth - backThickness);
                pts[18] = new Vector3(halfWidth, height, halfDepth - properties.backCap);
                pts[19] = new Vector3(halfWidth, height, halfDepth);
                pts[20] = new Vector3(-halfWidth + sideThickness, height, halfDepth);
                pts[21] = new Vector3(-halfWidth, height, halfDepth);
                pts[22] = new Vector3(-halfWidth, height, halfDepth - backThickness);
                pts[23] = new Vector3(-halfWidth, height, -halfDepth + frontThickness);
            }

            // bottom face
            AddVertex(pts[0]);
            AddVertex(pts[1]);
            AddVertex(pts[2]);
            AddVertex(pts[3]);

            AddVertex(pts[4]);
            AddVertex(pts[5]);
            AddVertex(pts[6]);
            AddVertex(pts[7]);

            AddVertex(pts[8]);
            AddVertex(pts[9]);
            AddVertex(pts[10]);
            AddVertex(pts[11]);

            // top face
            AddVertex(pts[12]);
            AddVertex(pts[13]);
            AddVertex(pts[14]);
            AddVertex(pts[15]);

            AddVertex(pts[16]);
            AddVertex(pts[17]);
            AddVertex(pts[18]);
            AddVertex(pts[19]);

            AddVertex(pts[20]);
            AddVertex(pts[21]);
            AddVertex(pts[22]);
            AddVertex(pts[23]);

            // front facing
            AddVertex(pts[0]);  // 24
            AddVertex(pts[2]);
            AddVertex(pts[14]);
            AddVertex(pts[12]);

            AddVertex(pts[5]);  // 28
            AddVertex(pts[6]);
            AddVertex(pts[18]);
            AddVertex(pts[17]);

            // back facing
            AddVertex(pts[7]);  // 32
            AddVertex(pts[9]);
            AddVertex(pts[21]);
            AddVertex(pts[19]);

            AddVertex(pts[3]);  // 36
            AddVertex(pts[4]);
            AddVertex(pts[16]);
            AddVertex(pts[15]);

            // left
            AddVertex(pts[9]);  // 40
            AddVertex(pts[0]);
            AddVertex(pts[12]);
            AddVertex(pts[21]);

            // right
            AddVertex(pts[2]);  // 44
            AddVertex(pts[3]);
            AddVertex(pts[15]);
            AddVertex(pts[14]);

            AddVertex(pts[4]);  // 48
            AddVertex(pts[5]);
            AddVertex(pts[17]);
            AddVertex(pts[16]);

            AddVertex(pts[6]);  // 52
            AddVertex(pts[7]);
            AddVertex(pts[19]);
            AddVertex(pts[18]);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);

            if (!properties.capThickness)
            {
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
            }
            else
            {
                Vector3 forwardNormal = new Vector3(properties.backThickness - properties.backCap,
                                                    0,
                                                    -(properties.width - properties.sideThickness));
                forwardNormal.Normalize();

                AddNormal(forwardNormal);
                AddNormal(forwardNormal);
                AddNormal(forwardNormal);
                AddNormal(forwardNormal);
            }

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            if (!properties.capThickness)
            {
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
            }
            else
            {
                Vector3 backNormal = new Vector3(-(properties.frontThickness - properties.frontCap),
                                                 0,
                                                 properties.width - properties.sideThickness);
                backNormal.Normalize();

                AddNormal(backNormal);
                AddNormal(backNormal);
                AddNormal(backNormal);
                AddNormal(backNormal);
            }

            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            AddUV(new Vector2(0, 1));   // bottom
            AddUV(new Vector2(sideThickness / width, 1));
            AddUV(new Vector2(1, 1));
            if (!properties.capThickness)
                AddUV(new Vector2(1, 1 - frontThickness / depth));
            else
                AddUV(new Vector2(1, 1- properties.frontCap / depth));

            AddUV(new Vector2(sideThickness / width, 1 - frontThickness / depth));
            AddUV(new Vector2(sideThickness / width, backThickness / depth));
            if (!properties.capThickness)
                AddUV(new Vector2(1, backThickness / depth));
            else
                AddUV(new Vector2(1, properties.backCap / depth));
            AddUV(new Vector2(1, 0));

            AddUV(new Vector2(sideThickness / width, 0));
            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(0, backThickness / depth));
            AddUV(new Vector2(0, 1 - frontThickness / depth));

            AddUV(new Vector2(0, 0));   // top
            AddUV(new Vector2(sideThickness / width, 0));
            AddUV(new Vector2(1, 0));
            if (!properties.capThickness)
                AddUV(new Vector2(1, frontThickness / depth));
            else
                AddUV(new Vector2(1, properties.frontCap / depth));

            AddUV(new Vector2(sideThickness / width, frontThickness / depth));
            AddUV(new Vector2(sideThickness / width, 1 - backThickness / depth));
            if (!properties.capThickness)
                AddUV(new Vector2(1, 1 - backThickness / depth));
            else
                AddUV(new Vector2(1, 1 - properties.backCap / depth));

            AddUV(new Vector2(1, 1));

            AddUV(new Vector2(sideThickness / width, 1));
            AddUV(new Vector2(0, 1));
            AddUV(new Vector2(0, 1 - backThickness / depth));
            AddUV(new Vector2(0, frontThickness / depth));

            AddUV(new Vector2(0, 0));       // front
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(sideThickness / width, 0));   
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(sideThickness / width, 1));        

            AddUV(new Vector2(0, 0));   // back
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(1, 0));   
            AddUV(new Vector2(sideThickness / width, 0));
            AddUV(new Vector2(sideThickness / width, 1));
            AddUV(new Vector2(1, 1));

            AddUV(new Vector2(0, 0));   // left
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0, 0));   // right
            if (!properties.capThickness)
            {
                AddUV(new Vector2(frontThickness / depth, 0));
                AddUV(new Vector2(frontThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 0));
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 1));
            }
            AddUV(new Vector2(0, 1));

            if (!properties.capThickness)
            {
                AddUV(new Vector2(frontThickness / depth, 0));
                AddUV(new Vector2(1 - backThickness / depth, 0));
                AddUV(new Vector2(1 - backThickness / depth, 1));
                AddUV(new Vector2(frontThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 0));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 0));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 1));
                AddUV(new Vector2(properties.frontCap / (depth - frontThickness + properties.frontCap), 1));
            }

            if (!properties.capThickness)
            {
                AddUV(new Vector2(1 - backThickness / depth, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - backThickness / depth, 1));
            }
            else
            {
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(1 - properties.backCap / (depth - backThickness + properties.backCap), 1));
            }
        }
        #endregion

        #region GenerateVerticesTType
        private void GenerateVerticesTType()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;
            //float frontThickness = properties.frontThickness;
            float backThickness = properties.backThickness;
            float sideThickness = properties.sideThickness;

            Vector3[] pts = new Vector3[20];
            if (!properties.capThickness)
            {
                pts[0] = new Vector3(-sideThickness * 0.5f, 0, -depth * 0.5f);
                pts[1] = new Vector3(sideThickness * 0.5f, 0, -depth * 0.5f);
                pts[2] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f - backThickness);
                pts[3] = new Vector3(width * 0.5f, 0, depth * 0.5f - backThickness);
                pts[4] = new Vector3(width * 0.5f, 0, depth * 0.5f);
                pts[5] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f);
                pts[6] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f);
                pts[7] = new Vector3(-width * 0.5f, 0, depth * 0.5f);
                pts[8] = new Vector3(-width * 0.5f, 0, depth * 0.5f - backThickness);
                pts[9] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f - backThickness);

                pts[10] = new Vector3(-sideThickness * 0.5f, height, -depth * 0.5f);
                pts[11] = new Vector3(sideThickness * 0.5f, height, -depth * 0.5f);
                pts[12] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f - backThickness);
                pts[13] = new Vector3(width * 0.5f, height, depth * 0.5f - backThickness);
                pts[14] = new Vector3(width * 0.5f, height, depth * 0.5f);
                pts[15] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f);
                pts[16] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f);
                pts[17] = new Vector3(-width * 0.5f, height, depth * 0.5f);
                pts[18] = new Vector3(-width * 0.5f, height, depth * 0.5f - backThickness);
                pts[19] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f - backThickness);
            }
            else
            {
                pts[0] = new Vector3(-properties.sideCap * 0.5f, 0, -depth * 0.5f);
                pts[1] = new Vector3(properties.sideCap * 0.5f, 0, -depth * 0.5f);
                pts[2] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f - backThickness);
                pts[3] = new Vector3(width * 0.5f, 0, depth * 0.5f - properties.backCap);
                pts[4] = new Vector3(width * 0.5f, 0, depth * 0.5f);
                pts[5] = new Vector3(sideThickness * 0.5f, 0, depth * 0.5f);
                pts[6] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f);
                pts[7] = new Vector3(-width * 0.5f, 0, depth * 0.5f);
                pts[8] = new Vector3(-width * 0.5f, 0, depth * 0.5f - properties.backCap);
                pts[9] = new Vector3(-sideThickness * 0.5f, 0, depth * 0.5f - backThickness);

                pts[10] = new Vector3(-properties.sideCap * 0.5f, height, -depth * 0.5f);
                pts[11] = new Vector3(properties.sideCap * 0.5f, height, -depth * 0.5f);
                pts[12] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f - backThickness);
                pts[13] = new Vector3(width * 0.5f, height, depth * 0.5f - properties.backCap);
                pts[14] = new Vector3(width * 0.5f, height, depth * 0.5f);
                pts[15] = new Vector3(sideThickness * 0.5f, height, depth * 0.5f);
                pts[16] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f);
                pts[17] = new Vector3(-width * 0.5f, height, depth * 0.5f);
                pts[18] = new Vector3(-width * 0.5f, height, depth * 0.5f - properties.backCap);
                pts[19] = new Vector3(-sideThickness * 0.5f, height, depth * 0.5f - backThickness);
            }

            // bottom face
            AddVertex(pts[0]);
            AddVertex(pts[1]);
            AddVertex(pts[2]);
            AddVertex(pts[3]);
            AddVertex(pts[4]);
            AddVertex(pts[5]);
            AddVertex(pts[6]);
            AddVertex(pts[7]);
            AddVertex(pts[8]);
            AddVertex(pts[9]);

            // top face
            AddVertex(pts[10]);
            AddVertex(pts[11]);           
            AddVertex(pts[12]);
            AddVertex(pts[13]);
            AddVertex(pts[14]);
            AddVertex(pts[15]);
            AddVertex(pts[16]);
            AddVertex(pts[17]);
            AddVertex(pts[18]);
            AddVertex(pts[19]);

            // front facing
            AddVertex(pts[0]);      // 20
            AddVertex(pts[1]);
            AddVertex(pts[11]);
            AddVertex(pts[10]);

            AddVertex(pts[2]);      // 24
            AddVertex(pts[3]);
            AddVertex(pts[13]);
            AddVertex(pts[12]);

            AddVertex(pts[8]);      // 28
            AddVertex(pts[9]);
            AddVertex(pts[19]);
            AddVertex(pts[18]);

            // back facing
            AddVertex(pts[4]);      // 32
            AddVertex(pts[5]);
            AddVertex(pts[15]);
            AddVertex(pts[14]);

            AddVertex(pts[5]);      // 36
            AddVertex(pts[6]);
            AddVertex(pts[16]);
            AddVertex(pts[15]);

            AddVertex(pts[6]);      // 40
            AddVertex(pts[7]);
            AddVertex(pts[17]);
            AddVertex(pts[16]);

            // left          
            AddVertex(pts[7]);      // 44
            AddVertex(pts[8]);
            AddVertex(pts[18]);
            AddVertex(pts[17]);

            AddVertex(pts[9]);      // 48
            AddVertex(pts[0]);
            AddVertex(pts[10]);
            AddVertex(pts[19]);

            // right
            AddVertex(pts[1]);      // 52
            AddVertex(pts[2]);
            AddVertex(pts[12]);
            AddVertex(pts[11]);

            AddVertex(pts[3]);      // 56
            AddVertex(pts[4]);
            AddVertex(pts[14]);
            AddVertex(pts[13]);

            #region normals
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.up);
            AddNormal(Vector3.up);

            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);
            AddNormal(Vector3.back);

            if (!properties.capThickness)
            {
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);

                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);

            }
            else
            {
                Vector3 forwardNormal0 = new Vector3(-(properties.backThickness - properties.backCap),
                                                    0,
                                                    -(properties.width - properties.sideThickness) * 0.5f);
                forwardNormal0.Normalize();

                AddNormal(forwardNormal0);
                AddNormal(forwardNormal0);
                AddNormal(forwardNormal0);
                AddNormal(forwardNormal0);

                Vector3 forwardNormal1 = new Vector3(properties.backThickness - properties.backCap,
                                                    0,
                                                    -(properties.width - properties.sideThickness) * 0.5f);
                forwardNormal1.Normalize();

                AddNormal(forwardNormal1);
                AddNormal(forwardNormal1);
                AddNormal(forwardNormal1);
                AddNormal(forwardNormal1);
            }

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);


            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            if (!properties.capThickness)
            {
                AddNormal(Vector3.left);
                AddNormal(Vector3.left);
                AddNormal(Vector3.left);
                AddNormal(Vector3.left);

                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
            }
            else
            {
                Vector3 leftNormal = new Vector3(-(properties.depth - properties.backThickness),
                                                 0,
                                                 -(properties.sideThickness - properties.sideCap) * 0.5f);
                leftNormal.Normalize();

                AddNormal(leftNormal);
                AddNormal(leftNormal);
                AddNormal(leftNormal);
                AddNormal(leftNormal);

                Vector3 rightNormal = new Vector3(properties.depth - properties.backThickness,
                                                  0,
                                                  -(properties.sideThickness - properties.sideCap) * 0.5f);
                rightNormal.Normalize();

                AddNormal(rightNormal);
                AddNormal(rightNormal);
                AddNormal(rightNormal);
                AddNormal(rightNormal);
            }

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            #endregion

            #region uv
            float side0 = sideThickness / width * 0.5f;
            float side1 = properties.sideCap * 0.5f / width;
            if (!properties.capThickness)
            {
                AddUV(new Vector2(0.5f - side0, 1));   // bottom
                AddUV(new Vector2(0.5f + side0, 1));
                AddUV(new Vector2(0.5f + side0, backThickness / depth));
                AddUV(new Vector2(1, backThickness / depth));
            }
            else
            {
                AddUV(new Vector2(0.5f - side1, 1));   
                AddUV(new Vector2(0.5f + side1, 1));
                AddUV(new Vector2(0.5f + side0, backThickness / depth));
                AddUV(new Vector2(1, properties.backCap / depth));
            }

            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0, 0));

            if (!properties.capThickness)
                AddUV(new Vector2(0, backThickness / depth));
            else
                AddUV(new Vector2(0, properties.backCap / depth));
            AddUV(new Vector2(0.5f - side0, backThickness / depth));

            if (!properties.capThickness)
            {
                AddUV(new Vector2(0.5f - side0, 0));   // top
                AddUV(new Vector2(0.5f + side0, 0));
                AddUV(new Vector2(0.5f + side0, 1 - backThickness / depth));
                AddUV(new Vector2(1, 1 - backThickness / depth));
            }
            else
            {
                AddUV(new Vector2(0.5f - side1, 0));
                AddUV(new Vector2(0.5f + side1, 0));
                AddUV(new Vector2(0.5f + side0, 1 - backThickness / depth));
                AddUV(new Vector2(1, 1- properties.backCap / depth));
            }

            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0.5f + side0, 1));
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0, 1));

            if (!properties.capThickness)
                AddUV(new Vector2(0, 1 - backThickness / depth));
            else
                AddUV(new Vector2(0, 1 - properties.backCap / depth));
            AddUV(new Vector2(0.5f - side0, 1 - backThickness / depth));

          
            if (!properties.capThickness)       // front
            {
                AddUV(new Vector2(0.5f - side0, 0));
                AddUV(new Vector2(0.5f + side0, 0));
                AddUV(new Vector2(0.5f + side0, 1));
                AddUV(new Vector2(0.5f - side0, 1));
            }
            else
            {
                AddUV(new Vector2(0.5f - side1, 0));
                AddUV(new Vector2(0.5f + side1, 0));
                AddUV(new Vector2(0.5f + side1, 1));
                AddUV(new Vector2(0.5f - side1, 1));
            }

            if (!properties.capThickness)
            {
                AddUV(new Vector2(0.5f + side0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0.5f + side0, 1));
            }
            else
            {
                AddUV(new Vector2(0.5f + side1, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0.5f + side1, 1));
            }

            AddUV(new Vector2(0, 0));   // front left
            if (!properties.capThickness)
            {
                AddUV(new Vector2(0.5f - side0, 0));
                AddUV(new Vector2(0.5f - side0, 1));
            }
            else
            {
                AddUV(new Vector2(0.5f - side1, 0));
                AddUV(new Vector2(0.5f - side1, 1));
            }
            AddUV(new Vector2(0, 1));


            AddUV(new Vector2(0, 0));   // back
            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f - side0, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0.5f - side0, 0));
            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(0.5f + side0, 1));
            AddUV(new Vector2(0.5f - side0, 1));

            AddUV(new Vector2(0.5f + side0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0.5f + side0, 1));

            float u0;
            if (!properties.capThickness)
            {
                u0 = backThickness / depth;
            }
            else
            {
                u0 = properties.backCap / (depth - backThickness + properties.backCap);
            }

            AddUV(new Vector2(0, 0));   // left
            AddUV(new Vector2(u0, 0));
            AddUV(new Vector2(u0, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(u0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(u0, 1));

            float u1 = 1 - u0;
            AddUV(new Vector2(0, 0));   // right
            AddUV(new Vector2(u1, 0));
            AddUV(new Vector2(u1, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(u1, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(u1, 1));
            #endregion           
        }
        #endregion

        #region GenerateTrianglesLType
        private void GenerateTrianglesLType()
        {
            faces.Add(new TriangleIndices(2, 0, 1));
            faces.Add(new TriangleIndices(0, 2, 7));

            faces.Add(new TriangleIndices(7, 2, 6));
            faces.Add(new TriangleIndices(6, 2, 5));

            faces.Add(new TriangleIndices(5, 2, 4));
            faces.Add(new TriangleIndices(4, 2, 3));

            // top
            faces.Add(new TriangleIndices(9, 8, 10));
            faces.Add(new TriangleIndices(10, 8, 15));

            faces.Add(new TriangleIndices(15, 14, 10));
            faces.Add(new TriangleIndices(10, 14, 13));

            faces.Add(new TriangleIndices(11, 10, 12));
            faces.Add(new TriangleIndices(12, 10, 13));

            // front
            faces.Add(new TriangleIndices(17, 16, 18));
            faces.Add(new TriangleIndices(18, 16, 19));

            faces.Add(new TriangleIndices(21, 20, 22));
            faces.Add(new TriangleIndices(22, 20, 23));

            // back
            faces.Add(new TriangleIndices(25, 24, 26));
            faces.Add(new TriangleIndices(26, 24, 27));

            // left
            faces.Add(new TriangleIndices(28, 31, 29));
            faces.Add(new TriangleIndices(29, 31, 30));

            // right
            faces.Add(new TriangleIndices(32, 35, 33));
            faces.Add(new TriangleIndices(33, 35, 34));

            faces.Add(new TriangleIndices(36, 39, 37));
            faces.Add(new TriangleIndices(37, 39, 38));
        }
        #endregion

        #region GenerateTrianglesIType
        private void GenerateTrianglesIType()
        {
            // bottom
            faces.Add(new TriangleIndices(1, 14, 0));
            faces.Add(new TriangleIndices(0, 14, 15));

            faces.Add(new TriangleIndices(1, 2, 14));
            faces.Add(new TriangleIndices(14, 2, 5));

            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(2, 4, 5));

            faces.Add(new TriangleIndices(5, 6, 14));
            faces.Add(new TriangleIndices(14, 6, 13));

            faces.Add(new TriangleIndices(13, 10, 12));
            faces.Add(new TriangleIndices(12, 10, 11));

            faces.Add(new TriangleIndices(13, 6, 10));
            faces.Add(new TriangleIndices(10, 6, 9));

            faces.Add(new TriangleIndices(7, 8, 6));
            faces.Add(new TriangleIndices(6, 8, 9));

            // top
            faces.Add(new TriangleIndices(16, 31, 17));
            faces.Add(new TriangleIndices(17, 31, 30));

            faces.Add(new TriangleIndices(18, 17, 21));
            faces.Add(new TriangleIndices(21, 17, 30));

            faces.Add(new TriangleIndices(18, 21, 19));
            faces.Add(new TriangleIndices(19, 21, 20));

            faces.Add(new TriangleIndices(30, 29, 21));
            faces.Add(new TriangleIndices(21, 29, 22));

            faces.Add(new TriangleIndices(22, 25, 23));
            faces.Add(new TriangleIndices(23, 25, 24));

            faces.Add(new TriangleIndices(22, 29, 25));
            faces.Add(new TriangleIndices(25, 29, 26));

            faces.Add(new TriangleIndices(28, 27, 29));
            faces.Add(new TriangleIndices(29, 27, 26));

            // front
            faces.Add(new TriangleIndices(33, 32, 34));
            faces.Add(new TriangleIndices(34, 32, 35));

            faces.Add(new TriangleIndices(36, 39, 37));
            faces.Add(new TriangleIndices(37, 39, 38));

            faces.Add(new TriangleIndices(41, 40, 42));
            faces.Add(new TriangleIndices(42, 40, 43));

            faces.Add(new TriangleIndices(45, 44, 46));
            faces.Add(new TriangleIndices(46, 44, 47));

            faces.Add(new TriangleIndices(49, 48, 50));
            faces.Add(new TriangleIndices(50, 48, 51));

            // back
            faces.Add(new TriangleIndices(53, 52, 54));
            faces.Add(new TriangleIndices(54, 52, 55));

            faces.Add(new TriangleIndices(56, 54, 57));
            faces.Add(new TriangleIndices(57, 54, 58));

            faces.Add(new TriangleIndices(61, 60, 62));
            faces.Add(new TriangleIndices(62, 60, 63));

            faces.Add(new TriangleIndices(65, 64, 66));
            faces.Add(new TriangleIndices(66, 64, 67));

            faces.Add(new TriangleIndices(69, 68, 70));
            faces.Add(new TriangleIndices(70, 68, 71));

            // left
            faces.Add(new TriangleIndices(72, 75, 73));
            faces.Add(new TriangleIndices(73, 75, 74));

            faces.Add(new TriangleIndices(76, 79, 77));
            faces.Add(new TriangleIndices(77, 79, 78));

            faces.Add(new TriangleIndices(80, 83, 81));
            faces.Add(new TriangleIndices(81, 83, 82));

            // right
            faces.Add(new TriangleIndices(84, 87, 85));
            faces.Add(new TriangleIndices(85, 87, 86));

            faces.Add(new TriangleIndices(88, 91, 89));
            faces.Add(new TriangleIndices(89, 91, 90));

            faces.Add(new TriangleIndices(92, 95, 93));
            faces.Add(new TriangleIndices(93, 95, 94));
        }
        #endregion

        #region GenerateTrianglesCType
        private void GenerateTrianglesCType()
        {
            // bottom
            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(2, 4, 1));

            faces.Add(new TriangleIndices(1, 4, 0));
            faces.Add(new TriangleIndices(0, 4, 11));

            faces.Add(new TriangleIndices(11, 4, 10));
            faces.Add(new TriangleIndices(10, 4, 5));

            faces.Add(new TriangleIndices(5, 8, 10));
            faces.Add(new TriangleIndices(10, 8, 9));

            faces.Add(new TriangleIndices(5, 6, 8));
            faces.Add(new TriangleIndices(8, 6, 7));

            // top
            faces.Add(new TriangleIndices(14, 13, 15));
            faces.Add(new TriangleIndices(15, 13, 16));

            faces.Add(new TriangleIndices(12, 23, 13));
            faces.Add(new TriangleIndices(13, 23, 16));

            faces.Add(new TriangleIndices(16, 23, 17));
            faces.Add(new TriangleIndices(17, 23, 22));

            faces.Add(new TriangleIndices(22, 21, 17));
            faces.Add(new TriangleIndices(17, 21, 20));

            faces.Add(new TriangleIndices(17, 20, 18));
            faces.Add(new TriangleIndices(18, 20, 19));

            // front
            faces.Add(new TriangleIndices(24, 27, 25));
            faces.Add(new TriangleIndices(25, 27, 26));

            faces.Add(new TriangleIndices(28, 31, 29));
            faces.Add(new TriangleIndices(29, 31, 30));

            // back
            faces.Add(new TriangleIndices(32, 35, 33));
            faces.Add(new TriangleIndices(33, 35, 34));

            faces.Add(new TriangleIndices(36, 39, 37));
            faces.Add(new TriangleIndices(37, 39, 38));

            // left
            faces.Add(new TriangleIndices(41, 40, 42));
            faces.Add(new TriangleIndices(42, 40, 43));

            // right
            faces.Add(new TriangleIndices(45, 44, 46));
            faces.Add(new TriangleIndices(46, 44, 47));

            faces.Add(new TriangleIndices(49, 48, 50));
            faces.Add(new TriangleIndices(50, 48, 51));

            faces.Add(new TriangleIndices(53, 52, 54));
            faces.Add(new TriangleIndices(54, 52, 55));
        }
        #endregion

        #region GenerateTrianglesTType
        private void GenerateTrianglesTType()
        {
            // bottom
            faces.Add(new TriangleIndices(1, 2, 0));
            faces.Add(new TriangleIndices(0, 2, 9));

            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(2, 4, 5));

            faces.Add(new TriangleIndices(9, 2, 6));
            faces.Add(new TriangleIndices(6, 2, 5));

            faces.Add(new TriangleIndices(9, 6, 8));
            faces.Add(new TriangleIndices(8, 6, 7));

            // top
            faces.Add(new TriangleIndices(10, 19, 11));
            faces.Add(new TriangleIndices(11, 19, 12));

            faces.Add(new TriangleIndices(12, 15, 13));
            faces.Add(new TriangleIndices(13, 15, 14));

            faces.Add(new TriangleIndices(12, 19, 15));
            faces.Add(new TriangleIndices(15, 19, 16));

            faces.Add(new TriangleIndices(18, 17, 19));
            faces.Add(new TriangleIndices(19, 17, 16));

            // front
            faces.Add(new TriangleIndices(21, 20, 22));
            faces.Add(new TriangleIndices(22, 20, 23));

            faces.Add(new TriangleIndices(25, 24, 26));
            faces.Add(new TriangleIndices(26, 24, 27));

            faces.Add(new TriangleIndices(29, 28, 30));
            faces.Add(new TriangleIndices(30, 28, 31));

            // back
            faces.Add(new TriangleIndices(33, 32, 34));
            faces.Add(new TriangleIndices(34, 32, 35));

            faces.Add(new TriangleIndices(36, 39, 37));
            faces.Add(new TriangleIndices(37, 39, 38));

            faces.Add(new TriangleIndices(41, 40, 42));
            faces.Add(new TriangleIndices(42, 40, 43));

            // left
            faces.Add(new TriangleIndices(44, 47, 45));
            faces.Add(new TriangleIndices(45, 47, 46));

            faces.Add(new TriangleIndices(48, 51, 49));
            faces.Add(new TriangleIndices(49, 51, 50));

            // right
            faces.Add(new TriangleIndices(52, 55, 53));
            faces.Add(new TriangleIndices(53, 55, 54));

            faces.Add(new TriangleIndices(56, 59, 57));
            faces.Add(new TriangleIndices(57, 59, 58));
        }
        #endregion
    }
}
