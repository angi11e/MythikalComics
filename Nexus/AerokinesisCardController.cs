using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class AerokinesisCardController : NexusOngoingCardController
	{
		/*
		 * increase projectile and lightning damage by 1.
		 * 
		 * whenever {Nexus} deals projectile damage to a target,
		 * 1 player may draw a card.
		 */

		public AerokinesisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(
			card,
			turnTakerController,
			DamageType.Projectile,
			DamageType.Lightning,
			new TriggerType[] { TriggerType.DrawCard }
		)
		{
		}

		protected override IEnumerator BaseDamageRewardResponse(DealDamageAction dd)
		{
			// 1 player may draw a card.
			IEnumerator drawCardCR = GameController.SelectHeroToDrawCard(
				DecisionMaker,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardCR);
			}

			yield break;
		}
	}
}