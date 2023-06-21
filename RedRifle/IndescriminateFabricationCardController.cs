using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.RedRifle
{
	public class IndescriminateFabricationCardController : RedRifleBaseCardController
	{
		/*
		 * Reveal the top card of each deck.
		 * Put any revealed targets and equipment into play.
		 * Discard the other revealed cards.
		 * For each card put into play this way, add 1 token to your trueshot pool.
		 */

		public IndescriminateFabricationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Reveal the top card of each deck.
			IEnumerator revealAndDoStuffCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria(tt => GameController.IsTurnTakerVisibleToCardSource(tt,GetCardSource())),
				SelectionType.RevealTopCardOfDeck,
				RevealAndDoStuffResponse,
				allowAutoDecide: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealAndDoStuffCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealAndDoStuffCR);
			}

			yield break;
		}

		private IEnumerator RevealAndDoStuffResponse(TurnTaker tt)
		{
			List<MoveCardAction> storedResults = new List<MoveCardAction>();

			// Put any revealed targets and equipment into play. Discard the other revealed cards.
			IEnumerator revealPlayDiscardCR = RevealCard_PlayItOrDiscardIt(
				DecisionMaker,
				tt.Deck,
				true,
				false,
				new LinqCardCriteria((Card c) => c.IsTarget || IsEquipment(c), "target or equipment"),
				storedResults,
				true,
				tt
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealPlayDiscardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealPlayDiscardCR);
			}

			// For each card put into play this way, add 1 token to your trueshot pool.
			if (storedResults.FirstOrDefault() != null && storedResults.FirstOrDefault().Destination.IsInPlay)
			{
				IEnumerator addTokenCR = RedRifleTrueshotPoolUtility.AddTrueshotTokens(this, 1);

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
	}
}