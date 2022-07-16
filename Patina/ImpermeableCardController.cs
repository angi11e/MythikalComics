using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class ImpermeableCardController : PatinaBaseCardController
	{
		/*
		 * Reduce cold, energy, fire, and radiant damage dealt to hero targets by the number of water cards in play.
		 * 
		 * At the end of your turn, {Patina} deals up to 2 targets 1 fire damage each.
		 * 
		 * When {Patina} would be dealt damage of a type reduced by this card, you may destroy this card.
		 *  If you do, you may redirect that Damage to a Target of your choice.
		 */

		public ImpermeableCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria());
			this.AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			// Reduce cold, energy, fire, and radiant damage dealt to hero targets by the number of water cards in play.
			Func<DealDamageAction, int> amountToDecrease = (DealDamageAction dd) => FindCardsWhere(
				(Card c) => c.IsInPlayAndHasGameText && IsWater(c)
			).Count();

			AddReduceDamageTrigger(
				(DealDamageAction dd) => dd.Target.IsHero && (
					dd.DamageType == DamageType.Cold
					|| dd.DamageType == DamageType.Energy
					|| dd.DamageType == DamageType.Radiant
					|| dd.DamageType == DamageType.Fire
				),
				amountToDecrease
			);

			// At the end of your turn, {Patina} deals up to 2 targets 1 fire damage each.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				(PhaseChangeAction p) => GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, base.CharacterCard),
					1,
					DamageType.Fire,
					2,
					false,
					0,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage
			);

			// When {Patina} would be dealt damage of a type reduced by this card...
			AddTrigger(
				(DealDamageAction dd) =>
					dd.Target == base.CharacterCard
					&& dd.Amount > 0
					&& (
						dd.DamageType == DamageType.Cold
						|| dd.DamageType == DamageType.Energy
						|| dd.DamageType == DamageType.Radiant
						|| dd.DamageType == DamageType.Fire
				),
				RedirectResponse,
				TriggerType.RedirectDamage,
				TriggerTiming.Before,
				ActionDescription.DamageTaken
			);

			base.AddTriggers();
		}

		private IEnumerator RedirectResponse(DealDamageAction dd)
		{
			// ... you may destroy this card.
			var storedYesNo = new List<YesNoCardDecision> { };
			IEnumerator yesOrNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.RedirectDamageDirectedAtTarget,
				dd.Target,
				action: dd,
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
				// If you do, you may redirect that Damage to a Target of your choice.
				IEnumerator redirectCR = RedirectDamage(
					dd,
					TargetType.SelectTarget,
					(Card c) => c.IsTarget
				);
				IEnumerator destroyCR = GameController.DestroyCard(
					DecisionMaker,
					base.Card,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
					GameController.ExhaustCoroutine(destroyCR);
				}
			}

			yield break;
		}
	}
}