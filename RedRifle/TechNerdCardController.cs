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
				this.TurnTakerController,
				this.TurnTaker.Deck,
				playMatchingCards: false,
				putMatchingCardsIntoPlay: false,
				// Put it into your hand and shuffle the rest of the revealed cards into your deck.
				moveMatchingCardsToHand: true,
				new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c), "ongoing or equipment cards"),
				1,
				revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discoverCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discoverCR);
			}

			IEnumerator cleanupCR = CleanupCardsAtLocations(
				new List<Location>() { this.TurnTaker.Revealed },
				this.TurnTaker.Deck
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cleanupCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cleanupCR);
			}

			List<PlayCardAction> storedResults = new List<PlayCardAction>();
			// You may play an Ongoing or Equipment card now.
			IEnumerator playTechCR = SelectAndPlayCardFromHand(
				this.HeroTurnTakerController,
				optional: true,
				storedResults,
				new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c), "ongoing or equipment card"),
				associateCardSource: true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playTechCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playTechCR);
			}

			if (storedResults.FirstOrDefault() == null || !storedResults.FirstOrDefault().WasCardPlayed)
			{
				// If no card entered play this way, add 2 tokens to your trueshot pool
				IEnumerator addTokensCR = AddTrueshotTokens(2);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addTokensCR);
				}

				// then one hero other than {RedRifle} may draw a card.
				IEnumerator grantDrawCR = GameController.SelectHeroToDrawCard(
					DecisionMaker,
					additionalCriteria: new LinqTurnTakerCriteria(
						(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && tt != this.TurnTaker
					),
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(grantDrawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(grantDrawCR);
				}
			}

			yield break;
		}
	}
}