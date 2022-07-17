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
	}

	public abstract class AngilleHeroTurnTakerController : HeroTurnTakerController
	{
		protected abstract IEnumerable<string> VillainsToAugment { get; }

		public AngilleHeroTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		public override IEnumerator StartGame()
		{
			// IEnumerable<string> villainsToAugment = new[] { "GloomWeaverCharacter", "AkashBhutaCharacter" };

			/* copy-to-hero version
			for (int i = 0; i < villainsToAugment.Count(); i++)
			{
				Card newNemesis = FindCardsWhere(
					(Card c) => c.Identifier == villainsToAugment.ElementAt(i)
				).FirstOrDefault();
				if (newNemesis != null)
				{
					CardController thisCCC = FindCardController(TurnTaker.CharacterCard);
					IEnumerator addNemesisCR = GameController.UpdateNemesisIdentifiers(
						thisCCC,
						newNemesis.NemesisIdentifiers,
						thisCCC.GetCardSource()
					);

					if (thisCCC.UseUnityCoroutines)
					{
						yield return thisCCC.GameController.StartCoroutine(addNemesisCR);
					}
					else
					{
						thisCCC.GameController.ExhaustCoroutine(addNemesisCR);
					}
				}
			}
			*/

			/* copy-to-villain version */
			for (int i = 0; i < VillainsToAugment.Count(); i++)
			{
				Card newNemesis = FindCardsWhere(
					(Card c) => c.Identifier == VillainsToAugment.ElementAt(i)
				).FirstOrDefault();
				if (newNemesis != null)
				{
					CardController thisCCC = FindCardController(TurnTaker.CharacterCard);
					IEnumerator addNemesisCR = GameController.UpdateNemesisIdentifiers(
						GameController.FindCardController(newNemesis),
						TurnTaker.CharacterCard.NemesisIdentifiers.Concat(newNemesis.NemesisIdentifiers),
						thisCCC.GetCardSource()
					);

					if (thisCCC.UseUnityCoroutines)
					{
						yield return thisCCC.GameController.StartCoroutine(addNemesisCR);
					}
					else
					{
						thisCCC.GameController.ExhaustCoroutine(addNemesisCR);
					}
				}
			}

			yield break;
		}
	}
}
