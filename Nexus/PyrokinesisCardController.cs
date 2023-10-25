using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class PyrokinesisCardController : NexusOngoingCardController
	{
		/*
		 * increase fire and radiant damage by 1.
		 * 
		 * whenever {Nexus} deals fire damage to a target,
		 * that target deals itself 1 fire damage.
		 */

		public PyrokinesisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card,
			turnTakerController,
			DamageType.Fire,
			DamageType.Radiant,
			new TriggerType[] { TriggerType.DealDamage }
		)
		{
		}

		protected override IEnumerator BaseDamageRewardResponse(DealDamageAction dd)
		{
			// that target deals itself 1 fire damage.
			if (!dd.DidDestroyTarget && dd.Target.IsInPlayAndNotUnderCard)
			{
				IEnumerator dealDamageCR = DealDamage(
					dd.Target,
					dd.Target,
					1,
					DamageType.Fire,
					cardSource: GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}
			}
			yield break;
		}
	}
}