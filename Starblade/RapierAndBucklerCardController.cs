using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class RapierAndBucklerCardController : StarbladeConstructCardController
	{
		/*
		 * reduce damage dealt to {Starblade} and construct cards by 1.
		 * 
		 * TECHNIQUE
		 * reduce the next damage dealt to a hero target by 2.
		 * {Starblade} deals 1 target 2 melee damage.
		 */

		public RapierAndBucklerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// reduce damage dealt to {Starblade} and construct cards by 1.
			AddReduceDamageTrigger((Card c) => c == this.CharacterCard || c.IsConstruct, 1);

			base.AddTriggers();
		}

		public override IEnumerator ActivateTechnique()
		{
			// reduce the next damage dealt to a hero target by 2.
			ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(2);
			reduceDamageStatusEffect.TargetCriteria.IsHero = true;
			reduceDamageStatusEffect.TargetCriteria.IsTarget = true;
			reduceDamageStatusEffect.NumberOfUses = 1;

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(AddStatusEffect(reduceDamageStatusEffect));
			}
			else
			{
				GameController.ExhaustCoroutine(AddStatusEffect(reduceDamageStatusEffect));
			}

			// {Starblade} deals 1 target 2 melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				2,
				DamageType.Melee,
				1,
				false,
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}
	}
}