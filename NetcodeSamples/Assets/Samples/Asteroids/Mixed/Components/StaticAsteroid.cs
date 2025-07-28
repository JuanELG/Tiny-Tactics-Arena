using Unity.Entities;
using Unity.Mathematics;

public struct StaticAsteroid : IComponentData
{
    public float2 InitialPosition;
    public float2 InitialVelocity;
    public float InitialAngle;
    public float SpawnTime;
}