using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class ResetWarpCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Discard your hand. Draw 5 cards. Play 1 card. {Speedrunner} regains 2 HP.
		 */

		public ResetWarpCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Discard your hand.

			// Draw 5 cards.

			// Play 1 card.

			// {Speedrunner} regains 2 HP.

			yield break;
		}
	}
}