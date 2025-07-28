using Unity.Entities;

public struct BulletAgeComponent : IComponentData
{
    public float age;
    public float maxAge;

    public BulletAgeComponent(float maxAge)
    {
        this.maxAge = maxAge;
        age = 0;
    }
}
