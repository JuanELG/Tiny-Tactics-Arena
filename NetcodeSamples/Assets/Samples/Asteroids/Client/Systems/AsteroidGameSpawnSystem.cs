using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode.Samples.Common;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct SinglePlayerSpawnSystem : ISystem
{
    private Entity shipPrefab;
    private Entity asteroidDynamicPrefab;
    private Entity asteroidStaticPrefab;
    private float asteroidRadius;
    private float shipRadius;
    private bool initialSpawnDone;

    private NativeReference<Random> random;
    private EntityQuery shipQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<AsteroidsSpawner>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

        uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
        random = new NativeReference<Random>(Random.CreateFromIndex(seed), Allocator.Persistent);

        shipQuery = state.GetEntityQuery(ComponentType.ReadOnly<ShipTagComponentData>());
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        if (random.IsCreated)
            random.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //TODO: code improvement to separate responsabilities and better performance
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var spawner = SystemAPI.GetSingleton<AsteroidsSpawner>();
        var level = SystemAPI.GetSingleton<GameSettings>().levelData;
        var entityManager = state.EntityManager;

        shipPrefab = spawner.Ship;
        asteroidDynamicPrefab = spawner.Asteroid;
        asteroidStaticPrefab = spawner.StaticAsteroid;

        if (!initialSpawnDone)
        {
            shipRadius = entityManager.GetComponentData<CollisionSphereComponent>(shipPrefab).radius;
            asteroidRadius = entityManager.GetComponentData<CollisionSphereComponent>(
                asteroidDynamicPrefab != Entity.Null ? asteroidDynamicPrefab : asteroidStaticPrefab).radius;
        }

        if (shipQuery.IsEmpty && (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space) || TouchInput.GetKey(TouchInput.KeyCode.Space)))
        {
            var shipScale = entityManager.GetComponentData<LocalTransform>(shipPrefab).Scale;
            float shipPadding = shipRadius + 3f;

            var shipRandomValue = random.Value;

            float3 spawnPosition = new float3(
                shipRandomValue.NextFloat(shipPadding, level.levelWidth - shipPadding),
                shipRandomValue.NextFloat(shipPadding, level.levelHeight - shipPadding),
                0f);

            random.Value = shipRandomValue;

            var shipRotation = quaternion.RotateZ(math.radians(90f));
            var shipEntity = ecb.Instantiate(shipPrefab);
            ecb.SetComponent(shipEntity, LocalTransform.FromPositionRotationScale(spawnPosition, shipRotation, shipScale));
        }

        if (initialSpawnDone)
            return;

        var randomValue = random.Value;

        var asteroidScale = entityManager.GetComponentData<LocalTransform>(
            asteroidDynamicPrefab != Entity.Null ? asteroidDynamicPrefab : asteroidStaticPrefab).Scale;

        float padding = asteroidRadius + 3f;
        float minDistanceFromShip = shipRadius + asteroidRadius + 100f;
        float minDistanceSqr = minDistanceFromShip * minDistanceFromShip;
        var asteroidPrefab = asteroidDynamicPrefab != Entity.Null ? asteroidDynamicPrefab : asteroidStaticPrefab;
        bool useDynamicAsteroids = asteroidDynamicPrefab != Entity.Null;

        var shipPosition = new float3(level.levelWidth / 2f, level.levelHeight / 2f, 0f);
        var shipRotInit = quaternion.RotateZ(math.radians(90f));
        var shipScaleInit = entityManager.GetComponentData<LocalTransform>(shipPrefab).Scale;

        for (int i = 0; i < level.numAsteroids; i++)
        {
            bool found = false;
            float3 asteroidPosition = default;
            float third = level.levelHeight / 3f;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                asteroidPosition = new float3(
                    randomValue.NextFloat(padding, level.levelWidth - padding),
                    randomValue.NextFloat(third + padding, (2f * third) - padding),
                    0f);

                if (math.distancesq(asteroidPosition, shipPosition) > minDistanceSqr)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                continue;

            float angle = randomValue.NextFloat(0f, 360f);
            float2 velocity = math.mul(quaternion.RotateZ(math.radians(angle)), new float3(0, level.asteroidVelocity, 0)).xy;
            var asteroidRotation = quaternion.RotateZ(math.radians(angle));
            var asteroidEntity = ecb.Instantiate(asteroidPrefab);
            ecb.SetComponent(asteroidEntity, LocalTransform.FromPositionRotationScale(asteroidPosition, asteroidRotation, asteroidScale));

            if (useDynamicAsteroids)
            {
                ecb.SetComponent(asteroidEntity, new Velocity { Value = velocity });
            }
            else
            {
                ecb.SetComponent(asteroidEntity, new StaticAsteroid
                {
                    InitialPosition = asteroidPosition.xy,
                    InitialVelocity = velocity,
                    InitialAngle = angle,
                    SpawnTime = (float)SystemAPI.Time.ElapsedTime
                });
            }
        }

        random.Value = randomValue;
        initialSpawnDone = true;
    }
}