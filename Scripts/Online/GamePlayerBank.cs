using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using package.stormiumteam.shared;
using Unity.Entities;

namespace package.stormiumteam.shared.online
{
    public class GamePlayerBank : ComponentSystem
    {
        public FastDictionary<Guid, GamePlayer> MasterPlayers;

        public List<GamePlayer> AllPlayers;
        public List<GamePlayer> LocalPlayers;

        public GamePlayer MainPlayer;

        protected override void OnCreateManager(int capacity)
        {
            MasterPlayers = new FastDictionary<Guid, GamePlayer>();
            AllPlayers    = new List<GamePlayer>();
            LocalPlayers  = new List<GamePlayer>();
        }

        protected override void OnUpdate()
        {

        }

        public void SetPlayer(int index, GamePlayer player)
        {
            //playerId.Id = index;


        }

        public GamePlayer GetPlayerFromIdent(SeqId id)
        {
            return MasterPlayers.FastGetOrDefault(id.ToGuid());
        }

        public void AddPlayer(MasterServerPlayerId masterId, GamePlayer player)
        {
            MasterPlayers[masterId.Id.ToGuid()] = player;
            if (!AllPlayers.Contains(player)) AllPlayers.Add(player);
            
            player.WorldPointer.SetOrAddComponentData(new PlayerEntityTag());
        }

        public void AddLocalPlayer(GamePlayer player)
        {
            if (!AllPlayers.Contains(player)) AllPlayers.Add(player);
            LocalPlayers.Add(player);

            player.WorldPointer.SetOrAddComponentData(new PlayerEntityTag());
        }

        public Entity GetEntity(GamePlayer player)
        {
            return player.WorldPointer;
        }
    }
}