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
        state.RequireForUpdate<GameSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        Entity phaseEntity = SystemAPI.GetSingletonEntity<GamePhaseComponent>();
        GamePhaseComponent phase = SystemAPI.GetComponent<GamePhaseComponent>(phaseEntity);
        GameSettings settings = SystemAPI.GetSingleton<GameSettings>();

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

        bool shouldAdvance = false;

        switch (phase.Value)
        {
            case GamePhase.ShipPositioning:
                {
                    phase.Timer -= deltaTime;

                    bool placementDone = SystemAPI.QueryBuilder().WithAll<PendingShipPlacement>().Build().IsEmpty;
                    shouldAdvance = placementDone || phase.Timer <= 0f;

                    if (shouldAdvance)
                    {
                        phase.Value = GamePhase.AsteroidsPositioning;
                        phase.Timer = settings.positioningDuration;

                        var e = state.EntityManager.CreateEntity();
                        state.EntityManager.AddComponentData(e, new PendingAsteroidPlacement { Remaining = settings.asteroidPlacementCount });
                    }
                    break;
                }

            case GamePhase.AsteroidsPositioning:
                {
                    phase.Timer -= deltaTime;

                    bool placementDone = SystemAPI.QueryBuilder().WithAll<PendingAsteroidPlacement>().Build().IsEmpty;
                    shouldAdvance = placementDone || phase.Timer <= 0f;

                    if (shouldAdvance)
                    {
                        phase.Value = GamePhase.Battle;
                        phase.Timer = 0f;
                    }
                    break;
                }

            case GamePhase.Battle:
                break;
        }

        SystemAPI.SetComponent(phaseEntity, phase);
    }
}
