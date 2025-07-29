using Asteroids.Client;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(BulletAgeSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct SinglePlayerCollisionSystem : ISystem
{
    private EntityQuery asteroidQuery;
    private EntityQuery bulletQuery;
    private EntityQuery shipQuery;
    private EntityQuery settingsQuery;
    private EntityQuery killBoxQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        asteroidQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<AsteroidTagComponentData, DynamicAsteroidTag, LocalTransform, CollisionSphereComponent>()
        .Build(ref state);
        bulletQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<BulletTagComponent, LocalTransform, CollisionSphereComponent, BulletAgeComponent>()
        .Build(ref state);
        shipQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<ShipTagComponentData, LocalTransform, CollisionSphereComponent>()
        .Build(ref state);
        settingsQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<GameSettings>()
        .Build(ref state);
        killBoxQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<VisualSideComponent, LocalTransform, CollisionBoxComponent>()
            .Build(ref state);

        state.RequireForUpdate(settingsQuery);
        state.RequireForUpdate<AsteroidScore>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var asteroidChunks = asteroidQuery.ToArchetypeChunkListAsync(state.WorldUpdateAllocator, out var asteroidHandle);
        var bulletChunks = bulletQuery.ToArchetypeChunkListAsync(state.WorldUpdateAllocator, out var bulletHandle);
        var shipChunks = shipQuery.ToArchetypeChunkListAsync(state.WorldUpdateAllocator, out var shipHandle);
        var zoneChunks = killBoxQuery.ToArchetypeChunkListAsync(state.WorldUpdateAllocator, out var zoneHandle);
        var killBoxes = killBoxQuery.ToComponentDataArray<CollisionBoxComponent>(Allocator.Temp);
        var killBoxTransforms = killBoxQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        var level = SystemAPI.GetSingleton<GameSettings>().levelData;
        var asteroidScoreEntity = SystemAPI.GetSingletonEntity<AsteroidScore>();
        var currentScore = SystemAPI.GetSingleton<AsteroidScore>().Value;

        var sphereHandle = state.GetComponentTypeHandle<CollisionSphereComponent>(true);
        var transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
        var bulletOwnerHandle = state.GetComponentTypeHandle<BulletOwner>(true);
        var entityHandle = state.GetEntityTypeHandle();

        var handles = new NativeArray<JobHandle>(5, Allocator.Temp);
        handles[0] = state.Dependency;
        handles[1] = asteroidHandle;
        handles[2] = bulletHandle;
        handles[3] = shipHandle;
        handles[4] = zoneHandle;
        state.Dependency = JobHandle.CombineDependencies(handles);
        handles.Dispose();

        int threadCount = JobsUtility.ThreadIndexCount;
        var scoreCounter = new NativeArray<int>(threadCount, Allocator.TempJob);

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var commandBuffer = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        var parallelCommandBuffer = commandBuffer.AsParallelWriter();

        var asteroidJob = new DestroyAsteroidsJob
        {
            bulletChunks = bulletChunks,
            asteroidDestructCounter = scoreCounter,
            commandBuffer = parallelCommandBuffer,
            level = level,
            sphereHandle = sphereHandle,
            transformHandle = transformHandle,
            entityHandle = entityHandle
        };
        var asteroidJobHandle = asteroidJob.ScheduleParallel(asteroidQuery, state.Dependency);
        var velocityLookup = state.GetComponentLookup<Velocity>(true);
        var shipJob = new DestroyShipsJob
        {
            bulletChunks = bulletChunks,
            asteroidChunks = asteroidChunks,
            commandBuffer = parallelCommandBuffer,
            level = level,
            sphereHandle = sphereHandle,
            transformHandle = transformHandle,
            entityHandle = entityHandle,
            ownerHandle = bulletOwnerHandle,
            velocityLookup = velocityLookup,
            zoneChunks = zoneChunks,
            boxHandle = state.GetComponentTypeHandle<CollisionBoxComponent>(true)
        };
        var shipJobHandle = shipJob.ScheduleParallel(shipQuery, asteroidJobHandle);

        var finalizeScoreJob = new UpdateScoreJob
        {
            counter = scoreCounter,
            commandBuffer = commandBuffer,
            currentScore = currentScore,
            scoreEntity = asteroidScoreEntity
        };
        var finalizeHandle = finalizeScoreJob.Schedule(shipJobHandle);
        var disposeHandle = scoreCounter.Dispose(finalizeHandle);

        state.Dependency = disposeHandle;
    }

    [BurstCompile]
    private struct DestroyAsteroidsJob : IJobChunk
    {
        [ReadOnly] public NativeList<ArchetypeChunk> bulletChunks;
        [NativeDisableParallelForRestriction] public NativeArray<int> asteroidDestructCounter;
        public EntityCommandBuffer.ParallelWriter commandBuffer;
        [ReadOnly] public LevelComponent level;

        [ReadOnly] public ComponentTypeHandle<CollisionSphereComponent> sphereHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> transformHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;

        [NativeSetThreadIndex] public int ThreadIndex;

        public void Execute(in ArchetypeChunk chunk, int chunkIndex, bool _, in v128 __)
        {
            var asteroidSpheres = chunk.GetNativeArray(ref sphereHandle);
            var asteroidTransforms = chunk.GetNativeArray(ref transformHandle);
            var asteroidEntities = chunk.GetNativeArray(entityHandle);

            for (int i = 0; i < asteroidEntities.Length; i++)
            {
                var asteroidPosition = asteroidTransforms[i].Position.xy;
                var asteroidRadius = asteroidSpheres[i].radius;

                if (!IsInsideBounds(asteroidPosition, asteroidRadius, level))
                {
                    commandBuffer.DestroyEntity(chunkIndex, asteroidEntities[i]);
                    continue;
                }

                foreach (var bulletChunk in bulletChunks)
                {
                    var bulletSpheres = bulletChunk.GetNativeArray(ref sphereHandle);
                    var bulletTransforms = bulletChunk.GetNativeArray(ref transformHandle);
                    var bulletEntities = bulletChunk.GetNativeArray(entityHandle);

                    for (int j = 0; j < bulletEntities.Length; j++)
                    {
                        var bulletPosition = bulletTransforms[j].Position.xy;
                        var bulletRadius = bulletSpheres[j].radius;

                        if (math.distancesq(asteroidPosition, bulletPosition) < math.pow(asteroidRadius + bulletRadius, 2f))
                        {
                            commandBuffer.DestroyEntity(chunkIndex, asteroidEntities[i]);
                            commandBuffer.DestroyEntity(chunkIndex, bulletEntities[j]);
                            asteroidDestructCounter[ThreadIndex]++;
                            break;
                        }
                    }
                }
            }
        }
    }

    [BurstCompile]
    private struct DestroyShipsJob : IJobChunk
    {
        [ReadOnly] public NativeList<ArchetypeChunk> asteroidChunks;
        [ReadOnly] public NativeList<ArchetypeChunk> bulletChunks;
        public EntityCommandBuffer.ParallelWriter commandBuffer;
        [ReadOnly] public LevelComponent level;

        [ReadOnly] public ComponentTypeHandle<CollisionSphereComponent> sphereHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> transformHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        [ReadOnly] public ComponentTypeHandle<BulletOwner> ownerHandle;
        [ReadOnly] public ComponentLookup<Velocity> velocityLookup;

        [ReadOnly] public NativeList<ArchetypeChunk> zoneChunks;
        [ReadOnly] public ComponentTypeHandle<CollisionBoxComponent> boxHandle;

        public void Execute(in ArchetypeChunk chunk, int chunkIndex, bool _, in v128 __)
        {
            var shipTransforms = chunk.GetNativeArray(ref transformHandle);
            var shipSpheres = chunk.GetNativeArray(ref sphereHandle);
            var shipEntities = chunk.GetNativeArray(entityHandle);

            for (int i = 0; i < shipEntities.Length; i++)
            {
                var shipPosition = shipTransforms[i].Position.xy;
                var shipRadius = shipSpheres[i].radius;

                if (!IsInsideBounds(shipPosition, shipRadius, level))
                {
                    var shipVelocity = GetVelocity(velocityLookup, shipEntities[i]);
                    var position = shipPosition;
                    bool bounced = false;

                    if (position.x - shipRadius < 0f)
                    {
                        position.x = shipRadius;
                        shipVelocity.x = math.abs(shipVelocity.x);
                        bounced = true;
                    }
                    else if (position.x + shipRadius > level.levelWidth)
                    {
                        position.x = level.levelWidth - shipRadius;
                        shipVelocity.x = -math.abs(shipVelocity.x);
                        bounced = true;
                    }

                    if (position.y - shipRadius < 0f)
                    {
                        position.y = shipRadius;
                        shipVelocity.y = math.abs(shipVelocity.y);
                        bounced = true;
                    }
                    else if (position.y + shipRadius > level.levelHeight)
                    {
                        position.y = level.levelHeight - shipRadius;
                        shipVelocity.y = -math.abs(shipVelocity.y);
                        bounced = true;
                    }

                    if (bounced)
                    {
                        commandBuffer.SetComponent(chunkIndex, shipEntities[i], new Velocity { Value = shipVelocity });

                        var newTransform = shipTransforms[i];
                        newTransform.Position.xy = position;
                        commandBuffer.SetComponent(chunkIndex, shipEntities[i], newTransform);
                    }
                }


                if (CheckCollisionWithZones(shipPosition, shipRadius))
                {
                    commandBuffer.DestroyEntity(chunkIndex, shipEntities[i]);
                    var entity = commandBuffer.CreateEntity(chunkIndex);
                    commandBuffer.AddComponent(chunkIndex, entity, new PlayerDestroyedTag());
                    continue;
                }

                if (CheckCollision(bulletChunks, shipPosition, shipRadius, shipEntities[i]) ||
                    CheckCollision(asteroidChunks, shipPosition, shipRadius, shipEntities[i]))
                {
                    commandBuffer.DestroyEntity(chunkIndex, shipEntities[i]);
                    var entity = commandBuffer.CreateEntity(chunkIndex);
                    commandBuffer.AddComponent(chunkIndex, entity, new PlayerDestroyedTag());
                }
            }
        }

        private bool CheckCollision(NativeList<ArchetypeChunk> chunks, float2 position, float radius, Entity self)
        {
            foreach (var chunk in chunks)
            {
                var transforms = chunk.GetNativeArray(ref transformHandle);
                var spheres = chunk.GetNativeArray(ref sphereHandle);
                var owners = chunk.Has(ref ownerHandle) ? chunk.GetNativeArray(ref ownerHandle) : default;

                for (int i = 0; i < transforms.Length; i++)
                {
                    float2 otherPosition = transforms[i].Position.xy;
                    float otherRadius = spheres[i].radius;

                    if (owners.IsCreated && owners.Length > i && owners[i].Owner == self)
                        continue;

                    if (math.distancesq(position, otherPosition) < math.pow(radius + otherRadius, 2f))
                        return true;
                }
            }
            return false;
        }

        private bool CheckCollisionWithZones(float2 position, float radius)
        {
            foreach (var chunk in zoneChunks)
            {
                var boxes = chunk.GetNativeArray(ref boxHandle);
                var transforms = chunk.GetNativeArray(ref transformHandle);

                for (int i = 0; i < boxes.Length; i++)
                {
                    var center = transforms[i].Position.xy;
                    var halfExtents = boxes[i].halfExtents;

                    float2 delta = math.abs(position - center);
                    if (delta.x < halfExtents.x + radius && delta.y < halfExtents.y + radius)
                        return true;
                }
            }

            return false;
        }

        private float2 GetVelocity(ComponentLookup<Velocity> lookup, Entity entity)
        {
            return lookup.HasComponent(entity) ? lookup[entity].Value : float2.zero;
        }
    }

    [BurstCompile]
    private struct UpdateScoreJob : IJob
    {
        [ReadOnly] public NativeArray<int> counter;
        public EntityCommandBuffer commandBuffer;
        public int currentScore;
        public Entity scoreEntity;

        public void Execute()
        {
            int total = currentScore;
            for (int i = 0; i < counter.Length; i++)
                total += counter[i];

            commandBuffer.SetComponent(scoreEntity, new AsteroidScore { Value = total });
        }
    }

    private static bool IsInsideBounds(float2 position, float radius, in LevelComponent bounds)
    {
        return position.x - radius >= 0 && position.y - radius >= 0 &&
               position.x + radius <= bounds.levelWidth && position.y + radius <= bounds.levelHeight;
    }
}
