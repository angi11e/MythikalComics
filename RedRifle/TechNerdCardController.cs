using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class TechNerdCardController : RedRifleBaseCardController
	{
		/*
		 * Reveal cards from the top of your deck until you reveal an Ongoing or Equipment card.
		 *  Put it into your hand and shuffle the rest of the revealed cards into your deck.
		 * You may play an Ongoing or Equipment card now.
		 * If no card entered play this way, add 2 tokens to your trueshot pool,
		 *  then one hero other than {RedRifle} may draw a card.
		 */

		public TechNerdCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Reveal cards from the top of your deck until you reveal an Ongoing or Equipment card.
			IEnumerator discoverCR = RevealCards_MoveMatching_ReturnNonMatchingCards(
				base.TurnTakerController,
				base.TurnTaker.Deck,
				playMatchingCards: false,
				putMatchingCardsIntoPlay: false,
				// Put it into your hand and shuffle the rest of the revealed cards into your deck.
				moveMatchingCardsToHand: true,
				new LinqCardCriteria((Card c) => c.IsOngoing || IsEquipment(c), "ongoing or equipment cards"),
				1,
				revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(discoverCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(discoverCR);
			}

			// You may play an Ongoing or Equipment card now.
			List<PlayCardAction> storedResults = new List<PlayCardAction>();
			IEnumerator playTechCR = SelectAndPlayCardFromHand(
				base.HeroTurnTakerController,
				optional: true,
				storedResults,
				new LinqCardCriteria((Card c) => c.IsOngoing || IsEquipment(c), "ongoing or equipment card")
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(playTechCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(playTechCR);
			}

			if (!storedResults.FirstOrDefault().WasCardPlayed)
			{
				// If no card entered play this way, add 2 tokens to your trueshot pool
				IEnumerator addTokensCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 2);

				// then one hero other than {RedRifle} may draw a card.
				IEnumerator grantDrawCR = GameController.SelectHeroToDrawCard(
					DecisionMaker,
					additionalCriteria: new LinqTurnTakerCriteria(
						(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && tt != base.TurnTaker
					),
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(addTokensCR);
					yield return base.GameController.StartCoroutine(grantDrawCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(addTokensCR);
					base.GameController.ExhaustCoroutine(grantDrawCR);
				}
			}

			yield break;
		}
	}
}