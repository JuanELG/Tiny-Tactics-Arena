using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.NetCode.Samples.Common;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(GamePhaseSystem))]
[BurstCompile]
public partial struct ShipPlacementInputSystem : ISystem
{
    private EntityQuery pendingPlacementQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        pendingPlacementQuery = state.GetEntityQuery(ComponentType.ReadOnly<PendingShipPlacement>());
        state.RequireForUpdate<GamePhaseComponent>();
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<AsteroidsSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var phase = SystemAPI.GetSingleton<GamePhaseComponent>();
        if (phase.Value != GamePhase.Positioning || pendingPlacementQuery.IsEmpty)
            return;

        bool inputPressed = Input.GetMouseButtonDown(0) || TouchInput.GetKey(TouchInput.KeyCode.Space);

        if (!inputPressed)
            return;

        var level = SystemAPI.GetSingleton<GameSettings>().levelData;
        if (!TryGetClickPosition(out float3 spawnPosition, level))
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

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private bool TryGetClickPosition(out float3 position, in LevelComponent level)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!Input.GetMouseButtonDown(0))
        {
            position = default;
            return false;
        }
        Vector3 mousePosition = Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IOS
    if (Input.touchCount == 0)
    {
        position = default;
        return false;
    }
    Vector3 mousePos = Input.GetTouch(0).position;
#else
    position = default;
    return false;
#endif

        var world = Camera.main.ScreenToWorldPoint(mousePosition);
        float3 worldPosition = new float3(world.x, world.y, 0f);

        bool isInside = worldPosition.x >= 0 && worldPosition.x <= level.levelWidth &&
                        worldPosition.y >= 0 && worldPosition.y <= level.levelHeight;

        if (!isInside)
        {
            position = default;
            return false;
        }

        position = worldPosition;
        return true;
    }
}
