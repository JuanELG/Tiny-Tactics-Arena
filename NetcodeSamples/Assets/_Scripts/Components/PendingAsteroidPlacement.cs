using Unity.Entities;

public struct PendingAsteroidPlacement : IComponentData
{
    public int Remaining;
}