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
			IEnumerator trashPlayCR = base.GameController.SelectAndPlayCard(
				DecisionMaker,
				(Card c) =>
					c.Location.IsTrash &&
					c.Location.IsEnvironment &&
					base.GameController.IsCardVisibleToCardSource(c, GetCardSource()),
				isPutIntoPlay: true,
				noValidCardsMessage: "There are no cards in the environment trash.",
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(trashPlayCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(trashPlayCR);
			}

			// Destroy 1 environment card.
			IEnumerator destroyCR = base.GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"),
				false,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(destroyCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(destroyCR);
			}

			yield break;
		}
	}
}