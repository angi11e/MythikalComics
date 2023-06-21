using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class DoubleEnhancerGlovesCardController : CaptainCainEquipmentCardController
	{
		/*
		 * increase damage dealt by {CaptainCainCharacter} by 1.
		 * 
		 * 👊: increase the next damage dealt by {CaptainCainCharacter} by 2.
		 *     Play 1 card.
		 * 
		 * 💧: the next time {CaptainCainCharacter} would deal damage to a hero target,
		 *    that target regains that many HP instead.
		 */

		public DoubleEnhancerGlovesCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// increase damage dealt by {CaptainCainCharacter} by 1.
			AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.CharacterCard), 1);

			base.AddTriggers();
		}

		protected override IEnumerator FistPower()
		{
			int damageNumeral = GetPowerNumeral(0, 2);
			int playNumeral = GetPowerNumeral(1, 1);

			// increase the next damage dealt by {CaptainCainCharacter} by 2.
			IncreaseDamageStatusEffect effect = new IncreaseDamageStatusEffect(damageNumeral);
			effect.SourceCriteria.IsSpecificCard = this.CharacterCard;
			effect.NumberOfUses = 1;
			effect.UntilTargetLeavesPlay(this.CharacterCard);
			// effect.CardDestroyedExpiryCriteria.Card = this.CharacterCard;
			IEnumerator increaseDamageCR = AddStatusEffect(effect);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(increaseDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(increaseDamageCR);
			}

			// Play 1 card.
			IEnumerator playCardsCR = GameController.SelectAndPlayCardsFromHand(
				DecisionMaker,
				playNumeral,
				false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCardsCR);
			}

			yield break;
		}

		protected override IEnumerator BloodPower()
		{
			// the next time {CaptainCainCharacter} would deal damage to a hero target,
			OnDealDamageStatusEffect effect = new OnDealDamageStatusEffect(
				this.CardWithoutReplacements,
				"HealInsteadResponse",
				$"the next hero target dealt damage by {this.CharacterCard.Title} regains that many hp instead.",
				new TriggerType[] { TriggerType.WouldBeDealtDamage },
				null,
				this.Card
			);
			effect.BeforeOrAfter = BeforeOrAfter.Before;
			effect.NumberOfUses = 1;
			effect.TargetCriteria.IsHero = true;
			effect.SourceCriteria.IsSpecificCard = this.CharacterCard;
			effect.UntilTargetLeavesPlay(this.CharacterCard);
			IEnumerator addEffectCR = AddStatusEffect(effect);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addEffectCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addEffectCR);
			}

			yield break;
		}

		public IEnumerator HealInsteadResponse(
			DealDamageAction dd,
			HeroTurnTaker hero,
			StatusEffect effect,
			int[] powerNumerals = null
		)
		{
			// that target regains that many HP instead.
			IEnumerator cancelCR = CancelAction(
				dd,
				isPreventEffect: true
			);
			IEnumerator healingCR = GameController.GainHP(
				dd.Target,
				dd.Amount,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cancelCR);
				yield return GameController.StartCoroutine(healingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cancelCR);
				GameController.ExhaustCoroutine(healingCR);
			}

			yield break;
		}
	}
}