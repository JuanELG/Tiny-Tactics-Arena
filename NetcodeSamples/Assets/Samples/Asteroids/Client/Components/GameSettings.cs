using Unity.Entities;

public struct GameSettings : IComponentData
{
    public LevelComponent levelData;
    public float positioningDuration;
}
