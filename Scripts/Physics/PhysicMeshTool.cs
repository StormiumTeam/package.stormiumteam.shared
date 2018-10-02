using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace package.stormiumteam.shared
{
    public static class PhysicMeshTool
    {
        private static FastDictionary<int, NativeArray<int>> s_TrianglesIds = new FastDictionary<int, NativeArray<int>>();
        private static FastDictionary<int, NativeArray<Vector3>> s_VerticesIds = new FastDictionary<int, NativeArray<Vector3>>();
        private static FastDictionary<int, int> s_VertexCount = new FastDictionary<int, int>();

        static PhysicMeshTool()
        {
            foreach (var na in s_TrianglesIds.Values)
            {
                if (na.IsCreated) na.Dispose();
            }
            
            foreach (var na in s_VerticesIds.Values)
            {
                if (na.IsCreated) na.Dispose();
            }
            
            s_TrianglesIds.Clear();
            s_VerticesIds.Clear();
            s_VertexCount.Clear();
        }
        
        public static NativeArray<int> GetTriangles(Mesh mesh)
        {
            var meshId = InitDb(mesh);
            
            return s_TrianglesIds[meshId];
        }
        
        public static NativeArray<Vector3> GetVertices(Mesh mesh)
        {
            var meshId = InitDb(mesh);
            
            return s_VerticesIds[meshId];
        }

        public static void Clean(Mesh mesh)
        {
            var meshId = mesh.GetInstanceID();
            if (ExistDb(meshId)) DestroyDb(meshId);
        }

        private static int InitDb(Mesh mesh)
        {
            var meshId = mesh.GetInstanceID();
            var exist  = ExistDb(meshId);

            if (exist && CheckCollisionDb(mesh, meshId))
            {
                DestroyDb(meshId);
                exist = false;
            }

            if (!exist)
            {
                CreateDb(mesh, meshId);
            }

            return meshId;
        }
        
        private static void CreateDb(Mesh mesh, int id)
        {
            s_TrianglesIds[id] = new NativeArray<int>(mesh.triangles, Allocator.Persistent);
            s_VerticesIds[id] = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
            s_VertexCount[id]  = mesh.vertexCount;
        }

        private static void DestroyDb(int id)
        {
            s_TrianglesIds[id].Dispose();
            s_VerticesIds[id].Dispose();

            s_TrianglesIds.Remove(id);
            s_VertexCount.Remove(id);
        }

        private static bool CheckCollisionDb(Mesh mesh, int id)
        {
            return s_VertexCount[id] != mesh.vertexCount;
        }

        private static bool ExistDb(int id)
        {
            return s_TrianglesIds.ContainsKey(id);
        }
    }
}