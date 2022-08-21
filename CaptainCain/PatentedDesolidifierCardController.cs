using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class PatentedDesolidifierCardController : CaptainCainEquipmentCardController
	{
		/*
		 * reduce damage dealt to {CaptainCainCharacter} by 1.
		 * 
		 * POWER 👊
		 * Destroy an environment card.
		 * 
		 * POWER 💧
		 * The next time damage would be dealt to {CaptainCainCharacter}, prevent it.
		 * Until then, {CaptainCainCharacter} cannot deal damage.
		 */

		public PatentedDesolidifierCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// reduce damage dealt to {CaptainCainCharacter} by 1.
			AddReduceDamageTrigger((Card c) => c == this.CharacterCard, 1);
		}

		protected override IEnumerator FistPower()
		{
			// Destroy an environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				false,
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

			yield break;
		}

		protected override IEnumerator BloodPower()
		{
			// The next time damage would be dealt to {CaptainCainCharacter}, prevent it.
			OnDealDamageStatusEffect preventDamageSE = new OnDealDamageStatusEffect(
				this.CardWithoutReplacements,
				"PreventAndCleanResponse",
				$"the next damage dealt to {this.CharacterCard.Title} will be prevented.",
				new TriggerType[] { TriggerType.WouldBeDealtDamage },
				null,
				this.Card
			);
			preventDamageSE.BeforeOrAfter = BeforeOrAfter.Before;
			preventDamageSE.NumberOfUses = 1;
			preventDamageSE.TargetCriteria.IsSpecificCard = this.CharacterCard;
			preventDamageSE.UntilTargetLeavesPlay(this.CharacterCard);
			IEnumerator addPreventCR = GameController.AddStatusEffect(preventDamageSE, true, GetCardSource());

			// Until then, {CaptainCainCharacter} cannot deal damage.
			CannotDealDamageStatusEffect cannotDealSE = new CannotDealDamageStatusEffect();
			cannotDealSE.SourceCriteria.IsSpecificCard = this.CharacterCard;
			cannotDealSE.UntilTargetLeavesPlay(this.CharacterCard);
			cannotDealSE.IsPreventEffect = true;
			IEnumerator addCannotCR = GameController.AddStatusEffect(cannotDealSE, true, GetCardSource());

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addPreventCR);
				yield return GameController.StartCoroutine(addCannotCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addPreventCR);
				GameController.ExhaustCoroutine(addCannotCR);
			}

			yield break;
		}

		public IEnumerator PreventAndCleanResponse(
			DealDamageAction dd,
			HeroTurnTaker hero,
			StatusEffect effect,
			int[] powerNumerals = null
		)
		{
			// ...prevent it.
			IEnumerator cancelCR = CancelAction(
				dd,
				isPreventEffect: true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cancelCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cancelCR);
			}

			// until then...
			List<StatusEffect> effects = GameController.StatusEffectControllers.Where(
				(StatusEffectController sec) =>
					sec.StatusEffect is CannotDealDamageStatusEffect cdds
					&& cdds.CardSource == this.Card
			).Select(sec => sec.StatusEffect).ToList();
			foreach (StatusEffect cannotDealSE in effects)
			{
				IEnumerator removeCannotCR = GameController.ExpireStatusEffect(cannotDealSE, GetCardSource());

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeCannotCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeCannotCR);
				}
			}

			yield break;
		}
	}
}