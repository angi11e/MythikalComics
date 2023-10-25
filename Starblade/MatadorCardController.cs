using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class MatadorCardController : CardController
	{
		/*
		 * damage dealt by {Starblade} and by construct cards is irreducible.
		 * 
		 * you may use a power.
		 */

		public MatadorCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// damage dealt by {Starblade} and by construct cards is irreducible.
			ITrigger irreducible = AddMakeDamageIrreducibleTrigger(
				(DealDamageAction dda) => dda.DamageSource.IsCard
					&& (dda.DamageSource.Card == this.CharacterCard || dda.DamageSource.Card.IsConstruct)
			);

			// you may use a power.
			IEnumerator usePowerCR = GameController.SelectAndUsePower(
				DecisionMaker,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(usePowerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(usePowerCR);
			}

			RemoveTrigger(irreducible);

			yield break;
		}
	}
}