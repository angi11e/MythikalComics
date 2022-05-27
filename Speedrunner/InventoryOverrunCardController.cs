using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class InventoryOverrunCardController : SpeedrunnerBaseCardController
	{
		/*
		 * Each player reveals cards from the top of their deck until an equipment card is revealed.
		 * They put that card in their hand and shuffle the other revealed cards back into their deck.
		 * One player may play a card now.
		 */

		public InventoryOverrunCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Each player reveals cards from the top of their deck until an equipment card is revealed.

			// They put that card in their hand and shuffle the other revealed cards back into their deck.

			// One player may play a card now.

			yield break;
		}
	}
}