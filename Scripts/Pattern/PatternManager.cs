using System.Collections.Generic;
using package.stormiumteam.shared;
using Unity.Entities;

namespace StormiumTeam.Shared
{
    /// <summary>
    ///     A system that give utilities for getting patterns of other network instances (or local instances).
    /// </summary>
    public class PatternManager : ComponentSystem
	{
		private Dictionary<long, PatternBankExchange> m_Exchanges;

		private Dictionary<int, PatternBank> m_ForeignBanks;
		public  PatternBank                  LocalBank { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			m_ForeignBanks = new Dictionary<int, PatternBank>();
			m_Exchanges    = new Dictionary<long, PatternBankExchange>();

			LocalBank         = new PatternBank(0);
			m_ForeignBanks[0] = LocalBank;
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();


			m_ForeignBanks.Clear();

			foreach (var exchange in m_Exchanges.Values)
				exchange.Dispose();
			m_Exchanges.Clear();

			m_ForeignBanks = null;
			m_Exchanges    = null;
		}

        /// <summary>
        ///     Get the pattern bank of a network instance (if it exist).
        /// </summary>
        /// <param name="instanceId">The instance id</param>
        /// <returns>The pattern bank of the instance</returns>
        public PatternBank GetBank(int instanceId)
		{
			return m_ForeignBanks[instanceId];
		}

		public PatternBankExchange GetLocalExchange(int destinationInstanceId)
		{
			return GetExchange(0, destinationInstanceId);
		}

		public PatternBankExchange GetExchange(int originInstanceId, int destinationInstanceId)
		{
			return m_Exchanges[new LongIntUnion {Int0Value = originInstanceId, Int1Value = destinationInstanceId}.LongValue];
		}
	}
}