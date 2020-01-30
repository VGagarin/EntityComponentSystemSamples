using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
[AddComponentMenu("DOTS Samples/SpawnAndRemove/Rotation Speed")]
[ConverterVersion("joe", 1)]
public class RotationSpeedAuthoring_SpawnAndRemove : MonoBehaviour, IConvertGameObjectToEntity
{
    public float DegreesPerSecond = 360;

    // Данные MonoBehaviour преобразуются в данные компонента о сущности.
    // Мы специально преобразуем из хорошего редактора представление данных (представленных в градусах)
    // К хорошему представлению времени выполнения (представленному в радианах)
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new RotationSpeed_SpawnAndRemove { RadiansPerSecond = math.radians(DegreesPerSecond) });
        dstManager.AddComponentData(entity, new LifeTime { Value = 0.0F });
    }
}
