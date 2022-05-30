using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Speedrunner
{
	public class HOTASCardController : SpeedrunnerBaseCardController
	{
		/*
		 * When this card is destroyed
		 *  destroy 1 ongoing card and
		 *  {Speedrunner} deals up to 2 targets 3 fire damage each.
		 * 
		 * POWER
		 * {Speedrunner} deals each non-hero target 1 projectile damage.
		 * You may destroy this card.
		 */

		public HOTASCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// When this card is destroyed...
			AddBeforeDestroyAction(DestructionResponse);

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(GameAction ga)
		{
			// ...destroy 1 ongoing card...
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing"),
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

			// ...and {Speedrunner} deals up to 2 targets 3 fire damage each.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				3,
				DamageType.Fire,
				2,
				false,
				0,
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

		public override IEnumerator UsePower(int index = 0)
		{
			int damageNumeral = GetPowerNumeral(0, 1);

			// {Speedrunner} deals each non-hero target 1 projectile damage.
			IEnumerator damageCR = DealDamage(
				this.CharacterCard,
				(Card c) => !c.IsHero,
				damageNumeral,
				DamageType.Projectile
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			// You may destroy this card.
			IEnumerator destroyCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				optional: true,
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