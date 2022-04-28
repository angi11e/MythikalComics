using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class TheonosisCardController : AspectBaseCardController
	{
		/*
		 * When this card enters play, destroy any other [u]aspect[/u] cards.
		 * 
		 * You may play an additional card during your play phase.
		 * 
		 * POWER
		 * Destroy 1 non-character hero card.
		 *  If you do so, destroy 1 ongoing card.
		 */

		public TheonosisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			GameController.AddCardControllerToList(CardControllerListType.IncreasePhaseActionCount, this);
		}

		public override IEnumerator Play()
		{
			IEnumerator basePlayCR = base.Play();

			IEnumerator morePlayCR = IncreasePhaseActionCountIfInPhase(
				(TurnTaker tt) => tt == base.TurnTaker,
				Phase.PlayCard,
				1
			);
			
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(basePlayCR);
				yield return GameController.StartCoroutine(morePlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(basePlayCR);
				GameController.ExhaustCoroutine(morePlayCR);
			}
		}

		public override void AddTriggers()
		{
			// You may play an additional card during your play phase.
			AddAdditionalPhaseActionTrigger(
				(TurnTaker tt) => ShouldIncreasePhaseActionCount(tt),
				Phase.PlayCard,
				1
			);

			base.AddTriggers();
		}

		private bool ShouldIncreasePhaseActionCount(TurnTaker tt)
		{
			return tt == base.TurnTaker;
		}

		public override bool AskIfIncreasingCurrentPhaseActionCount()
		{
			if (GameController.ActiveTurnPhase.IsPlayCard)
			{
				return ShouldIncreasePhaseActionCount(GameController.ActiveTurnTaker);
			}
			return false;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int destroyNumeral = GetPowerNumeral(0, 1);
			int ongoingNumeral = GetPowerNumeral(1, 1);

			// Destroy 1 non-character hero card.
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => c.IsHero && c.IsInPlayAndHasGameText && !c.IsCharacter,
					"non-character hero"
				),
				destroyNumeral,
				optional: false,
				storedResultsAction: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			// If you do so...
			if (DidDestroyCard(storedResults))
			{
				// ...destroy 1 ongoing card.
				IEnumerator destroyOngoingCR = GameController.SelectAndDestroyCards(
					DecisionMaker,
					new LinqCardCriteria(
						(Card c) => c.IsOngoing && c.IsInPlayAndHasGameText
					),
					ongoingNumeral,
					optional: false,
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
			}

			yield break;
		}
	}
}