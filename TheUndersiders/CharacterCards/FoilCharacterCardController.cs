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
			base.SpecialStringMaker.ShowHeroTargetWithHighestHP().Condition = () => !base.Card.IsFlipped;
			base.SpecialStringMaker.ShowHeroTargetWithLowestHP().Condition = () => !base.Card.IsFlipped;
		}

		public override IEnumerator Play()
		{
			yield break;
		}

		public override void AddSideTriggers()
		{
			if (!base.Card.IsFlipped)
			{
				// Damage dealt by {Foil} is irreducible.
				AddSideTrigger(AddMakeDamageIrreducibleTrigger(
					(DealDamageAction dd) => dd.DamageSource.IsSameCard(base.Card)
				));

				// At the end of the villain turn, {Foil} deals the hero target with the most HP {H - 2} melee damage, and the hero target with the least HP 1 projectile damage.",
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					DoHighLowDamage,
					TriggerType.DealDamage
				));

				// Treat {Blade} effects as active. (this is done by the cards)
			}
			else
			{
				// Damage dealt by villain targets is irreducible.
				AddSideTrigger(AddMakeDamageIrreducibleTrigger(
					(DealDamageAction dd) => dd.DamageSource.Card.IsVillainTarget
				));
			}
			base.AddSideTriggers();
		}

		private IEnumerator DoHighLowDamage(PhaseChangeAction p)
		{
			IEnumerator hitHighestCR = DealDamageToHighestHP(
				base.Card,
				1,
				(Card c) => c.IsHero,
				(Card c) => base.Game.H - 2,
				DamageType.Melee
			);
			IEnumerator hitLowestCR = DealDamageToLowestHP(
				base.Card,
				1,
				(Card c) => c.IsHero,
				(Card c) => 1,
				DamageType.Projectile
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(hitHighestCR);
				yield return base.GameController.StartCoroutine(hitLowestCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(hitHighestCR);
				base.GameController.ExhaustCoroutine(hitLowestCR);
			}
		}
	}
}
