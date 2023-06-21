using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Theurgy
{
	public class TheurgyCharacterCardController : TheurgyBaseCharacterCardController
	{
		public TheurgyCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController) {
			SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Hand, IsCharmCriteria());
		}

		public override IEnumerator UsePower(int index = 0)
		{
			if (HeroTurnTaker.GetCardsWhere((Card c) => c.Location == HeroTurnTaker.Hand && IsCharm(c)).Count() > 0)
			{
				// Draw a card or play a charm card. One hero target regains 1 hp.
				List<Function> functionList = new List<Function>();

				// first draw a card option
				functionList.Add(
					new Function(
						DecisionMaker,
						"Draw a card",
						SelectionType.DrawCard,
						() => GameController.DrawCards(DecisionMaker, 1)
					)
				);

				// ...or play a charm card option
				functionList.Add(
					new Function(
						DecisionMaker,
						"Play a charm card",
						SelectionType.PlayCard,
						() => SelectAndPlayCardFromHand(
							DecisionMaker,
							false,
							null,
							IsCharmCriteria()
						)
					)
				);

				// ask for which one
				SelectFunctionDecision selectFunction = new SelectFunctionDecision(
					GameController,
					DecisionMaker,
					functionList,
					false,
					cardSource: GetCardSource()
				);

				IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(selectFunction);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(selectFunctionCR);
				}
				else
				{
					GameController.ExhaustCoroutine(selectFunctionCR);
				}
			}
			else
			{
				IEnumerator drawCardCR = GameController.DrawCards(DecisionMaker, 1);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCardCR);
				}
			}

			// One hero target regains 1 HP.
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = FindCardsWhere(
				new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && IsHeroTarget(c))
			);
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				DecisionMaker,
				choices,
				selectedTarget,
				optional: true,
				selectionType: SelectionType.GainHP,
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

			if (selectedTarget != null && selectedTarget.Any())
			{
				SelectTargetDecision selectedTargetDecision = selectedTarget.FirstOrDefault();
				if (selectedTargetDecision != null && selectedTargetDecision.SelectedCard != null)
				{
					var healHP = GetPowerNumeral(0, 1);
					IEnumerator healTargetCR = GameController.GainHP(
						selectedTarget.FirstOrDefault().SelectedCard,
						healHP,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(healTargetCR);
					}
					else
					{
						GameController.ExhaustCoroutine(healTargetCR);
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
					IEnumerator drawCR = GameController.SelectHeroToDrawCard(
						DecisionMaker,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(drawCR);
					}
					else
					{
						GameController.ExhaustCoroutine(drawCR);
					}
					break;
				case 1:
					// Destroy an environment card.
					IEnumerator destroyOngoingCR = GameController.SelectAndDestroyCard(
						DecisionMaker,
						new LinqCardCriteria((Card c) => c.IsEnvironment && c.IsInPlay, "environment"),
						false,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroyOngoingCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroyOngoingCR);
					}
					break;
				case 2:
					// Move a card from a trash to the top of its deck.
					List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
					List<SelectCardDecision> selectCardDecision = new List<SelectCardDecision>();

					// select the card
					IEnumerator selectCardCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.MoveCardOnDeck,
						new LinqCardCriteria(
							(Card c) => c.IsInTrash
							&& GameController.IsLocationVisibleToSource(c.Location, GetCardSource())
						),
						selectCardDecision,
						false
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectCardCR);
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
					IEnumerator moveCardCR = GameController.MoveCard(
						TurnTakerController,
						selectCardDecision.FirstOrDefault().SelectedCard,
						list.FirstOrDefault().Location,
						doesNotEnterPlay: true,
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
					break;
			}
		}
	}
}