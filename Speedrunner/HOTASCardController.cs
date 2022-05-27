using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class HOTASCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card is destroyed
		 *  destroy 1 ongoing card and
		 *  {Speedrunner} deals up to 2 targets 3 fire damage each.
		 * 
		 * POWER
		 * {Speedrunner} deals each non-hero target 1 projectile damage.
		 * You may destroy this card.
		 */

		public HOTASCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// When this card is destroyed...

			this.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction dca)
		{
			// ...destroy 1 ongoing card...

			// ...and {Speedrunner} deals up to 2 targets 3 fire damage each.

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// {Speedrunner} deals each non-hero target 1 projectile damage.

			// You may destroy this card.

			yield break;
		}
	}
}