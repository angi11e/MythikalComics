using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class OwlSpiritCardController : YaojingBaseCardController
	{
		/*
		 * at the start of your turn,
		 * this card deals itself or {Supplicate} 2 irreducible psychic damage.
		 * 
		 * at the end of your turn, reveal the top card of any two decks.
		 * discard one and replace the other.
		 */

		public OwlSpiritCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// at the end of your turn,
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				RevealAndStuffResponse,
				TriggerType.RevealCard
			);
		}

		private IEnumerator RevealAndStuffResponse(PhaseChangeAction pca)
		{
			// reveal the top card of any two decks.
			List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
			IEnumerator selectDecksCR = SelectDecks(
				DecisionMaker,
				2,
				SelectionType.RevealTopCardOfDeck,
				(Location l) => true,
				storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectDecksCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectDecksCR);
			}

			IEnumerable<Location> decks = from l in storedResults
										  where l.Completed && l.SelectedLocation.Location != null
										  select l.SelectedLocation.Location;

			List<Card> storedCards = new List<Card>();
			for (int i = 0; i < decks.Count(); i++)
			{
				IEnumerator revealCR = GameController.RevealCards(
					TurnTakerController,
					decks.ElementAt(i),
					1,
					storedCards,
					false,
					RevealedCardDisplay.Message,
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
			}

			if (storedCards.Any())
			{
				// discard one and replace the other.
				IEnumerator selectDiscardCR = GameController.SelectCardsAndDoAction(
					DecisionMaker,
					new LinqCardCriteria((Card c) => storedCards.Contains(c)),
					SelectionType.DiscardCard,
					(Card c) => GameController.MoveCard(
						DecisionMaker,
						c,
						c.Owner.Trash,
						isDiscard: true,
						cardSource: GetCardSource()
					),
					1,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectDiscardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectDiscardCR);
				}

				Card otherCard = storedCards.Where((Card c) => c.Location.IsRevealed).FirstOrDefault();
				if (otherCard != null)
				{
					IEnumerator replaceCardCR = GameController.MoveCard(
						DecisionMaker,
						otherCard,
						otherCard.Owner.Deck,
						showMessage: true,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(replaceCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(replaceCardCR);
					}
				}
			}
			yield break;
		}
	}
}