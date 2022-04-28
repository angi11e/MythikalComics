using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class OracleCardController : AthenaBaseCardController
	{
		/*
		 * Reveal the top 2 cards of the Villain deck.
		 *  Put 1 of them on top of the Villain deck and the other on the bottom.
		 */

		public OracleCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
			IEnumerator getDeckCR = FindVillainDeck(
				DecisionMaker,
				SelectionType.RevealCardsFromDeck,
				storedResults,
				(Location l) => true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(getDeckCR);
			}
			else
			{
				GameController.ExhaustCoroutine(getDeckCR);
			}
			Location deckToReveal = GetSelectedLocation(storedResults);

			if (deckToReveal != null)
			{
				// Reveal the top 2 cards of the Villain deck.
				List<Card> revealedCards = new List<Card>();
				IEnumerator revealCR = RevealCardsFromTopOfDeck_PutOnTopAndOnBottom(
					DecisionMaker,
					base.TurnTakerController,
					deckToReveal
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
				}

				List<Location> list = new List<Location>();
				list.Add(deckToReveal.OwnerTurnTaker.Revealed);

				IEnumerator cleanCR = CleanupCardsAtLocations(
					list,
					deckToReveal,
					toBottom: false,
					cardsInList: revealedCards
				);
				
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(cleanCR);
				}
				else
				{
					GameController.ExhaustCoroutine(cleanCR);
				}
			}
			else
			{
				IEnumerator messageCR = GameController.SendMessageAction(
					"There was no deck to reveal cards from.",
					Priority.Medium,
					GetCardSource(),
					null,
					showCardSource: true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(messageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(messageCR);
				}
			}

			yield break;
		}
	}
}