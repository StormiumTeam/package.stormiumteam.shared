using System;
using Unity.Entities;

namespace package.stormiumteam.shared.ecs
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class UpdateInGroupMultipleAttribute : UpdateInGroupAttribute
	{
		public UpdateInGroupMultipleAttribute(Type groupType) : base(groupType)
		{
		}
	}
}