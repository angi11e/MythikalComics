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

		public ClairvoyanceCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

        public override IEnumerator Play()
        {
			// count the charm cards
			int drawNumeral = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsCharm(c)).Count() + 1;

			// choose the deck
			List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
			IEnumerator selectTurnTakerCR = base.GameController.SelectTurnTaker(
				DecisionMaker,
				SelectionType.RevealCardsFromDeck,
				storedResults,
				additionalCriteria: (TurnTaker tt) => !tt.IsHero || (tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame),
				numberOfCards: drawNumeral,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectTurnTakerCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectTurnTakerCR);
			}

			// reveal X cards, put back in any order
			if (DidSelectTurnTaker(storedResults))
			{
				if (drawNumeral != 1)
				{
					TurnTaker selectedTurnTaker = GetSelectedTurnTaker(storedResults);
					IEnumerator revealCardsCR = RevealTheTopCardsOfDeck_MoveInAnyOrder(
						DecisionMaker,
						DecisionMaker,
						selectedTurnTaker,
						drawNumeral
					);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(revealCardsCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(revealCardsCR);
					}
				}
				else
				{
					TurnTaker selectedTurnTaker = GetSelectedTurnTaker(storedResults);
					IEnumerator revealCardsCR = base.GameController.RevealCards(
						DecisionMaker,
						selectedTurnTaker.Deck,
						null,
						drawNumeral,
						null,
						RevealedCardDisplay.ShowRevealedCards,
						GetCardSource()
					);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(revealCardsCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(revealCardsCR);
					}
				}
			}
			yield break;
        }
    }
}