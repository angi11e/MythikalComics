using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public static class RedRifleTrueshotPoolUtility
	{
		public static TokenPool GetTrueshotPool(CardController cardController)
		{
			//Check Character Card first
			var prospect = cardController.CharacterCard.FindTokenPool("RedRifleTrueshotPool");
			if (prospect != null)
			{
				return prospect;
			}

			//If not, look for a "RedRifle" TurnTaker and get their character card
			var redRifle = cardController.GameController.Game.HeroTurnTakers.Where(
				htt => htt.Identifier == "RedRifle"
			).FirstOrDefault();
			if (redRifle != null)
			{
				return redRifle.CharacterCard.FindTokenPool("RedRifleTrueshotPool");
			}

			//If not there, try the card itself (for Representative of Earth purposes)
			prospect = cardController.CardWithoutReplacements.FindTokenPool("RedRifleTrueshotPool");
			if (prospect != null)
			{
				return prospect;
			}

			//If not, we have failed to find it - error handle!
			return null;
		}

		public static IEnumerator AddTrueshotTokens(CardController cardController, int amountToAdd)
		{
			IEnumerator coroutine;

			coroutine = AddTrueshotTokens<GameAction>(cardController, amountToAdd, null);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		public static IEnumerator AddTrueshotTokens<TAdd>(
			CardController cardController,
			int amountToAdd,
			Func<TAdd, IEnumerator> addTokenResponse,
			TAdd addTokenGameAction = null
		) where TAdd : GameAction
		{
			IEnumerator coroutine;

			coroutine = AddTrueshotTokenResponse(cardController, amountToAdd, addTokenResponse, addTokenGameAction);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		public static IEnumerator RemoveTrueshotTokens<TRemove>(
			CardController cardController,
			int amountToRemove,
			Func<TRemove, List<RemoveTokensFromPoolAction>, IEnumerator> removeTokenResponse = null,
			TRemove removeTokenGameAction = null,
			string insufficientTokenMessage = null,
			bool optional = false
		) where TRemove : GameAction
		{
			IEnumerator coroutine;
			List<RemoveTokensFromPoolAction> storedResults = new List<RemoveTokensFromPoolAction>();

			coroutine = RemoveTrueshotTokenResponse(
				cardController,
				amountToRemove,
				removeTokenResponse,
				removeTokenGameAction,
				storedResults,
				insufficientTokenMessage,
				optional
			);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		public static IEnumerator AddOrRemoveTrueshotTokens<TAdd, TRemove>(
			CardController cardController,
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

			if (GetTrueshotPool(cardController) == null)
			{
				coroutine = TrueshotPoolErrorMessage(cardController);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
				yield break;
			}

			List<Function> list = new List<Function>();
			List<RemoveTokensFromPoolAction> storedResults = new List<RemoveTokensFromPoolAction>();
			SelectFunctionDecision selectFunction;
			String addToken = "1 token";
			String removeDescriber = "";

			if (amountToAdd > 1)
			{
				addToken = $"{amountToAdd} tokens";
			}
			if (GetTrueshotPool(cardController).CurrentValue < amountToRemove)
			{
				removeDescriber = " to no effect";
			}
			else if (removeEffectDescription != null)
			{
				removeDescriber = " to " + removeEffectDescription;
			}

			list.Add(new Function(
				cardController.DecisionMaker,
				$"Add {addToken} to {GetTrueshotPool(cardController).Name}",
				SelectionType.AddTokens,
				() => AddTrueshotTokenResponse(
					cardController,
					amountToAdd,
					addTokenResponse,
					addTokenGameAction
				)
			));
			list.Add(new Function(
				cardController.DecisionMaker,
				$"Remove 3 tokens from {GetTrueshotPool(cardController).Name}" + removeDescriber,
				SelectionType.RemoveTokens,
				() => RemoveTrueshotTokenResponse(
					cardController,
					amountToRemove,
					removeTokenResponse,
					removeTokenGameAction,
					storedResults,
					insufficientTokenMessage
				)
			));

			selectFunction = new SelectFunctionDecision(
				cardController.GameController,
				cardController.DecisionMaker,
				list,
				false,
				triggerAction,
				null,
				null,
				cardController.GetCardSource()
			);

			coroutine = cardController.GameController.SelectAndPerformFunction(selectFunction);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		public static IEnumerator AddTrueshotTokenResponse<TAdd>(
			CardController cardController,
			int amountToAdd,
			Func<TAdd, IEnumerator> addTokenResponse,
			TAdd addTokenGameAction
		) where TAdd : GameAction
		{
			IEnumerator coroutine;
			if (GetTrueshotPool(cardController) == null)
			{
				coroutine = TrueshotPoolErrorMessage(cardController);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
				yield break;
			}

			coroutine = cardController.GameController.AddTokensToPool(
				GetTrueshotPool(cardController),
				amountToAdd,
				cardController.GetCardSource()
			);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			coroutine = SendMessageTrueshotTokensAdded(cardController, amountToAdd);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			if (addTokenResponse != null)
			{
				coroutine = addTokenResponse(addTokenGameAction);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
			}

			yield break;
		}

		public static IEnumerator RemoveTrueshotTokenResponse<TRemove>(
			CardController cardController,
			int amountToRemove,
			Func<TRemove, List<RemoveTokensFromPoolAction>, IEnumerator> removeTokenResponse,
			TRemove removeTokenGameAction,
			List<RemoveTokensFromPoolAction> storedResults,
			string insufficientTokenMessage,
			bool optional = false
		) where TRemove : GameAction
		{
			IEnumerator coroutine;
			if (GetTrueshotPool(cardController) == null)
			{
				coroutine = TrueshotPoolErrorMessage(cardController);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
				yield break;
			}

			coroutine = cardController.GameController.RemoveTokensFromPool(
				GetTrueshotPool(cardController),
				amountToRemove,
				storedResults,
				optional: optional,
				null,
				cardController.GetCardSource()
			);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			if (storedResults.FirstOrDefault() != null)
			{
				if ((storedResults.FirstOrDefault()?.NumberOfTokensActuallyRemoved ?? 0) >= amountToRemove)
				{
					coroutine = SendMessageTrueshotTokensRemoved(cardController, amountToRemove, storedResults);
				}
				else
				{
					coroutine = SendMessageAboutInsufficientTrueshotTokens(
						cardController,
						(storedResults.FirstOrDefault()?.NumberOfTokensActuallyRemoved ?? 0),
						insufficientTokenMessage
					);
				}

				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
			}

			if (removeTokenResponse != null)
			{
				coroutine = removeTokenResponse(removeTokenGameAction, storedResults);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
			}

			yield break;
		}

		public static IEnumerator SendMessageTrueshotTokensAdded(CardController cardController, int numberAdded)
		{
			IEnumerator coroutine;
			if (GetTrueshotPool(cardController) == null)
			{
				coroutine = TrueshotPoolErrorMessage(cardController);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
				yield break;
			}

			string message = "No tokens were added";

			if (numberAdded == 1)
			{
				message = "One token was added";
			}
			else
			{
				if (numberAdded > 1)
				{
					message = $"{numberAdded} tokens were added";
				}
			}

			message += $" to {GetTrueshotPool(cardController).Name}.";
			coroutine = cardController.GameController.SendMessageAction(
				message,
				Priority.Medium,
				cardController.GetCardSource(),
				null,
				showCardSource: true
			);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		public static IEnumerator SendMessageTrueshotTokensRemoved(
			CardController cardController,
			int numberRemoved,
			List<RemoveTokensFromPoolAction> removeTokensFromPoolActions
		)
		{
			IEnumerator coroutine;
			if (GetTrueshotPool(cardController) == null)
			{
				coroutine = TrueshotPoolErrorMessage(cardController);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
				yield break;
			}
			string message = "No tokens were removed";

			if (numberRemoved == 1)
			{
				message = "One token was removed";
			}
			else
			{
				if (numberRemoved > 1)
				{
					message = $"{numberRemoved} tokens were removed";
				}
			}

			message += $" from {GetTrueshotPool(cardController).Name}.";
			coroutine = cardController.GameController.SendMessageAction(
				message,
				Priority.Medium,
				cardController.GetCardSource(),
				null,
				showCardSource: true
			);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}

			yield break;
		}

		public static IEnumerator SendMessageAboutInsufficientTrueshotTokens(
			CardController cardController,
			int numberRemoved,
			string suffix = null
		)
		{
			IEnumerator coroutine;
			if (GetTrueshotPool(cardController) == null)
			{
				coroutine = TrueshotPoolErrorMessage(cardController);
				if (cardController.UseUnityCoroutines)
				{
					yield return cardController.GameController.StartCoroutine(coroutine);
				}
				else
				{
					cardController.GameController.ExhaustCoroutine(coroutine);
				}
				yield break;
			}
			string message = "There were no tokens to remove";

			if (numberRemoved == 1)
			{
				message = "Only one token was removed";
			}
			else if (numberRemoved > 1)
			{
				message = $"Only {numberRemoved} tokens were removed";
			}

			message += $" from {GetTrueshotPool(cardController).Name}";
			if (suffix != null && !string.IsNullOrEmpty(suffix.Trim()))
			{
				message = $"{message}, so {suffix}";
			}
			else
			{
				message += ".";
			}

			coroutine = cardController.GameController.SendMessageAction(
				message,
				Priority.Medium,
				cardController.GetCardSource(),
				null,
				showCardSource: true
			);
			if (cardController.UseUnityCoroutines)
			{
				yield return cardController.GameController.StartCoroutine(coroutine);
			}
			else
			{
				cardController.GameController.ExhaustCoroutine(coroutine);
			}
		}

		public static IEnumerator TrueshotPoolErrorMessage(CardController cardController)
		{
			return cardController.GameController.SendMessageAction(
				"No appropriate Trueshot Pool could be found.",
				Priority.High,
				cardController.GetCardSource()
			);
		}
	}
}
