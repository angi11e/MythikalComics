using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Spoiler
{
	public class SpoilerCharacterCardController : HeroCharacterCardController
	{
		public SpoilerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int revealNumeral = GetPowerNumeral(0, 3);
			int handNumeral = GetPowerNumeral(1, 1);
			int topNumeral = GetPowerNumeral(2, 1);

			// Reveal the top 3 cards of your deck.
			List<Card> revealedCards = new List<Card>();
			IEnumerator revealCR = GameController.RevealCards(
				this.TurnTakerController,
				this.TurnTaker.Deck,
				revealNumeral,
				revealedCards,
				revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards,
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

			if (revealedCards.Any())
			{
				// Put 1 of them in your hand...
				if (this.TurnTaker.Revealed.NumberOfCards > 0 && handNumeral > 0)
				{
					if (this.TurnTaker.Revealed.NumberOfCards == 1)
					{
						IEnumerator moveSingleCR = GameController.MoveCard(
							this.TurnTakerController,
							this.TurnTaker.Revealed.Cards.First(),
							this.HeroTurnTaker.Hand,
							showMessage: true,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(moveSingleCR);
						}
						else
						{
							GameController.ExhaustCoroutine(moveSingleCR);
						}
					}
					else
					{
						int handFinal = handNumeral > this.TurnTaker.Revealed.NumberOfCards
							? this.TurnTaker.Revealed.NumberOfCards : handNumeral;
						List<MoveCardDestination> destinations = new List<MoveCardDestination>
						{
							new MoveCardDestination(this.HeroTurnTaker.Hand)
						};
						IEnumerator moveChosenCR = GameController.SelectCardsFromLocationAndMoveThem(
							this.HeroTurnTakerController,
							this.TurnTaker.Revealed,
							handFinal,
							handFinal,
							new LinqCardCriteria((Card c) => true),
							destinations,
							selectionType: SelectionType.MoveCardToHand,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(moveChosenCR);
						}
						else
						{
							GameController.ExhaustCoroutine(moveChosenCR);
						}
					}
				}

				// ...1 on the top of your deck...
				if (this.TurnTaker.Revealed.NumberOfCards > 0 && topNumeral > 0)
				{
					if (this.TurnTaker.Revealed.NumberOfCards == 1)
					{
						IEnumerator moveSingleCR = GameController.MoveCard(
							this.TurnTakerController,
							this.TurnTaker.Revealed.Cards.First(),
							this.HeroTurnTaker.Deck,
							showMessage: true,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(moveSingleCR);
						}
						else
						{
							GameController.ExhaustCoroutine(moveSingleCR);
						}
					}
					else
					{
						int topFinal = topNumeral > this.TurnTaker.Revealed.NumberOfCards
							? this.TurnTaker.Revealed.NumberOfCards : topNumeral;
						List<MoveCardDestination> destinations = new List<MoveCardDestination>
						{
							new MoveCardDestination(this.HeroTurnTaker.Deck)
						};
						IEnumerator moveChosenCR = GameController.SelectCardsFromLocationAndMoveThem(
							this.HeroTurnTakerController,
							this.TurnTaker.Revealed,
							topFinal,
							topFinal,
							new LinqCardCriteria((Card c) => true),
							destinations,
							selectionType: SelectionType.MoveCardOnDeck,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(moveChosenCR);
						}
						else
						{
							GameController.ExhaustCoroutine(moveChosenCR);
						}
					}
				}

				// ...and discard the rest.
				if (this.TurnTaker.Revealed.NumberOfCards > 0)
				{
					IEnumerator moveRestCR = GameController.MoveCards(
						this.TurnTakerController,
						this.TurnTaker.Revealed,
						this.TurnTaker.Trash,
						isDiscard: true,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(moveRestCR);
					}
					else
					{
						GameController.ExhaustCoroutine(moveRestCR);
					}
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One player may play a card.
					IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCR);
					}
					break;

				case 1:
					// one hero may use a power.
					IEnumerator usePowerCR = GameController.SelectHeroToUsePower(DecisionMaker);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(usePowerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(usePowerCR);
					}
					break;

				case 2:
					// Reveal the top card of a Deck. You may discard it or put it into Play.
					List<SelectLocationDecision> storedLocation = new List<SelectLocationDecision>();
					IEnumerable<LocationChoice> possibleDestinations = from tt in FindTurnTakersWhere(
						(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame
					) select new LocationChoice(tt.Deck);

					IEnumerator selectCR = GameController.SelectLocation(
						DecisionMaker,
						possibleDestinations,
						SelectionType.RevealTopCardOfDeck,
						storedLocation,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectCR);
					}

					Location selectedLocation = GetSelectedLocation(storedLocation);
					if (selectedLocation != null)
					{
						IEnumerator revealCR = RevealCard_PlayItOrDiscardIt(
							this.TurnTakerController,
							selectedLocation,
							isPutIntoPlay: true,
							showRevealedCards: true,
							responsibleTurnTaker: this.TurnTaker
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
					break;

					// Redirect the next Damage that would be dealt to a Hero Target to another Target.
					// pw aa - maybe for a variant
			}
			yield break;
		}
	}
}