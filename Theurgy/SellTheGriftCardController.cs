using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	internal class SellTheGriftCardController : CharmBaseCardController
	{
		// Play this card next to a hero character card.
		// They may discard a card. If they do, they move a card from their trash into their hand.
		// That hero gains the [b]power:[/b] destroy this card.
		// Before this card is destroyed, the hero it's next to Discards any number of cards.
		// Move up to that many cards from their trash to their hand. That hero plays a card."

		public SellTheGriftCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(CharmedHero().Owner.ToHero());

			// They may discard a card.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				httc,
				1,
				optional: true,
				0,
				storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			int numberOfCards = storedResults.Count();
			if (numberOfCards > 0)
			{
				// If they do, they may put a card from their trash into their hand.
				IEnumerator recoverCR = GameController.SelectAndMoveCard(
					httc,
					(Card c) => c.IsInTrash && c.Owner == CharmedHero().Owner,
					httc.HeroTurnTaker.Hand,
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

		protected override IEnumerator CharmDestroyResponse(GameAction ga)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(CharmedHero().Owner.ToHero());

			// discard any number of cards
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				httc,
				null,
				optional: false,
				0,
				storedResults
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(discardCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(discardCR);
			}

			// how many was that?
			int numberOfCards = storedResults.Count();

			// choose up to that many cards from trash
			IEnumerable<MoveCardDestination> heroHand = new MoveCardDestination[] {
				new MoveCardDestination(httc.HeroTurnTaker.Hand)
			};
			IEnumerator recoverCR = GameController.SelectCardsFromLocationAndMoveThem(
				httc,
				httc.TurnTaker.Trash,
				null,
				numberOfCards,
				new LinqCardCriteria(
					(Card c) => c.IsInTrash
					&& GameController.IsLocationVisibleToSource(c.Location, GetCardSource())
				),
				heroHand,
				selectionType: SelectionType.ReturnToHand,
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

			// Play 1 card.
			IEnumerator playCardCR = SelectAndPlayCardFromHand(httc, false);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(playCardCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(playCardCR);
			}

			yield break;
		}
	}
}