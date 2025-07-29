using Unity.Entities;

public struct GamePhaseComponent : IComponentData
{
    public GamePhase Value;
    public float Timer;
}

public enum GamePhase : byte
{
    Positioning = 0,
    Battle = 1
}
