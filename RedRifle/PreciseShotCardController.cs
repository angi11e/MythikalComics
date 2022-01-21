using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class PreciseShotCardController : RedRifleBaseCardController
	{
		/*
		 * {RedRifle} deals 1 target X projectile damage,
		 *  where X equals the number of villain and environment targets in play,
		 *  to a maximum of the number of tokens in your trueshot pool.
		 * This damage cannot be redirected.
		 */

		public PreciseShotCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria(
				(Card c) => c.IsTarget && !c.IsHero,
				"villain and environment targets"
			));
			base.SpecialStringMaker.ShowTokenPool(base.TrueshotPool);
		}

		public override IEnumerator Play()
		{
			// where X equals the number of villain and environment targets in play,
			int nonHeroTargets = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.IsTarget && !c.IsHero).Count();

			// to a maximum of the number of tokens in your trueshot pool.
			if (base.TrueshotPool.CurrentValue < nonHeroTargets)
			{
				nonHeroTargets = base.TrueshotPool.CurrentValue;
			}

			// {RedRifle} deals 1 target X projectile damage,
			List<Card> allTheTargets = GameController.FindTargetsInPlay().ToList();
			List<SelectTargetDecision> storedResults = new List<SelectTargetDecision>();

			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				DecisionMaker,
				allTheTargets,
				storedResults,
				damageSource: base.CharacterCard,
				damageAmount: (Card c) => nonHeroTargets,
				damageType: DamageType.Projectile
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectTargetCR);
			}

			DealDamageAction dealDamageAction = new DealDamageAction(
				GetCardSource(),
				new DamageSource(GameController, base.CharacterCard),
				storedResults.FirstOrDefault().SelectedCard,
				nonHeroTargets,
				DamageType.Projectile
			);

			// This damage cannot be redirected.
			IEnumerator notRedirectableCR = DoAction(
				new MakeDamageNotRedirectableAction(
					GetCardSource(),
					dealDamageAction
				)
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(notRedirectableCR);
			}
			else
			{
				GameController.ExhaustCoroutine(notRedirectableCR);
			}

			IEnumerator dealDamageCR = DoAction(dealDamageAction);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}
	}
}