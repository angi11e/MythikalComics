using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class TASCardController : SpeedrunnerBaseCardController
	{
		/*
		 * increase damage dealt by {Speedrunner} by 1.
		 * 
		 * POWER
		 * One player draws 1 card, a second player plays 1 card, and a third player uses 1 power.
		 * Destroy this card.
		 */

		public TASCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// increase damage dealt by {Speedrunner} by 1.

			this.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// One player draws 1 card...

			// ...a second player plays 1 card...

			// ...and a third player uses 1 power.

			// Destroy this card.

			yield break;
		}
	}
}