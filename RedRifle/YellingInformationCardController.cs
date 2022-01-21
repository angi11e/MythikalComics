using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class YellingInformationCardController : RedRifleBaseCardController
	{
		/*
		 * Reduce all damage dealt by 1.
		 * POWER:
		 * Play the top card of each deck.
		 *  For each one-shot played this way, add 1 token to your trueshot pool.
		 *  Destroy this card.
		 */

		public YellingInformationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce all damage dealt by 1.
			AddReduceDamageTrigger((Card c) => true, 1);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Play the top card of each deck.
			IEnumerator playTopCardsCR = PlayTopCardOfEachDeckInTurnOrder(
				(TurnTakerController ttc) => true,
				(Location l) => true,
				base.TurnTaker
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(playTopCardsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(playTopCardsCR);
			}

			// For each one-shot played this turn, add 1 token to your trueshot pool.
			int tokensToAdd = (
				from pcje in Journal.PlayCardEntriesThisTurn()
				where pcje.CardPlayed.IsOneShot
				select pcje
			).Count();
			if (tokensToAdd > 0)
			{
				IEnumerator addTokensCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, tokensToAdd);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addTokensCR);
				}
			}

			// Destroy this card.
			IEnumerator destructionCR = GameController.DestroyCard(
				this.DecisionMaker,
				base.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}

			yield break;
		}
	}
}