using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class SituationalAwarenessCardController : RedRifleBaseCardController
	{
		/*
		 * Add or remove 3 tokens from your trueshot pool.
		 * If you removed 3 tokens this way, reveal the top 2 cards of any deck.
		 *  Put 1 on top of the deck and 1 on the bottom of the deck.
		 * Draw or play 1 card.
		 */

		public SituationalAwarenessCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowTokenPool(TrueshotPool);
		}

		public override IEnumerator Play()
		{
			// Add or remove 3 tokens from your trueshot pool.
			IEnumerator addOrRemoveCR = AddOrRemoveTrueshotTokens<GameAction, GameAction>(
				3,
				3,
				removeTokenResponse: RemoveTokensFromPoolResponse,
				insufficientTokenMessage: "nothing happens."
			);

			// Draw or play 1 card.
			IEnumerator drawOrPlayCR = DrawACardOrPlayACard(DecisionMaker, false);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(addOrRemoveCR);
				yield return GameController.StartCoroutine(drawOrPlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(addOrRemoveCR);
				GameController.ExhaustCoroutine(drawOrPlayCR);
			}

			yield break;
		}

		private IEnumerator RemoveTokensFromPoolResponse(
			GameAction ga,
			List<RemoveTokensFromPoolAction> storedResults
		)
		{
			// If you removed 3 tokens this way, reveal the top 2 cards of any deck.
			List<SelectLocationDecision> locationResults = new List<SelectLocationDecision>();
			IEnumerator selectDeckCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.RevealCardsFromDeck,
				location =>
					location.IsDeck
					&& location.IsRealDeck
					&& GameController.IsLocationVisibleToSource(
						location, GetCardSource()
					),
				locationResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectDeckCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectDeckCR);
			}

			// Put 1 on top of the deck and 1 on the bottom of the deck.
			Location deck = GetSelectedLocation(locationResults);

			if (deck != null)
			{
				IEnumerator revealAndMoveCR = RevealCardsFromTopOfDeck_PutOnTopAndOnBottom(
					DecisionMaker,
					DecisionMaker,
					deck
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealAndMoveCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealAndMoveCR);
				}
			}

			yield break;
		}
	}
}