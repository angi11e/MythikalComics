using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class NoClippingCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Destroy 1 environment card.
		 * If you do so, {Speedrunner} regains 2 HP.
		 * If not, play the top card of the environment deck, then you may use a power.
		 */

		public NoClippingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Destroy 1 environment card.

			// If you do so, {Speedrunner} regains 2 HP.

			// If not, play the top card of the environment deck...

			// ...then you may use a power.

			yield break;
		}
	}
}