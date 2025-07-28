using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

[BurstCompile]
public partial struct ActivateNearbyAsteroidsSystem : ISystem
{
    private EntityQuery asteroidQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ShipTagComponentData>();
        state.RequireForUpdate<ClientSettings>();

        asteroidQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<AsteroidTagComponentData>(),
            ComponentType.ReadOnly<LocalTransform>()
        );
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var settings = SystemAPI.GetSingleton<ClientSettings>();
        float baseRadius = settings.predictionRadius;
        float margin = settings.predictionRadiusMargin;

        float upperBoundSq = math.pow(baseRadius + margin, 2);
        float lowerBoundSq = math.pow(baseRadius - margin, 2);

        float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(
            SystemAPI.GetSingletonEntity<ShipTagComponentData>()
        ).Position;

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var entities = asteroidQuery.ToEntityArray(Allocator.Temp);
        var transforms = asteroidQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            float distSq = math.distancesq(transforms[i].Position, playerPosition);
            bool isNear = distSq < lowerBoundSq;
            bool isFar = distSq > upperBoundSq;

            bool hasTag = state.EntityManager.HasComponent<DynamicAsteroidTag>(entity);
            bool hasColor = state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(entity);

            if (isNear && !hasTag)
            {
                ecb.AddComponent<DynamicAsteroidTag>(entity);
                if (!hasColor)
                    ecb.AddComponent(entity, new URPMaterialPropertyBaseColor { Value = new float4(0, 1, 0, 1) });
            }
            else if (isFar && hasTag)
            {
                ecb.RemoveComponent<DynamicAsteroidTag>(entity);
                if (hasColor)
                    ecb.RemoveComponent<URPMaterialPropertyBaseColor>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        entities.Dispose();
        transforms.Dispose();
    }
}
