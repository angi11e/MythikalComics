using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.WhatsHerFace
{
	public class UmbraCharacterCardController : WhatsHerFaceBaseCharacterCardController
	{
		public UmbraCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int revealNumeral = GetPowerNumeral(0, 2);
			int playNumeral = GetPowerNumeral(1, 1);

			// Reveal the top 2 cards of your deck.
			List<Card> revealedCards = new List<Card>();
			IEnumerator revealCR = GameController.RevealCards(
				this.TurnTakerController,
				this.TurnTaker.Deck,
				revealNumeral,
				revealedCards,
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
				// Play one of them and put the rest in your hand.
				IEnumerator playCardCR = GameController.SelectAndPlayCard(
					this.HeroTurnTakerController,
					revealedCards,
					false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
				}

				if (this.TurnTaker.Revealed.NumberOfCards > 0)
				{
					IEnumerator moveRestCR = GameController.MoveCards(
						this.TurnTakerController,
						this.TurnTaker.Revealed,
						this.HeroTurnTaker.Hand,
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
					// Discard the top 3 cards of 1 deck.
					List<SelectLocationDecision> deckSelection = new List<SelectLocationDecision>();
					IEnumerator selectDeckCR = GameController.SelectADeck(
						DecisionMaker,
						SelectionType.DiscardFromDeck,
						l => true,
						deckSelection,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectDeckCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectDeckCR);
					}

					if (DidSelectLocation(deckSelection))
					{
						IEnumerator discardCR = GameController.DiscardTopCards(
							DecisionMaker,
							GetSelectedLocation(deckSelection),
							3,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(discardCR);
						}
						else
						{
							GameController.ExhaustCoroutine(discardCR);
						}
					}
					break;

				case 1:
					// Select a target.
					List<SelectCardDecision> cardSelection = new List<SelectCardDecision>();
					IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.SelectTargetFriendly,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && c.IsTarget,
							"target",
							useCardsSuffix: false,
							plural: "targets"
						),
						cardSelection,
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

					SelectCardDecision selected = cardSelection.FirstOrDefault();
					if (selected != null && selected.SelectedCard != null)
					{
						// Increase the next damage dealt to that target by 1...
						IncreaseDamageStatusEffect increaseDamageStatusEffect = new IncreaseDamageStatusEffect(1);
						increaseDamageStatusEffect.NumberOfUses = 1;
						increaseDamageStatusEffect.SourceCriteria.IsSpecificCard = selected.SelectedCard;
						increaseDamageStatusEffect.UntilTargetLeavesPlay(selected.SelectedCard);						
						IEnumerator increaseStatusCR = AddStatusEffect(increaseDamageStatusEffect);

						// ...and reduce the next damage dealt by that target by 1.
						ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(1);
						reduceDamageStatusEffect.NumberOfUses = 1;
						reduceDamageStatusEffect.TargetCriteria.IsSpecificCard = selected.SelectedCard;
						reduceDamageStatusEffect.UntilTargetLeavesPlay(selected.SelectedCard);
						IEnumerator decreaseStatusCR = AddStatusEffect(reduceDamageStatusEffect);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(increaseStatusCR);
							yield return GameController.StartCoroutine(decreaseStatusCR);
						}
						else
						{
							GameController.ExhaustCoroutine(increaseStatusCR);
							GameController.ExhaustCoroutine(decreaseStatusCR);
						}
					}
					break;

				case 2:
					// Destroy up to two ongoing cards. Only one may be a villain card.
					LinqCardCriteria ongoingCriteria = new LinqCardCriteria(
						(Card c) => c.IsOngoing && c.IsInPlay,
						"ongoing"
					);
					List<DestroyCardAction> destroySelection = new List<DestroyCardAction>();
					IEnumerator destroyFirstCR = GameController.SelectAndDestroyCard(
						DecisionMaker,
						ongoingCriteria,
						true,
						destroySelection,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroyFirstCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroyFirstCR);
					}

					if (DidDestroyCard(destroySelection))
					{
						if (destroySelection.FirstOrDefault().CardToDestroy.Card.IsVillain)
						{
							ongoingCriteria = new LinqCardCriteria(
								(Card c) => c.IsOngoing && c.IsInPlay && !c.IsVillain,
								"ongoing"
							);
						}

						IEnumerator destroySecondCR = GameController.SelectAndDestroyCard(
							DecisionMaker,
							ongoingCriteria,
							true,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(destroySecondCR);
						}
						else
						{
							GameController.ExhaustCoroutine(destroySecondCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}