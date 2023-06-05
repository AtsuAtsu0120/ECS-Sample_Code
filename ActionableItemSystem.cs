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
            //�A�C�e��������������悤��
            CollidesWith = 1u << 3
        };
    }
    public void OnDestroy(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        //OnCreate�ł�World����������Ă��Ȃ��̂ŁA���񂾂������ł��B
        if(player == Entity.Null)
        {
            //�I�u�W�F�N�g���擾
            player = state.GetEntityQuery(typeof(PlayerTag)).GetSingletonEntity();
        }
        //�����ƍ��W�����擾���邽�߂�LocalToWorld���擾
        localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(player);

        var physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var center = localToWorld.Position;

        //���C���o���čŊ��̃I�u�W�F�N�g���擾
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