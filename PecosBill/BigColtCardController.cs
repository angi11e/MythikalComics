using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class BigColtCardController : PecosBillBaseCardController
	{
		/*
		 * When this card is destroyed, {PecosBill} deals 1 target 2 projectile damage.
		 * 
		 * POWER
		 * {PecosBill} deals 1 target 3 Projectile damage.
		 */

		public BigColtCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When this card is destroyed, {PecosBill} deals 1 target 2 projectile damage.
			AddWhenDestroyedTrigger(
				(DestroyCardAction dc) => GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					2,
					DamageType.Projectile,
					1,
					false,
					1,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage
			);
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 3);

			// {PecosBill} deals 1 target 3 Projectile damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Projectile,
				targetNumeral,
				false,
				targetNumeral,
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