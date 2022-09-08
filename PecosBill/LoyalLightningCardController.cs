using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class LoyalLightningCardController : FolkBaseCardController
	{
		/*
		 * at the end of your turn, you may discard a card.
		 * If you do, [i]Loyal Lightning[/i] deals 1 target 2 lightning damage.
		 * 
		 * When this card would be destroyed,
		 * destroy all [u]hyperbole[/u] cards next to it instead and restore it to 5 HP.
		 * Otherwise, {PecosBill} deals himself 2 psychic damage, then destroy this card.
		 */

		public LoyalLightningCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override IEnumerator DiscardRewardResponse()
		{
			// If you do, [i]Loyal Lightning[/i] deals 1 target 2 lightning damage.
			IEnumerator strikeCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.Card),
				2,
				DamageType.Lightning,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(strikeCR);
			}
			else
			{
				GameController.ExhaustCoroutine(strikeCR);
			}

			yield break;
		}
	}
}