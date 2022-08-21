using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Athena
{
	public class ClassicAthenaCharacterCardController : HeroCharacterCardController
	{
		public ClassicAthenaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int revealNumeral = GetPowerNumeral(0, 3);
			int bottomNumeral = GetPowerNumeral(1, 1);

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
				// If an [u]aspect[/u] card is revealed you may play it now.
				List<Card> revealedAspects = revealedCards.Where((Card c) => IsAspect(c)).ToList();
				if (revealedAspects.Any())
				{
					IEnumerator playAspectCR = GameController.SelectAndPlayCard(
						this.HeroTurnTakerController,
						revealedAspects,
						true,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playAspectCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playAspectCR);
					}
				}

				// Move 1 revealed card to your hand, and move the rest to the bottom of your deck.
				if (this.TurnTaker.Revealed.NumberOfCards > 0)
				{
					if (this.TurnTaker.Revealed.NumberOfCards == 1)
					{
						IEnumerator moveSingleCR = GameController.MoveCard(
							this.TurnTakerController,
							this.TurnTaker.Revealed.Cards.First(),
							this.HeroTurnTaker.Hand,
							true,
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
						List<MoveCardDestination> destinations = new List<MoveCardDestination>
						{
							new MoveCardDestination(this.HeroTurnTaker.Hand)
						};
						IEnumerator moveChosenCR = GameController.SelectCardsFromLocationAndMoveThem(
							this.HeroTurnTakerController,
							this.TurnTaker.Revealed,
							bottomNumeral,
							bottomNumeral,
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

				if (this.TurnTaker.Revealed.NumberOfCards > 0)
				{
					IEnumerator moveRestCR = GameController.MoveCards(
						this.TurnTakerController,
						this.TurnTaker.Revealed,
						this.TurnTaker.Deck,
						toBottom: true,
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
					// One hero may use a power now.
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
				case 1:
					// Look at the bottom card of each deck and replace or discard each one.
					IEnumerator eachDeckCR = DoActionToEachTurnTakerInTurnOrder(
						tt => !tt.IsIncapacitatedOrOutOfGame,
						BottomDeckResponse,
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
					break;
				case 2:
					// Select a Hero Target. Until the start of your next turn, reduce damage deal to that Target by 1.
					List<SelectCardDecision> selectedTarget = new List<SelectCardDecision>();
					IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.ReduceDamageTaken,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && c.IsTarget && c.IsHero,
							"hero target",
							false,
							plural: "hero targets"
						),
						selectedTarget,
						optional: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectTargetCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectTargetCR);
					}

					SelectCardDecision selectCardDecision = selectedTarget.FirstOrDefault();
					if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
					{
						ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(1);
						reduceDamageStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
						reduceDamageStatusEffect.TargetCriteria.IsSpecificCard = selectCardDecision.SelectedCard;
						reduceDamageStatusEffect.UntilTargetLeavesPlay(selectCardDecision.SelectedCard);
						
						IEnumerator statusCR = AddStatusEffect(reduceDamageStatusEffect);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(statusCR);
						}
						else
						{
							GameController.ExhaustCoroutine(statusCR);
						}
					}
					break;
			}
			yield break;
		}

		private IEnumerator BottomDeckResponse(TurnTakerController ttc)
		{
			List<Card> revealedCards = new List<Card>();
			TurnTaker turnTaker = ttc.TurnTaker;

			foreach (Location deck in turnTaker.Decks)
			{
				Location trash = FindTrashFromDeck(deck);

				IEnumerator revealCR = GameController.RevealCards(
					ttc,
					deck,
					1,
					revealedCards,
					true,
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

				Card revealedCard = revealedCards.FirstOrDefault();
				if (revealedCard != null)
				{
					var destinations = new[]
					{
						new MoveCardDestination(deck, true),
						new MoveCardDestination(trash)
					};

					IEnumerator moveCardCR = GameController.SelectLocationAndMoveCard(
						DecisionMaker,
						revealedCard,
						destinations,
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
			}

			yield break;
		}

		protected LinqCardCriteria IsAspectCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsAspect(c), "aspect", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}

			return result;
		}

		protected bool IsAspect(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "aspect", evenIfUnderCard, evenIfFaceDown);
		}
	}
}