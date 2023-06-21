using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class HealingSpritzCardController : PatinaBaseCardController
	{
		/*
		 * increase HP recovery caused by your cards and Powers by 1.
		 * 
		 * POWER
		 * 1 target regains 1 HP. The next time that target is dealt damage, they regain 2 HP.
		 */

		public HealingSpritzCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// increase HP recovery caused by your cards and Powers by 1.
			AddTrigger(
				(GainHPAction hp) => hp.CardSource != null && (
					hp.CardSource.Card.Owner == this.TurnTaker
					|| (hp.CardSource.PowerSource != null
					&& hp.CardSource.PowerSource.TurnTakerController == this.TurnTakerController)
				),
				(GainHPAction hp) => GameController.IncreaseHPGain(hp, 1, GetCardSource()),
				new TriggerType[2]
				{
					TriggerType.IncreaseHPGain,
					TriggerType.ModifyHPGain
				},
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int healNumeral = GetPowerNumeral(1, 1);
			int moreHealNumeral = GetPowerNumeral(2, 2);

			// 1 target regains 1 HP.
			List<GainHPAction> storedResults = new List<GainHPAction>();
			IEnumerator firstGainCR = GameController.SelectAndGainHP(
				DecisionMaker,
				healNumeral,
				numberOfTargets: targetNumeral,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(firstGainCR);
			}
			else
			{
				GameController.ExhaustCoroutine(firstGainCR);
			}

			if (storedResults.Any())
			{
				for (int i = 0; i < storedResults.Count; i++)
				{
					// The next time that target is dealt damage, they regain 2 HP.
					Card healedTarget = storedResults[i].HpGainer;
					OnDealDamageStatusEffect damageEffect = new OnDealDamageStatusEffect(
						CardWithoutReplacements,
						"HealResponse",
						$"The next time that {healedTarget.Title} is dealt damage, they regain {moreHealNumeral} HP.",
						new TriggerType[] {TriggerType.GainHP},
						DecisionMaker.TurnTaker,
						this.Card
					);
					damageEffect.TargetCriteria.IsSpecificCard = healedTarget;
					damageEffect.TargetLeavesPlayExpiryCriteria.Card = healedTarget;
					damageEffect.NumberOfUses = moreHealNumeral;
					damageEffect.DamageAmountCriteria.GreaterThan = 0;
					damageEffect.DoesDealDamage = true;
					damageEffect.BeforeOrAfter = BeforeOrAfter.After;

					IEnumerator addStatusCR = AddStatusEffect(damageEffect);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(addStatusCR);
					}
					else
					{
						GameController.ExhaustCoroutine(addStatusCR);
					}
				}
			}

			yield break;
		}

		public IEnumerator HealResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
		{
			int moreHealNumeral = effect.NumberOfUses ?? 2;
			if (GameController.IsCardInPlayAndNotUnderCard(this.Card.Identifier))
			{
				// the base card's healing boost doesn't seem to affect this extra heal? STRANGE
				moreHealNumeral++;
			}
			IEnumerator healingCR = GameController.GainHP(
				dd.Target,
				moreHealNumeral,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healingCR);
			}

			GameController.StatusEffectManager.RemoveStatusEffect(effect);
			yield break;
		}
	}
}