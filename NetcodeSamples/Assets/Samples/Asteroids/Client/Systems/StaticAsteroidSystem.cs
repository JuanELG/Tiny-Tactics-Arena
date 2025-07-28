using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

namespace Asteroids.Client
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    [BurstCompile]
    public partial struct StaticAsteroidSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StaticAsteroid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float elapsedTime = (float)SystemAPI.Time.ElapsedTime;
            float deltaTime = SystemAPI.Time.DeltaTime;

            var asteroidJob = new StaticAsteroidJob
            {
                elapsedTime = elapsedTime,
                deltaTime = deltaTime
            };

            state.Dependency = asteroidJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        partial struct StaticAsteroidJob : IJobEntity
        {
            public float elapsedTime;
            public float deltaTime;

            public void Execute(ref LocalTransform transform, in StaticAsteroid staticAsteroid)
            {
                transform.Position = Utils.StaticAsteroidUtils.GetPosition(staticAsteroid, deltaTime);
                transform.Rotation = Utils.StaticAsteroidUtils.GetRotation(staticAsteroid, deltaTime);
            }
        }
    }
}
