using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class InputBufferCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card enters play, draw 2 cards.
		 * 
		 * POWER
		 * Discard any number of cards.
		 * Deal 1 target 2 energy damage, plus 1 for each card discarded this way.
		 */

		public InputBufferCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, draw 2 cards.

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Discard any number of cards.

			// ...plus 1 for each card discarded this way.
			
			// Deal 1 target 2 energy damage...
			
			yield break;
		}
	}
}