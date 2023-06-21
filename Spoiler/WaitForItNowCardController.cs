using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class WaitForItNowCardController : SpoilerOneshotCardController
	{
		public WaitForItNowCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) =>
				IsEquipment(c) && c.Location == HeroTurnTaker.PlayArea, "equipment"
			));
			SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) =>
				IsOngoing(c) && c.Location == HeroTurnTaker.PlayArea, "ongoing"
			));
		}

		public override IEnumerator Play()
		{
			// ...by the number of equipment cards in your play area.
			int dealtByNumeral = FindCardsWhere((Card c) =>
				c.IsInPlayAndHasGameText
				&& IsEquipment(c)
				&& c.Location == HeroTurnTaker.PlayArea
			).Count();

			if (dealtByNumeral > 0)
			{
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
					// increase the next damage dealt [i]by[/i] that target...
					IncreaseDamageStatusEffect idSE = new IncreaseDamageStatusEffect(dealtByNumeral);
					idSE.NumberOfUses = 1;
					idSE.SourceCriteria.IsSpecificCard = selected.SelectedCard;
					idSE.UntilTargetLeavesPlay(selected.SelectedCard);
					IEnumerator increaseStatusCR = AddStatusEffect(idSE);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(increaseStatusCR);
					}
					else
					{
						GameController.ExhaustCoroutine(increaseStatusCR);
					}
				}
			}

			// ...by the number of ongoing cards in your play area.
			int dealtToNumeral = FindCardsWhere((Card c) =>
				c.IsInPlayAndHasGameText
				&& c.Location == HeroTurnTaker.PlayArea
				&& IsOngoing(c)
			).Count();

			if (dealtByNumeral > 0)
			{
				// Select a target.
				List<SelectCardDecision> cardSelection = new List<SelectCardDecision>();
				IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
					DecisionMaker,
					SelectionType.SelectTarget,
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
					// increase the next damage dealt [i]to[/i] that target...
					IncreaseDamageStatusEffect idSE = new IncreaseDamageStatusEffect(dealtToNumeral);
					idSE.NumberOfUses = 1;
					idSE.TargetCriteria.IsSpecificCard = selected.SelectedCard;
					idSE.UntilTargetLeavesPlay(selected.SelectedCard);
					IEnumerator increaseStatusCR = AddStatusEffect(idSE);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(increaseStatusCR);
					}
					else
					{
						GameController.ExhaustCoroutine(increaseStatusCR);
					}
				}
			}

			// You may discard a card. If you do, activate a [u]rewind[/u] text.
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(DiscardToRewind());
			}
			else
			{
				GameController.ExhaustCoroutine(DiscardToRewind());
			}

			yield break;
		}
	}
}