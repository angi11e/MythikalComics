using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class RNGManipulationCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When a villain card would be played, you may play a card from under this card instead.
		 * At the start of your turn, if there are no cards under this card,
		 *  one villain target deals themself 2 melee damage, then destroy this card.
		 *  
		 * POWER
		 * Move 1 card from the villain trash or the top card of the villain deck under this card.
		 */

		public RNGManipulationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// When a villain card would be played, you may play a card from under this card instead.

			// At the start of your turn, if there are no cards under this card...

			this.AddTriggers();
		}

		private IEnumerator NoCardsResponse()
		{
			// one villain target deals themself 2 melee damage...

			// ...then destroy this card.

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// ...from the villain trash or...

			// ...the top card of the villain deck

			// Move 1 card... ...under this card.

			yield break;
		}
	}
}