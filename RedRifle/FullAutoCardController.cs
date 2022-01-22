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
		 * At the start of your turn, add 2 tokens to your trueshot pool.
		 * At the end of your turn, if you have fewer than 5 tokens in your trueshot pool, destroy this card.
		 * 
		 * POWER
		 * Remove any number of tokens from your trueshot pool.
		 * For each token you removed, you may increase one of the numerals in this power by one.
		 * {RedRifle} deals up to 1 target 1 projectile damage.
		 */

		public FullAutoCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowTokenPool(base.TrueshotPool);
		}

		public override void AddTriggers()
		{
			// At the start of your turn, add 2 tokens to your trueshot pool.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				(PhaseChangeAction p) => RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 2),
				TriggerType.AddTokensToPool
			);

			// At the end of your turn, if you have fewer than 5 tokens in your trueshot pool, destroy this card.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker && base.TrueshotPool.CurrentValue < 5,
				(PhaseChangeAction p) => GameController.DestroyCard(
					this.DecisionMaker,
					base.Card,
					cardSource: GetCardSource()
				),
				TriggerType.DestroySelf
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetCount = GetPowerNumeral(0, 1);
			int extraTargets = 0;
			int damageAmount = GetPowerNumeral(1, 1);
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

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(noTokensCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(noTokensCR);
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

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(howManyCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(howManyCR);
				}

				tokensRemoved = tokensToRemove.FirstOrDefault()?.SelectedNumber ?? 0;
			}

			// For each token removed, increase a numeral in this power by one.
			if (tokensRemoved > 0)
			{
				IEnumerator removeTokensCR = RedRifleTrueshotPoolUtility.RemoveTrueshotTokens<GameAction>(this, tokensRemoved);

				List<SelectNumberDecision> extraTargetsToHit = new List<SelectNumberDecision>();
				IEnumerator extraTargetsCR = GameController.SelectNumber(
					DecisionMaker,
					SelectionType.MakeTarget,
					0,
					tokensRemoved,
					storedResults: extraTargetsToHit,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(removeTokensCR);
					yield return base.GameController.StartCoroutine(extraTargetsCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(removeTokensCR);
					base.GameController.ExhaustCoroutine(extraTargetsCR);
				}

				extraTargets = extraTargetsToHit.FirstOrDefault()?.SelectedNumber ?? 0;
				if (extraTargets > 0)
				{
					targetCount += extraTargets;
				}

				if (extraTargets < tokensRemoved)
				{
					damageAmount += (tokensRemoved - extraTargets);
				}
			}

			// {RedRifle} deals up to 1 target 1 projectile damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, base.CharacterCard),
				damageAmount,
				DamageType.Projectile,
				targetCount,
				false,
				0,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}
	}
}