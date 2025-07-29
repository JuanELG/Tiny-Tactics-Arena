using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct GamePhaseSystem : ISystem
{
    private bool hasInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePhaseComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var phaseEntity = SystemAPI.GetSingletonEntity<GamePhaseComponent>();
        var phase = SystemAPI.GetComponent<GamePhaseComponent>(phaseEntity);
        var settings = SystemAPI.GetSingleton<GameSettings>();

        if (!hasInitialized)
        {
            phase.Value = GamePhase.ShipPositioning;
            phase.Timer = settings.positioningDuration;

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<PendingShipPlacement>(entity);

            hasInitialized = true;
            SystemAPI.SetComponent(phaseEntity, phase);
            return;
        }

        switch (phase.Value)
        {
            case GamePhase.ShipPositioning:
                if (!SystemAPI.QueryBuilder().WithAll<PendingShipPlacement>().Build().IsEmpty)
                    break;

                phase.Value = GamePhase.AsteroidsPositioning;
                phase.Timer = settings.positioningDuration;

                var e = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(e, new PendingAsteroidPlacement { Remaining = 3 });

                SystemAPI.SetComponent(phaseEntity, phase);
                break;

            case GamePhase.AsteroidsPositioning:
                if (!SystemAPI.QueryBuilder().WithAll<PendingAsteroidPlacement>().Build().IsEmpty)
                    break;

                phase.Value = GamePhase.Battle;
                phase.Timer = 0f;
                SystemAPI.SetComponent(phaseEntity, phase);
                break;
        }
    }
}
