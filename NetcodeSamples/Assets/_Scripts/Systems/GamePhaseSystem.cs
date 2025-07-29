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
            phase.Value = GamePhase.Positioning;
            phase.Timer = settings.positioningDuration;

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<PendingShipPlacement>(entity);

            hasInitialized = true;
            SystemAPI.SetComponent(phaseEntity, phase);
            return;
        }

        if (phase.Value == GamePhase.Positioning)
        {
            phase.Timer -= SystemAPI.Time.DeltaTime;

            if (phase.Timer <= 0f)
            {
                phase.Value = GamePhase.Battle;
                phase.Timer = 0f;
            }

            SystemAPI.SetComponent(phaseEntity, phase);
        }
    }
}