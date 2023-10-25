using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class LaserTurretCardController : CardController
	{
		/*
		 * at the start of the environment turn,
		 * this card deals the 2 hero targets with the highest HP {H} energy damage.
		 * 
		 * when this card would deal damage to a hero target,
		 * you may remove a token from a zone card's bias pool.
		 * if you do, redirect that damage to another target.
		 */

		public LaserTurretCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			this.AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			// at the start of the environment turn...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				// ...this card deals the 2 hero targets with the highest HP {H} energy damage.
				(PhaseChangeAction pca) => DealDamageToHighestHP(
					this.Card,
					1,
					(Card c) => IsHeroTarget(c),
					(Card c) => H,
					DamageType.Energy,
					numberOfTargets: () => 2
				),
				TriggerType.DealDamage
			);

			// when this card would deal damage to a hero target...
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsCard
					&& dd.DamageSource.Card == this.Card
					&& IsHeroTarget(dd.Target),
				TokenResponse,
				TriggerType.ModifyTokens,
				TriggerTiming.Before,
				ActionDescription.DamageTaken
			);

			base.AddTriggers();
		}

		private IEnumerator TokenResponse(DealDamageAction dd)
		{
			// ...you may remove a token from a zone card's bias pool.
			List<RemoveTokensFromPoolAction> tokenResults = new List<RemoveTokensFromPoolAction>();
			List<SelectCardDecision> zoneResults = new List<SelectCardDecision>();

			IEnumerator selectCardCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.RemoveTokens,
				new LinqCardCriteria(
					(Card c) => c.IsInPlayAndNotUnderCard
						&& c.DoKeywordsContain("zone")
						&& c.FindTokenPool("bias") != null
						&& c.FindTokenPool("bias").CurrentValue > 0,
					"zone cards with bias tokens"
				),
				zoneResults,
				true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCardCR);
			}

			if (DidSelectCard(zoneResults))
			{
				IEnumerator removeTokensCR = GameController.RemoveTokensFromPool(
					zoneResults.FirstOrDefault().SelectedCard.FindTokenPool("bias"),
					1,
					tokenResults,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokensCR);
				}
			}

			// if you do...
			if (DidRemoveTokens(tokenResults))
			{
				// ...redirect that damage to another target.
				IEnumerator redirectCR = RedirectDamage(
					dd,
					TargetType.SelectTarget,
					(Card c) => c.IsTarget
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
				}
			}

			yield break;
		}
	}
}