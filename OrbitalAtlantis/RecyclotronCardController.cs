using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class RecyclotronCardController : CardController
	{
		/*
		 * at the end of the environment turn, each player may discard a card.
		 * for each card discarded this way, add 1 token to this card's bias pool.
		 * 
		 * each time a villain card is discarded, remove 1 token from this card's bias pool.
		 */

		public RecyclotronCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// at the end of the environment turn...
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				PlayerResponse,
				TriggerType.DiscardCard
			);

			// each time a villain card is discarded...
			AddTrigger(
				(MoveCardAction mc) =>
					mc.IsDiscard
					&& mc.WasCardMoved
					&& IsVillain(mc.CardToMove),
				VillainResponse,
				TriggerType.ModifyTokens,
				TriggerTiming.After
			);

			AddBeforeLeavesPlayActions(ClearTokensResponse);

			base.AddTriggers();
		}

		private IEnumerator PlayerResponse(PhaseChangeAction p)
		{
			// ...each player may discard a card.
			List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
			IEnumerator discardToDrawCR = GameController.EachPlayerDiscardsCards(
				0,
				1,
				discardResults,
				cardSource: GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardToDrawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardToDrawCR);
			}

			// for each card discarded this way...
			// ...add 1 token to this card's bias pool.
			TokenPool biasPool = this.Card.FindTokenPool("bias");
			if (biasPool != null && discardResults.Any())
			{
				IEnumerator addTokensCR = GameController.AddTokensToPool(
					biasPool,
					discardResults.Count(),
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

			yield break;
		}

		private IEnumerator VillainResponse(MoveCardAction mc)
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