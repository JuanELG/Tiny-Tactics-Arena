using Unity.Entities;
using Unity.Mathematics;

public struct CollisionBoxComponent : IComponentData
{
    public float2 halfExtents;
}
