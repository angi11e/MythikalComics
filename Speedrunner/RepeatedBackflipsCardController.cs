using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class RepeatedBackflipsCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Draw a card. Discard a card. Play a card.
		 * Draw a card. Discard a card. Play a card.
		 * One player other than {Speedrunner} may use a power now.
		 */

		public RepeatedBackflipsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Draw a card.

			// Discard a card.
			
			// Play a card.

			// One player other than {Speedrunner} may use a power now.

			yield break;
		}
	}
}