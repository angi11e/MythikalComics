using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class TwisterOfFateCardController : HyperboleBaseCardController
	{
		/*
		 * Play this card next to [i]Tamed Twister[/i].
		 * If [i]Tamed Twister[/i] is ever not in play, destroy this card.
		 * 
		 * When a target other than [i]Tamed Twister[/i] in this play area deals damage to a target,
		 * [i]Tamed Twister[/i] deals that target 1 projectile damage.
		 * 
		 * TALL TALE
		 * [i]Tamed Twister[/i] deals each non-hero target 1 projectile damage.
		 */

		public TwisterOfFateCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "TamedTwister")
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When a target other than [i]Tamed Twister[/i] in this play area deals damage to a target...
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsCard
					&& dd.DamageSource.Card.Identifier != "TamedTwister"
					&& dd.DamageSource.Card.IsInLocation(this.TurnTaker.PlayArea)
					&& dd.DidDealDamage,
				(DealDamageAction dd) => DealDamage(
					// ...[i]Tamed Twister[/i] deals that target 1 projectile damage.
					GetCardThisCardIsNextTo(),
					dd.Target,
					1,
					DamageType.Projectile,
					cardSource: GetCardSource()
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);
		}

		public override IEnumerator ActivateTallTale()
		{
			// [i]Tamed Twister[/i] deals each non-hero target 1 projectile damage.
			IEnumerator damageCR = DealDamage(
				GetCardThisCardIsNextTo(),
				(Card c) => !IsHeroTarget(c),
				1,
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

			yield break;
		}
	}
}