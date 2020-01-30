using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
[AddComponentMenu("DOTS Samples/SpawnAndRemove/Spawner")]
[ConverterVersion("joe", 1)]
public class SpawnerAuthoring_SpawnAndRemove : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject Prefab;
    public int CountX;
    public int CountY;

    // Упомянутые префабы должны быть объявлены так, чтобы система преобразования знала о них заранее
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }

    // Позволяет преобразовать представление данных редактора в представление оптимальной среды выполнения объекта

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new Spawner_SpawnAndRemove
        {
            // Префаб со ссылками будет преобразован из-за объявления префабов со ссылками.
            // Поэтому здесь мы просто сопоставляем игровой объект с сущностью, относящейся к этой сборке.
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            CountX = CountX,
            CountY = CountY,
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}

