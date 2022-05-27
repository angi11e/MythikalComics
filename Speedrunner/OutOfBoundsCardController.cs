using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class OutOfBoundsCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card enters play, move the top card of the environment deck under it.
		 * Then {Speedrunner} deals 1 target 2 psychic damage.
		 * 
		 * POWER
		 * Swap the locations of an environment card in play with a card under this card.
		 * Then {Speedrunner} deals 1 target 2 psychic damage.
		 */

		public OutOfBoundsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, move the top card of the environment deck under it.

			// Then {Speedrunner} deals 1 target 2 psychic damage.
		
			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Swap the locations of an environment card in play with a card under this card.

			// Then {Speedrunner} deals 1 target 2 psychic damage.
			
			yield break;
		}
	}
}