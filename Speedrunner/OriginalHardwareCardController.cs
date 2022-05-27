using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	/*
	 * Reduce damage dealt to {Speedrunner} by 1.
	 * 
	 * POWER
	 * Discard a glitch or strat card.
	 * Search your deck for a card with the chosen keyword and 
	 *  either put it in your hand or into play, then shuffle your deck.
	 */

	public class OriginalHardwareCardController : SpeedrunnerBaseCardController
	{
		public OriginalHardwareCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Reduce damage dealt to {Speedrunner} by 1.

			this.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Discard a glitch or strat card.

			// Search your deck for a card with the chosen keyword and
			// either put it in your hand or into play, then shuffle your deck.

			yield break;
		}
	}
}