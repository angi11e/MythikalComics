using System.Collections;
using System.Collections.Generic;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class TeleportationCardController : TheurgyBaseCardController
	{
		// Put a card from the environment trash into play.
		// Destroy 1 environment card.

		public TeleportationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Put a card from the environment trash into play.
			IEnumerator trashPlayCR = GameController.SelectAndPlayCard(
				DecisionMaker,
				(Card c) =>
					c.Location.IsTrash &&
					c.Location.IsEnvironment &&
					GameController.IsCardVisibleToCardSource(c, GetCardSource()),
				isPutIntoPlay: true,
				noValidCardsMessage: "There are no cards in the environment trash.",
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(trashPlayCR);
			}
			else
			{
				GameController.ExhaustCoroutine(trashPlayCR);
			}

			// Destroy 1 environment card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				false,
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

			yield break;
		}
	}
}