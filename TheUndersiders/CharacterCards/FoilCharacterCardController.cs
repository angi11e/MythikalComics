using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class FoilCharacterCardController : TheUndersidersVillainCardController
	{
		public FoilCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHeroTargetWithHighestHP().Condition = () => !this.Card.IsFlipped;
			SpecialStringMaker.ShowHeroTargetWithLowestHP().Condition = () => !this.Card.IsFlipped;
		}

		public override void AddSideTriggers()
		{
			if (!this.Card.IsFlipped)
			{
				// Damage dealt by {Foil} is irreducible.
				AddSideTrigger(AddMakeDamageIrreducibleTrigger(
					(DealDamageAction dd) => dd.DamageSource.IsSameCard(this.Card)
				));

				// At the end of the villain turn, {Foil} deals the hero target with the most HP {H - 2} melee damage, and the hero target with the least HP 1 projectile damage.",
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					DoHighLowDamage,
					TriggerType.DealDamage
				));

				// Treat {Blade} effects as active. (this is done by the cards)
			}
			else
			{
				// Damage dealt by villain character targets is irreducible.
				AddSideTrigger(AddMakeDamageIrreducibleTrigger(
					(DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card.IsVillainCharacterCard
				));
			}
			base.AddSideTriggers();
		}

		private IEnumerator DoHighLowDamage(PhaseChangeAction p)
		{
			IEnumerator hitHighestCR = DealDamageToHighestHP(
				this.Card,
				1,
				(Card c) => IsHeroTarget(c),
				(Card c) => H - 2,
				DamageType.Melee
			);
			IEnumerator hitLowestCR = DealDamageToLowestHP(
				this.Card,
				1,
				(Card c) => IsHeroTarget(c),
				(Card c) => 1,
				DamageType.Projectile
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(hitHighestCR);
				yield return GameController.StartCoroutine(hitLowestCR);
			}
			else
			{
				GameController.ExhaustCoroutine(hitHighestCR);
				GameController.ExhaustCoroutine(hitLowestCR);
			}
		}
	}
}
