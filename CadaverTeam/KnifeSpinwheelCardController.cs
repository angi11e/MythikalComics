using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class KnifeSpinwheelCardController : CardController
	{
		public KnifeSpinwheelCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHighestHP(1, () => 2, new LinqCardCriteria(
				(Card c) => c.IsHero
			));
		}

		public override IEnumerator Play()
		{
			// {CadaverTeam} deals the hero target with the highest HP {H - 1} projectile damage.
			IEnumerator cadaverDamageCR = DealDamageToHighestHP(
				this.CharacterCard,
				1,
				(Card c) => c.IsHero,
				(Card c) => Game.H - 1,
				DamageType.Projectile
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cadaverDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cadaverDamageCR);
			}

			// [i]Lynne[/i] deals the hero target with the second highest HP {H - 1} projectile damage.
			Card lynne = TurnTaker.GetCardByIdentifier("Lynne");
			if (lynne != null && lynne.IsInPlayAndNotUnderCard)
			{
				IEnumerator lynneDamageCR = DealDamageToHighestHP(
					lynne,
					2,
					(Card c) => c.IsHero,
					(Card c) => Game.H - 1,
					DamageType.Projectile
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(lynneDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(lynneDamageCR);
				}
			}

			yield break;
		}
	}
}
