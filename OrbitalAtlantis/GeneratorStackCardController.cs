using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class GeneratorStackCardController : CardController
	{
		/*
		 * when a hero target regains HP or is dealt energy damage,
		 * add 1 token to this card's bias pool.
		 * 
		 * when a villain target regains HP or is dealt energy damage,
		 * remove 1 token from this card's bias pool.
		 */

		public GeneratorStackCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// when a hero target regains HP... 
			// when a villain target regains HP...
			AddTrigger(
				(GainHPAction ghpa) =>
					ghpa.AmountActuallyGained > 0
					&& (IsHeroTarget(ghpa.HpGainer) || IsVillainTarget(ghpa.HpGainer)),
				AdjustTokensResponse,
				TriggerType.ModifyTokens,
				TriggerTiming.After
			);

			// ...or is dealt energy damage
			AddTrigger(
				(DealDamageAction dd) =>
					dd.DidDealDamage
					&& (IsHeroTarget(dd.Target) || IsVillainTarget(dd.Target))
					&& dd.DamageType == DamageType.Energy,
				AdjustTokensResponse,
				TriggerType.ModifyTokens,
				TriggerTiming.After
			);

			AddBeforeLeavesPlayActions(ClearTokensResponse);

			base.AddTriggers();
		}

		private IEnumerator AdjustTokensResponse(GameAction ga)
		{
			TokenPool biasPool = this.Card.FindTokenPool("bias");
			if (biasPool == null)
			{
				yield break;
			}

			Card theTarget = null;
			if (ga is GainHPAction)
			{
				theTarget = (ga as GainHPAction).HpGainer;
			}
			else if (ga is DealDamageAction)
			{
				theTarget = (ga as DealDamageAction).Target;
			}
			else
			{
				yield break;
			}

			IEnumerator changeTokensCR = DoNothing();
			if (IsHeroTarget(theTarget))
			{
				// ...add 1 token to this card's bias pool. (hero)
				changeTokensCR = GameController.AddTokensToPool(
					biasPool,
					1,
					GetCardSource()
				);
			}
			else if (IsVillainTarget(theTarget) && biasPool.CurrentValue > 0)
			{
				// ...remove 1 token from this card's bias pool. (villain)
				changeTokensCR = GameController.RemoveTokensFromPool(
					biasPool,
					1,
					cardSource: GetCardSource()
				);
			}

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(changeTokensCR);
			}
			else
			{
				GameController.ExhaustCoroutine(changeTokensCR);
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