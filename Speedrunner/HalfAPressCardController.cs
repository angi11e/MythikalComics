using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class HalfAPressCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When you move a card to your trash, place it under this card instead.
		 * When you use a power or draw a card, add 1 token to this card.
		 * When this card is destroyed,
		 *  {Speedrunner} deals 1 target X projectile damage,
		 *  and that target deals {Speedrunner} Y radiant damage,
		 *  where X = the number of cards under this one,
		 *  and Y = the number of tokens on this card.
		 *  
		 * POWER
		 * Destroy this card.
		 */

		public HalfAPressCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// When you move a card to your trash, place it under this card instead.

			// When you use a power or draw a card, add 1 token to this card.

			// When this card is destroyed...

			this.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction dca)
		{
			// where X = the number of cards under this one,

			// and Y = the number of tokens on this card.

			// {Speedrunner} deals 1 target X projectile damage,

			// and that target deals {Speedrunner} Y radiant damage,

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Destroy this card.

			yield break;
		}
	}
}