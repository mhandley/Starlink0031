using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    [ExecuteInEditMode]
    public class QcBoxMesh : QcPrimitivesBase
    {
        [System.Serializable]
        public class QcBoxProperties : QcBaseProperties
        {
            [System.Serializable]
            public class SlantedSides
            {
                public Vector2 size;
            }

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

            public enum Options { None, SlantedSides, BeveledEdge, Hollow }
            
            public float width = 1;
            public float depth = 1;
            public float height = 1;

            public int widthSegments = 1;
            public int depthSegments = 1;
            public int heightSegments = 1;

            public Options option = Options.None;
            public SlantedSides slantedSides = new SlantedSides();
            public BeveledEdge beveledEdge = new BeveledEdge();
            public Hollow hollow = new Hollow();
            public bool textureWrapped;

            public void CopyFrom(QcBoxProperties source)
            {
                base.CopyFrom(source);

                this.width = source.width;
                this.height = source.height;
                this.depth = source.depth;

                this.widthSegments = source.widthSegments;
                this.heightSegments = source.heightSegments;
                this.depthSegments = source.depthSegments;

                this.beveledEdge.width = source.beveledEdge.width;
                this.slantedSides.size = source.slantedSides.size;

                this.hollow.thickness = source.hollow.thickness;
                this.hollow.height = source.hollow.height;

                this.textureWrapped = source.textureWrapped;

                this.option = source.option;
            }

            public bool Modified(QcBoxProperties source)
            {
                return ((this.width != source.width) || 
                        (this.height != source.height) || 
                        (this.depth != source.depth) ||
                        (this.widthSegments != source.widthSegments) ||
                        (this.heightSegments != source.heightSegments) ||
                        (this.depthSegments != source.depthSegments) ||
                        (this.offset[0] != source.offset[0]) ||
                        (this.offset[1] != source.offset[1]) ||
                        (this.offset[2] != source.offset[2]) ||
                        (this.genTextureCoords != source.genTextureCoords) ||
                        (this.textureWrapped != source.textureWrapped) ||
                        (this.addCollider != source.addCollider) ||
                        (this.option != source.option) ||
                        ((source.option == QcBoxProperties.Options.BeveledEdge) && 
                         (this.beveledEdge.width != source.beveledEdge.width)) ||
                        ((source.option == QcBoxProperties.Options.SlantedSides) && 
                         ((this.slantedSides.size[0] != source.slantedSides.size[0]) || 
                          (this.slantedSides.size[1] != source.slantedSides.size[1]))) ||
                        ((source.option == QcBoxProperties.Options.Hollow) && 
                         ((this.hollow.thickness != source.hollow.thickness) || 
                          (this.hollow.height != source.hollow.height))));
            }
        }

        public QcBoxProperties properties = new QcBoxProperties();

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
            float[] x = new float[4];
            float[] y = new float[4];

            // 4 vertices on the base rectangle
            if (properties.option == QcBoxProperties.Options.SlantedSides)
            {
                x[0] = -properties.width * 0.5f + properties.slantedSides.size[0];
                y[0] = -properties.depth * 0.5f;

                x[1] = properties.width * 0.5f - properties.slantedSides.size[0];
                y[1] = -properties.depth * 0.5f;
            }
            else
            {
                x[0] = -properties.width * 0.5f;
                y[0] = -properties.depth * 0.5f;

                x[1] = properties.width * 0.5f;
                y[1] = -properties.depth * 0.5f;
            }

            x[2] = properties.width * 0.5f;
            y[2] = properties.depth * 0.5f;

            x[3] = -properties.width * 0.5f;
            y[3] = properties.depth * 0.5f;


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

            if ((properties.option == QcBoxProperties.Options.BeveledEdge) && (properties.beveledEdge.width > 0))
            {
                GenerateVerticesBeveled();
                GenerateTrianglesBeveled();
            }
            else if ((properties.option == QcBoxProperties.Options.Hollow) && 
                     (properties.hollow.thickness > 0) && (properties.hollow.height > 0))
            {
                GenerateVerticesHollowed();
                GenerateTrianglesHollowed();
            }
            else if ((properties.option == QcBoxProperties.Options.SlantedSides) && 
                     ((properties.slantedSides.size[0] > 0) || (properties.slantedSides.size[1] > 0)))
            {
                GenerateVerticesSlanted();
                GenerateTrianglesSlanted();
            }
            else
            {
                GenerateVertices();
                GenerateTriangles();
            }

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

            if ((properties.option == QcBoxProperties.Options.BeveledEdge) && (properties.beveledEdge.width > 0))
            {
                GenerateVerticesBeveled();
                GenerateTrianglesBeveled();
            }
            else if ((properties.option == QcBoxProperties.Options.Hollow) && 
                     (properties.hollow.thickness > 0) && (properties.hollow.height > 0))
            {
                GenerateVerticesHollowed();
                GenerateTrianglesHollowed();
            }
            else if ((properties.option == QcBoxProperties.Options.SlantedSides) && 
                     ((properties.slantedSides.size[0] > 0) || (properties.slantedSides.size[1] > 0)))
            {
                GenerateVerticesSlanted();
                GenerateTrianglesSlanted();
            }
            else
            {
                GenerateVertices();
                GenerateTriangles();
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
            if ((properties.option == QcBoxProperties.Options.BeveledEdge) && (properties.beveledEdge.width > 0))
            {
                SetTextureCoordsBeveled();
            }
            else if ((properties.option == QcBoxProperties.Options.Hollow) && 
                        (properties.hollow.thickness > 0) && (properties.hollow.height > 0))
            {
                SetTextureCoordsHollowed();
            }
            else if ((properties.option == QcBoxProperties.Options.SlantedSides) && 
                        ((properties.slantedSides.size[0] > 0) || (properties.slantedSides.size[1] > 0)))
            {
                SetTextureCoordsSlanted();
            }
            else
            {
                SetTextureCoords();
            }

            gameObject.GetComponent<MeshFilter>().sharedMesh.uv = uvs.ToArray();
        }

        #region GenerateVertices
        private void GenerateVertices()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            // for bottom face
            for (int j = 0; j <= depthSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width * 0.5f + i * width / widthSeg, 
                                          -height * 0.5f, 
                                          depth * 0.5f - j * depth / depthSeg));
                    AddNormal(Vector3.down);
                }
            }

            // for top face
            for (int j = 0; j <= depthSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width * 0.5f + i * width / widthSeg,
                                          height * 0.5f,
                                          -depth * 0.5f + j * depth / depthSeg));
                    AddNormal(Vector3.up);
                }
            }

            // for front face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width * 0.5f + i * width / widthSeg,
                                          -height * 0.5f + j * height / heightSeg,
                                          -depth * 0.5f));
                    AddNormal(Vector3.back);
                }
            }

            // for back face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(width * 0.5f - i * width / widthSeg,
                                          -height * 0.5f + j * height / heightSeg,
                                          depth * 0.5f));
                    AddNormal(Vector3.forward);
                }
            }

            // for left side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    AddVertex(new Vector3(-width * 0.5f,
                                          -height * 0.5f + j * height / heightSeg,
                                          depth * 0.5f - i * depth / depthSeg));
                    AddNormal(Vector3.left);
                }
            }

            // for right side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    AddVertex(new Vector3(width * 0.5f,
                                          -height * 0.5f + j * height / heightSeg,
                                          -depth * 0.5f + i * depth / depthSeg));
                    AddNormal(Vector3.right);
                }
            }

            SetTextureCoords();
        }
        #endregion

        #region GenerateVerticesSlanted
        private void GenerateVerticesSlanted()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            Vector2 slant = properties.slantedSides.size;

            float width0 = properties.width - slant[0] * 2;
            float height0 = properties.height - slant[1] * 2;

            Vector3 topNormal = new Vector3(0, depth, -slant[1]);
            topNormal.Normalize();
            Vector3 bottomNormal = new Vector3(0, -depth, -slant[1]);
            bottomNormal.Normalize();            

            // for bottom face
            for (int j = 0; j <= depthSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    float w = ((float)j / depthSeg * width0 + (1.0f - (float)j / depthSeg) * width) * i / widthSeg +
                               (float)j / depthSeg * slant[0];
                    AddVertex(new Vector3(-width * 0.5f + w,
                                          -height * 0.5f + slant[1] * (float)j / depthSeg,
                                          depth * 0.5f - j * depth / depthSeg));
                    AddNormal(bottomNormal);
                }
            }

            // for top face
            for (int j = 0; j <= depthSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    float w = ((float)j / depthSeg * width + (1.0f - (float)j / depthSeg) * width0) * i / widthSeg + 
                               (1 - (float)j / depthSeg) * slant[0];
                    AddVertex(new Vector3(-width * 0.5f + w,
                                          height * 0.5f - slant[1] * (1 - (float)j / depthSeg),
                                          -depth * 0.5f + j * depth / depthSeg));
                    AddNormal(topNormal);
                }
            }

            // for front face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width0 * 0.5f + i * width0 / widthSeg,
                                          -height0 * 0.5f + j * height0 / heightSeg,
                                          -depth * 0.5f));
                    AddNormal(Vector3.back);
                }
            }

            // for back face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(width * 0.5f - i * width / widthSeg,
                                          -height * 0.5f + j * height / heightSeg,
                                          depth * 0.5f));
                    AddNormal(Vector3.forward);
                }
            }

            Vector3 leftNormal = new Vector3(-depth, 0, -slant[0]);
            leftNormal.Normalize();
            Vector3 rightNormal = new Vector3(depth, 0, -slant[0]);
            rightNormal.Normalize();

            // for left side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    float h = ((float)i / depthSeg * height0 + (1.0f - (float)i / depthSeg) * height) * j / heightSeg + 
                              (float)i / depthSeg * slant[1];
                    AddVertex(new Vector3(-width * 0.5f + slant[0] * (float)i / depthSeg,
                                          -height * 0.5f + h,
                                          depth * 0.5f - i * depth / depthSeg));
                    AddNormal(leftNormal);
                }
            }

            // for right side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    float h = ((float)i / depthSeg * height + (1.0f - (float)i / depthSeg) * height0) * j / heightSeg +
                              (1 - (float)i / depthSeg) * slant[1];
                    AddVertex(new Vector3(width * 0.5f - slant[0] * (1 - (float)i / depthSeg),
                                          -height * 0.5f + h,
                                          -depth * 0.5f + i * depth / depthSeg));
                    AddNormal(rightNormal);
                }
            }

            SetTextureCoordsSlanted();
        }
        #endregion

        #region GenerateVerticesBeveled
        private void GenerateVerticesBeveled()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            float width0 = properties.width - properties.beveledEdge.width * 2;
            float depth0 = properties.depth - properties.beveledEdge.width * 2;
            float height0 = height - properties.beveledEdge.width * 2;

            float[] x = new float[4];
            float[] y = new float[4];

            // 4 vertices on the base rectangle
            x[0] = -width * 0.5f;
            y[0] = -depth * 0.5f;

            x[1] = width * 0.5f;
            y[1] = -depth * 0.5f;

            x[2] = width * 0.5f;
            y[2] = depth * 0.5f;

            x[3] = -width * 0.5f;
            y[3] = depth * 0.5f;

            float[] x0 = new float[4];
            float[] y0 = new float[4];

            x0[0] = -width0 * 0.5f;
            y0[0] = -depth0 * 0.5f;

            x0[1] = width0 * 0.5f;
            y0[1] = -depth0 * 0.5f;

            x0[2] = width0 * 0.5f;
            y0[2] = depth0 * 0.5f;

            x0[3] = -width0 * 0.5f;
            y0[3] = depth0 * 0.5f;

            // Basic top, bottom, side faces

            List<Vector3> v = new List<Vector3>();

            // for bottom face
            v.Add(new Vector3(x0[3], -height * 0.5f, y0[3]));  // 0
            v.Add(new Vector3(x0[2], -height * 0.5f, y0[2]));
            v.Add(new Vector3(x0[1], -height * 0.5f, y0[1]));
            v.Add(new Vector3(x0[0], -height * 0.5f, y0[0]));
                 
            v.Add(new Vector3(x0[0], height * 0.5f, y0[0])); // 4
            v.Add(new Vector3(x0[1], height * 0.5f, y0[1]));
            v.Add(new Vector3(x0[2], height * 0.5f, y0[2]));
            v.Add(new Vector3(x0[3], height * 0.5f, y0[3]));
     
            v.Add(new Vector3(x0[0], -height0 * 0.5f, y[0])); // 8
            v.Add(new Vector3(x0[1], -height0 * 0.5f, y[1]));
            v.Add(new Vector3(x0[1], height0 * 0.5f, y[1]));
            v.Add(new Vector3(x0[0], height0 * 0.5f, y[0]));

            v.Add(new Vector3(x0[2], -height0 * 0.5f, y[2])); //12
            v.Add(new Vector3(x0[3], -height0 * 0.5f, y[3]));
            v.Add(new Vector3(x0[3], height0 * 0.5f, y[3]));
            v.Add(new Vector3(x0[2], height0 * 0.5f, y[2]));
           
            v.Add(new Vector3(x[3], -height0 * 0.5f, y0[3])); // 16
            v.Add(new Vector3(x[0], -height0 * 0.5f, y0[0]));
            v.Add(new Vector3(x[0], height0 * 0.5f, y0[0]));
            v.Add(new Vector3(x[3], height0 * 0.5f, y0[3]));
            
            v.Add(new Vector3(x[1], -height0 * 0.5f, y0[1]));  //20
            v.Add(new Vector3(x[2], -height0 * 0.5f, y0[2]));
            v.Add(new Vector3(x[2], height0 * 0.5f, y0[2]));
            v.Add(new Vector3(x[1], height0 * 0.5f, y0[1]));

            #region old code
            //// Bevels (rectangels)

            // front bottom
            //AddVertex(vertices[3]);
            //AddVertex(vertices[2]);
            //AddVertex(vertices[9]);
            //AddVertex(vertices[8]);

            //// front top
            //AddVertex(vertices[11]);
            //AddVertex(vertices[10]);
            //AddVertex(vertices[5]);
            //AddVertex(vertices[4]);

            //// back bottom
            //AddVertex(vertices[1]);
            //AddVertex(vertices[0]);
            //AddVertex(vertices[13]);
            //AddVertex(vertices[12]);

            //// back top
            //AddVertex(vertices[15]);
            //AddVertex(vertices[14]);
            //AddVertex(vertices[7]);
            //AddVertex(vertices[6]);

            // front left bevel
            //AddVertex(vertices[17]);
            //AddVertex(vertices[8]);
            //AddVertex(vertices[11]);
            //AddVertex(vertices[18]);

            //// front right bevel
            //AddVertex(vertices[9]);
            //AddVertex(vertices[20]);
            //AddVertex(vertices[23]);
            //AddVertex(vertices[10]);

            //// back left bevel
            //AddVertex(vertices[21]);
            //AddVertex(vertices[12]);
            //AddVertex(vertices[15]);
            //AddVertex(vertices[22]);

            //// back right bevel
            //AddVertex(vertices[13]);
            //AddVertex(vertices[16]);
            //AddVertex(vertices[19]);
            //AddVertex(vertices[14]);

            //// bottom left bevel
            //AddVertex(vertices[0]);
            //AddVertex(vertices[3]);
            //AddVertex(vertices[17]);
            //AddVertex(vertices[16]);

            //// top left bevel
            //AddVertex(vertices[19]);
            //AddVertex(vertices[18]);
            //AddVertex(vertices[4]);
            //AddVertex(vertices[7]);

            //// bottom right bevel
            //AddVertex(vertices[2]);
            //AddVertex(vertices[1]);
            //AddVertex(vertices[21]);
            //AddVertex(vertices[20]);

            //// top right bevel
            //AddVertex(vertices[23]);
            //AddVertex(vertices[22]);
            //AddVertex(vertices[6]);
            //AddVertex(vertices[5]);
            #endregion

            // front bottom
            AddVertex(v[3]);    // 0
            AddVertex(v[2]);
            AddVertex(v[9]);
            AddVertex(v[8]);
            
            // front top
            AddVertex(v[11]);   // 4
            AddVertex(v[10]);
            AddVertex(v[5]);
            AddVertex(v[4]);

            // back bottom
            AddVertex(v[1]);    // 8
            AddVertex(v[0]);
            AddVertex(v[13]);
            AddVertex(v[12]);

            // back top
            AddVertex(v[15]);   // 12
            AddVertex(v[14]);
            AddVertex(v[7]);
            AddVertex(v[6]);

            // front left bevel
            AddVertex(v[17]);   // 16
            AddVertex(v[8]);
            AddVertex(v[11]);
            AddVertex(v[18]);

            // front right bevel
            AddVertex(v[9]);    // 20
            AddVertex(v[20]);
            AddVertex(v[23]);
            AddVertex(v[10]);

            // back left bevel
            AddVertex(v[21]);   // 24
            AddVertex(v[12]);
            AddVertex(v[15]);
            AddVertex(v[22]);

            // back right bevel
            AddVertex(v[13]);   // 28
            AddVertex(v[16]);
            AddVertex(v[19]);
            AddVertex(v[14]);

            // bottom left bevel
            AddVertex(v[0]);    // 32
            AddVertex(v[3]);
            AddVertex(v[17]);
            AddVertex(v[16]);

            // top left bevel
            AddVertex(v[19]);   // 36
            AddVertex(v[18]);
            AddVertex(v[4]);
            AddVertex(v[7]);

            // bottom right bevel
            AddVertex(v[2]);    // 40
            AddVertex(v[1]);
            AddVertex(v[21]);
            AddVertex(v[20]);

            // top right bevel
            AddVertex(v[23]);   // 44
            AddVertex(v[22]);
            AddVertex(v[6]);
            AddVertex(v[5]);

            // Front corners

            // bottom left corner
            AddVertex(v[3]);    // 48
            AddVertex(v[17]);
            AddVertex(v[8]);

            // bottom right corner
            AddVertex(v[2]);    // 51
            AddVertex(v[9]);
            AddVertex(v[20]);

            // top left corner
            AddVertex(v[4]);    // 54
            AddVertex(v[11]);
            AddVertex(v[18]);

            // top right corner
            AddVertex(v[5]);    // 57
            AddVertex(v[23]);
            AddVertex(v[10]);

            // Back corners

            // bottom right corner
            AddVertex(v[1]);    // 60
            AddVertex(v[21]);
            AddVertex(v[12]);

            // bottom left corner
            AddVertex(v[0]);    // 63
            AddVertex(v[13]);
            AddVertex(v[16]);

            // top right corner
            AddVertex(v[6]);    // 66
            AddVertex(v[15]);
            AddVertex(v[22]);

            // top left corner
            AddVertex(v[7]);    // 69
            AddVertex(v[19]);
            AddVertex(v[14]);

            Vector3 frontBottomNormal = new Vector3(0f, -1f, -1f);
            frontBottomNormal.Normalize();
            Vector3 frontTopNormal = new Vector3(0f, 1f, -1f);
            frontTopNormal.Normalize();

            Vector3 backBottomNormal = new Vector3(0f, -1f, 1f);
            backBottomNormal.Normalize();
            Vector3 backTopNormal = new Vector3(0f, 1f, 1f);
            backTopNormal.Normalize();

            Vector3 frontLeftNormal = new Vector3(-1f, 0f, -1f);
            frontLeftNormal.Normalize();
            Vector3 frontRightNormal = new Vector3(1f, 0f, -1f);
            frontRightNormal.Normalize();

            Vector3 backLeftNormal = new Vector3(-1f, 0f, 1f);
            backLeftNormal.Normalize();
            Vector3 backRightNormal = new Vector3(1f, 0f, 1f);
            backRightNormal.Normalize();

            Vector3 bottomLeftNormal = new Vector3(-1f, -1f, 0f);
            bottomLeftNormal.Normalize();
            Vector3 bottomRightNormal = new Vector3(1f, -1f, 0f);
            bottomRightNormal.Normalize();

            Vector3 topLeftNormal = new Vector3(-1f, 1f, 0f);
            topLeftNormal.Normalize();
            Vector3 topRightNormal = new Vector3(1f, 1f, 0f);
            topRightNormal.Normalize();

            Vector3 frontBottomLeftNormal = new Vector3(-1f, -1f, -1f);
            frontBottomLeftNormal.Normalize();
            Vector3 frontTopLeftNormal = new Vector3(-1f, 1f, -1f);
            frontTopLeftNormal.Normalize();
            Vector3 frontBottomRightNormal = new Vector3(1f, -1f, -1f);
            frontBottomRightNormal.Normalize();
            Vector3 frontTopRightNormal = new Vector3(1f, 1f, -1f);
            frontTopRightNormal.Normalize();

            Vector3 backBottomLeftNormal = new Vector3(-1f, -1f, 1f);
            backBottomLeftNormal.Normalize();
            Vector3 backTopLeftNormal = new Vector3(-1f, 1f, 1f);
            backTopLeftNormal.Normalize();
            Vector3 backBottomRightNormal = new Vector3(1f, -1f, 1f);
            backBottomRightNormal.Normalize();
            Vector3 backTopRightNormal = new Vector3(1f, 1f, 1f);
            backTopRightNormal.Normalize();

            // front bottom bevel
            AddNormal(frontBottomNormal);
            AddNormal(frontBottomNormal);
            AddNormal(frontBottomNormal);
            AddNormal(frontBottomNormal);

            // front top bevels
            AddNormal(frontTopNormal);
            AddNormal(frontTopNormal);
            AddNormal(frontTopNormal);
            AddNormal(frontTopNormal);

            // back bottom bevel
            AddNormal(backBottomNormal);
            AddNormal(backBottomNormal);
            AddNormal(backBottomNormal);
            AddNormal(backBottomNormal);

            // back top bevle
            AddNormal(backTopNormal);
            AddNormal(backTopNormal);
            AddNormal(backTopNormal);
            AddNormal(backTopNormal);

            // front left bevel
            AddNormal(frontLeftNormal);
            AddNormal(frontLeftNormal);
            AddNormal(frontLeftNormal);
            AddNormal(frontLeftNormal);

            // front right bevel
            AddNormal(frontRightNormal);
            AddNormal(frontRightNormal);
            AddNormal(frontRightNormal);
            AddNormal(frontRightNormal);

            // back right bevel
            AddNormal(backRightNormal);
            AddNormal(backRightNormal);
            AddNormal(backRightNormal);
            AddNormal(backRightNormal);

            // back left bevel
            AddNormal(backLeftNormal);
            AddNormal(backLeftNormal);
            AddNormal(backLeftNormal);
            AddNormal(backLeftNormal);

            // bottom left bevel
            AddNormal(bottomLeftNormal);
            AddNormal(bottomLeftNormal);
            AddNormal(bottomLeftNormal);
            AddNormal(bottomLeftNormal);

            // bottom right bevel
            AddNormal(topLeftNormal);
            AddNormal(topLeftNormal);
            AddNormal(topLeftNormal);
            AddNormal(topLeftNormal);

            // top left bevel
            AddNormal(bottomRightNormal);
            AddNormal(bottomRightNormal);
            AddNormal(bottomRightNormal);
            AddNormal(bottomRightNormal);

            // top right bevel
            AddNormal(topRightNormal);
            AddNormal(topRightNormal);
            AddNormal(topRightNormal);
            AddNormal(topRightNormal);

            // front corners
            AddNormal(frontBottomLeftNormal);
            AddNormal(frontBottomLeftNormal);
            AddNormal(frontBottomLeftNormal);

            AddNormal(frontBottomRightNormal);
            AddNormal(frontBottomRightNormal);
            AddNormal(frontBottomRightNormal);

            AddNormal(frontTopLeftNormal);
            AddNormal(frontTopLeftNormal);
            AddNormal(frontTopLeftNormal);

            AddNormal(frontTopRightNormal);
            AddNormal(frontTopRightNormal);
            AddNormal(frontTopRightNormal);

            // back corners
            AddNormal(backBottomRightNormal);
            AddNormal(backBottomRightNormal);
            AddNormal(backBottomRightNormal);

            AddNormal(backBottomLeftNormal);
            AddNormal(backBottomLeftNormal);
            AddNormal(backBottomLeftNormal);

            AddNormal(backTopRightNormal);
            AddNormal(backTopRightNormal);
            AddNormal(backTopRightNormal);

            AddNormal(backTopLeftNormal);
            AddNormal(backTopLeftNormal);
            AddNormal(backTopLeftNormal);

            // for bottom face
            for (int j = 0; j <= depthSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width0 * 0.5f + i * width0 / widthSeg,
                                          -height * 0.5f,
                                          depth0 * 0.5f - j * depth0 / depthSeg));
                    AddNormal(Vector3.down);
                }
            }

            // for top face
            for (int j = 0; j <= depthSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width0 * 0.5f + i * width0 / widthSeg,
                                          height * 0.5f,
                                          -depth0 * 0.5f + j * depth0 / depthSeg));
                    AddNormal(Vector3.up);
                }
            }

            // for front face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width0 * 0.5f + i * width0 / widthSeg,
                                          -height0 * 0.5f + j * height0 / heightSeg,
                                          -depth * 0.5f));
                    AddNormal(Vector3.back);
                }
            }

            // for back face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(width0 * 0.5f - i * width0 / widthSeg,
                                          -height0 * 0.5f + j * height0 / heightSeg,
                                          depth * 0.5f));
                    AddNormal(Vector3.forward);
                }
            }

            // for left side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    AddVertex(new Vector3(-width * 0.5f,
                                          -height0 * 0.5f + j * height0 / heightSeg,
                                          depth0 * 0.5f - i * depth0 / depthSeg));
                    AddNormal(Vector3.left);
                }
            }

            // for right side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    AddVertex(new Vector3(width * 0.5f,
                                          -height0 * 0.5f + j * height0 / heightSeg,
                                          -depth0 * 0.5f + i * depth0 / depthSeg));
                    AddNormal(Vector3.right);
                }
            }

            //if (properties.genTextureCoords)
            //    SetTextureCoordsBeveled();

            SetTextureCoordsBeveled();

        }
        #endregion

        #region GenerateVerticesHollowed
        private void GenerateVerticesHollowed()
        {
            float width = properties.width;
            float height = properties.height;
            float depth = properties.depth;
            float thickness = properties.hollow.thickness;

            float[] x = new float[4];
            float[] y = new float[4];

            // 4 vertices on the base rectangle
            x[0] = -width * 0.5f;
            y[0] = -depth * 0.5f;

            x[1] = width * 0.5f;
            y[1] = -depth * 0.5f;

            x[2] = width * 0.5f;
            y[2] = depth * 0.5f;

            x[3] = -width * 0.5f;
            y[3] = depth * 0.5f;


            float[] x0 = new float[4];
            float[] y0 = new float[4];

            x0[0] = x[0] + thickness;
            y0[0] = y[0] + thickness;

            x0[1] = x[1] - thickness;
            y0[1] = y[1] + thickness;

            x0[2] = x[2] - thickness;
            y0[2] = y[2] - thickness;

            x0[3] = x[3] + thickness;
            y0[3] = y[3] - thickness;

            // Basic top, bottom, side faces
            float depth0 = properties.hollow.height;

            // for top faces
            AddVertex(new Vector3(x[0], height * 0.5f, y[0]));     // front
            AddVertex(new Vector3(x[1], height * 0.5f, y[1]));
            AddVertex(new Vector3(x0[1], height * 0.5f, y0[1]));
            AddVertex(new Vector3(x0[0], height * 0.5f, y0[0]));

            AddVertex(new Vector3(x[2], height * 0.5f, y[2]));     // back
            AddVertex(new Vector3(x[3], height * 0.5f, y[3]));
            AddVertex(new Vector3(x0[3], height * 0.5f, y0[3]));
            AddVertex(new Vector3(x0[2], height * 0.5f, y0[2]));

            AddVertex(new Vector3(x[3], height * 0.5f, y[3]));     // left
            AddVertex(new Vector3(x[0], height * 0.5f, y[0]));
            AddVertex(new Vector3(x0[0], height * 0.5f, y0[0]));
            AddVertex(new Vector3(x0[3], height * 0.5f, y0[3]));

            AddVertex(new Vector3(x[1], height * 0.5f, y[1]));     // right
            AddVertex(new Vector3(x[2], height * 0.5f, y[2]));
            AddVertex(new Vector3(x0[2], height * 0.5f, y0[2]));
            AddVertex(new Vector3(x0[1], height * 0.5f, y0[1]));

            // for inner walls
            AddVertex(new Vector3(x0[0], height * 0.5f - depth0, y0[0]));     // front
            AddVertex(new Vector3(x0[1], height * 0.5f - depth0, y0[1]));
            AddVertex(new Vector3(x0[1], height * 0.5f, y0[1]));
            AddVertex(new Vector3(x0[0], height * 0.5f, y0[0]));

            AddVertex(new Vector3(x0[2], height * 0.5f - depth0, y0[2]));     // back
            AddVertex(new Vector3(x0[3], height * 0.5f - depth0, y0[3]));
            AddVertex(new Vector3(x0[3], height * 0.5f, y0[3]));
            AddVertex(new Vector3(x0[2], height * 0.5f, y0[2]));

            AddVertex(new Vector3(x0[3], height * 0.5f - depth0, y0[3]));     // left
            AddVertex(new Vector3(x0[0], height * 0.5f - depth0, y0[0]));
            AddVertex(new Vector3(x0[0], height * 0.5f, y0[0]));
            AddVertex(new Vector3(x0[3], height * 0.5f, y0[3]));

            AddVertex(new Vector3(x0[1], height * 0.5f - depth0, y0[1]));     // right
            AddVertex(new Vector3(x0[2], height * 0.5f - depth0, y0[2]));
            AddVertex(new Vector3(x0[2], height * 0.5f, y0[2]));
            AddVertex(new Vector3(x0[1], height * 0.5f, y0[1]));

            if (depth0 == height)
            {
                // for bottom faces
                AddVertex(new Vector3(x[0], -height * 0.5f, y[0]));     // front
                AddVertex(new Vector3(x[1], -height * 0.5f, y[1]));
                AddVertex(new Vector3(x0[1], -height * 0.5f, y0[1]));
                AddVertex(new Vector3(x0[0], -height * 0.5f, y0[0]));

                AddVertex(new Vector3(x[2], -height * 0.5f, y[2]));     // back
                AddVertex(new Vector3(x[3], -height * 0.5f, y[3]));
                AddVertex(new Vector3(x0[3], -height * 0.5f, y0[3]));
                AddVertex(new Vector3(x0[2], -height * 0.5f, y0[2]));

                AddVertex(new Vector3(x[3], -height * 0.5f, y[3]));     // left
                AddVertex(new Vector3(x[0], -height * 0.5f, y[0]));
                AddVertex(new Vector3(x0[0], -height * 0.5f, y0[0]));
                AddVertex(new Vector3(x0[3], -height * 0.5f, y0[3]));

                AddVertex(new Vector3(x[1], -height * 0.5f, y[1]));     // right
                AddVertex(new Vector3(x[2], -height * 0.5f, y[2]));
                AddVertex(new Vector3(x0[2], -height * 0.5f, y0[2]));
                AddVertex(new Vector3(x0[1], -height * 0.5f, y0[1]));
            }
            else
            {
                // for bottom face
                AddVertex(new Vector3(x[3], -height * 0.5f, y[3]));
                AddVertex(new Vector3(x[2], -height * 0.5f, y[2]));
                AddVertex(new Vector3(x[1], -height * 0.5f, y[1]));
                AddVertex(new Vector3(x[0], -height * 0.5f, y[0]));

                // for inner bottom
                AddVertex(new Vector3(x0[0], height * 0.5f - depth0, y0[0]));     // front
                AddVertex(new Vector3(x0[1], height * 0.5f - depth0, y0[1]));
                AddVertex(new Vector3(x0[2], height * 0.5f - depth0, y0[2]));
                AddVertex(new Vector3(x0[3], height * 0.5f - depth0, y0[3]));
            }

            Vector3 frontNormal = new Vector3(0f, 0f, -1f);
            Vector3 backNormal = new Vector3(0f, 0f, 1f);
            Vector3 topNormal = new Vector3(0f, 1f, 0f);
            Vector3 bottomNormal = new Vector3(0f, -1f, 0f);
            Vector3 leftNormal = new Vector3(-1f, 0f, 0f);
            Vector3 rightNormal = new Vector3(1f, 0f, 0f);

            // top faces
            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);

            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);

            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);

            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);
            AddNormal(topNormal);

            // inner side walls
            AddNormal(backNormal);
            AddNormal(backNormal);
            AddNormal(backNormal);
            AddNormal(backNormal);

            AddNormal(frontNormal);
            AddNormal(frontNormal);
            AddNormal(frontNormal);
            AddNormal(frontNormal);

            AddNormal(rightNormal);
            AddNormal(rightNormal);
            AddNormal(rightNormal);
            AddNormal(rightNormal);

            AddNormal(leftNormal);
            AddNormal(leftNormal);
            AddNormal(leftNormal);
            AddNormal(leftNormal);

            if (depth0 == height)
            {
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);

                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);

                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);

                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
            }
            else
            {
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);
                AddNormal(bottomNormal);

                // inner bottom face
                AddNormal(topNormal);
                AddNormal(topNormal);
                AddNormal(topNormal);
                AddNormal(topNormal);
            }

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            // for front face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(-width * 0.5f + i * width / widthSeg,
                                          -height * 0.5f + j * height / heightSeg,
                                          -depth * 0.5f));
                    AddNormal(Vector3.back);
                }
            }

            // for back face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= widthSeg; ++i)
                {
                    AddVertex(new Vector3(width * 0.5f - i * width / widthSeg,
                                          -height * 0.5f + j * height / heightSeg,
                                          depth * 0.5f));
                    AddNormal(Vector3.forward);
                }
            }

            // for left side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    AddVertex(new Vector3(-width * 0.5f,
                                          -height * 0.5f + j * height / heightSeg,
                                          depth * 0.5f - i * depth / depthSeg));
                    AddNormal(Vector3.left);
                }
            }

            // for right side face
            for (int j = 0; j <= heightSeg; ++j)
            {
                for (int i = 0; i <= depthSeg; ++i)
                {
                    AddVertex(new Vector3(width * 0.5f,
                                          -height * 0.5f + j * height / heightSeg,
                                          -depth * 0.5f + i * depth / depthSeg));
                    AddNormal(Vector3.right);
                }
            }

            if (properties.genTextureCoords)
                SetTextureCoordsHollowed();
        }
        #endregion

        #region GenerateTriangles
        private void GenerateTriangles()
        {
            faces.Clear();

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            // bottom
            int baseIndex = 0;
            for (int i = 0; i < depthSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // top
            for (int i = 0; i < depthSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // front
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // back
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // left
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);

            // right
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);
        }
        #endregion

        #region GenerateTrianglesSlanted
        private void GenerateTrianglesSlanted()
        {
            faces.Clear();

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            // bottom
            int baseIndex = 0;
            for (int i = 0; i < depthSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // top
            for (int i = 0; i < depthSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // front
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // back
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // left
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);

            // right
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);
        }
        #endregion

        #region GenerateTrianglesBeveled
        private void GenerateTrianglesBeveled()
        {
            //GenerateTriangles();

            // Bevels

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            // front bottom
            faces.Add(new TriangleIndices(3, 2, 0));
            faces.Add(new TriangleIndices(0, 2, 1));

            // front top
            faces.Add(new TriangleIndices(7, 6, 4));
            faces.Add(new TriangleIndices(4, 6, 5));

            // back bottom
            faces.Add(new TriangleIndices(11, 10, 8));
            faces.Add(new TriangleIndices(8, 10, 9));

            // back top
            faces.Add(new TriangleIndices(15, 14, 12));
            faces.Add(new TriangleIndices(12, 14, 13));


            // front left
            faces.Add(new TriangleIndices(19, 18, 16));
            faces.Add(new TriangleIndices(16, 18, 17));

            // front right
            faces.Add(new TriangleIndices(23, 22, 20));
            faces.Add(new TriangleIndices(20, 22, 21));

            // back right
            faces.Add(new TriangleIndices(27, 26, 24));
            faces.Add(new TriangleIndices(24, 26, 25));

            // back left
            faces.Add(new TriangleIndices(31, 30, 28));
            faces.Add(new TriangleIndices(28, 30, 29));


            // bottom left
            faces.Add(new TriangleIndices(35, 34, 32));
            faces.Add(new TriangleIndices(32, 34, 33));

            // top left
            faces.Add(new TriangleIndices(39, 38, 36));
            faces.Add(new TriangleIndices(36, 38, 37));

            // bottom right
            faces.Add(new TriangleIndices(43, 42, 40));
            faces.Add(new TriangleIndices(40, 42, 41));

            // top right
            faces.Add(new TriangleIndices(47, 46, 44));
            faces.Add(new TriangleIndices(44, 46, 45));


            // front corners
            faces.Add(new TriangleIndices(48, 49, 50));
            faces.Add(new TriangleIndices(51, 52, 53));
            faces.Add(new TriangleIndices(54, 55, 56));
            faces.Add(new TriangleIndices(57, 58, 59));

            // back corners
            faces.Add(new TriangleIndices(60, 61, 62));
            faces.Add(new TriangleIndices(63, 64, 65));
            faces.Add(new TriangleIndices(66, 67, 68));
            faces.Add(new TriangleIndices(69, 70, 71));

            // bottom
            int baseIndex = 72;
            for (int i = 0; i < depthSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // top
            for (int i = 0; i < depthSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // front
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // back
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // left
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);

            // right
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);
        }
        #endregion

        #region GenerateTrianglesHollowed
        private void GenerateTrianglesHollowed()
        {
            faces.Clear();

            int baseIndex = 0;

            // top faces
            faces.Add(new TriangleIndices(3, 2, 0));
            faces.Add(new TriangleIndices(0, 2, 1));
            faces.Add(new TriangleIndices(5, 4, 6));
            faces.Add(new TriangleIndices(6, 4, 7));

            faces.Add(new TriangleIndices(8, 11, 9));
            faces.Add(new TriangleIndices(9, 11, 10));
            faces.Add(new TriangleIndices(14, 13, 15));
            faces.Add(new TriangleIndices(15, 13, 12));

            // inner side walls
            faces.Add(new TriangleIndices(18, 19, 17));
            faces.Add(new TriangleIndices(17, 19, 16));
            faces.Add(new TriangleIndices(22, 23, 21));
            faces.Add(new TriangleIndices(21, 23, 20));

            faces.Add(new TriangleIndices(26, 27, 25));
            faces.Add(new TriangleIndices(25, 27, 24));
            faces.Add(new TriangleIndices(30, 31, 29));
            faces.Add(new TriangleIndices(29, 31, 28));

            if (properties.hollow.height == properties.height)
            {
                // bottom faces
                faces.Add(new TriangleIndices(34, 35, 33));
                faces.Add(new TriangleIndices(33, 35, 32));
                faces.Add(new TriangleIndices(36, 37, 39));
                faces.Add(new TriangleIndices(39, 37, 38));

                faces.Add(new TriangleIndices(43, 40, 42));
                faces.Add(new TriangleIndices(42, 40, 41));
                faces.Add(new TriangleIndices(45, 46, 44));
                faces.Add(new TriangleIndices(44, 46, 47));

                baseIndex = 48;
            }
            else
            {
                // bottom
                faces.Add(new TriangleIndices(33, 32, 34));
                faces.Add(new TriangleIndices(34, 32, 35));

                // innner bottom
                faces.Add(new TriangleIndices(39, 38, 36));
                faces.Add(new TriangleIndices(36, 38, 37));

                baseIndex = 40;
            }

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            // front
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // back
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < widthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + widthSeg + 2));
                    faces.Add(new TriangleIndices(index + widthSeg + 2, index, index + widthSeg + 1));
                }
                baseIndex += (widthSeg + 1);
            }
            baseIndex += (widthSeg + 1);

            // left
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);

            // right
            for (int i = 0; i < heightSeg; ++i)
            {
                for (int j = 0; j < depthSeg; ++j)
                {
                    int index = baseIndex + j;
                    faces.Add(new TriangleIndices(index + 1, index, index + depthSeg + 2));
                    faces.Add(new TriangleIndices(index + depthSeg + 2, index, index + depthSeg + 1));
                }
                baseIndex += (depthSeg + 1);
            }
            baseIndex += (depthSeg + 1);
        }
        #endregion

        #region SetTextureCoords
        private void SetTextureCoords()
        {
            if (!properties.genTextureCoords) return;

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            if (!properties.textureWrapped)
            {
                // for bottom face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / depthSeg));
                    }
                }

                // for top face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / depthSeg));
                    }
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2(1.0f - (float)i / widthSeg, 1.0f - (float)j / heightSeg));
                    }
                }

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }
            }
            else
            {
                float v0 = properties.depth / properties.width;
                float v1 = 1 - v0;

                // for bottom face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, v0 * (float)j / depthSeg + v1));
                    }
                }

                // for top face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, v0 * (float)j / depthSeg));
                    }
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2(1.0f - (float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2(v0 * (float)i / depthSeg + v1, (float)j / heightSeg));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2(v0 * (float)i / depthSeg, (float)j / heightSeg));
                    }
                }
            }
        }
        #endregion

        #region SetTextureCoordsBeveled
        private void SetTextureCoordsBeveled()
        {
            if (!properties.genTextureCoords) return;

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            if (!properties.textureWrapped)
            {
                // bevels                

                // front bottom          
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // front top             
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // back bottom           
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // front top             
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // front left            
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // front right           
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // back left             
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // back right            
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // bottom left bevel     
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // bottom right bevel    
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // top left bevel        
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // bottom right bevel    
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // Front corners         

                // bottom left corner    
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // bottom right corner   
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // top left corner       
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // top right corner      
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // back corners          

                // bottom left corner    
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // bottom right corner   
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // top left corner       
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // top right corner      
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));

                // for bottom face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / depthSeg));
                    }
                }

                // for top face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / depthSeg));
                    }
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2(1.0f - (float)i / widthSeg, 1.0f - (float)j / heightSeg));
                    }
                }

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }
            }
            else
            {
                float u0 = properties.beveledEdge.width / properties.width;
                float u1 = 1.0f - u0;
                float v0 = properties.beveledEdge.width / properties.height;
                float v1 = 1.0f - v0;

                //float v0 = properties.depth / properties.width;
                //float v1 = 1 - v0;

                // bevels                

                // front bottom          
                AddUV(new Vector2(u0, 0f));
                AddUV(new Vector2(u1, 0f));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(u0, v0));

                // front top             
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(u1, v1));
                AddUV(new Vector2(u1, 1f));
                AddUV(new Vector2(u0, 1f));

                // back bottom           
                AddUV(new Vector2(u0, 0f));
                AddUV(new Vector2(u1, 0f));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(u0, v0));

                // front top             
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(u1, v1));
                AddUV(new Vector2(u1, 1f));
                AddUV(new Vector2(u0, 1f));

                // front left            
                AddUV(new Vector2(0f, v0));
                AddUV(new Vector2(u0, v0));
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(0f, v1));

                // front right           
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(1f, v0));
                AddUV(new Vector2(1f, v1));
                AddUV(new Vector2(u1, v1));

                // back left             
                AddUV(new Vector2(0f, v0));
                AddUV(new Vector2(u0, v0));
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(0f, v1));

                // back right            
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(1f, v0));
                AddUV(new Vector2(1f, v1));
                AddUV(new Vector2(u1, v1));

                // bottom left bevel     
                AddUV(new Vector2(u0, 0f));
                AddUV(new Vector2(u1, 0f));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(u0, v0));

                // top left bevel        
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(u1, v1));
                AddUV(new Vector2(u1, 1f));
                AddUV(new Vector2(u0, 1f));

                // bottom left bevel     
                AddUV(new Vector2(u0, 0f));
                AddUV(new Vector2(u1, 0f));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(u0, v0));

                // top right bevel       
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(u1, v1));
                AddUV(new Vector2(u1, 1f));
                AddUV(new Vector2(u0, 1f));

                // Front corners

                // bottom left corner
                AddUV(new Vector2(u0, 0f));
                AddUV(new Vector2(0f, v0));
                AddUV(new Vector2(u0, v0));

                // bottom right corner   
                AddUV(new Vector2(u1, 0f));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(1f, v0));

                // top left corner       
                AddUV(new Vector2(u0, 1f));
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(0f, v1));

                // top right corner      
                AddUV(new Vector2(u1, 1f));
                AddUV(new Vector2(1f, v1));
                AddUV(new Vector2(u1, v1));

                // back corners          

                // bottom left corner    
                AddUV(new Vector2(u0, 0f));
                AddUV(new Vector2(0f, v0));
                AddUV(new Vector2(u0, v0));

                // bottom right corner   
                AddUV(new Vector2(u1, 0f));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(1f, v0));

                // top left corner       
                AddUV(new Vector2(u0, 1f));
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(0f, v1));

                // top right corner      
                AddUV(new Vector2(u1, 1f));
                AddUV(new Vector2(1f, v1));
                AddUV(new Vector2(u1, v1));

                // for bottom face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((u1 - u0) * (float)i / widthSeg + u0, (v1 - v0) * (float)j / depthSeg + v0));
                    }
                }

                // for top face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((u1 - u0) * (float)i / widthSeg + u0, (v1 - v0) * (float)j / depthSeg + v0));
                    }
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((u1 - u0) * (float)i / widthSeg + u0, (v1 - v0) * (float)j / depthSeg + v0));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((u1 - u0) * (float)i / widthSeg + u0, (v1 - v0) * (float)j / depthSeg + v0));
                    }
                }

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((u1 - u0) * (float)i / depthSeg + u0, (v1 - v0) * (float)j / depthSeg + v0));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((u1 - u0) * (float)i / depthSeg + u0, (v1 - v0) * (float)j / depthSeg + v0));
                    }
                }
            }
        }
        #endregion

        #region SetTextureCoordsSlanted
        private void SetTextureCoordsSlanted()
        {
            if (!properties.genTextureCoords) return;

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            if (!properties.textureWrapped)
            {
                // for bottom face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / depthSeg));
                    }
                }

                // for top face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / depthSeg));
                    }
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2(1.0f - (float)i / widthSeg, 1.0f - (float)j / heightSeg));
                    }
                }

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }
            }
            else
            {
                float slantWidth = Mathf.Sqrt(properties.slantedSides.size[0] * properties.slantedSides.size[0] + properties.depth * properties.depth);
                float slantHeight = Mathf.Sqrt(properties.slantedSides.size[1] * properties.slantedSides.size[1] + properties.depth * properties.depth);
                float totalWidth = (properties.width - properties.slantedSides.size[0] * 2f) + slantWidth * 2f;
                float totalHeight = (properties.height - properties.slantedSides.size[1] * 2f) + slantHeight * 2f;

                float u0 = slantWidth / totalWidth;
                float u1 = 1.0f - u0;

                float v0 = slantHeight / totalHeight;
                float v1 = 1.0f - v0;

                // for bottom face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        float u = ((float)j / depthSeg * (u1 - u0) + (1.0f - (float)j / depthSeg) * 1) * i / widthSeg +
                               (float)j / depthSeg * u0;
                        AddUV(new Vector2(u, v0 * (float)j / depthSeg));
                    }
                }

                // for top face
                for (int j = 0; j <= depthSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        float u = ((float)j / depthSeg * 1 + (1.0f - (float)j / depthSeg) * (u1 - u0)) * i / widthSeg +
                               (1 - (float)j / depthSeg) * u0;
                        AddUV(new Vector2(u, v1 + v0 * (float)j / depthSeg));
                    }
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg * (u1 - u0) + u0, (float)j / heightSeg * (v1 - v0) + v0));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2(1.0f - (float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        float v = ((float)i / depthSeg * (v1 - v0) + (1.0f - (float)i / depthSeg) * 1) * j / heightSeg +
                               (float)i / depthSeg * v0;
                        AddUV(new Vector2(u0 * (float)i / depthSeg, v));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        float v = ((float)i / depthSeg * 1 + (1.0f - (float)i / depthSeg) * (v1 - v0)) * j / heightSeg +
                               (1 - (float)i / depthSeg) * v0;
                        AddUV(new Vector2(u1 + u0 * (float)i / depthSeg, v));
                    }
                }
            }
        }
        #endregion

        #region SetTextureCoordsHollowed
        private void SetTextureCoordsHollowed()
        {
            if (!properties.genTextureCoords) return;

            int widthSeg = properties.widthSegments;
            int heightSeg = properties.heightSegments;
            int depthSeg = properties.depthSegments;

            if (!properties.textureWrapped)
            {
                // top faces
                AddUV(new Vector2(0f, 0f));      // front
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                AddUV(new Vector2(0f, 0f));      // back
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                AddUV(new Vector2(0f, 0f));      // left
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                AddUV(new Vector2(0f, 0f));      // right
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                // inner walls
                AddUV(new Vector2(0f, 0f));      // front
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                AddUV(new Vector2(0f, 0f));      // back
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                AddUV(new Vector2(0f, 0f));      // left
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                AddUV(new Vector2(0f, 0f));      // right
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(0f, 1f));

                if (properties.hollow.height == properties.height)
                {
                    AddUV(new Vector2(0f, 0f));      // front
                    AddUV(new Vector2(1f, 0f));
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(0f, 1f));

                    AddUV(new Vector2(0f, 0f));      // back
                    AddUV(new Vector2(1f, 0f));
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(0f, 1f));

                    AddUV(new Vector2(0f, 0f));      // left
                    AddUV(new Vector2(1f, 0f));
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(0f, 1f));

                    AddUV(new Vector2(0f, 0f));      // right
                    AddUV(new Vector2(1f, 0f));
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(0f, 1f));
                }
                else
                {
                    AddUV(new Vector2(0f, 0f));       // bottom
                    AddUV(new Vector2(1f, 0f));
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(0f, 1f));

                    // inner bottom          
                    AddUV(new Vector2(0f, 0f));
                    AddUV(new Vector2(1f, 0f));
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(0f, 1f));
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2(1.0f - (float)i / widthSeg, 1.0f - (float)j / heightSeg));
                    }
                }

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / depthSeg, (float)j / heightSeg));
                    }
                }
            }
            else
            {
                float u0 = properties.hollow.thickness / properties.width;
                float u1 = 1.0f - u0;
                float v0 = properties.hollow.thickness / properties.depth;
                float v1 = 1.0f - v0;    

                // top faces
                AddUV(new Vector2(0f, 0f));      // front
                AddUV(new Vector2(1f, 0f));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(u0, v0));

                AddUV(new Vector2(1f, 1f));      // back
                AddUV(new Vector2(0f, 1f));
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(u1, v1));

                AddUV(new Vector2(0f, 1f));      // left
                AddUV(new Vector2(0f, 0f));
                AddUV(new Vector2(u0, v0));
                AddUV(new Vector2(u0, v1));

                AddUV(new Vector2(1f, 0f));      // right
                AddUV(new Vector2(1f, 1f));
                AddUV(new Vector2(u1, v1));
                AddUV(new Vector2(u1, v0));

                // inner walls
                float v2 = properties.hollow.height / properties.depth;
                AddUV(new Vector2(u0, v0 + v2));      // front
                AddUV(new Vector2(u1, v0 + v2));
                AddUV(new Vector2(u1, v0));
                AddUV(new Vector2(u0, v0));

                AddUV(new Vector2(u1, v1 - v2));      // back
                AddUV(new Vector2(u0, v1 - v2));
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(u1, v1));

                AddUV(new Vector2(u0 + v2, v0));      // left
                AddUV(new Vector2(u0 + v2, v1));
                AddUV(new Vector2(u0, v1));
                AddUV(new Vector2(u0, v0));

                AddUV(new Vector2(u1 - v2, v0));      // right
                AddUV(new Vector2(u1 - v2, v1));
                AddUV(new Vector2(u1, v1));
                AddUV(new Vector2(u1, v0));

                if (properties.hollow.height == properties.height)
                {
                    AddUV(new Vector2(0f, 1f));      // front
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(u1, v1));
                    AddUV(new Vector2(u0, v1));

                    AddUV(new Vector2(1f, 0f));      // back
                    AddUV(new Vector2(0f, 0f));
                    AddUV(new Vector2(u0, v0));
                    AddUV(new Vector2(u1, v0));

                    AddUV(new Vector2(1f, 0f));      // left
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(u1, v1));
                    AddUV(new Vector2(u1, v0));

                    AddUV(new Vector2(0f, 0f));      // right
                    AddUV(new Vector2(0f, 1f));
                    AddUV(new Vector2(u0, v1));
                    AddUV(new Vector2(u0, v0));
                }
                else
                {
                    AddUV(new Vector2(0f, 0f));       // bottom
                    AddUV(new Vector2(1f, 0f));
                    AddUV(new Vector2(1f, 1f));
                    AddUV(new Vector2(0f, 1f));

                    // inner bottom          
                    AddUV(new Vector2(u0, v0));
                    AddUV(new Vector2(u1, v0));
                    AddUV(new Vector2(u1, v1));
                    AddUV(new Vector2(u0, v1));
                }

                // for front face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2((float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                // for back face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= widthSeg; ++i)
                    {
                        AddUV(new Vector2(1.0f - (float)i / widthSeg, (float)j / heightSeg));
                    }
                }

                float v00 = properties.depth / properties.width;
                float v01 = 1 - v00;

                // for left side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2(v00 * (float)i / depthSeg + v01, (float)j / heightSeg));
                    }
                }

                // for right side face
                for (int j = 0; j <= heightSeg; ++j)
                {
                    for (int i = 0; i <= depthSeg; ++i)
                    {
                        AddUV(new Vector2(v00 * (float)i / depthSeg, (float)j / heightSeg));
                    }
                }
            }
        }
        #endregion

    }
}

