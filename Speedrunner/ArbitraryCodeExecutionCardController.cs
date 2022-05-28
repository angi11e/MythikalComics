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
			AddWhenDestroyedTrigger(DestructionResponse, new TriggerType[1] { TriggerType.PlayCard });

			this.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction dca)
		{
			// ...first play each card from under it, in any order.

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int extraNumeral = GetPowerNumeral(0, 1);

			// Put 1 card from your hand under this card.

			// Destroy this card.
			IEnumerator destroyCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			yield break;
		}
	}
}