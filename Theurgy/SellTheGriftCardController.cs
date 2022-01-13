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
		// At the start of their turn, they may put a card from their trash into their hand.
		// That hero gains the following power:
		// Power: discard any number of cards. You may put up to that many cards from your trash into your hand.
		//  Destroy this card.

		public SellTheGriftCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override string CharmPowerText => "Discard any number of cards. You may put up to that many cards from your trash into your hand. Destroy Sell the Grift.";

		public override void AddTriggers()
		{
			base.AddTriggers();

			// at the start of their turn...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == GetCardThisCardIsNextTo().Owner,
				RecoverCardResponse,
				TriggerType.MoveCard
			);
		}

		private IEnumerator RecoverCardResponse(PhaseChangeAction phaseChange)
		{
			// they may put a card from their trash into their hand.
			HeroTurnTakerController hero = phaseChange.DecisionMaker;

			IEnumerator recoverCR = GameController.SelectAndMoveCard(
				hero,
				(Card c) => c.IsInTrash && c.Owner == GetCardThisCardIsNextTo().Owner,
				GetCardThisCardIsNextTo().Owner.ToHero().Hand,
				optional: true,
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
			yield break;
		}

		protected override IEnumerator CharmPowerResponse(CardController cc)
		{
			HeroTurnTakerController hero = cc.HeroTurnTakerController;

			// discard any number of cards
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				hero,
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
				new MoveCardDestination(hero.HeroTurnTaker.Hand)
			};
			IEnumerator recoverCR = base.GameController.SelectCardsFromLocationAndMoveThem(
				hero,
				hero.TurnTaker.Trash,
				null,
				numberOfCards,
				new LinqCardCriteria(
					(Card c) => c.IsInTrash
					&& this.GameController.IsLocationVisibleToSource(c.Location, base.GetCardSource(null))
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

			// destroy this card.
			IEnumerator destructionCR = GameController.DestroyCard(
				hero,
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