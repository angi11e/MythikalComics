using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class FullAutoCardController : RedRifleBaseCardController
	{
		/*
		 * At the start of your turn, add 1 token to your trueshot pool.
		 * At the end of your turn, if you have fewer than 5 tokens in your trueshot pool, destroy this card.
		 * 
		 * POWER
		 * Remove any number of tokens from your trueshot pool.
		 * {RedRifle} deals up to that many targets that much Projectile damage, evenly distributed.
		 */

		public FullAutoCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowTokenPool(TrueshotPool);
		}

		public override void AddTriggers()
		{
			// At the start of your turn, add 1 token to your trueshot pool.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction p) => RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 1),
				TriggerType.AddTokensToPool
			);

			// At the end of your turn, if you have fewer than 5 tokens in your trueshot pool, destroy this card.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker && TrueshotPool.CurrentValue < 5,
				(PhaseChangeAction p) => GameController.DestroyCard(
					this.DecisionMaker,
					this.Card,
					cardSource: GetCardSource()
				),
				TriggerType.DestroySelf
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int totalTargets = 0;
			int lowDamageTargets = 0;
			int highDamageTargets = 0;
			int damageValue = 0;
			int tokensRemoved = 0;
			string message = null;
			TokenPool trueshotPool = RedRifleTrueshotPoolUtility.GetTrueshotPool(this);

			// Remove any number of tokens from your trueshot pool.
			if (trueshotPool == null)
			{
				message = "There is no trueshot pool to remove tokens from.";
			}
			else if (trueshotPool.CurrentValue == 0)
			{
				message = $"There are no tokens in {trueshotPool.Name} to remove.";
			}
			if (message != null)
			{
				IEnumerator noTokensCR = GameController.SendMessageAction(
					message,
					Priority.Medium,
					GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(noTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(noTokensCR);
				}
			}
			else
			{
				List<SelectNumberDecision> tokensToRemove = new List<SelectNumberDecision>();
				IEnumerator howManyCR = GameController.SelectNumber(
					DecisionMaker,
					SelectionType.RemoveTokens,
					0,
					trueshotPool.CurrentValue,
					storedResults: tokensToRemove,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(howManyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(howManyCR);
				}

				tokensRemoved = tokensToRemove.FirstOrDefault()?.SelectedNumber ?? 0;
			}

			// Deal up to that many targets that much Projectile damage, evenly distributed.
			if (tokensRemoved > 0)
			{
				IEnumerator removeTokensCR = RedRifleTrueshotPoolUtility.RemoveTrueshotTokens<GameAction>(this, tokensRemoved);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokensCR);
				}

				IEnumerator targetMessageCR = GameController.SendMessageAction(
					"Choose the number of targets to hit.",
					Priority.Medium,
					GetCardSource()
				);

				List<SelectNumberDecision> totalTargetsToHit = new List<SelectNumberDecision>();
				IEnumerator extraTargetsCR = GameController.SelectNumber(
					DecisionMaker,
					SelectionType.SelectNumeral,
					1,
					tokensRemoved,
					storedResults: totalTargetsToHit,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(targetMessageCR);
					yield return GameController.StartCoroutine(extraTargetsCR);
				}
				else
				{
					GameController.ExhaustCoroutine(targetMessageCR);
					GameController.ExhaustCoroutine(extraTargetsCR);
				}

				totalTargets = totalTargetsToHit.FirstOrDefault()?.SelectedNumber ?? 0;
				if (totalTargets > 0)
				{
					highDamageTargets = tokensRemoved % totalTargets;
					lowDamageTargets = totalTargets - highDamageTargets;
					damageValue = tokensRemoved / totalTargets;
				}
				else
				{
					yield break;
				}
			}

			// {RedRifle} deals up to that many targets that much Projectile damage, evenly distributed.
			List<SelectCardDecision> attacks = new List<SelectCardDecision>();
			if (highDamageTargets > 0)
			{
				IEnumerator dealHighDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					damageValue + 1,
					DamageType.Projectile,
					highDamageTargets,
					false,
					highDamageTargets,
					storedResultsDecisions: attacks,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealHighDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealHighDamageCR);
				}
			}
			if (lowDamageTargets > 0)
			{
				System.Func<Card, bool> criteria = null;
				if (attacks.Count() > 0)
				{
					criteria = (Card c) => !(from scd in attacks select scd.SelectedCard).Distinct().ToList().Contains(c);
				}

				IEnumerator dealLowDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					damageValue,
					DamageType.Projectile,
					lowDamageTargets,
					false,
					lowDamageTargets,
					additionalCriteria: criteria,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealLowDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealLowDamageCR);
				}
			}

			yield break;
		}
	}
}