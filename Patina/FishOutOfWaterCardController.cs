using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class FishOutOfWaterCardController : PatinaBaseCardController
	{
		/*
		 * Reveal cards from the top of your deck until you reveal 2 water cards.
		 *  Put them into your hand.
		 *  Shuffle the rest of the revealed cards into your deck.
		 * Play 1 water card.
		 */

		public FishOutOfWaterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsAtLocation(base.HeroTurnTaker.Deck, IsWaterCriteria());
		}

		public override IEnumerator Play()
		{
			// Reveal cards from the top of your deck until you reveal 2 water cards.
			// Put them into your hand.
			// Shuffle the rest of the revealed cards into your deck.
			IEnumerator revealCR = RevealCards_MoveMatching_ReturnNonMatchingCards(
				DecisionMaker,
				this.HeroTurnTaker.Deck,
				false,
				false,
				true,
				IsWaterCriteria(),
				2,
				revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards
			);

			// Play 1 water card.
			IEnumerator playCR = SelectAndPlayCardFromHand(
				DecisionMaker,
				false,
				cardCriteria: IsWaterCriteria()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealCR);
				yield return GameController.StartCoroutine(playCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealCR);
				GameController.ExhaustCoroutine(playCR);
			}

			yield break;
		}
	}
}