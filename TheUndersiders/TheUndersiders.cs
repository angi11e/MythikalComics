using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public static SpecialString ShowHeroWithMostCardsInTrash(this SpecialStringMaker maker, LinqCardCriteria additionalCriteria = null, Func<bool> showInEffectsList = null)
		{
			FieldInfo field = maker.GetType().GetField("_cardController", BindingFlags.Instance | BindingFlags.NonPublic);
			CardController _cardController = (CardController)field.GetValue(maker);
			return maker.ShowSpecialString(delegate
			{
				IEnumerable<TurnTaker> enumerable = _cardController.GameController.FindTurnTakersWhere(
					(TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame, _cardController.BattleZone
				);
				List<string> list = new List<string>();
				int num = 0;
				foreach (HeroTurnTaker hero in enumerable)
				{
					IEnumerable<Card> cardsWhere = hero.GetCardsWhere(
						(Card c) => c.IsInTrash && c.Location.OwnerTurnTaker == hero
					);
					List<Card> source = ((additionalCriteria == null) ? cardsWhere.ToList() : cardsWhere.Where(additionalCriteria.Criteria).ToList());
					if (source.Count() > num)
					{
						list.RemoveAll((string htt) => true);
						list.Add(hero.Name);
						num = source.Count();
					}
					else if (source.Count() == num)
					{
						list.Add(hero.Name);
					}
				}
				string text = list.Count().ToString_SingularOrPlural("Hero", "Heroes");
				string text2 = " in trash";
				string text3 = " cards";
				if (additionalCriteria != null)
				{
					text3 = " " + additionalCriteria.GetDescription();
				}
				return (list.Count() > 0) ? string.Format("{0} with the most{3}{2}: {1}.", text, list.ToRecursiveString(), text2, text3) : "Warning: No heroes found";
			}, showInEffectsList);
		}
	}
}
