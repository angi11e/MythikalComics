using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class ArbitraryCodeExecutionCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card enters play, put one one-shot card from each trash under it.
		 * When this card is destroyed, first play each card from under it, in any order.
		 * 
		 * POWER
		 * Put 1 card from your hand under this card. Destroy this card.
		 */

		public ArbitraryCodeExecutionCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// When this card enters play, put one one-shot card from each trash under it.

			yield break;
		}

		public override void AddTriggers()
		{
			// When this card is destroyed...

			this.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction dca)
		{
			// ...first play each card from under it, in any order.

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Put 1 card from your hand under this card.
			
			// Destroy this card.
			
			yield break;
		}
	}
}