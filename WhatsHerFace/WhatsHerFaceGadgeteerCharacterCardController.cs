using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.WhatsHerFace
{
	public class WhatsHerFaceGadgeteerCharacterCardController : WhatsHerFaceBaseCharacterCardController
	{
		/*
		 * Destroy a [u]recall[/u] card.
		 * If you do so, deal 1 target 3 sonic damage.
		 */

		public WhatsHerFaceGadgeteerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Destroy a [u]recall[/u] card.
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = base.GameController.SelectAndDestroyCard(
				base.HeroTurnTakerController,
				new LinqCardCriteria((Card c) => IsRecall(c), "recall"),
				optional: false,
				storedResults,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(destroyCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(destroyCR);
			}

			// was one destroyed?
			if (DidDestroyCard(storedResults))
			{
				// If you do so, deal 1 target 3 sonic damage.
				int targetNumeral = GetPowerNumeral(0, 1);
				int damageNumeral = GetPowerNumeral(1, 3);
				IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, base.CharacterCard),
					damageNumeral,
					DamageType.Sonic,
					targetNumeral,
					false,
					targetNumeral,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// Move an equipment card from a hero trash into play.
					List<SelectCardDecision> selectCardDecision = new List<SelectCardDecision>();

					// select the card
					IEnumerator selectCardCR = this.GameController.SelectCardAndStoreResults(
						this.HeroTurnTakerController,
						SelectionType.MoveCardOnDeck,
						new LinqCardCriteria(
							(Card c) => c.IsInTrash && IsEquipment(c) && c.IsHero
							&& this.GameController.IsLocationVisibleToSource(c.Location, base.GetCardSource(null))
						),
						selectCardDecision,
						false
					);
					if (UseUnityCoroutines)
					{
						yield return this.GameController.StartCoroutine(selectCardCR);
					}
					else
					{
						this.GameController.ExhaustCoroutine(selectCardCR);
					}

					if (!DidSelectCard(selectCardDecision))
					{
						yield break;
					}

					// move the card
					IEnumerator moveCardCR = this.GameController.MoveCard(
						this.TurnTakerController,
						selectCardDecision.FirstOrDefault().SelectedCard,
						selectCardDecision.FirstOrDefault().SelectedCard.Owner.PlayArea,
						isPutIntoPlay: true,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return this.GameController.StartCoroutine(moveCardCR);
					}
					else
					{
						this.GameController.ExhaustCoroutine(moveCardCR);
					}
					break;

				case 1:
					// Each player may discard a card. Any player that does draws 2 cards.
					List<DiscardCardAction> discardedCards = new List<DiscardCardAction>();
					IEnumerator discardCR = GameController.EachPlayerDiscardsCards(
						0,
						1,
						discardedCards,
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

					foreach (DiscardCardAction item in discardedCards)
					{
						if (item.WasCardDiscarded)
						{
							IEnumerator drawCR = DrawCards(item.HeroTurnTakerController, 2);
							if (UseUnityCoroutines)
							{
								yield return GameController.StartCoroutine(drawCR);
							}
							else
							{
								GameController.ExhaustCoroutine(drawCR);
							}
						}
					}
					break;

				case 2:
					// Select a hero target. Until the start of your next turn, reduce damage dealt to that target by 1.
					List<SelectCardDecision> protectedList = new List<SelectCardDecision>();
					IEnumerator chooseHeroCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.ReduceDamageTaken,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && c.IsTarget && c.IsHero,
							"hero target",
							useCardsSuffix: true,
							plural: "hero targets"
						),
						protectedList,
						optional: false,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(chooseHeroCR);
					}
					else
					{
						GameController.ExhaustCoroutine(chooseHeroCR);
					}

					SelectCardDecision protectedTarget = protectedList.FirstOrDefault();
					if (protectedTarget != null && protectedTarget.SelectedCard != null)
					{
						ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(1);
						reduceDamageStatusEffect.UntilStartOfNextTurn(TurnTaker);
						reduceDamageStatusEffect.TargetCriteria.IsSpecificCard = protectedTarget.SelectedCard;
						reduceDamageStatusEffect.UntilTargetLeavesPlay(protectedTarget.SelectedCard);
						IEnumerator protectedCR = AddStatusEffect(reduceDamageStatusEffect);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(protectedCR);
						}
						else
						{
							GameController.ExhaustCoroutine(protectedCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}