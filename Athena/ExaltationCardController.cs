using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class ExaltationCardController : AthenaBaseCardController
	{
		/*
		 * You may draw a card.
		 * Search your trash or deck for an aspect card and put it in your hand.
		 * If you searched your deck, shuffle your deck.
		 * You may play a card.
		 */

		public ExaltationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowListOfCardsAtLocation(
				this.HeroTurnTaker.Deck,
				new LinqCardCriteria((Card c) => IsManifest(c), "manifest", true)
			);
			SpecialStringMaker.ShowListOfCardsAtLocation(
				this.HeroTurnTaker.Trash,
				new LinqCardCriteria((Card c) => IsManifest(c), "manifest", true)
			);
		}

		public override IEnumerator Play()
		{
			// You may draw a card.
			IEnumerator drawCR = DrawCard(HeroTurnTaker, true);

			// Search your trash or deck for an aspect card and put it in your hand.
			// If you searched your deck, shuffle your deck.
			IEnumerator searchCR = SearchForCards(
				DecisionMaker,
				searchDeck: true,
				searchTrash: true,
				1,
				1,
				new LinqCardCriteria(c => IsManifest(c), "manifest", true),
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

			yield break;
		}
	}
}