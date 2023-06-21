using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class NotRoundMeYouDontCardController : PecosBillBaseCardController
	{
		/*
		 * When a non-hero target enters play, {PecosBill} may deal that target 1 melee damage.
		 * 
		 * POWER
		 * Until the start of your next turn,
		 * when a hero target would be dealt damage
		 * you may redirect it to a target in this play area.
		 */

		public NotRoundMeYouDontCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When a non-hero target enters play, {PecosBill} may deal that target 1 melee damage.
			AddTargetEntersPlayTrigger(
				(Card c) => !IsHeroTarget(c),
				(Card c) => DealDamage(
					this.CharacterCard,
					c,
					1,
					DamageType.Melee,
					optional: true
				),
				TriggerType.DealDamage,
				TriggerTiming.After
			);
		}

		public override IEnumerator UsePower(int index = 0)
		{
			RedirectDamageStatusEffect redirectSE = new RedirectDamageStatusEffect()
			{
				// when a hero target would be dealt damage...
				TargetCriteria = { IsHero = true },
				// ...you may redirect it to a target in this play area.
				IsOptional = true,
				RedirectableTargets = { IsPlayAreaOf = this.TurnTaker }
			};

			// Until the start of your next turn,
			redirectSE.UntilStartOfNextTurn(this.TurnTaker);
			redirectSE.UntilCardLeavesPlay(this.CharacterCard);

			IEnumerator addEffectCR = AddStatusEffect(redirectSE);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addEffectCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addEffectCR);
			}
			yield break;
		}
	}
}