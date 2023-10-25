using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class ToxinDistillerCardController : NexusEquipmentCardController
	{
		/*
		 * the first time each turn {Nexus} deals cold damage to a target,
		 * she also deals that target 1 toxic damage.
		 * 
		 * POWER
		 * until the end of your next turn, each time {Nexus} deals toxic damage to a target,
		 * reduce the next damage dealt by that target by 1.
		 * while [i]Pyrokinesis[/i] is in play, this applies to cold and fire damage as well.
		 */

		public ToxinDistillerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Cold, DamageType.Toxic)
		{
		}

		protected bool PyroInPlay => FindCardsWhere(c => c.Identifier == "Pyrokinesis" && c.IsInPlayAndHasGameText).Any();

		public override IEnumerator UsePower(int index = 0)
		{
			int reduceNumeral = GetPowerNumeral(0, 1);
			int[] powerNumerals = new int[1] { reduceNumeral };

			// until the end of your next turn, each time {Nexus} deals toxic damage to a target,
			OnDealDamageStatusEffect oddse = new OnDealDamageStatusEffect(
				CardWithoutReplacements,
				"ReduceDamageResponse",
				"Whenever " + TurnTaker.Name + " deals certain types of damage, reduce the next damage dealt by that target by " + reduceNumeral,
				new TriggerType[1] {TriggerType.AddStatusEffectToDamage},
				TurnTaker,
				this.Card,
				powerNumerals
			);
			oddse.SourceCriteria.IsSpecificCard = this.CharacterCard;
			oddse.CanEffectStack = true;
			oddse.UntilEndOfNextTurn(this.TurnTaker);
			oddse.DoesDealDamage = true;

			IEnumerator addEffectCR = AddStatusEffect(oddse);
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

		public IEnumerator ReduceDamageResponse(
			DealDamageAction dda,
			HeroTurnTaker htt,
			StatusEffect effect,
			int[] powerNumerals = null
		)
		{
			if (dda.DidDestroyTarget)
			{
				yield break;
			}

			int reduceNumeral = 1;
			if (powerNumerals != null)
			{
				reduceNumeral = powerNumerals.ElementAtOrDefault(0);
			}

			if (dda.DamageType == DamageType.Toxic || (
				// while [i]Pyrokinesis[/i] is in play, this applies to cold and fire damage as well.
				PyroInPlay && (dda.DamageType == DamageType.Cold || dda.DamageType == DamageType.Fire)
			))
			{
				// reduce the next damage dealt by that target by 1.
				ReduceDamageStatusEffect rdse = new ReduceDamageStatusEffect(reduceNumeral);
				rdse.SourceCriteria.IsSpecificCard = dda.Target;
				rdse.NumberOfUses = 1;
				rdse.UntilTargetLeavesPlay(dda.Target);

				IEnumerator addEffectCR = AddStatusEffect(rdse);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addEffectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addEffectCR);
				}
			}

			yield break;
		}
	}
}