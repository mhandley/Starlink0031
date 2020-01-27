using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcStairMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcStairProperties : QcBaseProperties
        {
            public enum Types { Box, Closed, Open }

            public float width = 1;
            public float depth = 1;
            public float height = 1;

            public int steps = 5;

            public float treadDepth = 1;
            public float treadThickness = 1;

            public Types type = new Types();

            public bool spiral = false;
            public float innerRadius = 0;
            //public float outerRadius = 0;
            public bool conical = false;
            public float radius = 0.0f;
            public float rotations = 1;

            public enum WindingDirection {  Clockwise, Counterclockwise };
            public WindingDirection windingDirection = WindingDirection.Counterclockwise;

            //public bool textureWrapped;

            public void CopyFrom(QcStairProperties source)
            {
                base.CopyFrom(source);

                this.width = source.width;
                this.height = source.height;
                this.depth = source.depth;

                this.steps = source.steps;
                this.treadDepth = source.treadDepth;
                this.treadThickness = source.treadThickness;

                this.spiral = source.spiral;
                this.innerRadius = source.innerRadius;
                this.conical = source.conical;
                this.radius = source.radius;
                //this.outerRadius = source.outerRadius;
                this.rotations = source.rotations;
                this.windingDirection = source.windingDirection;

                this.type = source.type;

                //this.textureWrapped = source.textureWrapped;
            }

            public bool Modified(QcStairProperties source)
            {
                return ((this.width != source.width) ||
                        (this.height != source.height) ||
                        (this.depth != source.depth) ||
                        (this.steps != source.steps) ||
                        (this.treadDepth != source.treadDepth) ||
                        (this.treadThickness != source.treadThickness) ||
                        (this.spiral != source.spiral) ||
                        (this.spiral && 
                         ((this.innerRadius != source.innerRadius) ||
                          (this.conical != source.conical) ||
                          (this.radius != source.radius) ||
                          (this.rotations != source.rotations) || 
                          //(this.outerRadius != source.outerRadius) ||
                          (this.windingDirection != source.windingDirection))) ||
                        (this.offset[0] != source.offset[0]) ||
                        (this.offset[1] != source.offset[1]) ||
                        (this.offset[2] != source.offset[2]) ||
                        (this.genTextureCoords != source.genTextureCoords) ||
                        (this.addCollider != source.addCollider) ||
                        (this.type != source.type));
            }
        }

        public QcStairProperties properties = new QcStairProperties();

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
                if (!properties.spiral)
                {
                    // set collider bound
                    BoxCollider collider = gameObject.GetComponent<BoxCollider>();
                    if (collider == null)
                    {
                        collider = gameObject.AddComponent<BoxCollider>();
                    }

                    collider.enabled = true;
                    collider.center = properties.offset + new Vector3(0, properties.height * 0.5f, 0);
                    collider.size = new Vector3(properties.width, properties.height, properties.depth);

                    CapsuleCollider oldCollider = gameObject.GetComponent<CapsuleCollider>();
                    if (oldCollider != null) oldCollider.enabled = false;
                }
                else
                {
                    CapsuleCollider collider = gameObject.GetComponent<CapsuleCollider>();
                    if (collider == null)
                    {
                        collider = gameObject.AddComponent<CapsuleCollider>();
                    }

                    collider.enabled = true;
                    collider.center = properties.offset + new Vector3(0, properties.height * 0.5f, 0);
                    collider.radius = properties.innerRadius + properties.width;
                    collider.height = properties.height;

                    BoxCollider oldCollider = gameObject.GetComponent<BoxCollider>();
                    if (oldCollider != null) oldCollider.enabled = false;
                }
            }
            else
            {
                Collider collider = gameObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        private void GenerateGeometry()
        {
            if (!properties.spiral)
            {
                switch (properties.type)
                {
                    case QcStairProperties.Types.Box:
                    default:
                        GenerateVerticesBox();
                        GenerateTrianglesBox();
                        break;

                    case QcStairProperties.Types.Closed:
                        GenerateVerticesClosed();
                        GenerateTrianglesClosed();
                        break;

                    case QcStairProperties.Types.Open:
                        GenerateVerticesOpen();
                        GenerateTrianglesOpen();
                        break;
                }
            }
            else
            {
                switch (properties.type)
                {
                    case QcStairProperties.Types.Box:
                        GenerateVerticesSpiralBox();
                        break;

                    case QcStairProperties.Types.Closed:
                        GenerateVerticesSpiralClosed();
                        break;

                    case QcStairProperties.Types.Open:
                        GenerateVerticesSpiralOpen();
                        GenerateTrianglesOpen();
                        break;
                }
            }
        }

        #region GenerateVerticesBox
        private void GenerateVerticesBox()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;

            float halfWidth = width * 0.5f;
            float halfDepth = depth * 0.5f;

            AddVertex(new Vector3(-halfWidth, 0, halfDepth));    // bottom face
            AddVertex(new Vector3(halfWidth, 0, halfDepth));
            AddVertex(new Vector3(halfWidth, 0, -halfDepth));
            AddVertex(new Vector3(-halfWidth, 0, -halfDepth));

            AddVertex(new Vector3(-halfWidth, height, halfDepth));    // back face
            AddVertex(new Vector3(halfWidth, height, halfDepth));
            AddVertex(new Vector3(halfWidth, 0, halfDepth));
            AddVertex(new Vector3(-halfWidth, 0, halfDepth));

            AddVertex(new Vector3(-halfWidth, 0, -halfDepth));    // left face
            AddVertex(new Vector3(-halfWidth, height, halfDepth));
            AddVertex(new Vector3(-halfWidth, 0, halfDepth));

            AddVertex(new Vector3(halfWidth, 0, -halfDepth));    // right face
            AddVertex(new Vector3(halfWidth, 0, halfDepth));
            AddVertex(new Vector3(halfWidth, height, halfDepth));

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0, 1));

            AddUV(new Vector2(0, 1));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(0, 0));

            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(0, 1));
            AddUV(new Vector2(0, 0));

            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(1, 1));

            float stepHeight = height / properties.steps;
            float stepDepth = depth / properties.steps;

            for (int i = 0; i < properties.steps; ++i)
            {
                float stepBaseHeight = i * stepHeight;
                float stepBaseDepth = i * stepDepth - halfDepth;

                AddVertex(new Vector3(-halfWidth, stepBaseHeight, stepBaseDepth));    // step front
                AddVertex(new Vector3(halfWidth, stepBaseHeight, stepBaseDepth));
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));    // step top
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));

                AddVertex(new Vector3(-halfWidth, stepBaseHeight, stepBaseDepth));  // left cap
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));

                AddVertex(new Vector3(halfWidth, stepBaseHeight, stepBaseDepth));   // roght cap
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));

                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);

                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);

                AddNormal(Vector3.left);
                AddNormal(Vector3.left);
                AddNormal(Vector3.left);

                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);

                AddUV(new Vector2(0, (float)i / properties.steps));
                AddUV(new Vector2(1, (float)i / properties.steps));
                AddUV(new Vector2(1, (i + 0.5f) / properties.steps));
                AddUV(new Vector2(0, (i + 0.5f) / properties.steps));

                AddUV(new Vector2(0, (i + 0.5f) / properties.steps));
                AddUV(new Vector2(1, (i + 0.5f) / properties.steps));
                AddUV(new Vector2(1, (i + 1.0f) / properties.steps));
                AddUV(new Vector2(0, (i + 1.0f) / properties.steps));

                float currentDepth = i * stepDepth;
                AddUV(new Vector2((depth - currentDepth) / depth, stepBaseHeight / height));
                AddUV(new Vector2((depth - currentDepth) / depth, (stepBaseHeight + stepHeight) / height));
                AddUV(new Vector2((depth - currentDepth - stepDepth) / depth, (stepBaseHeight + stepHeight) / height));

                AddUV(new Vector2(currentDepth / depth, stepBaseHeight / height));
                AddUV(new Vector2((currentDepth) / depth, (stepBaseHeight + stepHeight) / height));
                AddUV(new Vector2((currentDepth + stepDepth) / depth, (stepBaseHeight + stepHeight) / height));
            }
        }
        #endregion

        #region GenerateVerticesClosed
        private void GenerateVerticesClosed()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;

            float halfWidth = width * 0.5f;
            float halfDepth = depth * 0.5f;

            float stepHeight = height / properties.steps;
            float stepDepth = depth / properties.steps;

            AddVertex(new Vector3(-halfWidth, 0, -halfDepth + stepDepth));    // bottom face
            AddVertex(new Vector3(halfWidth, 0, -halfDepth + stepDepth));
            AddVertex(new Vector3(halfWidth, 0, -halfDepth));
            AddVertex(new Vector3(-halfWidth, 0, -halfDepth));

            AddVertex(new Vector3(-halfWidth, 0, -halfDepth + stepDepth));        // slanted bottom face
            AddVertex(new Vector3(halfWidth, 0, -halfDepth + stepDepth));
            AddVertex(new Vector3(halfWidth, height - stepHeight, halfDepth));
            AddVertex(new Vector3(-halfWidth, height - stepHeight, halfDepth));

            AddVertex(new Vector3(-halfWidth, height, halfDepth));    // back face
            AddVertex(new Vector3(halfWidth, height, halfDepth));
            AddVertex(new Vector3(halfWidth, height - stepHeight, halfDepth));
            AddVertex(new Vector3(-halfWidth, height - stepHeight, halfDepth));

            AddVertex(new Vector3(-halfWidth, 0, -halfDepth + stepDepth));  // left face
            AddVertex(new Vector3(-halfWidth, 0, -halfDepth));    
            AddVertex(new Vector3(-halfWidth, height, halfDepth));
            AddVertex(new Vector3(-halfWidth, height - stepHeight, halfDepth));

            AddVertex(new Vector3(halfWidth, 0, -halfDepth + stepDepth));   // right face
            AddVertex(new Vector3(halfWidth, 0, -halfDepth));
            AddVertex(new Vector3(halfWidth, height, halfDepth));
            AddVertex(new Vector3(halfWidth, height - stepHeight, halfDepth));

            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);
            AddNormal(Vector3.down);

            Vector3 slantedVector = new Vector3(0, -depth, height);
            slantedVector.Normalize();

            AddNormal(slantedVector);
            AddNormal(slantedVector);
            AddNormal(slantedVector);
            AddNormal(slantedVector);

            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);
            AddNormal(Vector3.forward);

            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);
            AddNormal(Vector3.left);

            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);
            AddNormal(Vector3.right);

            float bottomLength = (Mathf.Sqrt((depth - stepDepth) * (depth - stepDepth) + (height - stepHeight) * (height - stepHeight))
                                 + stepDepth + stepHeight);
            AddUV(new Vector2(1, stepDepth / bottomLength));
            AddUV(new Vector2(0, stepDepth / bottomLength));
            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(1, 0));

            AddUV(new Vector2(1, stepDepth / bottomLength));
            AddUV(new Vector2(0, stepDepth / bottomLength));
            AddUV(new Vector2(0, (bottomLength - stepHeight) / bottomLength));
            AddUV(new Vector2(1, (bottomLength - stepHeight) / bottomLength));

            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(0, 1));
            AddUV(new Vector2(0, (bottomLength - stepHeight) / bottomLength));
            AddUV(new Vector2(1, (bottomLength - stepHeight) / bottomLength));

            AddUV(new Vector2((depth - stepDepth) / depth, 0));
            AddUV(new Vector2(1, 0));
            AddUV(new Vector2(0, 1));
            AddUV(new Vector2(0, (height - stepHeight) / height));

            AddUV(new Vector2(stepDepth / depth, 0));
            AddUV(new Vector2(0, 0));
            AddUV(new Vector2(1, 1));
            AddUV(new Vector2(1, (height - stepHeight) / height));

            for (int i = 0; i < properties.steps; ++i)
            {
                float stepBaseHeight = i * stepHeight;
                float stepBaseDepth = i * stepDepth - halfDepth;

                AddVertex(new Vector3(-halfWidth, stepBaseHeight, stepBaseDepth));    // step front
                AddVertex(new Vector3(halfWidth, stepBaseHeight, stepBaseDepth));
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));

                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));    // step top
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));

                AddVertex(new Vector3(-halfWidth, stepBaseHeight, stepBaseDepth));  // left cap
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));

                AddVertex(new Vector3(halfWidth, stepBaseHeight, stepBaseDepth));   // roght cap
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth));
                AddVertex(new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + stepDepth));

                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);

                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);

                AddNormal(Vector3.left);
                AddNormal(Vector3.left);
                AddNormal(Vector3.left);

                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);

                AddUV(new Vector2(0, (float)i / properties.steps));
                AddUV(new Vector2(1, (float)i / properties.steps));
                AddUV(new Vector2(1, (i + 0.5f) / properties.steps));
                AddUV(new Vector2(0, (i + 0.5f) / properties.steps));

                AddUV(new Vector2(0, (i + 0.5f) / properties.steps));
                AddUV(new Vector2(1, (i + 0.5f) / properties.steps));
                AddUV(new Vector2(1, (i + 1.0f) / properties.steps));
                AddUV(new Vector2(0, (i + 1.0f) / properties.steps));

                float currentDepth = i * stepDepth;
                AddUV(new Vector2((depth - currentDepth) / depth, stepBaseHeight / height));
                AddUV(new Vector2((depth - currentDepth) / depth, (stepBaseHeight + stepHeight) / height));
                AddUV(new Vector2((depth - currentDepth - stepDepth) / depth, (stepBaseHeight + stepHeight) / height));

                AddUV(new Vector2(currentDepth / depth, stepBaseHeight / height));
                AddUV(new Vector2((currentDepth) / depth, (stepBaseHeight + stepHeight) / height));
                AddUV(new Vector2((currentDepth + stepDepth) / depth, (stepBaseHeight + stepHeight) / height));
            }
        }
        #endregion

        #region GenerateVerticesOpen
        private void GenerateVerticesOpen()
        {
            float halfWidth = properties.width * 0.5f;
            float halfDepth = properties.depth * 0.5f;

            float stepHeight = properties.height / properties.steps;
            float stepDepth = properties.depth / properties.steps;
            float treadThickness = properties.treadThickness;
            float treadDepth = properties.treadDepth;

            for (int i = 0; i < properties.steps; ++i)
            {
                float stepBaseHeight = i * stepHeight;
                float stepBaseDepth = i * stepDepth - halfDepth;

                Vector3[] pts = new Vector3[8];
                pts[0] = new Vector3(-halfWidth, stepBaseHeight + stepHeight - treadThickness, stepBaseDepth);
                pts[1] = new Vector3(halfWidth, stepBaseHeight + stepHeight - treadThickness, stepBaseDepth);
                pts[2] = new Vector3(halfWidth, stepBaseHeight + stepHeight - treadThickness, stepBaseDepth + treadDepth);
                pts[3] = new Vector3(-halfWidth, stepBaseHeight + stepHeight - treadThickness, stepBaseDepth + treadDepth);

                pts[4] = new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth);
                pts[5] = new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth);
                pts[6] = new Vector3(halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + treadDepth);
                pts[7] = new Vector3(-halfWidth, stepBaseHeight + stepHeight, stepBaseDepth + treadDepth);

                AddVertex(pts[3]);    // step bottom
                AddVertex(pts[2]);
                AddVertex(pts[1]);
                AddVertex(pts[0]);

                AddVertex(pts[0]);    // step front
                AddVertex(pts[1]);
                AddVertex(pts[5]);
                AddVertex(pts[4]);

                AddVertex(pts[4]);    // step top
                AddVertex(pts[5]);
                AddVertex(pts[6]);
                AddVertex(pts[7]);

                AddVertex(pts[7]);    // step back
                AddVertex(pts[6]);
                AddVertex(pts[2]);
                AddVertex(pts[3]);

                AddVertex(pts[3]);    // step left
                AddVertex(pts[0]);
                AddVertex(pts[4]);
                AddVertex(pts[7]);

                AddVertex(pts[1]);    // step right
                AddVertex(pts[2]);
                AddVertex(pts[6]);
                AddVertex(pts[5]);

                AddNormal(Vector3.down);
                AddNormal(Vector3.down);
                AddNormal(Vector3.down);
                AddNormal(Vector3.down);

                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);
                AddNormal(Vector3.back);

                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);

                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);
                AddNormal(Vector3.forward);

                AddNormal(Vector3.left);
                AddNormal(Vector3.left);
                AddNormal(Vector3.left);
                AddNormal(Vector3.left);

                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);
                AddNormal(Vector3.right);

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));
                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));
            }
        }
        #endregion
        
        #region GenerateVerticesSpiralBox
        private void GenerateVerticesSpiralBox()
        {
            float stepHeight = properties.height / properties.steps;

            float innerRadius0 = properties.innerRadius;
            float outerRadius0 = innerRadius0 + properties.width;
            float innerRadius1 = innerRadius0;
            float outerRadius1 = outerRadius0;

            float rotations = properties.rotations;

            List<Vector3> pts = new List<Vector3>();
            for (int i = 0; i < properties.steps; ++i)
            {
                float stepBaseHeight = i * stepHeight;

                float angle0 = (properties.windingDirection == QcStairProperties.WindingDirection.Counterclockwise) ?
                                twoPi * rotations / properties.steps * i :
                                -twoPi * rotations / properties.steps * i;
                float angle1 = (properties.windingDirection == QcStairProperties.WindingDirection.Counterclockwise) ?
                                twoPi * rotations / properties.steps * (i + 1):
                                -twoPi * rotations / properties.steps * (i + 1);

                if (properties.conical)
                {
                    innerRadius0 = properties.innerRadius + (float)i * (properties.radius - properties.innerRadius) / (properties.steps - 1);
                    outerRadius0 = innerRadius0 + properties.width;
                    innerRadius1 = properties.innerRadius + (float)(i + 1) * (properties.radius - properties.innerRadius) / (properties.steps - 1);
                    outerRadius1 = innerRadius1 + properties.width;
                }

                pts.Clear();

                pts.Add(new Vector3(innerRadius0 * Mathf.Cos(angle0), stepBaseHeight, innerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(innerRadius1 * Mathf.Cos(angle1), stepBaseHeight, innerRadius1 * Mathf.Sin(angle1)));
                pts.Add(new Vector3(innerRadius0 * Mathf.Cos(angle0), stepBaseHeight + stepHeight, innerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(innerRadius1 * Mathf.Cos(angle1), stepBaseHeight + stepHeight, innerRadius1 * Mathf.Sin(angle1)));

                pts.Add(new Vector3(outerRadius0 * Mathf.Cos(angle0), stepBaseHeight, outerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(outerRadius1 * Mathf.Cos(angle1), stepBaseHeight, outerRadius1 * Mathf.Sin(angle1)));
                pts.Add(new Vector3(outerRadius0 * Mathf.Cos(angle0), stepBaseHeight + stepHeight, outerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(outerRadius1 * Mathf.Cos(angle1), stepBaseHeight + stepHeight, outerRadius1 * Mathf.Sin(angle1)));

                pts.Add(new Vector3(innerRadius0 * Mathf.Cos(angle0), 0, innerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(innerRadius1 * Mathf.Cos(angle1), 0, innerRadius1 * Mathf.Sin(angle1)));

                pts.Add(new Vector3(outerRadius0 * Mathf.Cos(angle0), 0, outerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(outerRadius1 * Mathf.Cos(angle1), 0, outerRadius1 * Mathf.Sin(angle1)));

                if (properties.windingDirection == QcStairProperties.WindingDirection.Clockwise)
                {
                    AddVertex(pts[8]);  // back(inner)
                    AddVertex(pts[9]);
                    AddVertex(pts[3]);
                    AddVertex(pts[2]);

                    AddVertex(pts[11]);  // front(outer)
                    AddVertex(pts[10]);
                    AddVertex(pts[6]);
                    AddVertex(pts[7]);

                    AddVertex(pts[2]);  // top
                    AddVertex(pts[3]);
                    AddVertex(pts[7]);
                    AddVertex(pts[6]);

                    AddVertex(pts[8]);  // bottom
                    AddVertex(pts[10]);
                    AddVertex(pts[11]);
                    AddVertex(pts[9]);

                    AddVertex(pts[4]);  // right
                    AddVertex(pts[0]);
                    AddVertex(pts[2]);
                    AddVertex(pts[6]);

                    
                    Vector3 normal0Out = new Vector3(pts[4].x, 0f, pts[4].z);
                    normal0Out.Normalize();
                    Vector3 normal0In = new Vector3(-pts[4].x, 0f, -pts[4].z);
                    normal0In.Normalize();

                    Vector3 normal1Out = new Vector3(pts[5].x, 0f, pts[5].z);
                    normal0Out.Normalize();
                    Vector3 normal1In = new Vector3(-pts[5].x, 0f, -pts[5].z);
                    normal0In.Normalize();

                    AddNormal(normal0In);
                    AddNormal(normal1In);
                    AddNormal(normal1In);
                    AddNormal(normal0In);

                    AddNormal(normal1Out);
                    AddNormal(normal0Out);
                    AddNormal(normal0Out);
                    AddNormal(normal1Out);

                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);

                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);

                    Vector3 capNormal1 = ComputeNormal(pts[2], pts[0], pts[4]);

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    int nSteps = properties.steps;
                    AddUV(new Vector2((float)i / nSteps, 0));
                    AddUV(new Vector2((i + 1f) / nSteps, 0));
                    AddUV(new Vector2((i + 1f) / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2((float)i / nSteps, (i + 1f) / nSteps));

                    AddUV(new Vector2(1 - (i + 1f) / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, (i + 1f) / nSteps));

                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));                    
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 0));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));

                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, 0));

                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));

                    if (i == properties.steps - 1)
                    {
                        AddVertex(pts[9]);  // left
                        AddVertex(pts[11]);
                        AddVertex(pts[7]);
                        AddVertex(pts[3]);

                        Vector3 capNormal2 = ComputeNormal(pts[7], pts[5], pts[1]);

                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);

                        AddUV(new Vector2(0, 0));
                        AddUV(new Vector2(1, 0));
                        AddUV(new Vector2(1, 1));
                        AddUV(new Vector2(0, 1));
                    }
                }
                else        // counterclockwise
                {
                    AddVertex(pts[9]);  // back(inner)
                    AddVertex(pts[8]);
                    AddVertex(pts[2]);
                    AddVertex(pts[3]);

                    AddVertex(pts[10]);  // front(outer)
                    AddVertex(pts[11]);
                    AddVertex(pts[7]);
                    AddVertex(pts[6]);

                    AddVertex(pts[6]);  // top
                    AddVertex(pts[7]);
                    AddVertex(pts[3]);
                    AddVertex(pts[2]);

                    AddVertex(pts[10]);  // bottom
                    AddVertex(pts[8]);
                    AddVertex(pts[9]);
                    AddVertex(pts[11]);

                    AddVertex(pts[0]);  // left
                    AddVertex(pts[4]);
                    AddVertex(pts[6]);
                    AddVertex(pts[2]);


                    Vector3 normal0Out = new Vector3(pts[4].x, 0f, pts[4].z);
                    normal0Out.Normalize();
                    Vector3 normal0In = new Vector3(-pts[4].x, 0f, -pts[4].z);
                    normal0In.Normalize();

                    Vector3 normal1Out = new Vector3(pts[5].x, 0f, pts[5].z);
                    normal0Out.Normalize();
                    Vector3 normal1In = new Vector3(-pts[5].x, 0f, -pts[5].z);
                    normal0In.Normalize();

                    AddNormal(normal1In);
                    AddNormal(normal0In);
                    AddNormal(normal0In);
                    AddNormal(normal1In);

                    AddNormal(normal0Out);
                    AddNormal(normal1Out);
                    AddNormal(normal1Out);
                    AddNormal(normal0Out);

                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);

                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);

                    Vector3 capNormal1 = ComputeNormal(pts[2], pts[0], pts[4]);

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    int nSteps = properties.steps;
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, (i + 1f) / nSteps));

                    AddUV(new Vector2((float)i / nSteps, 0));
                    AddUV(new Vector2((i + 1f) / nSteps, 0));
                    AddUV(new Vector2((i + 1f) / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2((float)i / nSteps, (i + 1f) / nSteps));

                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 0));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));


                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, 0));

                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));

                    if (i == properties.steps - 1)
                    {
                        AddVertex(pts[11]);  // right
                        AddVertex(pts[9]);
                        AddVertex(pts[3]);
                        AddVertex(pts[7]);

                        Vector3 capNormal2 = ComputeNormal(pts[7], pts[5], pts[1]);

                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);

                        AddUV(new Vector2(0, 0));
                        AddUV(new Vector2(1, 0));
                        AddUV(new Vector2(1, 1));
                        AddUV(new Vector2(0, 1));
                    }
                }


                for (int j = 0; j < 6; ++j)
                {
                    int bi = i * 20 + j * 4;
                    faces.Add(new TriangleIndices(bi + 1, bi + 0, bi + 2));
                    faces.Add(new TriangleIndices(bi + 2, bi + 0, bi + 3));
                }

                int ci = (i + 1) * 20;
                faces.Add(new TriangleIndices(ci + 1, ci + 0, ci + 2));
                faces.Add(new TriangleIndices(ci + 2, ci + 0, ci + 3));
            }
        }
        #endregion

        #region GenerateVerticesSpiralClosed
        private void GenerateVerticesSpiralClosed()
        {
            float stepHeight = properties.height / properties.steps;

            float innerRadius0 = properties.innerRadius;
            float outerRadius0 = innerRadius0 + properties.width;
            float innerRadius1 = innerRadius0;
            float outerRadius1 = outerRadius0;

            float rotations = properties.rotations;

            List<Vector3> pts = new List<Vector3>();
            for (int i = 0; i < properties.steps; ++i)
            {
                float stepBaseHeight = i * stepHeight;

                float angle0 = (properties.windingDirection == QcStairProperties.WindingDirection.Counterclockwise) ?
                                twoPi * rotations / properties.steps * i :
                                -twoPi * rotations / properties.steps * i;
                float angle1 = (properties.windingDirection == QcStairProperties.WindingDirection.Counterclockwise) ?
                                twoPi * rotations / properties.steps * (i + 1) :
                                -twoPi * rotations / properties.steps * (i + 1);
                
                if (properties.conical)
                {
                    innerRadius0 = properties.innerRadius + (float)i * (properties.radius - properties.innerRadius) / (properties.steps - 1);
                    outerRadius0 = innerRadius0 + properties.width;
                    innerRadius1 = properties.innerRadius + (float)(i + 1) * (properties.radius - properties.innerRadius) / (properties.steps - 1);
                    outerRadius1 = innerRadius1 + properties.width;
                }

                pts.Clear();

                pts.Add(new Vector3(innerRadius0 * Mathf.Cos(angle0), stepBaseHeight, innerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(innerRadius1 * Mathf.Cos(angle1), stepBaseHeight, innerRadius1 * Mathf.Sin(angle1)));
                pts.Add(new Vector3(innerRadius0 * Mathf.Cos(angle0), stepBaseHeight + stepHeight, innerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(innerRadius1 * Mathf.Cos(angle1), stepBaseHeight + stepHeight, innerRadius1 * Mathf.Sin(angle1)));

                pts.Add(new Vector3(outerRadius0 * Mathf.Cos(angle0), stepBaseHeight, outerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(outerRadius1 * Mathf.Cos(angle1), stepBaseHeight, outerRadius1 * Mathf.Sin(angle1)));
                pts.Add(new Vector3(outerRadius0 * Mathf.Cos(angle0), stepBaseHeight + stepHeight, outerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(outerRadius1 * Mathf.Cos(angle1), stepBaseHeight + stepHeight, outerRadius1 * Mathf.Sin(angle1)));

                pts.Add(new Vector3(innerRadius0 * Mathf.Cos(angle0), stepBaseHeight - stepHeight, innerRadius0 * Mathf.Sin(angle0)));
                pts.Add(new Vector3(outerRadius0 * Mathf.Cos(angle0), stepBaseHeight - stepHeight, outerRadius0 * Mathf.Sin(angle0)));

                if (properties.windingDirection == QcStairProperties.WindingDirection.Clockwise)
                {
                    if (i == 0)
                        AddVertex(pts[0]);  // back(inner)
                    else
                        AddVertex(pts[8]);  // back(inner)
                    AddVertex(pts[1]);
                    AddVertex(pts[3]);
                    AddVertex(pts[2]);

                   AddVertex(pts[5]);  // front(outer)
                    if (i == 0)
                        AddVertex(pts[4]);
                    else
                        AddVertex(pts[9]);
                    AddVertex(pts[6]);
                    AddVertex(pts[7]);

                    AddVertex(pts[2]);  // top
                    AddVertex(pts[3]);
                    AddVertex(pts[7]);
                    AddVertex(pts[6]);
                    
                    if (i == 0)     // bottom
                    {
                        AddVertex(pts[0]);
                        AddVertex(pts[4]);
                    }
                    else
                    {
                        AddVertex(pts[8]);
                        AddVertex(pts[9]);
                    }
                    AddVertex(pts[5]);
                    AddVertex(pts[1]);  

                    AddVertex(pts[4]);  // right
                    AddVertex(pts[0]);
                    AddVertex(pts[2]);
                    AddVertex(pts[6]);

                    Vector3 normal0Out = new Vector3(pts[4].x, 0f, pts[4].z);
                    normal0Out.Normalize();
                    Vector3 normal0In = new Vector3(-pts[4].x, 0f, -pts[4].z);
                    normal0In.Normalize();

                    Vector3 normal1Out = new Vector3(pts[5].x, 0f, pts[5].z);
                    normal1Out.Normalize();
                    Vector3 normal1In = new Vector3(-pts[5].x, 0f, -pts[5].z);
                    normal1In.Normalize();

                    AddNormal(normal0In);
                    AddNormal(normal1In);
                    AddNormal(normal1In);
                    AddNormal(normal0In);

                    AddNormal(normal1Out);
                    AddNormal(normal0Out);
                    AddNormal(normal0Out);
                    AddNormal(normal1Out);

                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);

                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);

                    Vector3 capNormal1 = ComputeNormal(pts[2], pts[0], pts[4]);

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    int nSteps = properties.steps;
                    if (i == 0)
                    {
                        AddUV(new Vector2((float)i / nSteps, (float)i / nSteps));
                        AddUV(new Vector2((i + 1f) / nSteps, (float)i / nSteps));
                    }
                    else
                    {
                        AddUV(new Vector2((float)i / nSteps, (i - 1f) / nSteps));
                        AddUV(new Vector2((i + 1f) / nSteps, (float)i / nSteps));
                    }
                    AddUV(new Vector2((i + 1f) / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2((float)i / nSteps, (i + 1f) / nSteps));

                    if (i == 0)
                    {
                        AddUV(new Vector2(1 - (i + 1f) / nSteps, (float)i / nSteps));
                        AddUV(new Vector2(1 - (float)i / nSteps, (float)i / nSteps));
                    }
                    else
                    {
                        AddUV(new Vector2(1 - (i + 1f) / nSteps, (float)i / nSteps));
                        AddUV(new Vector2(1 - (float)i / nSteps, (i - 1f) / nSteps));
                    }
                    AddUV(new Vector2(1 - (float)i / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, (i + 1f) / nSteps));

                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 0));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));

                    AddUV(new Vector2(1 - (float)i / (nSteps + 1), 0));
                    AddUV(new Vector2(1 - (float)i / (nSteps + 1), 1));
                    AddUV(new Vector2(1 - (i + 1f) / (nSteps + 1), 1));
                    AddUV(new Vector2(1 - (i + 1f) / (nSteps + 1), 0));

                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));

                    if (i == properties.steps - 1)
                    {
                        AddVertex(pts[1]);  // left
                        AddVertex(pts[5]);
                        AddVertex(pts[7]);
                        AddVertex(pts[3]);

                        Vector3 capNormal2 = ComputeNormal(pts[7], pts[5], pts[1]);

                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        
                        AddUV(new Vector2(1.0f / (nSteps + 1), 0));
                        AddUV(new Vector2(1.0f / (nSteps + 1), 1));
                        AddUV(new Vector2(0, 1));
                        AddUV(new Vector2(0, 0));
                    }
                }
                else        // counterclockwise
                {
                    AddVertex(pts[1]);  // back(inner)
                    if (i == 0)
                        AddVertex(pts[0]);
                    else
                        AddVertex(pts[8]);
                    AddVertex(pts[2]);
                    AddVertex(pts[3]);

                    if (i == 0)
                        AddVertex(pts[4]);  // front(outer)
                    else
                        AddVertex(pts[9]);
                    AddVertex(pts[5]);
                    AddVertex(pts[7]);
                    AddVertex(pts[6]);

                    AddVertex(pts[6]);  // top
                    AddVertex(pts[7]);
                    AddVertex(pts[3]);
                    AddVertex(pts[2]);

                    if (i == 0)
                    {
                        AddVertex(pts[4]);  // bottom
                        AddVertex(pts[0]);
                        AddVertex(pts[1]);
                        AddVertex(pts[5]);
                    }
                    else
                    {
                        AddVertex(pts[9]);  // bottom
                        AddVertex(pts[8]);
                        AddVertex(pts[1]);
                        AddVertex(pts[5]);
                    }

                    AddVertex(pts[0]);  // left
                    AddVertex(pts[4]);
                    AddVertex(pts[6]);
                    AddVertex(pts[2]);

                    Vector3 normal0Out = new Vector3(pts[4].x, 0f, pts[4].z);
                    normal0Out.Normalize();
                    Vector3 normal0In = new Vector3(-pts[4].x, 0f, -pts[4].z);
                    normal0In.Normalize();

                    Vector3 normal1Out = new Vector3(pts[5].x, 0f, pts[5].z);
                    normal0Out.Normalize();
                    Vector3 normal1In = new Vector3(-pts[5].x, 0f, -pts[5].z);
                    normal0In.Normalize();

                    AddNormal(normal1In);
                    AddNormal(normal0In);
                    AddNormal(normal0In);
                    AddNormal(normal1In);

                    AddNormal(normal0Out);
                    AddNormal(normal1Out);
                    AddNormal(normal1Out);
                    AddNormal(normal0Out);

                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);
                    AddNormal(Vector3.up);

                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);
                    AddNormal(Vector3.down);

                    Vector3 capNormal1 = ComputeNormal(pts[6], pts[4], pts[0]);

                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);
                    AddNormal(capNormal1);

                    int nSteps = properties.steps;
                    if (i == 0)
                    {
                        AddUV(new Vector2(1 - (i + 1f) / nSteps, (float)i / nSteps));
                        AddUV(new Vector2(1 - (float)i / nSteps, (float)i / nSteps));
                    }
                    else
                    {
                        AddUV(new Vector2(1 - (i + 1f) / nSteps, (float)i / nSteps));
                        AddUV(new Vector2(1 - (float)i / nSteps, (i - 1f) / nSteps));
                    }
                    AddUV(new Vector2(1 - (float)i / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2(1 - (i + 1f) / nSteps, (i + 1f) / nSteps));

                    if (i == 0)
                    {
                        AddUV(new Vector2((float)i / nSteps, (float)i / nSteps));
                        AddUV(new Vector2((i + 1f) / nSteps, (float)i / nSteps));
                    }
                    else
                    {
                        AddUV(new Vector2((float)i / nSteps, (i - 1f) / nSteps));
                        AddUV(new Vector2((i + 1f) / nSteps, (float)i / nSteps));
                    }
                    AddUV(new Vector2((i + 1f) / nSteps, (i + 1f) / nSteps));
                    AddUV(new Vector2((float)i / nSteps, (i + 1f) / nSteps));

                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 1.0f) / nSteps, 0));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));

                    AddUV(new Vector2(1 - (float)i / (nSteps + 1), 0));
                    AddUV(new Vector2(1 - (float)i / (nSteps + 1), 1));
                    AddUV(new Vector2(1 - (i + 1f) / (nSteps + 1), 1));
                    AddUV(new Vector2(1 - (i + 1f) / (nSteps + 1), 0));

                    AddUV(new Vector2(1 - (float)i / nSteps, 0));
                    AddUV(new Vector2(1 - (float)i / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 1));
                    AddUV(new Vector2(1 - (i + 0.5f) / nSteps, 0));

                    if (i == properties.steps - 1)
                    {
                        AddVertex(pts[5]);  // right
                        AddVertex(pts[1]);
                        AddVertex(pts[3]);
                        AddVertex(pts[7]);

                        Vector3 capNormal2 = ComputeNormal(pts[3], pts[1], pts[5]);

                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);
                        AddNormal(capNormal2);

                        AddUV(new Vector2(1.0f / (nSteps + 1), 0));
                        AddUV(new Vector2(1.0f / (nSteps + 1), 1));
                        AddUV(new Vector2(0, 1));
                        AddUV(new Vector2(0, 0));
                    }
                }

                for (int j = 0; j < 5; ++j)
                {
                    int bi = i * 20 + j * 4;
                    faces.Add(new TriangleIndices(bi + 1, bi + 0, bi + 2));
                    faces.Add(new TriangleIndices(bi + 2, bi + 0, bi + 3));
                }

                int ci = i * 20 + 20;
                faces.Add(new TriangleIndices(ci + 1, ci + 0, ci + 2));
                faces.Add(new TriangleIndices(ci + 2, ci + 0, ci + 3));
            }
        }
        #endregion

        #region GenerateVerticesSpiralOpen
        private void GenerateVerticesSpiralOpen()
        {
            float stepHeight = properties.height / properties.steps;
            float treadThickness = properties.treadThickness;
            float treadDepth = properties.treadDepth;

            float rotations = properties.rotations;

            float innerRadius = properties.innerRadius;
            float outerRadius = innerRadius + properties.width;

            for (int i = 0; i < properties.steps; ++i)
            {
                float stepBaseHeight = i * stepHeight;
                float stepHalfDepth = treadDepth * 0.5f;

                if (properties.conical)
                {
                    innerRadius = properties.innerRadius + (float)i * (properties.radius - properties.innerRadius) / (properties.steps - 1);
                    outerRadius = innerRadius + properties.width;
                }

                Vector3[] pts = new Vector3[8];
                pts[0] = new Vector3(innerRadius, stepBaseHeight + stepHeight - treadThickness, -stepHalfDepth);
                pts[1] = new Vector3(outerRadius, stepBaseHeight + stepHeight - treadThickness, -stepHalfDepth);
                pts[2] = new Vector3(outerRadius, stepBaseHeight + stepHeight - treadThickness, stepHalfDepth);
                pts[3] = new Vector3(innerRadius, stepBaseHeight + stepHeight - treadThickness, stepHalfDepth);

                pts[4] = new Vector3(innerRadius, stepBaseHeight + stepHeight, -stepHalfDepth);
                pts[5] = new Vector3(outerRadius, stepBaseHeight + stepHeight, -stepHalfDepth);
                pts[6] = new Vector3(outerRadius, stepBaseHeight + stepHeight, stepHalfDepth);
                pts[7] = new Vector3(innerRadius, stepBaseHeight + stepHeight, stepHalfDepth);

                float rotDegree = (properties.windingDirection == QcStairProperties.WindingDirection.Clockwise) ?
                                  360.0f * rotations / properties.steps * i :
                                  -360.0f * rotations / properties.steps * i;
                //float rotRad = rotDegree * Mathf.Deg2Rad;
                for (int pi = 0; pi < 8; ++pi)
                {
                    pts[pi] = Quaternion.Euler(0, rotDegree, 0) * pts[pi];
                }

                AddVertex(pts[3]);    // step bottom
                AddVertex(pts[2]);
                AddVertex(pts[1]);
                AddVertex(pts[0]);

                AddVertex(pts[0]);    // step front
                AddVertex(pts[1]);
                AddVertex(pts[5]);
                AddVertex(pts[4]);

                AddVertex(pts[4]);    // step top
                AddVertex(pts[5]);
                AddVertex(pts[6]);
                AddVertex(pts[7]);

                AddVertex(pts[7]);    // step back
                AddVertex(pts[6]);
                AddVertex(pts[2]);
                AddVertex(pts[3]);

                AddVertex(pts[3]);    // step left
                AddVertex(pts[0]);
                AddVertex(pts[4]);
                AddVertex(pts[7]);

                AddVertex(pts[1]);    // step right
                AddVertex(pts[2]);
                AddVertex(pts[6]);
                AddVertex(pts[5]);

                AddNormal(Vector3.down);
                AddNormal(Vector3.down);
                AddNormal(Vector3.down);
                AddNormal(Vector3.down);


                Vector3 frontNormal = Quaternion.Euler(0, rotDegree, 0) * Vector3.back;
                AddNormal(frontNormal);
                AddNormal(frontNormal);
                AddNormal(frontNormal);
                AddNormal(frontNormal);

                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);
                AddNormal(Vector3.up);

                Vector3 backNormal = Quaternion.Euler(0, rotDegree, 0) * Vector3.forward;
                AddNormal(backNormal);
                AddNormal(backNormal);
                AddNormal(backNormal);
                AddNormal(backNormal);

                Vector3 leftNormal = Quaternion.Euler(0, rotDegree, 0) * Vector3.left;
                AddNormal(leftNormal);
                AddNormal(leftNormal);
                AddNormal(leftNormal);
                AddNormal(leftNormal);

                Vector3 rightNormal = Quaternion.Euler(0, rotDegree, 0) * Vector3.right;
                AddNormal(rightNormal);
                AddNormal(rightNormal);
                AddNormal(rightNormal);
                AddNormal(rightNormal);

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));
                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));

                AddUV(new Vector2(0, 0));
                AddUV(new Vector2(1, 0));
                AddUV(new Vector2(1, 1));
                AddUV(new Vector2(0, 1));
            }
        }
        #endregion

        #region GenerateTrianglesBox
        private void GenerateTrianglesBox()
        {
            // bottom triangles
            faces.Add(new TriangleIndices(0, 3, 1));
            faces.Add(new TriangleIndices(1, 3, 2));

            //// top triangles
            //faces.Add(new TriangleIndices(5, 4, 6));
            //faces.Add(new TriangleIndices(6, 4, 7));

            // back triangles
            faces.Add(new TriangleIndices(4, 7, 5));
            faces.Add(new TriangleIndices(5, 7, 6));

            // side triangles
            faces.Add(new TriangleIndices(8, 10, 9));
            faces.Add(new TriangleIndices(11, 13, 12));
            
            for (int i = 0; i < properties.steps; ++i)
            {
                int baseIndex = 14 + i * 14;
                faces.Add(new TriangleIndices(baseIndex + 1, baseIndex + 0, baseIndex + 2));    // step front
                faces.Add(new TriangleIndices(baseIndex + 2, baseIndex + 0, baseIndex + 3));

                faces.Add(new TriangleIndices(baseIndex + 5, baseIndex + 4, baseIndex + 6));    // step top
                faces.Add(new TriangleIndices(baseIndex + 6, baseIndex + 4, baseIndex + 7));

                faces.Add(new TriangleIndices(baseIndex + 8, baseIndex + 10, baseIndex + 9));    // step sides
                faces.Add(new TriangleIndices(baseIndex + 12, baseIndex + 13, baseIndex + 11));
            }
        }
        #endregion

        #region GenerateTrianglesClosed
        private void GenerateTrianglesClosed()
        {
            // bottom triangles
            faces.Add(new TriangleIndices(0, 3, 1));
            faces.Add(new TriangleIndices(1, 3, 2));

            // slanted bottom triangles
            faces.Add(new TriangleIndices(4, 5, 7));
            faces.Add(new TriangleIndices(7, 5, 6));

            // back triangles
            faces.Add(new TriangleIndices(8, 11, 9));
            faces.Add(new TriangleIndices(9, 11, 10));

            // side triangles
            faces.Add(new TriangleIndices(12, 15, 13));
            faces.Add(new TriangleIndices(13, 15, 14));

            faces.Add(new TriangleIndices(16, 17, 19));
            faces.Add(new TriangleIndices(19, 17, 18));

            for (int i = 0; i < properties.steps; ++i)
            {
                int baseIndex = 20 + i * 14;
                faces.Add(new TriangleIndices(baseIndex + 1, baseIndex + 0, baseIndex + 2));    // step front
                faces.Add(new TriangleIndices(baseIndex + 2, baseIndex + 0, baseIndex + 3));

                faces.Add(new TriangleIndices(baseIndex + 5, baseIndex + 4, baseIndex + 6));    // step top
                faces.Add(new TriangleIndices(baseIndex + 6, baseIndex + 4, baseIndex + 7));

                faces.Add(new TriangleIndices(baseIndex + 8, baseIndex + 10, baseIndex + 9));    // step sides
                faces.Add(new TriangleIndices(baseIndex + 12, baseIndex + 13, baseIndex + 11));
            }
        }
        #endregion

        #region GenerateTrianglesOpen
        private void GenerateTrianglesOpen()
        {
            for (int i = 0; i < properties.steps; ++i)
            {
                int baseIndex = i * 24;

                faces.Add(new TriangleIndices(baseIndex + 1, baseIndex + 0, baseIndex + 2));    // step bottom
                faces.Add(new TriangleIndices(baseIndex + 2, baseIndex + 0, baseIndex + 3));

                faces.Add(new TriangleIndices(baseIndex + 4, baseIndex + 7, baseIndex + 5));    // step front
                faces.Add(new TriangleIndices(baseIndex + 5, baseIndex + 7, baseIndex + 6));

                faces.Add(new TriangleIndices(baseIndex + 9, baseIndex + 8, baseIndex + 10));    // step top
                faces.Add(new TriangleIndices(baseIndex + 10, baseIndex + 8, baseIndex + 11));

                faces.Add(new TriangleIndices(baseIndex + 12, baseIndex + 15, baseIndex + 13));    // step back
                faces.Add(new TriangleIndices(baseIndex + 13, baseIndex + 15, baseIndex + 14));

                faces.Add(new TriangleIndices(baseIndex + 17, baseIndex + 16, baseIndex + 18));    // step left side
                faces.Add(new TriangleIndices(baseIndex + 18, baseIndex + 16, baseIndex + 19));

                faces.Add(new TriangleIndices(baseIndex + 21, baseIndex + 20, baseIndex + 22));    // step right side
                faces.Add(new TriangleIndices(baseIndex + 22, baseIndex + 20, baseIndex + 23));
            }
        }
        #endregion
    }
}
