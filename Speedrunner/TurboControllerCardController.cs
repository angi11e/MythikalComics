using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class TurboControllerCardController : SpeedrunnerBaseCardController
	{
		/*
		 * If you play a card outside of your play phase, you may draw a card.
		 * If you use a power outside of your power phase, {Speedrunner} deals 1 target 2 fire damage.
		 * 
		 * POWER
		 * {Speedrunner} deals 1 target 1 melee damage X times, where X = the number of your glitch and strat cards in play.
		 */

		public TurboControllerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// If you play a card outside of your play phase, you may draw a card.

			// If you use a power outside of your power phase, {Speedrunner} deals 1 target 2 fire damage.

			this.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// ...where X = the number of your glitch and strat cards in play.

			// {Speedrunner} deals 1 target 1 melee damage X times...

			yield break;
		}
	}
}