using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class VisualCalculusCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a character card.
		 * You may look at the top card of their deck at any time.
		 * If that target leaves play, return this card to your hand.
		 */

		public VisualCalculusCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			// You may look at the top card of their deck at any time.
			SpecialStringMaker.ShowListOfCards(
				new LinqCardCriteria(
					(Card c) => c == base.Card.Location.OwnerTurnTaker.Deck.TopCard,
					"top card of deck",
					useCardsSuffix: false
				)
			).Condition = () => base.Card.IsInPlayAndHasGameText;
		}

		// Play this card next to a character card.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => c.IsCharacter && c.IsInPlayAndHasGameText && !c.IsIncapacitatedOrOutOfGame,
			"character card"
		);

		public override void AddTriggers()
		{
			// If that target leaves play, return this card to your hand.
			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger();

			base.AddTriggers();
		}
	}
}