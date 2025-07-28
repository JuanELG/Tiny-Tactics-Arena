using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct SinglePlayerSteeringSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
        state.RequireForUpdate<AsteroidsSpawner>();
        state.RequireForUpdate<ShipInputComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var level = SystemAPI.GetSingleton<GameSettings>().levelData;
        var bulletPrefab = SystemAPI.GetSingleton<AsteroidsSpawner>().Bullet;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, velocity, stateComponent, input, entity) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRW<Velocity>, RefRW<ShipStateComponentData>, RefRO<ShipInputComponent>>()
                          .WithEntityAccess())
        {
            stateComponent.ValueRW.WeaponCooldownTimer -= deltaTime;

            HandleMovement(ref transform.ValueRW, ref velocity.ValueRW, input.ValueRO, level, deltaTime);
            HandleShooting(entity, ref transform.ValueRW, stateComponent, bulletPrefab, ref ecb, level, input.ValueRO);

            stateComponent.ValueRW.State = input.ValueRO.thrust > 0f ? 1 : 0;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private static void HandleMovement(ref LocalTransform transform, ref Velocity velocity, in ShipInputComponent input, LevelComponent level, float deltaTime)
    {
        if (input.left > 0f)
            transform.Rotation = math.mul(transform.Rotation, quaternion.RotateZ(math.radians(level.shipRotationRate * deltaTime)));

        if (input.right > 0f)
            transform.Rotation = math.mul(transform.Rotation, quaternion.RotateZ(math.radians(-level.shipRotationRate * deltaTime)));

        if (input.thrust > 0f)
        {
            float3 forward = new float3(0, level.shipForwardForce * deltaTime, 0);
            velocity.Value += math.mul(transform.Rotation, forward).xy;
        }

        transform.Position.xy += velocity.Value * deltaTime;
    }

    private static void HandleShooting(Entity shooterEntity, ref LocalTransform transform, RefRW<ShipStateComponentData> stateComponent,
        Entity bulletPrefab, ref EntityCommandBuffer ecb, LevelComponent level, in ShipInputComponent input)
    {
        if (input.shoot == 0f || stateComponent.ValueRO.WeaponCooldownTimer > 0f || bulletPrefab == Entity.Null)
            return;

        var bullet = ecb.Instantiate(bulletPrefab);
        ecb.AddComponent(bullet, new BulletOwner { Owner = shooterEntity });

        var bulletTransform = transform;
        bulletTransform.Scale = 10f;

        var velocity = new Velocity
        {
            Value = math.mul(transform.Rotation, new float3(0, level.bulletVelocity, 0)).xy
        };

        ecb.SetComponent(bullet, bulletTransform);
        ecb.SetComponent(bullet, velocity);

        stateComponent.ValueRW.WeaponCooldownTimer = stateComponent.ValueRO.WeaponCooldownDuration;
    }
}
