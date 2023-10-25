using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class LithokinesisCardController : NexusOngoingCardController
	{
		/*
		 * increase melee and infernal damage by 1.
		 * 
		 * whenever {Nexus} deals melee damage to a target,
		 * discard the top card of that target's deck.
		 */

		public LithokinesisCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card,
			turnTakerController,
			DamageType.Melee,
			DamageType.Infernal,
			new TriggerType[] { TriggerType.DiscardCard }
		)
		{
		}

		protected override IEnumerator BaseDamageRewardResponse(DealDamageAction dd)
		{
			// discard the top card of that target's deck.
			IEnumerator discardCR = DiscardCardsFromTopOfDeck(
				FindTurnTakerController(dd.Target.Owner),
				1,
				responsibleTurnTaker: this.TurnTaker
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			yield break;
		}
	}
}