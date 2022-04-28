using System.Collections;
using System.Collections.Generic;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class FollowThePathCardController : CharmBaseCardController
	{
		// Play this card next to a hero character card. That hero draws a card.
		// That hero gains the [b]power:[/b] destroy this card.
		// Before this card is destroyed, the hero it's next to Reveals the top 4 cards of their deck.
		//  Put 1 in their hand. Put 1 on top of their deck. Discard 1. Play 1.

		public FollowThePathCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[4] {
			TriggerType.RevealCard,
			TriggerType.DrawCard,
			TriggerType.DiscardCard,
			TriggerType.PlayCard
		};

		public override IEnumerator Play()
		{
			return DrawCard(CharmedHero().Owner.ToHero());
		}

		protected override IEnumerator CharmDestroyResponse(GameAction ga)
		{
			// FOR SOME REASON THE DECISIONMAKER IS THEURGY!?!?
			HeroTurnTaker hero = CharmedHero().Owner.ToHero();

			List<MoveCardDestination> list = new List<MoveCardDestination>();
			list.Add(new MoveCardDestination(hero.Hand));
			list.Add(new MoveCardDestination(hero.Deck));
			list.Add(new MoveCardDestination(hero.Trash));
			list.Add(new MoveCardDestination(hero.PlayArea));

			// reveal cards and put them places
			return RevealCardsFromDeckToMoveToOrderedDestinations(
				FindTurnTakerController(hero),
				hero.Deck,
				list,
				fromBottom: false,
				sendCleanupMessageIfNecessary: true
			);
		}
	}
}