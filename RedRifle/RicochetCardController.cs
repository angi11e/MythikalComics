using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class RicochetCardController : RedRifleBaseCardController
	{
		/*
		 * Whenever {RedRifle} is dealt damage, add 1 token to your trueshot pool.
		 */

		public RicochetCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever {RedRifle} is dealt damage, add 1 token to your trueshot pool.
			AddTrigger(
				(DealDamageAction dd) => dd.DidDealDamage && dd.Target == this.CharacterCard,
				(DealDamageAction dd) => AddTrueshotTokens(1),
				TriggerType.AddTokensToPool,
				TriggerTiming.After
			);

			base.AddTriggers();
		}
	}
}