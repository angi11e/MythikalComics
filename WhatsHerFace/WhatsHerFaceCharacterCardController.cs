using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.WhatsHerFace
{
	public class WhatsHerFaceCharacterCardController : WhatsHerFaceBaseCharacterCardController
	{
		/*
		 * Reveal the top card of the villain deck.
		 *  Discard it or put it into play.
		 *  If you put it into play, you may play a [u]recall[/u] card now.
		 */

		public WhatsHerFaceCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Reveal the top card of the villain deck.
			List<SelectLocationDecision> villainDecks = new List<SelectLocationDecision>();
			IEnumerator findVillainCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.RevealTopCardOfDeck,
				(Location l) => l.IsVillain,
				villainDecks,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(findVillainCR);
			}
			else
			{
				GameController.ExhaustCoroutine(findVillainCR);
			}

			SelectLocationDecision villainDeck = villainDecks.FirstOrDefault();

			if (villainDeck != null && villainDeck.SelectedLocation.Location != null)
			{
				List<MoveCardAction> revealResults = new List<MoveCardAction>();

				// Discard it or put it into play.
				IEnumerator revealCR = RevealCard_PlayItOrDiscardIt(
					DecisionMaker,
					villainDeck.SelectedLocation.Location,
					true,
					storedResults: revealResults,
					showRevealedCards: true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
				}

				// If you put it into play, you may play a [u]recall[/u] card now.
				if (revealResults.FirstOrDefault() != null && !revealResults.FirstOrDefault().IsDiscard)
				{
					IEnumerator playRecallCR = SelectAndPlayCardFromHand(
						HeroTurnTakerController,
						false,
						null,
						IsRecallCriteria()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playRecallCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playRecallCR);
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
					// One Player may draw a card.
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
					// Destroy an ongoing card.
					IEnumerator destroyOngoingCR = GameController.SelectAndDestroyCard(
						DecisionMaker,
						new LinqCardCriteria((Card c) => c.IsOngoing && c.IsInPlay, "ongoing"),
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
					// Select a target. Reduce the next damage dealt to that target by 2.
					List<SelectCardDecision> storedCard = new List<SelectCardDecision>();
					IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.ReduceNextDamageTaken,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && c.IsTarget,
							"target",
							useCardsSuffix: false
						),
						storedCard,
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

					SelectCardDecision selectCardDecision = storedCard.FirstOrDefault();
					if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
					{
						ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(2);
						reduceDamageStatusEffect.NumberOfUses = 1;
						reduceDamageStatusEffect.TargetCriteria.IsSpecificCard = selectCardDecision.SelectedCard;
						IEnumerator reduceCR = AddStatusEffect(reduceDamageStatusEffect);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(reduceCR);
						}
						else
						{
							GameController.ExhaustCoroutine(reduceCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}