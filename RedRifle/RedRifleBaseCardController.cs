using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public abstract class RedRifleBaseCardController : CardController
	{
		protected TokenPool TrueshotPool => this.CharacterCard.FindTokenPool("RedRifleTrueshotPool");

		protected RedRifleBaseCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		protected IEnumerator AddTrueshotTokens(int amountToAdd)
		{
			IEnumerator coroutine;

			coroutine = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, amountToAdd);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		protected IEnumerator AddTrueshotTokens<TAdd>(
			int amountToAdd,
			Func<TAdd, IEnumerator> addTokenResponse,
			TAdd addTokenGameAction = null
		) where TAdd : GameAction
		{
			IEnumerator coroutine;

			coroutine = RedRifleTrueshotPoolUtility.AddTrueshotTokenResponse<TAdd>(
				this,
				amountToAdd,
				addTokenResponse,
				addTokenGameAction
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		protected IEnumerator RemoveTrueshotTokens<TRemove>(
			int amountToRemove,
			Func<TRemove, List<RemoveTokensFromPoolAction>, IEnumerator> removeTokenResponse = null,
			TRemove removeTokenGameAction = null,
			string insufficientTokenMessage = null,
			bool optional = false
		) where TRemove : GameAction
		{
			IEnumerator coroutine;

			coroutine = RedRifleTrueshotPoolUtility.RemoveTrueshotTokens<TRemove>(
				this,
				amountToRemove,
				removeTokenResponse,
				removeTokenGameAction,
				insufficientTokenMessage,
				optional
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		protected IEnumerator AddOrRemoveTrueshotTokens<TAdd, TRemove>(
			int amountToAdd,
			int amountToRemove,
			Func<TAdd, IEnumerator> addTokenResponse = null,
			TAdd addTokenGameAction = null,
			Func<TRemove, List<RemoveTokensFromPoolAction>, IEnumerator> removeTokenResponse = null,
			TRemove removeTokenGameAction = null,
			string insufficientTokenMessage = null,
			string removeEffectDescription = null,
			GameAction triggerAction = null
		)
			where TAdd : GameAction
			where TRemove : GameAction
		{
			IEnumerator coroutine;

			coroutine = RedRifleTrueshotPoolUtility.AddOrRemoveTrueshotTokens<TAdd, TRemove>(
				this,
				amountToAdd,
				amountToRemove,
				addTokenResponse,
				addTokenGameAction,
				removeTokenResponse,
				removeTokenGameAction,
				insufficientTokenMessage,
				removeEffectDescription,
				triggerAction
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		protected IEnumerator AddTrueshotTokenResponse<TAdd>(
			int amountToAdd,
			Func<TAdd, IEnumerator> addTokenResponse,
			TAdd addTokenGameAction
		) where TAdd : GameAction
		{
			IEnumerator coroutine;

			coroutine = RedRifleTrueshotPoolUtility.AddTrueshotTokenResponse<TAdd>(
				this,
				amountToAdd,
				addTokenResponse,
				addTokenGameAction
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		protected IEnumerator RemoveTrueshotTokenResponse<TRemove>(
			int amountToRemove,
			Func<TRemove, List<RemoveTokensFromPoolAction>, IEnumerator> removeTokenResponse,
			TRemove removeTokenGameAction,
			List<RemoveTokensFromPoolAction> storedResults,
			string insufficientTokenMessage,
			bool optional = false
		) where TRemove : GameAction
		{
			IEnumerator coroutine;

			coroutine = RedRifleTrueshotPoolUtility.RemoveTrueshotTokenResponse<TRemove>(
				this,
				amountToRemove,
				removeTokenResponse,
				removeTokenGameAction,
				storedResults,
				insufficientTokenMessage,
				optional
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}
			yield break;
		}
	}
}