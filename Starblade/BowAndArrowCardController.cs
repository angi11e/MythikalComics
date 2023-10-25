using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class BowAndArrowCardController : StarbladeConstructCardController
	{
		/*
		 * whenever {Starblade} deals a non-hero target melee damage,
		 * this card deals that target 2 projectile damage.
		 * 
		 * TECHNIQUE
		 * this card deals 1 target 2 projectile damage.
		 */

		public BowAndArrowCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// whenever {Starblade} deals a non-hero target melee damage,
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DidDealDamage
					&& dd.DamageSource.IsCard
					&& dd.DamageSource.Card == this.CharacterCard
					&& !IsHeroTarget(dd.Target)
					&& dd.DamageType == DamageType.Melee,
				// this card deals that target 2 projectile damage.
				(DealDamageAction dd) => DealDamage(
					this.Card,
					dd.Target,
					2,
					DamageType.Projectile,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		public override IEnumerator ActivateTechnique()
		{
			// this card deals 1 target 2 projectile damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.Card),
				2,
				DamageType.Projectile,
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