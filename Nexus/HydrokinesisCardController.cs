using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class HydrokinesisCardController : NexusOngoingCardController
	{
		/*
		 * increase cold and toxic damage by 1.
		 * 
		 * whenever {Nexus} deals cold damage to a target,
		 * 1 target regains 2 HP.
		 */

		public HydrokinesisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card,
			turnTakerController,
			DamageType.Cold,
			DamageType.Toxic,
			new TriggerType[] { TriggerType.GainHP }
		)
		{
		}

		protected override IEnumerator BaseDamageRewardResponse(DealDamageAction dd)
		{
			// 1 target regains 2 HP.
			IEnumerator healCR = GameController.SelectAndGainHP(
				HeroTurnTakerController,
				2,
				numberOfTargets: 1,
				requiredDecisions: 1,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healCR);
			}

			yield break;
		}
	}
}