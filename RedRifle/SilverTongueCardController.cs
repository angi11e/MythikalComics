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
		private int? _reduceAmount;

		public SilverTongueCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			_reduceDamage = null;
			SpecialStringMaker.ShowTokenPool(TrueshotPool);
		}

		public override bool AllowFastCoroutinesDuringPretend { get => TrueshotPool.CurrentValue < 1; }

		public override IEnumerator Play()
		{
			// When this card enters play, add 2 tokens to your trueshot pool.
			IEnumerator addTokensCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 2);

			// Each hero target gains 1 HP.
			IEnumerator healingCR = GameController.GainHP(
				DecisionMaker,
				(Card c) => IsHero(c),
				1,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addTokensCR);
				yield return GameController.StartCoroutine(healingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addTokensCR);
				GameController.ExhaustCoroutine(healingCR);
			}

			yield break;
		}

		public override void AddTriggers()
		{
			// Whenever a villain target deals a non-villain target damage,
			_reduceDamage = AddTrigger(
				(DealDamageAction dd) => 
					!IsVillainTarget(dd.Target)
					&& dd.DamageSource.IsVillainTarget
					&& dd.Amount > 0
					&& !dd.IsIrreducible
					&& TrueshotPool.CurrentValue > 0,
				(DealDamageAction dd) => ReduceDamageResponse(dd),
				new TriggerType[] {TriggerType.ModifyTokens, TriggerType.ReduceDamage},
				TriggerTiming.Before,
				isActionOptional: false
			);
			base.AddTriggers();
		}

		private IEnumerator ReduceDamageResponse(DealDamageAction dd)
		{
			// you may remove any number of tokens from your trueshot pool.
			if (GameController.PretendMode || _reduceAmount == null)
			{
				int maxTokens = TrueshotPool.CurrentValue < dd.Amount ? TrueshotPool.CurrentValue : dd.Amount;

				List<Card> associatedCards = new List<Card> { dd.Target, dd.DamageSource.Card };
				SelectNumberDecision numbers = new SelectNumberDecision(
					GameController,
					DecisionMaker,
					SelectionType.RemoveTokens,
					0,
					maxTokens,
					associatedCards: associatedCards,
					cardSource: GetCardSource()
				);
				IEnumerator numbersCR = GameController.MakeDecisionAction(numbers);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(numbersCR);
				}
				else
				{
					GameController.ExhaustCoroutine(numbersCR);
				}

				_reduceAmount = numbers?.SelectedNumber ?? maxTokens;
			}

			int tokensRemoved = _reduceAmount.GetValueOrDefault(0);
			if (tokensRemoved > 0)
			{
				IEnumerator removeTokensCR = RedRifleTrueshotPoolUtility.RemoveTrueshotTokens<GameAction>(this, tokensRemoved);

				IEnumerator reduceDamageCR = GameController.ReduceDamage(
					dd,
					tokensRemoved,
					_reduceDamage,
					GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokensCR);
					yield return GameController.StartCoroutine(reduceDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokensCR);
					GameController.ExhaustCoroutine(reduceDamageCR);
				}
			}

			if (!GameController.PretendMode)
			{
				_reduceAmount = null;
			}

			yield break;
		}
	}
}