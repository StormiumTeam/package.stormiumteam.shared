using Unity.Entities;

namespace package.stormiumteam.shared.online
{
    public struct GamePlayer
    {
        public bool IsCreated;
        public Entity WorldPointer;

        public GamePlayer(Entity entity)
        {
            IsCreated = true;
            WorldPointer = entity;
        }
    }

    public struct MasterServerPlayerId : ISharedComponentData
    {
        /// <summary>
        /// The ID of the player
        /// </summary>
        /// <remarks>
        /// No matter what, the client and the server should possess the same id for the same player
        /// </remarks>
        public SeqId Id;

        public bool IsLocal
        {
            get
            {
                var plBank = World.Active.GetOrCreateSystem<GamePlayerBank>();
                var playerFromIdent = plBank.GetPlayerFromIdent(Id);
                if (!playerFromIdent.IsCreated) return false;

                var localPlayers = plBank.LocalPlayers;
                foreach (var localPlayer in localPlayers)
                {
                    if (localPlayer.WorldPointer == playerFromIdent.WorldPointer)
                        return true;
                }

                return false;
            }
        }
    }
}