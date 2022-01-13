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
			IEnumerator destroyCR = base.GameController.SelectAndDestroyCard(
				base.HeroTurnTakerController,
				new LinqCardCriteria((Card c) => c.IsOngoing || c.IsEnvironment, "ongoing or environment"),
				optional: false,
				storedResults,
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

			// was it a hero card?
			if (DidDestroyCard(storedResults) && storedResults.First().CardToDestroy.Card.IsHero)
            {
				// if so, they play a card from their trash
				HeroTurnTakerController hero = storedResults.First().CardToDestroy.DecisionMaker;

				IEnumerator recoverCR = GameController.SelectAndMoveCard(
					hero,
					(Card c) => c.IsInTrash && c.Owner == hero.TurnTaker,
					hero.HeroTurnTaker.PlayArea,
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