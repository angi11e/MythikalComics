using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class ClairvoyanceCardController : TheurgyBaseCardController
	{
		// Reveal X cards from the top of any deck, where X = the number of charm cards in play plus 1.
		// Put them back in any order.
		// You may estroy a [u]charm[/u] card.

		public ClairvoyanceCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// choose the deck
			List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
			IEnumerator selectTurnTakerCR = GameController.SelectTurnTaker(
				DecisionMaker,
				SelectionType.RevealCardsFromDeck,
				storedResults,
				additionalCriteria: (TurnTaker tt) => !IsHero(tt) || (IsHero(tt) && !tt.ToHero().IsIncapacitatedOrOutOfGame),
				numberOfCards: CharmCardsInPlay + 1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectTurnTakerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectTurnTakerCR);
			}

			// reveal X cards, put back in any order
			if (DidSelectTurnTaker(storedResults))
			{
				if (CharmCardsInPlay != 0)
				{
					TurnTaker selectedTurnTaker = GetSelectedTurnTaker(storedResults);
					IEnumerator revealCardsCR = RevealTheTopCardsOfDeck_MoveInAnyOrder(
						DecisionMaker,
						DecisionMaker,
						selectedTurnTaker,
						CharmCardsInPlay + 1
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(revealCardsCR);
					}
					else
					{
						GameController.ExhaustCoroutine(revealCardsCR);
					}
				}
				else
				{
					TurnTaker selectedTurnTaker = GetSelectedTurnTaker(storedResults);
					IEnumerator revealCardsCR = GameController.RevealCards(
						DecisionMaker,
						selectedTurnTaker.Deck,
						null,
						1,
						null,
						RevealedCardDisplay.ShowRevealedCards,
						GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(revealCardsCR);
					}
					else
					{
						GameController.ExhaustCoroutine(revealCardsCR);
					}
				}
			}

			// Destroy a [u]charm[/u] card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				IsCharmCriteria(),
				true,
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

			yield break;
		}
	}
}
