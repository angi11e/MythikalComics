using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Handelabra;

namespace Angille
{
	public static class ExtensionMethods
	{
		public static void ReorderTokenPool(this TokenPool[] tokenPools, string poolThatShouldBeFirst)
		{
			var temp = new List<TokenPool>(tokenPools);
			int targetIndex = temp.FindIndex(tp => string.Equals(
				tp.Identifier,
				poolThatShouldBeFirst,
				StringComparison.Ordinal
			));
			//if targetIndex == -1, no matching pool found, make no change.
			//if targetIndex == 0, matching pool already first, make no change.
			if (targetIndex > 0)
			{
				var newFirst = tokenPools[targetIndex];

				//shuffle all other indexes forward without changing the relative order
				int index = targetIndex;
				while (index > 0)
				{
					tokenPools[index] = tokenPools[--index];
				}
				tokenPools[0] = newFirst;
			}
		}

		public static IEnumerator AugmentNemesisBugs(
			IEnumerable<string> villainsToAugment,
			CardController nemesisSource
		)
		{
			for (int i = 0; i < villainsToAugment.Count(); i++)
			{
				Card newNemesis = nemesisSource.FindCardsWhere(
					(Card c) => c.Identifier == villainsToAugment.ElementAt(i),
					ignoreBattleZone: true
				).FirstOrDefault();
				if (newNemesis != null)
				{
					IEnumerator addNemesisCR = nemesisSource.GameController.UpdateNemesisIdentifiers(
						nemesisSource.GameController.FindCardController(newNemesis),
						nemesisSource.Card.NemesisIdentifiers,
						nemesisSource.GetCardSource()
					);

					if (nemesisSource.UseUnityCoroutines)
					{
						yield return nemesisSource.GameController.StartCoroutine(addNemesisCR);
					}
					else
					{
						nemesisSource.GameController.ExhaustCoroutine(addNemesisCR);
					}
				}
			}
			yield break;
		}

		/* FAILED EXPERIMENT
		[Serializable]
		public class ReduceDamageToSetAmountStatusEffect : DealDamageStatusEffect
		{
			public int Amount { get; private set; }

			public override bool CombineWithExistingInstance
			{
				get
				{
					if (base.NumberOfUses.HasValue)
					{
						return true;
					}
					return false;
				}
			}

			public ReduceDamageToSetAmountStatusEffect(int amount)
			{
				Amount = amount;
			}

			public override bool IsSameAs(StatusEffect other)
			{
				if (other is ReduceDamageToSetAmountStatusEffect)
				{
					ReduceDamageToSetAmountStatusEffect otherEffect = other as ReduceDamageToSetAmountStatusEffect;
					if (
						base.CardSource.Identifier == otherEffect.CardSource.Identifier
						&& base.TargetCriteria.IsSpecificCard == otherEffect.TargetCriteria.IsSpecificCard
					)
					{
						return base.SourceCriteria.IsSpecificCard == otherEffect.SourceCriteria.IsSpecificCard;
					}
					return false;
				}
				return false;
			}

			public override string ToString()
			{
				bool plural = ((!base.NumberOfUses.HasValue || base.NumberOfUses.Value != 1) ? true : false);
				string text = $"Reduce {TheNextString()}{base.DamageTypeCriteria.DamageString()} dealt {base.SourceCriteria.ByWhoString()} {base.TargetCriteria.ToWhoString(plural)} to {Amount}.".TrimExtraSpaces();
				return text;

			}
		}
		*/

		/* ended up putting this in a BaseCardController instead
		public static IEnumerator DiscardResponse(this CardController card)
		{
			yield break;
		}
		*/
	}
}
