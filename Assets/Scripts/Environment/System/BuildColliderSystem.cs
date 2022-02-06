using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Environment.System
{
    [BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
    public struct BuildColliderSystem : IJobParallelFor
    {
        public NativeArray<int> meshIds;

        public void Execute(int index) => Physics.BakeMesh(meshIds[index], false);
    }
}