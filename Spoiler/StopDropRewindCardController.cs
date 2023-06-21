using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class StopDropRewindCardController : SpoilerOneshotCardController
	{
		public StopDropRewindCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowListOfCardsAtLocation(
				this.HeroTurnTaker.Deck,
				new LinqCardCriteria((Card c) => IsOngoing(c))
			);
			SpecialStringMaker.ShowListOfCardsAtLocation(
				this.HeroTurnTaker.Trash,
				new LinqCardCriteria((Card c) => IsOngoing(c))
			);
		}

		public override IEnumerator Play()
		{
			// You may draw a card.
			IEnumerator drawCR = DrawCard(HeroTurnTaker, true);

			// Search your deck or trash for an ongoing card and put it in your hand.
			// If you searched your deck, shuffle your deck.
			IEnumerator searchCR = SearchForCards(
				DecisionMaker,
				searchDeck: true,
				searchTrash: true,
				1,
				1,
				new LinqCardCriteria(c => IsOngoing(c), "ongoing", true),
				putIntoPlay: false,
				putInHand: true,
				putOnDeck: false
			);

			// You may play a card.
			IEnumerator playCardCR = SelectAndPlayCardFromHand(this.HeroTurnTakerController);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
				yield return GameController.StartCoroutine(searchCR);
				yield return GameController.StartCoroutine(playCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
				GameController.ExhaustCoroutine(searchCR);
				GameController.ExhaustCoroutine(playCardCR);
			}

			// You may discard a card. If you do, activate a [u]rewind[/u] text.
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(DiscardToRewind());
			}
			else
			{
				GameController.ExhaustCoroutine(DiscardToRewind());
			}

			yield break;
		}
	}
}