using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Supplicate
{
	public class SupplicateCharacterCardController : SupplicateBaseCharacterCardController
	{
		public SupplicateCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Reveal the top card of your deck.
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
			if (revealedCard != null)
			{
				// if it is yaojing or limited, move it to your hand. Otherwise, play it.
				Location theDestination = (revealedCard.IsLimited || IsYaojing(revealedCard))
					? this.HeroTurnTaker.Hand
					: this.HeroTurnTaker.PlayArea;

				IEnumerator moveCardCR = GameController.MoveCard(
					DecisionMaker,
					revealedCard,
					theDestination,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCardCR);
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may draw a card.
					IEnumerator drawCardCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCardCR);
					}
					break;

				case 1:
					// Each target regains 1 HP.
					IEnumerator gainHPCR = GameController.GainHP(
						DecisionMaker,
						(Card c) => true,
						1,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(gainHPCR);
					}
					else
					{
						GameController.ExhaustCoroutine(gainHPCR);
					}
					break;

				case 2:
					// Each player may discard 2 cards.
					IEnumerator discardAndPlayCR = GameController.SelectTurnTakersAndDoAction(
						DecisionMaker,
						new LinqTurnTakerCriteria(
							(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame
							&& IsHero(tt)
							&& tt.ToHero().Hand.NumberOfCards >= 2
						),
						SelectionType.DiscardCard,
						DiscardAndPlayResponse,
						requiredDecisions: 0,
						allowAutoDecide: true,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(discardAndPlayCR);
					}
					else
					{
						GameController.ExhaustCoroutine(discardAndPlayCR);
					}
					break;
			}
			yield break;
		}

		private IEnumerator DiscardAndPlayResponse(TurnTaker tt)
		{
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				FindHeroTurnTakerController(tt.ToHero()),
				2,
				true,
				storedResults: storedResults
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (DidDiscardCards(storedResults, 2))
			{
				// Any player that does may play a card.
				IEnumerator playCardCR = SelectAndPlayCardFromHand(FindHeroTurnTakerController(tt.ToHero()));
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
				}
			}
			yield break;
		}
	}
}