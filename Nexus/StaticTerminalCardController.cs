using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class StaticTerminalCardController : NexusEquipmentCardController
	{
		/*
		 * the first time each turn {Nexus} deals projectile damage to a target,
		 * she also deals that target 1 lightning damage.
		 * 
		 * POWER
		 * {Nexus} deals each target 2 lightning damage.
		 * if [i]Lithokinesis[/i] is in play, reduce damage dealt to hero targets this way to 0.
		 */

		public StaticTerminalCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Projectile, DamageType.Lightning)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			ITrigger reduceTrigger = null;
			int damageNumeral = GetPowerNumeral(0, 2);
			int reduceNumeral = GetPowerNumeral(1, 0);

			// if [i]Lithokinesis[/i] is in play
			if (FindCardsWhere(c => c.Identifier == "Lithokinesis" && c.IsInPlayAndHasGameText).Any())
			{
				// reduce damage dealt to hero targets this way to 0.
				reduceTrigger = AddTrigger(
					(DealDamageAction dd) =>
						dd.CardSource.Card == this.Card
						&& IsHeroTarget(dd.Target)
						&& dd.CanDealDamage
						&& dd.Amount > reduceNumeral,
					(DealDamageAction dd) => GameController.ReduceDamage(
						dd,
						dd.Amount - reduceNumeral,
						reduceTrigger,
						GetCardSource()
					),
					new TriggerType[1] { TriggerType.ReduceDamageOneUse },
					TriggerTiming.Before
				);
			}

			// {Nexus} deals each target 2 lightning damage.
			IEnumerator damageCR = DealDamage(
				this.CharacterCard,
				(Card c) => c.IsTarget,
				damageNumeral,
				DamageType.Lightning
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			if (reduceTrigger != null)
			{
				RemoveTrigger(reduceTrigger);
			}

			yield break;
		}
	}
}