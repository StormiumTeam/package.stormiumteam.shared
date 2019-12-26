using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared.modding
{
	public class CModInfo
	{
		private Assembly[] m_Assemblies;

		public CModInfo(SModInfoData data, int id)
		{
			Data = data;
			Id   = id;

			if (NameId.Contains('/')
			    || NameId.Contains('\\')
			    || NameId.Contains('?')
			    || NameId.Contains(':')
			    || NameId.Contains('|')
			    || NameId.Contains('*')
			    || NameId.Contains('<')
			    || NameId.Contains('>'))
				throw new Exception($"Name id {NameId} got invalid characters");

			// Create the path to the project...
			if (!Directory.Exists(StreamingPath))
				Directory.CreateDirectory(StreamingPath);
		}

		public int Id { get; }

		public SModInfoData Data          { get; }
		public string       StreamingPath => Application.streamingAssetsPath + "\\" + NameId;

		public string DisplayName => Data.DisplayName;
		public string NameId      => Data.NameId;

		public ReadOnlyCollection<Assembly> AttachedAssemblies => new ReadOnlyCollection<Assembly>(m_Assemblies);

		public static CModInfo CurrentMod
		{
			get
			{
				var assembly = Assembly.GetCallingAssembly();
				return World.Active.GetOrCreateSystem<CModManager>().GetAssemblyMod(assembly);
			}
		}

		public static ModWorld CurrentModWorld
		{
			get
			{
				var assembly = Assembly.GetCallingAssembly();
				return World.Active.GetOrCreateSystem<CModManager>().GetAssemblyMod(assembly).GetWorld();
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class InjectAttribute : Attribute
		{
		}
	}

	public static class CModInfoExtensions
	{
		public static ModWorld GetWorld(this CModInfo modInfo)
		{
			return World.Active.GetOrCreateSystem<CModManager>().GetModWorld(modInfo);
		}

		/* TODO: public static ModInputManager GetInputManager(this CModInfo modInfo)
		{
		    return modInfo.GetWorld().GetOrCreateSystem<ModInputManager>();
		}*/
	}
}