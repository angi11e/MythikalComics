using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public abstract class SpoilerOneshotCardController : CardController
	{
		protected SpoilerOneshotCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowListOfCardsInPlay(
				new LinqCardCriteria((Card c) => IsOngoing(c) && c.Owner == this.TurnTaker, "ongoing")
			);
		}

		protected IEnumerator DiscardToRewind()
		{
			// You may discard a card.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.SelectAndDiscardCard(
				DecisionMaker,
				true,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (DidDiscardCards(storedResults))
			{
				// If you do, activate a [u]rewind[/u] text.
				IEnumerator activateCR = GameController.SelectAndActivateAbility(
					DecisionMaker,
					"rewind",
					optional: true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(activateCR);
				}
				else
				{
					GameController.ExhaustCoroutine(activateCR);
				}
			}

			yield break;
		}
	}
}