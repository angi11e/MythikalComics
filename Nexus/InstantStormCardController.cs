using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class InstantStormCardController : NexusOneShotCardController
	{
		/*
		 * {Nexus} deals 1 target 2 projectile damage and 1 different target 2 cold damage, in either order.
		 * 
		 * Up to 5 targets regain 1 HP each.
		 */

		public InstantStormCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Projectile, DamageType.Cold)
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

			// Up to 5 targets regain 1 HP each.
			IEnumerator healCR = GameController.SelectAndGainHP(
				DecisionMaker,
				1,
				false,
				(Card c) => true,
				5,
				0,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healCR);
			}

			yield break;
		}
	}
}