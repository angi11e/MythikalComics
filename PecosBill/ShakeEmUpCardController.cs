using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class ShakeEmUpCardController : HyperboleBaseCardController
	{
		/*
		 * Play this card next to [i]Shake the Snake[/i].
		 * If [i]Shake the Snake[/i] is ever not in play, destroy this card.
		 * 
		 * When exactly 1 damage would be dealt by a villain target,
		 * redirect that damage to that target.
		 * 
		 * TALL TALE
		 * Until the end of the turn, increase damage dealt by 1.
		 */

		public ShakeEmUpCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "ShakeTheSnake")
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When exactly 1 damage would be dealt by a villain target, redirect that damage to that target.
			AddTrigger(
				(DealDamageAction dda) => dda.Amount == 1 && dda.DamageSource.IsTarget && dda.DamageSource.IsVillain,
				(DealDamageAction dda) => GameController.RedirectDamage(
					dda,
					dda.DamageSource.Card,
					cardSource: GetCardSource()
				),
				TriggerType.RedirectDamage,
				TriggerTiming.Before
			);
		}

		public override IEnumerator ActivateTallTale()
		{
			// Until the end of the turn, increase damage dealt by 1.
			IncreaseDamageStatusEffect increaseDamageSE = new IncreaseDamageStatusEffect(1);
			increaseDamageSE.UntilThisTurnIsOver(Game);
			IEnumerator addEffectCR = AddStatusEffect(increaseDamageSE);

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
	}
}