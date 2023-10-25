using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class ControlAmphitheaterCardController : CardController
	{
		/*
		 * whenever a Villain card enters play,
		 * if it is not the first time a villain card has entered play this turn,
		 * remove a token from this card's bias pool.
		 * 
		 * Each hero has access to this power:
		 * POWER
		 * Add 3 tokens to this card's bias pool.
		 */

		public ControlAmphitheaterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddAsPowerContributor();
		}

		public override void AddTriggers()
		{
			// whenever a Villain card enters play...
			// ...if it is not the first time a villain card has entered play this turn..
			AddTrigger<CardEntersPlayAction>(
				(CardEntersPlayAction cepa) => IsVillain( cepa.CardEnteringPlay )
					&& GameController.IsCardVisibleToCardSource( cepa.CardEnteringPlay, GetCardSource() )
					&& (
						from pcje
						in Journal.PlayCardEntriesThisTurn()
						where IsVillain( pcje.CardPlayed )
						select pcje
					).Count() > 1,
				RemoveTokenResponse,
				TriggerType.ModifyTokens,
				TriggerTiming.After
			);

			AddBeforeLeavesPlayActions(ClearTokensResponse);

			base.AddTriggers();
		}

		private IEnumerator RemoveTokenResponse(CardEntersPlayAction cepa)
		{
			// ...remove a token from this card's bias pool.
			TokenPool biasPool = this.Card.FindTokenPool("bias");

			if (biasPool != null && biasPool.CurrentValue > 0)
			{
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

		public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
		{
			if (
				cardController.HeroTurnTakerController != null
				&& cardController.Card.IsHeroCharacterCard
				&& cardController.Card.Owner.IsPlayer
				&& !cardController.Card.Owner.IsIncapacitatedOrOutOfGame
				&& !cardController.Card.IsFlipped
			)
			{
				Power power = new Power(
					cardController.HeroTurnTakerController,
					cardController,
					"Add 3 tokens to Control Amphitheater's bias pool.",
					() => GrantedPower( cardController ),
					0,
					null,
					GetCardSource()
				);
				return new Power[1] { power };
			}
			return null;
		}

		private IEnumerator GrantedPower( CardController characterCard )
		{
			// Add 3 tokens to this card's bias pool.
			int tokenNumeral = GetPowerNumeral( 0, 3 );
			TokenPool biasPool = this.Card.FindTokenPool( "bias" );

			if ( biasPool != null )
			{
				IEnumerator addTokenCR = GameController.AddTokensToPool(
					biasPool,
					tokenNumeral,
					GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addTokenCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addTokenCR);
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