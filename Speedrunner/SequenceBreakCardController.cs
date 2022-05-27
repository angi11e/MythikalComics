using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class SequenceBreakCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Whenever a Hero target would be dealt damage by a non-hero card,
		 *  you may redirect it to {Speedrunner}. If you do, one player may draw a card.
		 * When this card is destroyed, one player may play a card.
		 * At the start of your turn, destroy this card.
		 */

		public SequenceBreakCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever a Hero target would be dealt damage by a non-hero card, you may redirect it to {Speedrunner}.
			// If you do, one player may draw a card.

			// When this card is destroyed, one player may play a card.

			// At the start of your turn, destroy this card.

			this.AddTriggers();
		}
	}
}