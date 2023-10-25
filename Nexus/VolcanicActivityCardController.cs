using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class VolcanicActivityCardController : NexusOneShotCardController
	{
		/*
		 * {Nexus} deals 1 target 2 melee damage and 1 different target 2 fire damage, in either order.
		 * 
		 * Destroy 1 environment card.
		 */

		public VolcanicActivityCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Melee, DamageType.Fire)
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

			// Destroy 1 environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				1,
				false,
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