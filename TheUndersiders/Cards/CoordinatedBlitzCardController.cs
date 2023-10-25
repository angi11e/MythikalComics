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
			SpecialStringMaker.ShowNonVillainTargetWithHighestHP();
			SpecialStringMaker.ShowSpecialString(() => GetSpecialStringIcons("dog", "blade"));
		}

		public override IEnumerator Play()
		{
			// Blade: Damage on this card is irreducible.
			// (integrated into the deal damage CRs)

			// Each villain character card deals the non-villain target with the highest HP 2 projectile damage, one at a time.
			IEnumerator mainDamageCR = MultipleDamageSourcesDealDamage(
				new LinqCardCriteria((Card c) => c.IsVillainCharacterCard && !c.IsFlipped && c.IsTarget && c.IsInPlayAndNotUnderCard),
				TargetType.HighestHP,
				1,
				new LinqCardCriteria((Card c) => c.IsTarget && !IsVillainTarget(c), "non-villain"),
				2,
				DamageType.Projectile,
				IsEnabled("blade")
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(mainDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(mainDamageCR);
			}

			// Dog: Each dog, plush, and swarm card deals the non-villain target with the highest HP 1 toxic damage, one at a time.
			if (IsEnabled("dog"))
			{
				IEnumerator petDamageCR = MultipleDamageSourcesDealDamage(
					new LinqCardCriteria((Card c) =>
						c.IsInPlayAndHasGameText && IsVillainTarget(c)
						&& (c.DoKeywordsContain("dog") || c.DoKeywordsContain("plush") || c.DoKeywordsContain("swarm"))
					),
					TargetType.HighestHP,
					1,
					new LinqCardCriteria((Card c) => c.IsTarget && !IsVillainTarget(c), "non-villain"),
					1,
					DamageType.Toxic,
					IsEnabled("blade")
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(petDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(petDamageCR);
				}
			}

			yield break;
		}
	}
}
