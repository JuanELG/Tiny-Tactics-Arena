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

        float3 spawnPos = GetClickPosition();

        if (float.IsNaN(spawnPos.x))
            return;

        var level = SystemAPI.GetSingleton<GameSettings>().levelData;
        var spawner = SystemAPI.GetSingleton<AsteroidsSpawner>();
        var shipPrefab = spawner.Ship;
        var shipScale = state.EntityManager.GetComponentData<LocalTransform>(shipPrefab).Scale;

        var rotation = quaternion.RotateZ(math.radians(90f));
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        var ship = ecb.Instantiate(shipPrefab);
        ecb.SetComponent(ship, LocalTransform.FromPositionRotationScale(spawnPos, rotation, shipScale));

        foreach (var entity in pendingPlacementQuery.ToEntityArray(Unity.Collections.Allocator.Temp))
            ecb.DestroyEntity(entity);

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private float3 GetClickPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!Input.GetMouseButtonDown(0))
            return float3.zero;

        Vector3 mousePos = Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 0)
            return float3.zero;

        Vector3 mousePos = Input.GetTouch(0).position;
#else
        return float3.zero;
#endif
        var world = Camera.main.ScreenToWorldPoint(mousePos);
        return new float3(world.x, world.y, 0f);
    }
}
