using System.Collections;
using System.Collections.Generic;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class FollowThePathCardController : CharmBaseCardController
	{
		// Play this card next to a hero character card.
		// At the start of that hero's turn, their player may draw a card.
		// That hero gains the following power:
		// Power: Reveal the top 4 cards of your deck.
		//  Put 1 of them in your hand, 1 on top of your deck, play 1, and discard the rest.
		//  Destroy this card.

		public FollowThePathCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override string CharmPowerText => "Reveal the top 4 cards of your deck. Put 1 in your hand. Put 1 on top of your deck. Play 1. Discard the rest. Destroy follow the path.";

		public override void AddTriggers()
		{
			// At the start of that hero's turn, their player may draw a card.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.Card.Location.OwnerTurnTaker,
				(PhaseChangeAction p) => DrawCard(base.Card.Location.OwnerTurnTaker.ToHero(), true),
				TriggerType.DrawCard
			);

			base.AddTriggers();
		}

		protected override IEnumerator CharmPowerResponse(CardController cc)
		{
			int revealNumeral = GetPowerNumeral(0, 4);
			int handNumeral = GetPowerNumeral(1, 1);
			int deckNumeral = GetPowerNumeral(2, 1);
			int playNumeral = GetPowerNumeral(3, 1);

			// FOR SOME REASON THE DECISIONMAKER IS THEURGY
			HeroTurnTaker hero = cc.Card.Owner.ToHero();

			List<MoveCardDestination> list = new List<MoveCardDestination>();
			list.Add(new MoveCardDestination(hero.Hand));
			if (handNumeral > 1)
			{
				list.Add(new MoveCardDestination(hero.Hand));
			}
			list.Add(new MoveCardDestination(hero.Deck));
			if (deckNumeral > 1)
			{
				list.Add(new MoveCardDestination(hero.Deck));
			}
			list.Add(new MoveCardDestination(hero.PlayArea));
			if (playNumeral > 1)
			{
				list.Add(new MoveCardDestination(hero.PlayArea));
			}
			if (revealNumeral > (handNumeral + deckNumeral + playNumeral))
			{
				list.Add(new MoveCardDestination(hero.Trash));
				if (revealNumeral > list.Count)
				{
					list.Add(new MoveCardDestination(hero.Trash));
				}
			}

			// reveal cards and put them places
			IEnumerator allTheCardsCR = RevealCardsFromDeckToMoveToOrderedDestinations(
				cc.DecisionMaker,
				hero.Deck,
				list,
				fromBottom: false,
				sendCleanupMessageIfNecessary: true
			);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(allTheCardsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(allTheCardsCR);
			}

			// destroy this card
			IEnumerator destructionCR = GameController.DestroyCard(
				cc.HeroTurnTakerController,
				base.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}
			yield break;
		}
	}
}