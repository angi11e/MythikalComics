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
			base.AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				RevealCardResponse,
				TriggerType.RevealCard
			);

			base.AddTriggers();
		}

		private IEnumerator RevealCardResponse(PhaseChangeAction action)
		{
			// ...reveal the top card of your deck.
			List<Card> cards = new List<Card>();
			IEnumerator revealCR = base.GameController.RevealCards(
				base.TurnTakerController,
				base.TurnTaker.Deck,
				1,
				cards,
				fromBottom: false,
				RevealedCardDisplay.ShowRevealedCards,
				cardSource: GetCardSource()
			);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(revealCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(revealCR);
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
					this.DecisionMaker,
					"Return " + revealedCard.Title,
					SelectionType.ReturnToDeck,
					() => base.GameController.MoveCard(
						base.TurnTakerController,
						revealedCard,
						base.TurnTaker.Deck,
						cardSource: GetCardSource()
					)
				)
			);

			// ...or switch with hand
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"switch with a card in your hand",
					SelectionType.MoveCardToHand,
					() => WorkWithCardsResponse(revealedCard, base.HeroTurnTaker.Hand)
				)
			);

			// ...or switch with trash
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"switch with a card in your trash",
					SelectionType.MoveCardToTrash,
					() => WorkWithCardsResponse(revealedCard, base.HeroTurnTaker.Trash)
				)
			);

			// ask for which one
			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				base.GameController,
				this.DecisionMaker,
				functionList,
				false,
				associatedCards: new Card[1] { revealedCard },
				cardSource: base.GetCardSource()
			);

			IEnumerator selectFunctionCR = base.GameController.SelectAndPerformFunction(
				selectFunction,
				associatedCards: new Card[1] { revealedCard }
			);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectFunctionCR);
			}

			IEnumerator cleanupCR = CleanupCardsAtLocations(
				new List<Location>() {base.TurnTaker.Revealed},
				base.TurnTaker.Deck
			);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(cleanupCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(cleanupCR);
			}

			yield break;
		}

		private IEnumerator WorkWithCardsResponse(Card revealedCard, Location swapLocation)
		{
			IEnumerator moveFirstCardCR = GameController.SelectCardFromLocationAndMoveIt(
				this.DecisionMaker,
				swapLocation,
				new LinqCardCriteria((Card c) => true),
				new MoveCardDestination[] { new MoveCardDestination(base.HeroTurnTaker.Deck) }
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(moveFirstCardCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(moveFirstCardCR);
			}

			IEnumerator moveSecondCardCR = GameController.MoveCard(
				this.DecisionMaker,
				revealedCard,
				swapLocation,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(moveSecondCardCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(moveSecondCardCR);
			}

			yield break;
		}
	}
}