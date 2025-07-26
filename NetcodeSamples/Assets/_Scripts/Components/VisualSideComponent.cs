using Unity.Entities;

public struct VisualSideComponent : IComponentData
{
    public PlayerSide Side;
}

public enum PlayerSide : byte
{
    Top,
    Bottom
}