using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class YoinkCardController : TheurgyBaseCardController
	{
		// Destroy 1 ongoing or equipment card.
		// If you destroyed a hero card, that hero's player may put a card from their trash into play.

		public YoinkCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// destroy 1 ongoing or equipment card
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => IsOngoing(c) || c.DoKeywordsContain("equipment"),
					"ongoing or equipment"
				),
				optional: false,
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			// was it a hero card?
			if (DidDestroyCard(storedResults) && IsHero(storedResults.First().CardToDestroy.Card))
			{
				// if so, they play a card from their trash
				HeroTurnTakerController httc = storedResults.First().CardToDestroy.DecisionMaker;

				IEnumerator recoverCR = GameController.SelectAndMoveCard(
					httc,
					(Card c) => c.IsInTrash && c.Owner == httc.TurnTaker,
					httc.HeroTurnTaker.PlayArea,
					optional: true,
					isPutIntoPlay: true,
					cardSource: GetCardSource()
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(recoverCR);
				}
				else
				{
					GameController.ExhaustCoroutine(recoverCR);
				}
			}

			yield break;
		}
	}
}