using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(GamePhaseSystem))]
public partial struct ShipPlacementInputSystem : ISystem
{
    private EntityQuery pendingPlacementQuery;

    public void OnCreate(ref SystemState state)
    {
        pendingPlacementQuery = state.GetEntityQuery(ComponentType.ReadOnly<PendingShipPlacement>());
        state.RequireForUpdate<GamePhaseComponent>();
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<AsteroidsSpawner>();
    }

    
    public void OnUpdate(ref SystemState state)
    {
        var phase = SystemAPI.GetSingleton<GamePhaseComponent>();
        if (phase.Value != GamePhase.ShipPositioning || pendingPlacementQuery.IsEmpty)
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
        var shipPrefab = spawner.Ship;
        var shipScale = state.EntityManager.GetComponentData<LocalTransform>(shipPrefab).Scale;
        var rotation = quaternion.RotateZ(math.radians(90f));
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        var ship = ecb.Instantiate(shipPrefab);
        ecb.SetComponent(ship, LocalTransform.FromPositionRotationScale(spawnPosition, rotation, shipScale));

        foreach (var entity in pendingPlacementQuery.ToEntityArray(Unity.Collections.Allocator.Temp))
            ecb.DestroyEntity(entity);

        var placementEntity = ecb.CreateEntity();
        ecb.AddComponent(placementEntity, new PendingAsteroidPlacement { Remaining = 3 });

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
