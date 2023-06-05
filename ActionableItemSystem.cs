using Unity.Entities;
using Unity.Burst;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics.Systems;

using AtsuAtsu.Components.Player;
using AtsuAtsu.Components.SpawnButton;
using AtsuAtsu.Components.CustomRender;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct ActionableItemSystem : ISystem
{
    CollisionFilter filter;
    LocalToWorld localToWorld;

    Entity player;
    public void OnCreate(ref SystemState state)
    {
        filter = new CollisionFilter()
        {
            GroupIndex = 0,
            BelongsTo = 1 << 1,
            //アイテムだけ反応するように
            CollidesWith = 1u << 3
        };
    }
    public void OnDestroy(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        //OnCreateではWorldが生成されていないので、初回だけここでやる。
        if(player == Entity.Null)
        {
            //オブジェクトを取得
            player = state.GetEntityQuery(typeof(PlayerTag)).GetSingletonEntity();
        }
        //向きと座標情報を取得するためにLocalToWorldを取得
        localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(player);

        var physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var center = localToWorld.Position;

        //レイを出して最寄りのオブジェクトを取得
        if (physics.BoxCast(center, quaternion.identity, new float3(0.5f, 2.5f, 0.5f), localToWorld.Forward, 5.0f, out var hit, filter))
        {
            state.EntityManager.AddComponent<ChangeOutlineTag>(hit.Entity);
        }
        Outline(state);
    }
    public void Outline(SystemState state)
    {
        foreach (var outline in SystemAPI.Query<RefRW<OutlineShader>>().WithNone<ChangeOutlineTag>())
        {
            outline.ValueRW.outlineThickness = 0.0f;
        }
        var ecb = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
        foreach (var (_, outline, entity) in SystemAPI.Query<ChangeOutlineTag, RefRW<OutlineShader>>().WithEntityAccess())
        {
            outline.ValueRW.outlineThickness = 0.03f;

            ecb.RemoveComponent<ChangeOutlineTag>(entity);
        }
    }
}
