using System.Collections;
using System.Collections.Generic;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class IAmAProfessionalCardController : CardController
	{
		// At the start of your turn, reveal the top card of your deck.
		// You may switch it with one card from your hand or your trash.

		public IAmAProfessionalCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// At the start of your turn...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				RevealCardResponse,
				TriggerType.RevealCard
			);

			base.AddTriggers();
		}

		private IEnumerator RevealCardResponse(PhaseChangeAction action)
		{
			// ...reveal the top card of your deck.
			List<Card> cards = new List<Card>();
			IEnumerator revealCR = GameController.RevealCards(
				this.TurnTakerController,
				this.TurnTaker.Deck,
				1,
				cards,
				fromBottom: false,
				RevealedCardDisplay.ShowRevealedCards,
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
			Card revealedCard = GetRevealedCard(cards);
			if (revealedCard == null)
			{
				yield break;
			}

			// You may switch it with one card from your hand or your trash.
			List<Function> functionList = new List<Function>();

			// first put it back
			functionList.Add(
				new Function(
					DecisionMaker,
					"Return " + revealedCard.Title,
					SelectionType.ReturnToDeck,
					() => GameController.MoveCard(
						this.TurnTakerController,
						revealedCard,
						this.TurnTaker.Deck,
						cardSource: GetCardSource()
					),
					forcedActionMessage: "no cards in hand or trash to switch with"
				)
			);

			// ...or switch with hand
			functionList.Add(
				new Function(
					DecisionMaker,
					"switch with a card in your hand",
					SelectionType.MoveCardToHand,
					() => WorkWithCardsResponse(revealedCard, this.HeroTurnTaker.Hand),
					this.HeroTurnTaker.HasCardsInHand
				)
			);

			// ...or switch with trash
			functionList.Add(
				new Function(
					DecisionMaker,
					"switch with a card in your trash",
					SelectionType.MoveCardToTrash,
					() => WorkWithCardsResponse(revealedCard, this.HeroTurnTaker.Trash),
					this.HeroTurnTaker.Trash.HasCards
				)
			);

			// ask for which one
			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				GameController,
				DecisionMaker,
				functionList,
				false,
				associatedCards: new Card[1] { revealedCard },
				cardSource: GetCardSource()
			);

			IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(
				selectFunction,
				associatedCards: new Card[1] { revealedCard }
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectFunctionCR);
			}

			IEnumerator cleanupCR = CleanupCardsAtLocations(
				new List<Location>() {this.TurnTaker.Revealed},
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

			yield break;
		}

		private IEnumerator WorkWithCardsResponse(Card revealedCard, Location swapLocation)
		{
			IEnumerator moveFirstCardCR = GameController.SelectCardFromLocationAndMoveIt(
				DecisionMaker,
				swapLocation,
				new LinqCardCriteria((Card c) => true),
				new MoveCardDestination[] { new MoveCardDestination(this.HeroTurnTaker.Deck) }
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveFirstCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveFirstCardCR);
			}

			IEnumerator moveSecondCardCR = GameController.MoveCard(
				DecisionMaker,
				revealedCard,
				swapLocation,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveSecondCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveSecondCardCR);
			}

			yield break;
		}
	}
}