using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    private class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            PlayerComponent component = new PlayerComponent
            {
            };
            AddComponent(entity, component);
        }
    }
}

public struct PlayerComponent : IComponentData
{

}