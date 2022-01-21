using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class SilverTongueCardController : RedRifleBaseCardController
	{
		/*
		 * When this card enters play, add 2 tokens to your trueshot pool.
		 *  Each hero target gains 1 HP.
		 * 
		 * Whenever a villain target deals a non-villain target damage,
		 *  you may remove any number of tokens from your trueshot pool.
		 *  Reduce that damage by that amount.
		 */

		private ITrigger _reduceDamage;

		public SilverTongueCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowTokenPool(base.TrueshotPool);
		}

		public override IEnumerator Play()
		{
			// When this card enters play, add 2 tokens to your trueshot pool.
			IEnumerator addTokensCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 2);

			// Each hero target gains 1 HP.
			IEnumerator healingCR = GameController.GainHP(
				DecisionMaker,
				(Card c) => c.IsHero,
				1,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(addTokensCR);
				yield return base.GameController.StartCoroutine(healingCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(addTokensCR);
				base.GameController.ExhaustCoroutine(healingCR);
			}

			yield break;
		}

		public override void AddTriggers()
		{
			// Whenever a villain target deals a non-villain target damage,
			_reduceDamage = AddTrigger(
				(DealDamageAction dd) => 
					!dd.Target.IsVillain
					&& dd.DamageSource.IsVillain
					&& dd.Amount > 0
					&& !dd.IsIrreducible
					&& TrueshotPool.CurrentValue > 0,
				(DealDamageAction dd) => ReduceDamageResponse(dd),
				new TriggerType[] {TriggerType.ModifyTokens, TriggerType.ReduceDamage},
				TriggerTiming.Before,
				isActionOptional: true
			);
			base.AddTriggers();
		}

		private IEnumerator ReduceDamageResponse(DealDamageAction dd)
		{
			// you may remove any number of tokens from your trueshot pool.
			YesNoDecision yesNo = new YesNoDecision(
				GameController,
				DecisionMaker,
				SelectionType.ReduceDamageTaken,
				gameAction: dd,
				cardSource: GetCardSource()
			);
			IEnumerator yesNoCR = GameController.MakeDecisionAction(yesNo);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(yesNoCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(yesNoCR);
			}

			if (yesNo != null && yesNo.Answer.HasValue && yesNo.Answer.Value)
			{
				List<SelectNumberDecision> selectNumberDecisions = new List<SelectNumberDecision>();
				int maxTokens = TrueshotPool.CurrentValue < dd.Amount ? TrueshotPool.CurrentValue : dd.Amount;

				IEnumerator howManyCR = GameController.SelectNumber(
					DecisionMaker,
					SelectionType.RemoveTokens,
					1,
					maxTokens,
					storedResults: selectNumberDecisions,
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

				int tokensRemoved = selectNumberDecisions.FirstOrDefault()?.SelectedNumber ?? 0;

				// Reduce that damage by that amount.
				if (tokensRemoved > 0)
				{
					IEnumerator removeTokensCR = RedRifleTrueshotPoolUtility.RemoveTrueshotTokens<GameAction>(this, tokensRemoved);

					IEnumerator reduceDamageCR = GameController.ReduceDamage(
						dd,
						tokensRemoved,
						_reduceDamage,
						GetCardSource()
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(removeTokensCR);
						yield return base.GameController.StartCoroutine(reduceDamageCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(removeTokensCR);
						base.GameController.ExhaustCoroutine(reduceDamageCR);
					}
				}
			}

			yield break;
		}
	}
}