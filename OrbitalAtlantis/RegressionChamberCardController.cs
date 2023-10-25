using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class RegressionChamberCardController : CardController
	{
		/*
		 * each time a hero target would deal 3 or more damage to a villain target,
		 * you may reduce it by 2. if you do, add 1 token to this card's bias pool.
		 * 
		 * each time damage dealt by a villain target is prevented or reduced to 0,
		 * remove 1 token from this card's bias pool.
		 */

		private ITrigger _reduceDamage;

		public RegressionChamberCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			_reduceDamage = null;
			this.AllowFastCoroutinesDuringPretend = false;
		}

		public override void AddTriggers()
		{
			// each time a hero target would deal 3 or more damage to a villain target...
			_reduceDamage = AddTrigger(
				(DealDamageAction dd) =>
					IsVillainTarget(dd.Target)
					&& dd.DamageSource.IsCard
					&& IsHeroTarget(dd.DamageSource.Card)
					&& dd.Amount >= 3,
				ReduceResponse,
				TriggerType.ReduceDamage,
				TriggerTiming.Before
			);

			// each time damage dealt by a villain target is negated or reduced to 0...
			AddTrigger(
				(DealDamageAction dd) => CheckDamageCriteria(dd),
				RemoveTokenResponse,
				TriggerType.ReduceDamage,
				TriggerTiming.After
			);
			AddTrigger(
				(CancelAction c) =>
					c.ActionToCancel is DealDamageAction
					&& c.IsPreventEffect
					&& CheckDamageCriteria(c.ActionToCancel as DealDamageAction),
				(CancelAction c) => RemoveTokenResponse(c.ActionToCancel as DealDamageAction),
				TriggerType.ReduceDamage,
				TriggerTiming.After
			);

			AddBeforeLeavesPlayActions(ClearTokensResponse);

			base.AddTriggers();
		}

		private bool CheckDamageCriteria(DealDamageAction dd)
		{
			if (IsVillainTarget(dd.DamageSource.Card))
			{
				return WasDamageToTargetAvoided(dd, dd.Target);
			}
			return false;
		}

		private IEnumerator ReduceResponse(DealDamageAction dd)
		{
			if (!dd.IsIrreducible)
			{
				// ...you may reduce it by 2.
				List<YesNoCardDecision> yesOrNo = new List<YesNoCardDecision>();
				IEnumerator yesNoCR = GameController.MakeYesNoCardDecision(
					DecisionMaker,
					SelectionType.ReduceDamageTaken,
					dd.DamageSource.Card,
					storedResults: yesOrNo,
					associatedCards: new Card[] {this.Card},
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(yesNoCR);
				}
				else
				{
					GameController.ExhaustCoroutine(yesNoCR);
				}

				if (yesOrNo.Count > 0 && yesOrNo.FirstOrDefault().Answer == true)
				{
					// if you do...
					IEnumerator reduceCR = GameController.ReduceDamage(
						dd,
						2,
						_reduceDamage,
						GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(reduceCR);
					}
					else
					{
						GameController.ExhaustCoroutine(reduceCR);
					}

					// ...add 1 token to this card's bias pool.
					TokenPool biasPool = this.Card.FindTokenPool("bias");
					if (biasPool != null)
					{
						IEnumerator addTokensCR = GameController.AddTokensToPool(
							biasPool,
							1,
							GetCardSource()
						);
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(addTokensCR);
						}
						else
						{
							GameController.ExhaustCoroutine(addTokensCR);
						}
					}
				}
			}

			yield break;
		}

		private IEnumerator RemoveTokenResponse(DealDamageAction dda)
		{
			TokenPool biasPool = this.Card.FindTokenPool("bias");
			if (biasPool != null)
			{
				// ...remove 1 token from this card's bias pool.
				IEnumerator removeTokenCR = GameController.RemoveTokensFromPool(
					biasPool,
					1,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokenCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokenCR);
				}
			}

			yield break;
		}

		private IEnumerator ClearTokensResponse(GameAction ga)
		{
			TokenPool biasPool = this.Card.FindTokenPool("bias");
			if (biasPool != null)
			{
				IEnumerator clearTokensCR = GameController.RemoveTokensFromPool(
					biasPool,
					biasPool.CurrentValue,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(clearTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(clearTokensCR);
				}
			}
		}
	}
}