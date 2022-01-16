using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class TheScholarsJournalCardController : TheurgyBaseCardController
	{
		// when this card enters play, draw 2 cards.
		// When this card is destroyed, select a deck.
		//  Cards from that deck cannot be played until the start of your next turn.
		//  Remove this card from the game."
		// Power: Destroy a [u]charm[/u] card.
		//  Search your trash or deck for a [u]charm[/u] card.
		//  Put it into play or in your hand.

		public TheScholarsJournalCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
			base.SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Hand, IsCharmCriteria());
			base.SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Deck, IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// Theurgy draws 2 cards
			IEnumerator drawCR = DrawCards(
				base.HeroTurnTakerController,
				2
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(drawCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(drawCR);
			}

			yield break;
		}

		public override void AddTriggers()
		{
			// when this card is destroyed...
			AddWhenDestroyedTrigger(
				DestructionResponse,
				new TriggerType[2] {
					TriggerType.PreventPhaseAction,
					TriggerType.RemoveFromGame
				}
			);

			base.AddTriggers();
		}

		private IEnumerator DestructionResponse(DestroyCardAction d)
		{
			// select a deck.
			List<SelectLocationDecision> storedResultsDeck = new List<SelectLocationDecision>();

			IEnumerator selectDeckCR = base.GameController.SelectADeck(
				DecisionMaker,
				SelectionType.CannotPlayCards,
				(Location deck) => true,
				storedResultsDeck,
				optional: true,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectDeckCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectDeckCR);
			}
			if (!DidSelectDeck(storedResultsDeck))
			{
				yield break;
			}

			// cards from that deck cannot be played until the start of your next turn.
			LocationChoice location = storedResultsDeck.First().SelectedLocation;

			CannotPlayCardsStatusEffect cannotPlayCardsStatusEffect = new CannotPlayCardsStatusEffect();
			cannotPlayCardsStatusEffect.CardCriteria.NativeDeck = location.Location;
			cannotPlayCardsStatusEffect.UntilStartOfNextTurn(base.TurnTaker);
			IEnumerator inhibitorCR = AddStatusEffect(cannotPlayCardsStatusEffect);
			
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(inhibitorCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(inhibitorCR);
			}

			if (!location.Location.IsSubDeck)
			{
				PreventPhaseActionStatusEffect preventPhaseActionStatusEffect = new PreventPhaseActionStatusEffect();
				preventPhaseActionStatusEffect.ToTurnPhaseCriteria.Phase = Phase.PlayCard;
				preventPhaseActionStatusEffect.ToTurnPhaseCriteria.TurnTaker = location.Location.OwnerTurnTaker;
				preventPhaseActionStatusEffect.UntilStartOfNextTurn(base.TurnTaker);
				IEnumerator inhibitSubdeckCR = AddStatusEffect(preventPhaseActionStatusEffect);
			
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(inhibitSubdeckCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(inhibitSubdeckCR);
				}
			}

			// remove this card from the game
			d.OverridePostDestroyDestination(
				base.TurnTaker.OutOfGame,
				cardSource: GetCardSource()
			);

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// destroy a charm card.
			IEnumerator destroyCR = base.GameController.SelectAndDestroyCard(
				DecisionMaker,
				IsCharmCriteria(),
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

			// Search your trash or deck for a charm card. Put it into play or in your hand.
			IEnumerator discoverCharmCR = SearchForCards(
				base.HeroTurnTakerController,
				searchDeck: true,
				searchTrash: true,
				1,
				1,
				IsCharmCriteria(),
				putIntoPlay: true,
				putInHand: true,
				putOnDeck: false
			);
			
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(discoverCharmCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(discoverCharmCR);
			}

			yield break;
		}
	}
}