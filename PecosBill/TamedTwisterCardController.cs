using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class TamedTwisterCardController : FolkBaseCardController
	{
		/*
		 * at the end of your turn, you may discard a card.
		 * If you do, play a card.
		 * 
		 * When this card would be destroyed,
		 * destroy all [u]hyperbole[/u] cards next to it instead and restore it to 5 HP.
		 * Otherwise, {PecosBill} deals himself 2 psychic damage, then destroy this card.
		 */

		public TamedTwisterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override IEnumerator DiscardRewardResponse()
		{
			// If you do, play a card.
			IEnumerator playCardCR = SelectAndPlayCardFromHand(this.HeroTurnTakerController);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCardCR);
			}

			yield break;
		}
	}
}