using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class SequenceBreakCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Whenever a Hero target would be dealt damage by a non-hero card,
		 *  you may redirect it to {Speedrunner}. If you do, one player may draw a card.
		 * When this card is destroyed, one player may play a card.
		 * At the start of your turn, destroy this card.
		 */

		public SequenceBreakCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever a Hero target would be dealt damage by a non-hero card, you may redirect it to {Speedrunner}.
			// If you do, one player may draw a card.
			AddTrigger(
				(DealDamageAction dd) =>
					dd.Target.IsHero
					&& !dd.DamageSource.IsHero
					&& dd.Amount > 0,
				RedirectResponse,
				TriggerType.RedirectDamage,
				TriggerTiming.Before,
				ActionDescription.DamageTaken
			);

			// When this card is destroyed, one player may play a card.
			AddWhenDestroyedTrigger(
				(DestroyCardAction dca) => SelectHeroToPlayCard(DecisionMaker),
				TriggerType.PlayCard
			);

			// At the start of your turn, destroy this card.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction pca) => GameController.DestroyCard(
					DecisionMaker,
					this.Card,
					cardSource: GetCardSource()
				),
				TriggerType.DestroySelf
			);

			base.AddTriggers();
		}

		private IEnumerator RedirectResponse(DealDamageAction dda)
		{
			var storedYesNo = new List<YesNoCardDecision> { };
			IEnumerator yesOrNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.RedirectDamage,
				dda.Target,
				action: dda,
				storedResults: storedYesNo,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesOrNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesOrNoCR);
			}

			if (DidPlayerAnswerYes(storedYesNo))
			{
				// If you do so...
				IEnumerator redirectCR = RedirectDamage(
					dda,
					TargetType.All,
					(Card c) => c == this.CharacterCard
				);
				IEnumerator drawCR = GameController.SelectHeroToDrawCard(
					DecisionMaker,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
					GameController.ExhaustCoroutine(drawCR);
				}
			}

			yield break;
		}
	}
}