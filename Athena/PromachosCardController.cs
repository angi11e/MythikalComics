using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class PromachosCardController : AspectBaseCardController
	{
		/*
		 * When this card enters play, destroy any other [u]aspect[/u] cards.
		 * 
		 * Whenever a hero target is dealt damage, you may redirect it to {Athena}.
		 *  Reduce damage redirected to {Athena} by 1.
		 *  
		 * POWER
		 * {Athena} deals each target 1 sonic damage.
		 *  Destroy 1 environment card.
		 */

		public PromachosCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			/*
			// Reduce damage redirected to {Athena} by 1.
			AddReduceDamageTrigger(
				(DealDamageAction dd) => dd.Target == this.CharacterCard && dd.NumberOfTimesRedirected > 0,
				(DealDamageAction dd) => 1
			);
			*/

			// Whenever a hero target is dealt damage, you may redirect it to {Athena}.
			AddRedirectDamageTrigger(
				(DealDamageAction dd) => dd.Target != this.CharacterCard && IsHero(dd.Target),
				() => this.CharacterCard,
				optional: true
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int damageNumeral = GetPowerNumeral(0, 1);
			int destroyNumeral = GetPowerNumeral(1, 1);

			// {Athena} deals each non-hero target 1 sonic damage.
			IEnumerator damageCR = DealDamage(
				this.CharacterCard,
				(Card c) => c.IsTarget,
				damageNumeral,
				DamageType.Sonic
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			// Destroy 1 environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				destroyNumeral,
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