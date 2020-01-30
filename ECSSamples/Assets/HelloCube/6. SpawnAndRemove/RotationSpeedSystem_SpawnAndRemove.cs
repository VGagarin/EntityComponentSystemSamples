using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// Эта система обновляет все объекты в сцене с помощью компонента Rotation Speed_Spawn и компонента Remove and Rotation.
public class RotationSpeedSystem_SpawnAndRemove : JobComponentSystem
{
    // OnUpdate runs on the main thread.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var deltaTime = Time.DeltaTime;

        // Ключевое слово in компонента Rotation Speed_Spawn and Remove сообщает планировщику заданий, 
        // что это задание не будет записываться в rotSpeedSpawnAndRemove
        return Entities
            .WithName("RotationSpeedSystem_SpawnAndRemove")
            .ForEach((ref Rotation rotation, in RotationSpeed_SpawnAndRemove rotSpeedSpawnAndRemove) =>
        {
            // Поверните что-нибудь около его вектора вверх со скоростью, заданной вращением Speed_Spawn, и удалите.
            rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotSpeedSpawnAndRemove.RadiansPerSecond * deltaTime));
        }).Schedule(inputDependencies);
    }
}
