using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Asteroids.Utils
{
    [BurstCompile]
    public static class StaticAsteroidUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPosition(in float2 initialPosition, in float2 initialVelocity, float spawnTime, float currentTime)
        {
            float deltaTime = currentTime - spawnTime;
            return new float3(initialPosition + initialVelocity * deltaTime, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion GetRotation(float initialAngle, float spawnTime, float currentTime)
        {
            float deltaTime = currentTime - spawnTime;
            float angle = math.fmod(deltaTime * 100 + initialAngle, 360.0f);
            return quaternion.RotateZ(math.radians(angle));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPosition(in StaticAsteroid asteroid, float currentTime)
        {
            return GetPosition(asteroid.InitialPosition, asteroid.InitialVelocity, asteroid.SpawnTime, currentTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion GetRotation(in StaticAsteroid asteroid, float currentTime)
        {
            return GetRotation(asteroid.InitialAngle, asteroid.SpawnTime, currentTime);
        }
    }
}