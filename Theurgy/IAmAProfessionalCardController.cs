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

			// You may switch it with one card from your hand or your trash.
			List<Function> functionList = new List<Function>();

			// first put it back
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"Return the card",
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
					() => base.GameController.MoveCard(
						base.TurnTakerController,
						revealedCard,
						base.HeroTurnTaker.Hand,
						cardSource: GetCardSource()
					)
				)
			);

			// ...or switch with trash
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"switch with a card in your trash",
					SelectionType.MoveCardToTrash,
					() => base.GameController.MoveCard(
						base.TurnTakerController,
						revealedCard,
						base.HeroTurnTaker.Trash,
						cardSource: GetCardSource()
					)
				)
			);

			// ask for which one
			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				base.GameController,
				this.DecisionMaker,
				functionList,
				false,
				cardSource: base.GetCardSource()
			);

			IEnumerator selectFunctionCR = base.GameController.SelectAndPerformFunction(selectFunction);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectFunctionCR);
			}

			if (revealedCard.Location != base.HeroTurnTaker.Deck)
			{
				IEnumerator selectAndMoveCR = base.GameController.SelectAndMoveCard(
					DecisionMaker,
					c => c.IsInLocation(revealedCard.Location),
					base.HeroTurnTaker.Deck,
					cardSource: base.GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(selectAndMoveCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(selectAndMoveCR);
				}
			}

			yield break;
		}
	}
}