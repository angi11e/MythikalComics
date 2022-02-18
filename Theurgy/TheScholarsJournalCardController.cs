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
		// Power: Search your trash or deck for a [u]charm[/u] card.
		//  Put it into play or in your hand.
		//  Destroy a [u]charm[/u] card.

		public TheScholarsJournalCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
			SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Hand, IsCharmCriteria());
			SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Deck, IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			// Theurgy draws 2 cards
			return DrawCards(HeroTurnTakerController, 2);
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

			IEnumerator selectDeckCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.CannotPlayCards,
				(Location deck) => true,
				storedResultsDeck,
				optional: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectDeckCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectDeckCR);
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
			
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(inhibitorCR);
			}
			else
			{
				GameController.ExhaustCoroutine(inhibitorCR);
			}

			if (!location.Location.IsSubDeck)
			{
				PreventPhaseActionStatusEffect preventPhaseActionStatusEffect = new PreventPhaseActionStatusEffect();
				preventPhaseActionStatusEffect.ToTurnPhaseCriteria.Phase = Phase.PlayCard;
				preventPhaseActionStatusEffect.ToTurnPhaseCriteria.TurnTaker = location.Location.OwnerTurnTaker;
				preventPhaseActionStatusEffect.UntilStartOfNextTurn(base.TurnTaker);
				IEnumerator inhibitSubdeckCR = AddStatusEffect(preventPhaseActionStatusEffect);
			
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(inhibitSubdeckCR);
				}
				else
				{
					GameController.ExhaustCoroutine(inhibitSubdeckCR);
				}
			}

			// remove this card from the game
			d.SetPostDestroyDestination(base.HeroTurnTaker.OutOfGame);

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// Search your trash or deck for a charm card. Put it into play or in your hand.
			IEnumerator discoverCharmCR = SearchForCards(
				HeroTurnTakerController,
				searchDeck: true,
				searchTrash: true,
				1,
				1,
				IsCharmCriteria(),
				putIntoPlay: true,
				putInHand: true,
				putOnDeck: false
			);
			
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discoverCharmCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discoverCharmCR);
			}

			// destroy a charm card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				IsCharmCriteria(),
				false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			yield break;
		}
	}
}