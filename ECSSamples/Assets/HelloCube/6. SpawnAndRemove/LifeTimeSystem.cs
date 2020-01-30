using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct LifeTime : IComponentData
{
    public float Value;
}

// Эта система обновляет все объекты в сцене с помощью компонента Rotation Speed_Spawn и компонента Remove and Rotation.
public class LifeTimeSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // Используйте атрибут [Burst Compile] для компиляции задания с помощью Burst.
    // Вы можете увидеть значительное ускорение, так что попробуйте!
    [BurstCompile]
    struct LifeTimeJob : IJobForEachWithEntity<LifeTime>
    {
        public float DeltaTime;

        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int jobIndex, ref LifeTime lifeTime)
        {
            lifeTime.Value -= DeltaTime;

            if (lifeTime.Value < 0.0f)
            {
                CommandBuffer.DestroyEntity(jobIndex, entity);
            }
        }
    }

    // OnUpdate runs on the main thread.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

        var job = new LifeTimeJob
        {
            DeltaTime = Time.DeltaTime,
            CommandBuffer = commandBuffer,

        }.Schedule(this, inputDependencies);

        m_Barrier.AddJobHandleForProducer(job);

        return job;
    }
}
