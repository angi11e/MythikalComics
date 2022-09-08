using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class ShakeTheSnakeCardController : FolkBaseCardController
	{
		/*
		 * at the end of your turn, you may discard a card.
		 * If you do, draw 2 cards.
		 * 
		 * When this card would be destroyed,
		 * destroy all [u]hyperbole[/u] cards next to it instead and restore it to 5 HP.
		 * Otherwise, {PecosBill} deals himself 2 psychic damage, then destroy this card.
		 */

		public ShakeTheSnakeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override IEnumerator DiscardRewardResponse()
		{
			// If you do, draw 2 cards.
			IEnumerator drawCardCR = DrawCards(this.HeroTurnTakerController, 2);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCardCR);
			}

			yield break;
		}
	}
}