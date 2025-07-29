using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct GamePhaseBootstrapSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<GamePhaseComponent>())
        {
            var settings = SystemAPI.GetSingleton<GameSettings>();

            Entity entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new GamePhaseComponent
            {
                Value = GamePhase.ShipPositioning,
                Timer = settings.positioningDuration
            });
        }
    }
}