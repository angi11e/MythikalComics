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
	public class HailOfBladesCardController : TheUndersidersBaseCardController
	{
		public HailOfBladesCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			// Bear: Targets dealt damage by the results of this card cannot deal damage until the start of the villain turn.

			// The villain character card with the highest HP deals the {H - 1} hero targets with the highest HP {H - 2} projectile damage.
			List<DealDamageAction> storedResults = new List<DealDamageAction>();
			IEnumerator mainDamageCR = DealDamageToHighestHP(
				null,
				1,
				(Card c) => c.IsHero,
				(Card c) => base.H - 2,
				DamageType.Projectile,
				storedResults: storedResults,
				numberOfTargets: () => base.H - 1,
				addStatusEffect: (DealDamageAction dd) => IsEnabled("bear")
					? base.TargetsDealtDamageCannotDealDamageUntilTheStartOfNextTurnResponse(dd)
					: DoNothing(),
				damageSourceInfo: new TargetInfo(
					HighestLowestHP.HighestHP,
					1,
					1,
					new LinqCardCriteria((Card c) => c.IsVillainTarget, "The villain target with the highest HP")
				)
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(mainDamageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(mainDamageCR);
			}

			// Blade: {FoilCharacter} deals any hero targets not dealt damage this turn 2 projectile damage.
			if (IsEnabled("blade"))
			{
				Card maybeFoil = FoilCharacter;
				if (maybeFoil.IsFlipped)
				{
					List<Card> villainList = new List<Card>();
					IEnumerator findVillainCR = GameController.FindTargetWithHighestHitPoints(
						1,
						(Card c) => c.IsVillainCharacterCard,
						villainList,
						cardSource: GetCardSource()
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(findVillainCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(findVillainCR);
					}

					maybeFoil = villainList.FirstOrDefault();
				}

				if (maybeFoil.IsTarget)
				{
					List<Card> affectedList = GameController.Game.Journal.DealDamageEntriesThisTurn().Select(
						ddje => ddje.TargetCard
					).Distinct().ToList();

					IEnumerator foilDamage = DealDamage(
						maybeFoil,
						(Card c) => !affectedList.Contains(c) && c.IsHero,
						2,
						DamageType.Projectile,
						isIrreducible: true
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(foilDamage);
					}
					else
					{
						base.GameController.ExhaustCoroutine(foilDamage);
					}
				}
			}

			yield break;
		}
	}
}
