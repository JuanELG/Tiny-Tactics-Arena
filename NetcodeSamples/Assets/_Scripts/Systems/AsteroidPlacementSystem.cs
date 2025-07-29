using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct AsteroidPlacementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PendingAsteroidPlacement>();
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<AsteroidsSpawner>();
        state.RequireForUpdate<GamePhaseComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var phase = SystemAPI.GetSingleton<GamePhaseComponent>();
        if (phase.Value != GamePhase.AsteroidsPositioning)
            return;

        var hasPendingShipPlacement = state.GetEntityQuery(ComponentType.ReadOnly<PendingShipPlacement>()).CalculateEntityCount() > 0;
        if (hasPendingShipPlacement)
            return;

        bool inputPressed = Input.GetMouseButtonDown(0) || Input.touchCount > 0;
        if (!inputPressed)
            return;

        var level = SystemAPI.GetSingleton<GameSettings>().levelData;

        if (!PlacementUtils.TryGetClickPosition(out float3 spawnPosition, level))
            return;

        float bottomBoundTopY = -1f;
        foreach (var (border, transform, scaleMatrix) in
                 SystemAPI.Query<RefRO<LevelBorder>, RefRO<LocalTransform>, RefRO<PostTransformMatrix>>())
        {
            if (border.ValueRO.Side == 0)
            {
                float height = math.abs(scaleMatrix.ValueRO.Value.c1.y);
                bottomBoundTopY = transform.ValueRO.Position.y + height * 0.5f;
                break;
            }
        }

        float visualSideBottomY = -1f;
        foreach (var (side, transform) in
                 SystemAPI.Query<RefRO<VisualSideComponent>, RefRO<LocalTransform>>())
        {
            if (side.ValueRO.Side == PlayerSide.Bottom)
            {
                visualSideBottomY = transform.ValueRO.Position.y;
                break;
            }
        }

        if (!PlacementUtils.IsInsideSafePlayerZone(spawnPosition, bottomBoundTopY, visualSideBottomY, level.levelWidth))
            return;

        var spawner = SystemAPI.GetSingleton<AsteroidsSpawner>();
        var prefab = spawner.StaticAsteroid;
        var scale = state.EntityManager.GetComponentData<LocalTransform>(prefab).Scale;
        var rotation = quaternion.RotateZ(math.radians(UnityEngine.Random.Range(0f, 360f)));

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var asteroid = ecb.Instantiate(prefab);

        ecb.SetComponent(asteroid, LocalTransform.FromPositionRotationScale(spawnPosition, rotation, scale));

        ecb.SetComponent(asteroid, new StaticAsteroid
        {
            InitialPosition = spawnPosition.xy,
            InitialVelocity = float2.zero,
            InitialAngle = math.degrees(math.atan2(rotation.value.z, rotation.value.w)) * 2f,
            SpawnTime = (float)SystemAPI.Time.ElapsedTime
        });
        ecb.AddComponent(asteroid, new PlayerPlacedAsteroidTag());

        foreach (var (placement, ent) in SystemAPI.Query<RefRW<PendingAsteroidPlacement>>().WithEntityAccess())
        {
            placement.ValueRW.Remaining--;
            if (placement.ValueRW.Remaining <= 0)
            {
                ecb.DestroyEntity(ent);

                var gamePhaseEntity = SystemAPI.GetSingletonEntity<GamePhaseComponent>();
                var newPhase = SystemAPI.GetComponent<GamePhaseComponent>(gamePhaseEntity);
                newPhase.Value = GamePhase.Battle;
                newPhase.Timer = 0;
                ecb.SetComponent(gamePhaseEntity, newPhase);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
