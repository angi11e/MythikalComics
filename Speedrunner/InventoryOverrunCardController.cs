using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class InventoryOverrunCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Each player reveals cards from the top of their deck until an equipment card is revealed.
		 * They put that card in their hand and shuffle the other revealed cards back into their deck.
		 * One player may play a card now.
		 */

		public InventoryOverrunCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Each player...
			IEnumerator findEquipCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria(
					tt => GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())
					&& tt.IsHero && !tt.IsIncapacitatedOrOutOfGame
					&& tt.Deck.Cards.Where((Card c) => IsEquipment(c)).Any()
				),
				SelectionType.RevealCardsFromDeck,
				(TurnTaker tt) => RevealCards_MoveMatching_ReturnNonMatchingCards(
					// ...reveals cards from the top of their deck until an equipment card is revealed.
					FindTurnTakerController(tt),
					tt.Deck,
					playMatchingCards: false,
					putMatchingCardsIntoPlay: false,
					// They put that card in their hand and shuffle the other revealed cards back into their deck.
					moveMatchingCardsToHand: true,
					new LinqCardCriteria((Card c) => IsEquipment(c), "equipment cards"),
					1,
					revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards
				),
				allowAutoDecide: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findEquipCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findEquipCR);
			}

			// One player may play a card now.
			IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCR);
			}

			yield break;
		}
	}
}