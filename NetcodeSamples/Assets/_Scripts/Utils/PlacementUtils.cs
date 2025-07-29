using Unity.Mathematics;

public static class PlacementUtils
{
    public static bool TryGetClickPosition(out float3 position, in LevelComponent level)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!UnityEngine.Input.GetMouseButtonDown(0))
        {
            position = default;
            return false;
        }
        UnityEngine.Vector3 mousePosition = UnityEngine.Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IOS
        if (UnityEngine.Input.touchCount == 0)
        {
            position = default;
            return false;
        }
        UnityEngine.Vector3 mousePosition = UnityEngine.Input.GetTouch(0).position;
#else
        position = default;
        return false;
#endif

        UnityEngine.Vector3 world = UnityEngine.Camera.main.ScreenToWorldPoint(mousePosition);
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

    public static bool IsInsideSafePlayerZone(
        float3 position,
        float bottomBoundTopY,
        float visualSideBottomY,
        float levelWidth,
        float safePadding = 10f)
    {
        float2 pos = position.xy;

        return pos.x >= safePadding &&
               pos.x <= (levelWidth - safePadding) &&
               pos.y >= (bottomBoundTopY + safePadding) &&
               pos.y <= (visualSideBottomY - safePadding);
    }
}
