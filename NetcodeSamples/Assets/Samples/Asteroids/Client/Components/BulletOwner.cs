using Unity.Entities;

public struct BulletOwner : IComponentData
{
    public Entity Owner;
}