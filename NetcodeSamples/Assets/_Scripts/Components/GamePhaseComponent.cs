using Unity.Entities;

public struct GamePhaseComponent : IComponentData
{
    public GamePhase Value;
    public float Timer;
}

public enum GamePhase : byte
{
    ShipPositioning = 0,
    AsteroidsPositioning = 1,
    Battle = 2
}
