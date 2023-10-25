using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class MudslideWarningCardController : NexusOneShotCardController
	{
		/*
		 * {Nexus} deals 1 target 2 melee damage and 1 different target 2 cold damage, in either order.
		 * 
		 * Destroy 1 ongoing card.
		 */

		public MudslideWarningCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Melee, DamageType.Cold)
		{
		}

		public override IEnumerator Play()
		{
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(base.Play());
			}
			else
			{
				GameController.ExhaustCoroutine(base.Play());
			}

			// Destroy 1 ongoing card.
			IEnumerator destroyOngoingCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => IsOngoing(c) && c.IsInPlayAndHasGameText
				),
				1,
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyOngoingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyOngoingCR);
			}

			yield break;
		}
	}
}