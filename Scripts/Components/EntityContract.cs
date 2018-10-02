using System;
using Unity.Entities;

namespace package.stormiumteam.shared
{
    public enum CmdState
    {
        Simple = 0,
        Begin = 1,
        End = 2
    }
    
    public struct EntityCommand : IComponentData
    {
        public int HeaderTypeIndex;

        public Type GetHeaderType() => TypeManager.GetType(HeaderTypeIndex);
    }

    public struct EntityCommandResult : IComponentData
    {
        public byte IsAuthorized;

        public bool AsBool()
        {
            return IsAuthorized == 1;
        }
    }

    public struct EntityCommandSource : IComponentData
    {
        public Entity Value;

        public EntityCommandSource(Entity value)
        {
            Value = value;
        }
    }

    public struct EntityCommandTarget : IComponentData
    {
        public Entity Value;

        public EntityCommandTarget(Entity value)
        {
            Value = value;
        }
    }
}