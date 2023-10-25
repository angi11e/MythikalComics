using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class AquilaCardController : CardController
	{
		/*
		 * you may destroy an environment card.
		 * 
		 * if no environment card is destroyed this way,
		 * reveal the top card of each deck, then either discard or replace each revealed card.
		 */

		public AquilaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// you may destroy an environment card.
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				true,
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			// if no environment card is destroyed this way,
			if (!DidDestroyCard(storedResults))
			{
				// reveal the top card of each deck, then either discard or replace each revealed card.
				IEnumerator eachDeckCR = DoActionToEachTurnTakerInTurnOrder(
					tt => !tt.IsIncapacitatedOrOutOfGame,
					RevealDeckResponse,
					this.TurnTaker
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(eachDeckCR);
				}
				else
				{
					GameController.ExhaustCoroutine(eachDeckCR);
				}
			}

			yield break;
		}

		private IEnumerator RevealDeckResponse(TurnTakerController ttc)
		{
			TurnTaker tt = ttc.TurnTaker;
			List<Location> decks = new List<Location>();
			if (GameController.IsLocationVisibleToSource(TurnTaker.Deck, GetCardSource()))
			{
				decks.Add(tt.Deck);
			}
			decks = decks.Concat(tt.SubDecks.Where(
				l => l.BattleZone == Card.BattleZone
					&& l.IsRealDeck
					&& GameController.IsLocationVisibleToSource(l, GetCardSource())
			)).ToList();

			Location trash;
			foreach (Location deck in decks)
			{
				trash = deck.IsSubDeck ? tt.FindSubTrash(deck.Identifier) : tt.Trash;
				List<Card> revealedCards = new List<Card>();

				IEnumerator revealCR = GameController.RevealCards(
					ttc,
					deck,
					1,
					revealedCards,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
				}

				Card revealedCard = revealedCards.FirstOrDefault<Card>();
				if (revealedCard != null)
				{
					IEnumerator moveCardsCR = GameController.SelectLocationAndMoveCard(
						DecisionMaker,
						revealedCard,
						new MoveCardDestination[] {
							new MoveCardDestination(deck),
							new MoveCardDestination(trash)
						},
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(moveCardsCR);
					}
					else
					{
						GameController.ExhaustCoroutine(moveCardsCR);
					}
				}
			}

			yield break;
		}
	}
}