using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ConvertAsteroidsToDynamicSystem : ISystem
{
    private EntityQuery asteroidQuery;
    private bool converted;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePhaseComponent>();

        asteroidQuery = SystemAPI.QueryBuilder()
        .WithAll<AsteroidTagComponentData, LocalTransform>()
        .WithNone<DynamicAsteroidTag, PlayerPlacedAsteroidTag>()
        .Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var phase = SystemAPI.GetSingleton<GamePhaseComponent>();

        if (phase.Value == GamePhase.Battle && !converted)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var entities = asteroidQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                ecb.AddComponent<DynamicAsteroidTag>(entity);
                ecb.AddComponent(entity, new URPMaterialPropertyBaseColor { Value = new float4(0, 1, 0, 1) });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            entities.Dispose();

            converted = true;
        }
    }
}