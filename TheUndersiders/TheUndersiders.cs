using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Angille.TheUndersiders
{
	public static class TheUndersiders
	{
		public enum Villain
		{
			Unknown = 0,
			Bitch,
			Foil,
			Grue,
			Imp,
			Parian,
			Regent,
			Skitter,
			Tattletale
		}

		private static readonly Dictionary<Villain, string> _Icons = new Dictionary<Villain, string>()
		{
			[Villain.Bitch] = "dog",
			[Villain.Foil] = "blade",
			[Villain.Grue] = "skull",
			[Villain.Imp] = "mask",
			[Villain.Parian] = "bear",
			[Villain.Regent] = "crown",
			[Villain.Skitter] = "spider",
			[Villain.Tattletale] = "tattle"
		};

		public static string GetIcon(this Villain villain)
		{
			return _Icons[villain];
		}

		public static Villain GetVillainFromIcon(string icon)
		{
			return _Icons.FindKeyByValue(icon);
		}

		public static IEnumerable<string> Icons => _Icons.Values;

		private static readonly Dictionary<Villain, string> _Identifiers = new Dictionary<Villain, string>() {
			[Villain.Bitch] = "BitchCharacter",
			[Villain.Foil] = "FoilCharacter",
			[Villain.Grue] = "GrueCharacter",
			[Villain.Imp] = "ImpCharacter",
			[Villain.Parian] = "ParianCharacter",
			[Villain.Regent] = "RegentCharacter",
			[Villain.Skitter] = "SkitterCharacter",
			[Villain.Tattletale] = "TattletaleCharacter"
		};

		public static string GetIdentifier(this Villain villain)
		{
			return _Identifiers[villain];
		}

		public static Villain GetVillainFromIdentifier(string identifier)
		{
			return _Identifiers.FindKeyByValue(identifier);
		}

		public static IEnumerable<string> Identifiers => _Identifiers.Values;
	}

	public static class Extensions
	{
		public static K FindKeyByValue<K, V>(this Dictionary<K, V> dict, V value)
		{
			Dictionary<V, K> revDict = dict.ToDictionary(pair => pair.Value, pair => pair.Key);
			return revDict[value];
		}
	}
}
