using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

// Системы компонентов заданий могут работать в рабочих потоках.
// Однако создание и удаление сущностей может выполняться только в главном потоке для предотвращения условий гонки.
// Система использует буфер команд сущности для отложения задач, которые не могут быть выполнены внутри задания.

// ReSharper disable once InconsistentNaming
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SpawnerSystem_SpawnAndRemove : JobComponentSystem {
    
    // BeginInitializationEntityCommandBuffer System используется для создания командного буфера, 
    // который затем будет воспроизводиться
    // когда эта система барьеров выполняется.
    //
    // Хотя команда создания экземпляра записана в задании Spawn, она фактически не обрабатывается (или " воспроизводится")
    // до тех пор, пока не будет обновлена соответствующая буферная система команд сущности. 
    // Чтобы убедиться, что система преобразования имеет шанс
    // для запуска на вновь порожденных сущностях до того, как они будут отрисованы в первый раз, объект Spawner System_From
    // будет использовать команду начать моделирование лица буферной системы, чтобы воспроизвести его команды. 
    // Это приводит к задержке в один кадр
    // между записью команд и созданием экземпляров сущностей, но на практике это обычно не заметно.
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        // Кэшируйте буферную систему команд Begin Initialization Entity в поле, поэтому нам не нужно создавать ее каждый кадр
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        // Вместо непосредственного выполнения структурных изменений задание может добавить команду в буфер команд сущности, чтобы
        // выполнить такие изменения в главном потоке после завершения задания. Буферы команд позволяют выполнять
        // любые потенциально дорогостоящие вычисления в рабочем потоке при постановке в очередь фактических вставок и
        // удаление на потом.
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        // Запланируйте задание, которое добавит команды экземпляра в буфер команд сущности.
        // Поскольку это задание выполняется только на первом кадре, мы хотим убедиться, что пакет компилирует его перед запуском, 
        // чтобы получить лучшую производительность (3-й параметр WithBurst)
        // Фактическое задание будет кэшировано после компиляции (оно будет Компилировано только один раз).

        var jobHandle = Entities
            .WithName("SpawnerSystem_SpawnAndRemove")
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex, in Spawner_SpawnAndRemove spawner, in LocalToWorld location) =>
        {
            var random = new Random(1);

            for (var x = 0; x < spawner.CountX; x++)
            {
                for (var y = 0; y < spawner.CountY; y++)
                {
                    var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);

                    // Place the instantiated in a grid with some noise
                    var position = math.transform(location.Value, new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));
                    commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation { Value = position });
                    commandBuffer.SetComponent(entityInQueryIndex, instance, new LifeTime { Value = random.NextFloat(10.0F, 100.0F) });
                    commandBuffer.SetComponent(entityInQueryIndex, instance, new RotationSpeed_SpawnAndRemove { RadiansPerSecond = math.radians(random.NextFloat(25.0F, 90.0F)) });
                }
            }

            commandBuffer.DestroyEntity(entityInQueryIndex, entity);
        }).Schedule(inputDependencies);

        // Задание Spawn выполняется параллельно без точки синхронизации до тех пор, пока не будет выполнена система барьеров.
        // Когда система барьера выполняется, мы хотим завершить задание спавна, а затем воспроизвести команды
        // (Создание сущностей и их размещение). Мы должны сказать системе барьеров, какую работу она должна выполнить.
        // завершить, прежде чем он сможет воспроизвести команды.
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
