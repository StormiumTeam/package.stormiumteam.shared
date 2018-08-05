using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace package.stormiumteam.shared
{
    /// <summary>
    ///     Recursively paritions a mesh's vertices to allow to more quickly
    ///     narrow down the search for a nearest point on it's surface with respect to another
    ///     point
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public class BSPTreeMeshCollision : MonoBehaviour
    {
        [SerializeField] private bool m_DrawMeshTreeOnStart;

        private Mesh m_Mesh;

        private Node m_Tree;

        private int       m_TriangleCount;
        private Vector3[] m_TriangleNormals;
        private int[]     m_Tris;
        private int       m_VertexCount;
        private Vector3[] m_Vertices;

        private void Awake()
        {
            m_Mesh = GetComponent<MeshCollider>().sharedMesh;

            m_Tris     = m_Mesh.triangles;
            m_Vertices = m_Mesh.vertices;

            m_VertexCount   = m_Mesh.vertices.Length;
            m_TriangleCount = m_Mesh.triangles.Length / 3;

            m_TriangleNormals = new Vector3[m_TriangleCount];

            for (var i = 0; i < m_Tris.Length; i += 3)
            {
                var normal = Vector3.Cross((m_Vertices[m_Tris[i + 1]] - m_Vertices[m_Tris[i]]).normalized,
                    (m_Vertices[m_Tris[i + 2]] - m_Vertices[m_Tris[i]]).normalized).normalized;

                m_TriangleNormals[i / 3] = normal;
            }

            Profiler.BeginSample("Awake() --> Draw Tree");
            Debug.Log("Drawn tree!");
            if (!m_DrawMeshTreeOnStart)
                BuildTriangleTree();
            Profiler.EndSample();
        }

        private void Start()
        {
            Profiler.BeginSample("Start() --> Draw Tree");
            if (m_DrawMeshTreeOnStart)
                BuildTriangleTree();
            Profiler.EndSample();
        }

        private void OnDestroy()
        {
            m_TriangleNormals = null;
            m_Vertices        = null;

            if (m_Tree != null)
                m_Tree.Triangles = null;
        }

        /// <summary>
        ///     Returns the closest point on the mesh with respect to Vector3 point to
        /// </summary>
        public Vector3 ClosestPointOn(Vector3 to, float radius, out Vector3 normal)
        {
            to = transform.InverseTransformPoint(to);

            var triangles = new List<int>();

            FindClosestTriangles(m_Tree, to, radius, triangles);

            var closest = ClosestPointOnTriangle(triangles, to, out normal);

            return transform.TransformPoint(closest);
        }

        private void FindClosestTriangles(Node node, Vector3 to, float radius, List<int> triangles)
        {
            if (node.Triangles == null)
                if (PointDistanceFromPlane(node.PartitionPoint, node.PartitionNormal, to) <= radius)
                {
                    FindClosestTriangles(node.PositiveChild, to, radius, triangles);
                    FindClosestTriangles(node.NegativeChild, to, radius, triangles);
                }
                else if (PointAbovePlane(node.PartitionPoint, node.PartitionNormal, to))
                {
                    FindClosestTriangles(node.PositiveChild, to, radius, triangles);
                }
                else
                {
                    FindClosestTriangles(node.NegativeChild, to, radius, triangles);
                }
            else
                triangles.AddRange(node.Triangles);
        }

        private Vector3 ClosestPointOnTriangle(List<int> triangles, Vector3 to, out Vector3 normal)
        {
            var shortestDistance = float.MaxValue;

            var shortestPoint = Vector3.zero;
            normal = Vector3.zero;

            // Iterate through all triangles
            foreach (var triangle in triangles)
            {
                var p1 = m_Vertices[m_Tris[triangle]];
                var p2 = m_Vertices[m_Tris[triangle + 1]];
                var p3 = m_Vertices[m_Tris[triangle + 2]];

                Vector3 nearest;

                ClosestPointOnTriangleToPoint(ref p1, ref p2, ref p3, ref to, out nearest);

                var distance = (to - nearest).sqrMagnitude;

                if (distance <= shortestDistance)
                {
                    shortestDistance = distance;
                    shortestPoint    = nearest;

                    normal = Vector3.Cross((p2 - p1).normalized, (p3 - p1).normalized).normalized;
                }
            }

            return shortestPoint;
        }

        private void BuildTriangleTree()
        {
            var rootTriangles = new List<int>();

            for (var i = 0; i < m_Tris.Length; i += 3) rootTriangles.Add(i);

            m_Tree = new Node();

            RecursivePartition(rootTriangles, 0, m_Tree);
        }

        private void RecursivePartition(List<int> triangles, int depth, Node parent)
        {
            var partitionPoint = Vector3.zero;

            var maxExtents = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);
            var minExtents = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (var triangle in triangles)
            {
                partitionPoint += m_Vertices[m_Tris[triangle]] + m_Vertices[m_Tris[triangle + 1]] +
                                  m_Vertices[m_Tris[triangle + 2]];

                minExtents.x = Mathf.Min(minExtents.x, m_Vertices[m_Tris[triangle]].x,
                    m_Vertices[m_Tris[triangle + 1]].x,
                    m_Vertices[m_Tris[triangle + 2]].x);
                minExtents.y = Mathf.Min(minExtents.y, m_Vertices[m_Tris[triangle]].y,
                    m_Vertices[m_Tris[triangle + 1]].y,
                    m_Vertices[m_Tris[triangle + 2]].y);
                minExtents.z = Mathf.Min(minExtents.z, m_Vertices[m_Tris[triangle]].z,
                    m_Vertices[m_Tris[triangle + 1]].z,
                    m_Vertices[m_Tris[triangle + 2]].z);

                maxExtents.x = Mathf.Max(maxExtents.x, m_Vertices[m_Tris[triangle]].x,
                    m_Vertices[m_Tris[triangle + 1]].x,
                    m_Vertices[m_Tris[triangle + 2]].x);
                maxExtents.y = Mathf.Max(maxExtents.y, m_Vertices[m_Tris[triangle]].y,
                    m_Vertices[m_Tris[triangle + 1]].y,
                    m_Vertices[m_Tris[triangle + 2]].y);
                maxExtents.z = Mathf.Max(maxExtents.z, m_Vertices[m_Tris[triangle]].z,
                    m_Vertices[m_Tris[triangle + 1]].z,
                    m_Vertices[m_Tris[triangle + 2]].z);
            }

            // Centroid of all vertices
            partitionPoint /= m_VertexCount;

            // Better idea? Center of bounding box
            partitionPoint = minExtents +
                             (maxExtents - minExtents).normalized * (maxExtents - minExtents).magnitude * 0.5f;

            var extentsMagnitude = new Vector3(Mathf.Abs(maxExtents.x - minExtents.x),
                Mathf.Abs(maxExtents.y - minExtents.y), Mathf.Abs(maxExtents.z - minExtents.z));

            Vector3 partitionNormal;

            if (extentsMagnitude.x >= extentsMagnitude.y && extentsMagnitude.x >= extentsMagnitude.z)
                partitionNormal = Vector3.right;
            else if (extentsMagnitude.y >= extentsMagnitude.x && extentsMagnitude.y >= extentsMagnitude.z)
                partitionNormal = Vector3.up;
            else
                partitionNormal = Vector3.forward;

            List<int> positiveTriangles;
            List<int> negativeTriangles;

            Split(triangles, partitionPoint, partitionNormal, out positiveTriangles, out negativeTriangles);

            parent.PartitionNormal = partitionNormal;
            parent.PartitionPoint  = partitionPoint;

            var posNode = new Node();
            parent.PositiveChild = posNode;

            var negNode = new Node();
            parent.NegativeChild = negNode;

            if (positiveTriangles.Count < triangles.Count && positiveTriangles.Count > 3)
                RecursivePartition(positiveTriangles, depth + 1, posNode);
            else
                posNode.Triangles = positiveTriangles.ToArray();

            if (negativeTriangles.Count < triangles.Count && negativeTriangles.Count > 3)
                RecursivePartition(negativeTriangles, depth + 1, negNode);
            else
                negNode.Triangles = negativeTriangles.ToArray();
        }

        /// <summary>
        ///     Splits a a set of input triangles by a partition plane into positive and negative sets, with triangles
        ///     that are intersected by the partition plane being placed in both sets
        /// </summary>
        private void Split(List<int>     triangles,         Vector3       partitionPoint, Vector3 partitionNormal,
                           out List<int> positiveTriangles, out List<int> negativeTriangles)
        {
            positiveTriangles = new List<int>();
            negativeTriangles = new List<int>();

            foreach (var triangle in triangles)
            {
                var firstPointAbove = PointAbovePlane(partitionPoint, partitionNormal, m_Vertices[m_Tris[triangle]]);
                var secondPointAbove =
                    PointAbovePlane(partitionPoint, partitionNormal, m_Vertices[m_Tris[triangle + 1]]);
                var thirdPointAbove =
                    PointAbovePlane(partitionPoint, partitionNormal, m_Vertices[m_Tris[triangle + 2]]);

                if (firstPointAbove && secondPointAbove && thirdPointAbove)
                {
                    positiveTriangles.Add(triangle);
                }
                else if (!firstPointAbove && !secondPointAbove && !thirdPointAbove)
                {
                    negativeTriangles.Add(triangle);
                }
                else
                {
                    positiveTriangles.Add(triangle);
                    negativeTriangles.Add(triangle);
                }
            }
        }

        private static bool PointAbovePlane(Vector3 planeOrigin, Vector3 planeNormal, Vector3 point)
        {
            return Vector3.Dot(point - planeOrigin, planeNormal) >= 0;
        }

        private static float PointDistanceFromPlane(Vector3 planeOrigin, Vector3 planeNormal, Vector3 point)
        {
            return Mathf.Abs(Vector3.Dot(point - planeOrigin, planeNormal));
        }

        /// <summary>
        ///     Determines the closest point between a point and a triangle.
        ///     Borrowed from RPGMesh class of the RPGController package for Unity, by fholm
        ///     The code in this method is copyrighted by the SlimDX Group under the MIT license:
        ///     Copyright (c) 2007-2010 SlimDX Group
        ///     Permission is hereby granted, free of charge, to any person obtaining a copy
        ///     of this software and associated documentation files (the "Software"), to deal
        ///     in the Software without restriction, including without limitation the rights
        ///     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        ///     copies of the Software, and to permit persons to whom the Software is
        ///     furnished to do so, subject to the following conditions:
        ///     The above copyright notice and this permission notice shall be included in
        ///     all copies or substantial portions of the Software.
        ///     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        ///     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        ///     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        ///     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        ///     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        ///     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        ///     THE SOFTWARE.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="vertex1">The first vertex to test.</param>
        /// <param name="vertex2">The second vertex to test.</param>
        /// <param name="vertex3">The third vertex to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointOnTriangleToPoint(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3,
                                                         ref Vector3 point,   out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 136

            //Check if P in vertex region outside A
            var ab = vertex2 - vertex1;
            var ac = vertex3 - vertex1;
            var ap = point - vertex1;

            var d1 = Vector3.Dot(ab, ap);
            var d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                result = vertex1; //Barycentric coordinates (1,0,0)
                return;
            }

            //Check if P in vertex region outside B
            var bp = point - vertex2;
            var d3 = Vector3.Dot(ab, bp);
            var d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                result = vertex2; // barycentric coordinates (0,1,0)
                return;
            }

            //Check if P in edge region of AB, if so return projection of P onto AB
            var vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                var v = d1 / (d1 - d3);
                result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
                return;
            }

            //Check if P in vertex region outside C
            var cp = point - vertex3;
            var d5 = Vector3.Dot(ab, cp);
            var d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                result = vertex3; //Barycentric coordinates (0,0,1)
                return;
            }

            //Check if P in edge region of AC, if so return projection of P onto AC
            var vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                var w = d2 / (d2 - d6);
                result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
                return;
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            var va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && d4 - d3 >= 0.0f && d5 - d6 >= 0.0f)
            {
                var w = (d4 - d3) / (d4 - d3 + (d5 - d6));
                result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
                return;
            }

            //P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            var denom = 1.0f / (va + vb + vc);
            var v2    = vb * denom;
            var w2    = vc * denom;
            result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }

        public class Node
        {
            public Node    NegativeChild;
            public Vector3 PartitionNormal;
            public Vector3 PartitionPoint;

            public Node PositiveChild;

            public int[] Triangles;
        }
    }
}