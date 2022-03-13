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
	public class CoordinatedBlitzCardController : TheUndersidersBaseCardController
	{
		public CoordinatedBlitzCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			// Blade: Damage done while this card resolves is irreducible.
			if (IsEnabled("blade"))
			{
				MakeDamageIrreducibleStatusEffect effect = new MakeDamageIrreducibleStatusEffect();
				effect.UntilCardLeavesPlay(base.Card);
				effect.UntilEndOfPhase(TurnTaker, Phase.End);
				effect.CreateImplicitExpiryConditions();
				IEnumerator makeIrreducibleCR = AddStatusEffect(effect);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(makeIrreducibleCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(makeIrreducibleCR);
				}
			}

			// Each villain character card deals the non-villain target with the highest HP 2 projectile damage, one at a time.
			IEnumerator mainDamageCR = MultipleDamageSourcesDealDamage(
				new LinqCardCriteria((Card c) => c.IsVillainCharacterCard && !c.IsFlipped && c.IsInPlayAndNotUnderCard),
				TargetType.HighestHP,
				1,
				new LinqCardCriteria((Card c) => !c.IsVillain, "non-villain"),
				2,
				DamageType.Projectile
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(mainDamageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(mainDamageCR);
			}

			// Dog: Each dog, plush, and swarm card deals the non-villain target with the highest HP 1 toxic damage, one at a time.
			if (IsEnabled("dog"))
			{
				IEnumerator petDamageCR = MultipleDamageSourcesDealDamage(
					new LinqCardCriteria((Card c) =>
						c.IsInPlayAndHasGameText && c.IsVillain
						&& (c.DoKeywordsContain("dog") || c.DoKeywordsContain("plush") || c.DoKeywordsContain("swarm"))
					),
					TargetType.HighestHP,
					1,
					new LinqCardCriteria((Card c) => !c.IsVillain, "non-villain"),
					1,
					DamageType.Toxic
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(petDamageCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(petDamageCR);
				}
			}

			yield break;
		}
	}
}
