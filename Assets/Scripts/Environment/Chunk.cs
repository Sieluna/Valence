using System;
using System.Collections;
using Environment.Data;
using Environment.System;
using Unity.Mathematics;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        private int3 chunkPosition;
        private int3 chunkSize;

        private bool initialized;
        private bool dirty;
        private bool argent;
        private Block[] blocks;
        private Coroutine meshUpdater;

        // Mesh
        private Mesh mesh;
        private Mesh colmesh;
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        public event Func<bool> OnChunkUpdate;
        
        private NativeMapData blockData;
        private NativeLightData lightData;
        private NativeMeshData meshData;

        public bool Dirty => dirty;
        public bool Updating => meshUpdater != null;
        public bool Initialized => initialized;
        public Block[] Blocks => blocks;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            mesh = new Mesh();
            colmesh = new Mesh();
            OnChunkUpdate = () => true;
        }

        private void OnDestroy()
        {
            blockData?.jobHandle.Complete(); blockData?.Dispose();
            meshData?.jobHandle.Complete(); meshData?.Dispose();
            lightData?.jobHandle.Complete(); lightData?.Dispose();
        }

        private void Start()
        {
            meshFilter.mesh = mesh;
        }

        public void InitChunk(int3 position, Material[] materials, int3 size)
        {
            chunkPosition = position;
            meshRenderer.materials = materials;
            chunkSize = size;

            StartCoroutine(InitUpdater());
        }

        private IEnumerator InitUpdater()
        {
            blocks = new Block[chunkSize.x * chunkSize.y * chunkSize.z]; // 实例化一堆Air         
            blockData = new NativeMapData(chunkSize);
            yield return blockData.Generate(blocks, chunkPosition, chunkSize); // 等待noise生成
            dirty = true;
            initialized = true;
        }

        private void Update()
        {
            if (initialized && !Updating && dirty && OnChunkUpdate != null && OnChunkUpdate())
                meshUpdater = StartCoroutine(UpdateMesh());
        }

        private IEnumerator UpdateMesh()
        {
            if (Updating || !World.Instance.CanUpdate) yield break;

            World.Instance.UpdatingChunks++;

            var neighborBlocks = World.Instance.GetNeighborBlocks(chunkPosition, 1);
            
            lightData?.Dispose();
            lightData = new NativeLightData(chunkSize);
            yield return lightData.ScheduleLightingJob(neighborBlocks, chunkPosition, chunkSize, 1, argent);

            meshData?.Dispose();
            meshData = new NativeMeshData(chunkSize);
            yield return meshData.ScheduleMeshingJob(blocks, lightData, chunkSize, argent);

            meshData.GetMeshInformation(out int verticesSize);

            if (verticesSize > 0)
            {
                mesh.Clear();
                colmesh.Clear();
                mesh.subMeshCount = 3;
                mesh.SetVertices(meshData.nativeVertices, 0, verticesSize);
                mesh.SetNormals(meshData.nativeNormals, 0, verticesSize);
                mesh.SetColors(meshData.nativeColors, 0, verticesSize);
                mesh.SetUVs(0, meshData.nativeUVs, 0, verticesSize);
                mesh.SetIndices(meshData.nativeIndices.AsArray(), MeshTopology.Triangles, 0);
                colmesh.SetVertices(meshData.nativeVertices, 0, verticesSize);
                colmesh.SetIndices(meshData.nativeIndices.AsArray(), MeshTopology.Triangles, 0);
                mesh.SetIndices(meshData.nativeSubIndices.AsArray(), MeshTopology.Triangles, 1);
                mesh.SetIndices(meshData.nativeMorIndices.AsArray(), MeshTopology.Triangles, 2);

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                if (argent)
                    SetSharedMesh(colmesh);
                else
                    BuildColliderSystem.Instance.Enqueue(this, colmesh);
            }

            lightData.Dispose();
            meshData.Dispose();
            dirty = false;
            argent = false;
            gameObject.layer = LayerMask.NameToLayer("Block");
            meshUpdater = null;

            World.Instance.UpdatingChunks--;
        }

        public void SetSharedMesh(Mesh bakedMesh) => meshCollider.sharedMesh = bakedMesh;

        public bool GetBlock(int3 gridPosition, out Block block)
        {
            if (!initialized) { block = Block.Empty; return false; }
            if (!gridPosition.BoundaryCheck(chunkSize)) { block = Block.Empty; return false; }
            
            block = blocks[gridPosition.To1DIndex(chunkSize)];
            return true;
        }

        public bool SetBlock(int3 gridPosition, BlockType type)
        {
            if (!initialized) return false;
            if (!gridPosition.BoundaryCheck(chunkSize)) return false;

            if (type == BlockType.Air && GetBlock(gridPosition + new int3(0, 1, 0), out Block block) && block.type == BlockType.Grass)
                blocks[(gridPosition + new int3(0, 1, 0)).To1DIndex(chunkSize)].type = BlockType.Air;
            
            blocks[gridPosition.To1DIndex(chunkSize)].type = type;
            dirty = argent = true;
            return true;
        }

        public void NeighborChunkIsChanged() => dirty = argent = true;
    }
}