using AtsuAtsu.Components.Player;
using AtsuAtsu.Components.SpawnButton;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct RaycastSystem : ISystem
{
    RaycastInput input;
    PhysicsWorldSingleton physics;

    public void OnCreate(ref SystemState state)
    {
        var filter = new CollisionFilter()
        {
            GroupIndex = 0,
            BelongsTo = 1 << 1,
            //レイヤー6（Player自身）だけ反応しないようにする。
            CollidesWith =  ~(~ 1u << 5 & 1u << 6)
        };
        Debug.Log($"{filter.CollidesWith}");
        input = new RaycastInput
        {
            Start = new float3(0, 0, 0),
            Filter = filter,
            End = new float3(0, 0, 0)
        };
    }
    public void OnDestroy(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        //Raycastを飛ばすオブジェクトを取得
        var player = state.GetEntityQuery(typeof(PlayerTag)).GetSingletonEntity();

        //向きと座標情報を取得するためにLocalToWorldを取得
        var localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(player);

        input.Start = localToWorld.Position;

        //終点を正面になるように計算
        var distance = 5;
        var goal = localToWorld.Forward * distance;
        input.End = goal + localToWorld.Position;

        //レイを出して最寄りのオブジェクトを取得
        if (physics.CastRay(input, out var hit))
        {
            var name = state.EntityManager.GetName(hit.Entity);
            Debug.Log(name);
            state.EntityManager.AddComponent<ChangeOutlineTag>(hit.Entity);
        }
    }
}
