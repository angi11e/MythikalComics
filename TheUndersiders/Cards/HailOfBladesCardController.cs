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
			SpecialStringMaker.ShowVillainCharacterCardWithHighestHP();
			SpecialStringMaker.ShowHeroTargetWithHighestHP(1, H - 1);

			SpecialStringMaker.ShowSpecialString(() => GetSpecialStringIcons("bear", "blade"));
		}

		public override IEnumerator Play()
		{
			// Bear: Targets dealt damage by the main text of this card cannot deal
			//  damage until the start of the villain turn.

			// The villain character card with the highest HP deals the {H - 1}
			//  hero targets with the highest HP {H - 2} projectile damage.
			List<DealDamageAction> storedResults = new List<DealDamageAction>();
			IEnumerator mainDamageCR = DealDamageToHighestHP(
				null,
				1,
				(Card c) => IsHeroTarget(c),
				(Card c) => H - 2,
				DamageType.Projectile,
				storedResults: storedResults,
				numberOfTargets: () => H - 1,
				addStatusEffect: (DealDamageAction dd) => IsEnabled("bear")
					? TargetsDealtDamageCannotDealDamageUntilTheStartOfNextTurnResponse(dd)
					: DoNothing(),
				damageSourceInfo: new TargetInfo(
					HighestLowestHP.HighestHP,
					1,
					1,
					new LinqCardCriteria((Card c) => IsVillainTarget(c), "The villain target with the highest HP")
				)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(mainDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(mainDamageCR);
			}

			// Blade: {FoilCharacter} deals each hero character card not dealt damage this turn 2 projectile damage.
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

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(findVillainCR);
					}
					else
					{
						GameController.ExhaustCoroutine(findVillainCR);
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
						(Card c) => !affectedList.Contains(c) && IsHeroCharacterCard(c),
						2,
						DamageType.Projectile,
						isIrreducible: true
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(foilDamage);
					}
					else
					{
						GameController.ExhaustCoroutine(foilDamage);
					}
				}
			}

			yield break;
		}
	}
}
