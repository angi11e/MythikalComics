using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Theurgy
{
	public class TheurgyCharacterCardController : HeroCharacterCardController
	{
		public TheurgyCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
			base.SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Hand, IsCharmCriteria());
		}

		public override IEnumerator UsePower(int index = 0)
		{
			if (this.HeroTurnTaker.GetCardsWhere((Card c) => c.Location == HeroTurnTaker.Hand && IsCharm(c)).Count() > 0)
			{
				// Draw a card or play a charm card. One hero target regains 1 hp.
				List<Function> functionList = new List<Function>();

				// first draw a card option
				functionList.Add(
					new Function(
						this.DecisionMaker,
						"Draw a card",
						SelectionType.DrawCard,
						() => base.GameController.DrawCards(
							this.HeroTurnTakerController,
							1
						)
					)
				);

				// ...or play a charm card option
				functionList.Add(
					new Function(
						this.DecisionMaker,
						"Play a charm card",
						SelectionType.PlayCard,
						() => base.SelectAndPlayCardFromHand(
							this.HeroTurnTakerController,
							false,
							null,
							IsCharmCriteria()
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
			}
			else
			{
				IEnumerator drawCardCR = base.GameController.DrawCards(
					this.HeroTurnTakerController,
					1
				);

				if (UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(drawCardCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(drawCardCR);
				}
			}

			// One hero target regains 1 HP.
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = base.FindCardsWhere(
				new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget && c.IsInPlayAndHasGameText)
			);
			IEnumerator selectTargetCR = base.GameController.SelectTargetAndStoreResults(
				base.HeroTurnTakerController,
				choices,
				selectedTarget,
				optional: true,
				selectionType: SelectionType.GainHP,
				cardSource: base.GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectTargetCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectTargetCR);
			}

			if (selectedTarget != null && selectedTarget.Any())
			{
				SelectTargetDecision selectedTargetDecision = selectedTarget.FirstOrDefault();
				if (selectedTargetDecision != null && selectedTargetDecision.SelectedCard != null)
				{
					var healHP = GetPowerNumeral(0, 1);
					IEnumerator healTargetCR = base.GameController.GainHP(
						selectedTarget.FirstOrDefault().SelectedCard,
						healHP,
						cardSource: base.GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(healTargetCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(healTargetCR);
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
					// One player may draw a card now.
					IEnumerator drawCR = this.GameController.SelectHeroToDrawCard(
						this.DecisionMaker,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return this.GameController.StartCoroutine(drawCR);
					}
					else
					{
						this.GameController.ExhaustCoroutine(drawCR);
					}
					break;
				case 1:
					// Destroy an environment card.
					IEnumerator destroyOngoingCR = this.GameController.SelectAndDestroyCard(
						this.DecisionMaker,
						new LinqCardCriteria((Card c) => c.IsEnvironment && c.IsInPlay, "environment"),
						false,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return this.GameController.StartCoroutine(destroyOngoingCR);
					}
					else
					{
						this.GameController.ExhaustCoroutine(destroyOngoingCR);
					}
					break;
				case 2:
					// Move a card from a trash to the top of its deck.
					List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
					List<SelectCardDecision> selectCardDecision = new List<SelectCardDecision>();

					// select the card
					IEnumerator selectCardCR = this.GameController.SelectCardAndStoreResults(
						this.HeroTurnTakerController,
						SelectionType.MoveCardOnDeck,
						new LinqCardCriteria(
							(Card c) => c.IsInTrash
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

					List<MoveCardDestination> list = new List<MoveCardDestination>
					{
						new MoveCardDestination(GetSelectedCard(selectCardDecision).NativeDeck)
					};

					// move the card
					IEnumerator moveCardCR = this.GameController.MoveCard(
						this.TurnTakerController,
						selectCardDecision.FirstOrDefault().SelectedCard,
						list.FirstOrDefault().Location,
						doesNotEnterPlay: true,
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
			}
		}

		protected LinqCardCriteria IsCharmCriteria(Func<Card, bool> additionalCriteria = null)
		{
			var result = new LinqCardCriteria(c => IsCharm(c), "charm", true);
			if (additionalCriteria != null)
			{
				result = new LinqCardCriteria(result, additionalCriteria);
			}
				
			return result;
		}

		protected bool IsCharm(Card card, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
		{
			return card != null && base.GameController.DoesCardContainKeyword(card, "charm", evenIfUnderCard, evenIfFaceDown);
		}
	}
}