using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickPrimitives
{
    public class QcPrimitivesBase : MonoBehaviour
    {
        protected const float oneOver2PI = 1 / (Mathf.PI * 2);
        protected const float twoPi = Mathf.PI * 2f;

        public struct TriangleIndices
        {
            public int v1;
            public int v2;
            public int v3;

            public TriangleIndices(int v1, int v2, int v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        public List<Vector3> vertices = new List<Vector3>();
        protected List<Vector3> normals = new List<Vector3>();
        protected List<Vector2> uvs = new List<Vector2>();
        protected int numVertices = 0;

        public List<TriangleIndices> faces = new List<TriangleIndices>();

        protected int AddVertex(Vector3 point, Vector3 normal, Vector2 uv)
        {
            vertices.Add(point);
            normals.Add(normal);
            uvs.Add(uv);

            return numVertices++;
        }

        protected int AddVertex(Vector3 point)
        {
            vertices.Add(point);

            return numVertices++;
        }

        protected void AddNormal(Vector3 normal)
        {
            normals.Add(normal);
        }

        protected void AddUV(Vector2 uv)
        {
            uvs.Add(uv);
        }

        protected Vector3 GetVertex(int index)
        {
            return vertices[index];
        }

        protected virtual void ClearVertices()
        {
            if (vertices != null) vertices.Clear();
            if (normals != null) normals.Clear();
            if (uvs != null) uvs.Clear();
            numVertices = 0;

            if (faces != null) faces.Clear();
        }

        protected void AddOffset(Vector3 offsetPosition)
        {
            for (int i = 0; i < vertices.Count; ++i)
            {
                vertices[i] += offsetPosition;
            }
        }

        protected Vector3 ComputeNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 side1 = b - a;
            Vector3 side2 = c - a;
            Vector3 perp = Vector3.Cross(side1, side2);
            var perpLength = perp.magnitude;
            perp /= perpLength;

            return perp;
        }

        protected virtual void BuildGeometry() { }
        public virtual void RebuildGeometry() { }
    }
}
