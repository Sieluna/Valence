using System;
using System.Collections;
using Environment.Data;
using Unity.Mathematics;
using UnityEngine;

namespace Environment
{
    public class Chunk : MonoBehaviour
    {
        private int3 m_chunkPosition;
        private int3 m_chunkSize;

        private bool m_initialized;
        private bool m_dirty;
        private bool m_argent;
        private Block[] m_blocks;
        private Coroutine m_meshUpdater;

        // Mesh
        private Mesh m_mesh;
        private Mesh m_colMesh;
        
        private MeshFilter m_meshFilter;
        private MeshRenderer m_meshRenderer;
        private MeshCollider m_meshCollider;

        public event Func<bool> OnChunkUpdate;
        
        private NativeMapData m_blockData;
        private NativeMeshData m_meshData;

        public bool Dirty => m_dirty;
        public bool Updating => m_meshUpdater != null;
        public bool Initialized => m_initialized;
        public Block[] Blocks => m_blocks;

        private void Awake()
        {
            m_meshFilter = gameObject.AddComponent<MeshFilter>();
            m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
            m_meshCollider = gameObject.AddComponent<MeshCollider>();
            m_mesh = new Mesh();
            m_colMesh = new Mesh();
            OnChunkUpdate = () => true;
        }

        private void OnDestroy()
        {
            m_blockData?.jobHandle.Complete(); m_blockData?.Dispose();
            m_meshData?.jobHandle.Complete(); m_meshData?.Dispose();
        }

        private void Start()
        {
            m_meshFilter.mesh = m_mesh;
        }

        public void InitChunk(int3 position, Material[] materials, int3 size)
        {
            m_chunkPosition = position;
            m_meshRenderer.materials = materials;
            m_chunkSize = size;

            StartCoroutine(InitUpdater());
        }

        private IEnumerator InitUpdater()
        {
            m_blocks = new Block[m_chunkSize.x * m_chunkSize.y * m_chunkSize.z];
            m_blockData = new NativeMapData(m_chunkSize);
            yield return m_blockData.Generate(m_blocks, m_chunkPosition, m_chunkSize);
            m_dirty = true;
            m_initialized = true;
        }

        private void Update()
        {
            if (m_initialized && !Updating && m_dirty && OnChunkUpdate != null && OnChunkUpdate())
                m_meshUpdater = StartCoroutine(UpdateMesh());
        }

        private IEnumerator UpdateMesh()
        {
            if (Updating || !World.Instance.CanUpdate) yield break;

            World.Instance.UpdatingChunks++;
            
            m_meshData?.Dispose();
            m_meshData = new NativeMeshData(m_chunkSize);
            yield return m_meshData.Generate(m_blocks, m_chunkSize, m_argent);

            m_meshData.GetMeshInformation(out int verticesSize);

            if (verticesSize > 0)
            {
                m_mesh.Clear();
                m_colMesh.Clear();
                m_mesh.subMeshCount = 4;
                m_mesh.SetVertices(m_meshData.nativeVertices, 0, verticesSize);
                m_mesh.SetNormals(m_meshData.nativeNormals, 0, verticesSize);
                m_mesh.SetColors(m_meshData.nativeColors, 0, verticesSize);
                m_mesh.SetUVs(0, m_meshData.nativeUVs, 0, verticesSize);
                m_mesh.SetIndices(m_meshData.nativeBlockIndices.AsArray(), MeshTopology.Triangles, 0);
                m_mesh.SetIndices(m_meshData.nativeLiquidIndices.AsArray(), MeshTopology.Triangles, 1);
                m_mesh.SetIndices(m_meshData.nativeFoliageIndices.AsArray(), MeshTopology.Triangles, 2);
                m_mesh.SetIndices(m_meshData.nativeTransparentIndices.AsArray(), MeshTopology.Triangles, 3);
                m_colMesh.subMeshCount = 2;
                m_colMesh.SetVertices(m_meshData.nativeVertices, 0, verticesSize);
                m_colMesh.SetIndices(m_meshData.nativeBlockIndices.AsArray(), MeshTopology.Triangles, 0);
                m_colMesh.SetIndices(m_meshData.nativeTransparentIndices.AsArray(), MeshTopology.Triangles, 1);
                
                m_mesh.RecalculateNormals();
                m_mesh.RecalculateBounds();

                if (m_argent)
                    SetSharedMesh(m_colMesh);
                else
                    NativeColliderData.Instance.Enqueue(this, m_colMesh);
            }

            m_meshData.Dispose();
            m_dirty = false;
            m_argent = false;
            gameObject.layer = LayerMask.NameToLayer("Block");
            m_meshUpdater = null;

            World.Instance.UpdatingChunks--;
        }

        public void SetSharedMesh(Mesh bakedMesh) => m_meshCollider.sharedMesh = bakedMesh;

        public bool GetBlock(int3 gridPosition, out Block block)
        {
            if (!m_initialized) { block = Block.Empty; return false; }
            if (!gridPosition.BoundaryCheck(m_chunkSize)) { block = Block.Empty; return false; }
            
            block = m_blocks[gridPosition.To1DIndex(m_chunkSize)];
            return true;
        }

        public bool SetBlock(int3 gridPosition, BlockType type)
        {
            if (!m_initialized) return false;
            if (!gridPosition.BoundaryCheck(m_chunkSize)) return false;

            if (type == BlockType.Air && GetBlock(gridPosition + new int3(0, 1, 0), out Block block) && block.type == BlockType.Grass)
                m_blocks[(gridPosition + new int3(0, 1, 0)).To1DIndex(m_chunkSize)].type = BlockType.Air;
            
            m_blocks[gridPosition.To1DIndex(m_chunkSize)].type = type;
            m_dirty = m_argent = true;
            return true;
        }

        public void NeighborChunkIsChanged() => m_dirty = m_argent = true;
    }
}