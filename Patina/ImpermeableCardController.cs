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
		 * When {Patina} would be dealt damage of a type reduced by this card, you may destroy this card.
		 *  If you do, you may redirect that Damage to a Target of your choice.
		 */

		public ImpermeableCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria((Card c) => !c.IsOneShot));
			this.AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			// Reduce cold, energy, fire, and radiant damage dealt to hero targets by the number of water cards in play.
			Func<DealDamageAction, int> amountToDecrease = (DealDamageAction dd) => WaterCardsInPlay;

			AddReduceDamageTrigger(
				(DealDamageAction dd) => IsHeroTarget(dd.Target) && (
					dd.DamageType == DamageType.Cold
					|| dd.DamageType == DamageType.Energy
					|| dd.DamageType == DamageType.Radiant
					|| dd.DamageType == DamageType.Fire
				),
				amountToDecrease
			);

			// When {Patina} would be dealt damage of a type reduced by this card...
			AddTrigger(
				(DealDamageAction dd) =>
					dd.Target == this.CharacterCard
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
			// ...you may destroy this card.
			List<DestroyCardAction> actions = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				optional: true,
				actions,
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

			// If you do...
			if (actions.Any() && actions.FirstOrDefault().WasCardDestroyed)
			{
				GameController.AddCardControllerToList(CardControllerListType.CanCauseDamageOutOfPlay, this);
				GameController.AddInhibitorException(this, (GameAction g) => true);

				// ...you may redirect that Damage to a Target of your choice.
				IEnumerator redirectCR = RedirectDamage(
					dd,
					TargetType.SelectTarget,
					(Card c) => c.IsTarget
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
				}

				GameController.RemoveCardControllerFromList(CardControllerListType.CanCauseDamageOutOfPlay, this);
				GameController.RemoveInhibitorException(this);
			}

			yield break;
		}
	}
}