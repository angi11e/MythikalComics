using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class IveReadYourFileCardController : SpoilerOngoingCardController
	{
		public IveReadYourFileCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// when this card enters play, reveal and replace the top card of each villain deck.
			IEnumerator revealCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria(
					(TurnTaker tt) => (IsVillain(tt) || tt.IsVillainTeam) && !tt.IsIncapacitatedOrOutOfGame,
					"active villain"
				),
				SelectionType.RevealTopCardOfDeck,
				(TurnTaker tt) => RevealCardsFromTopOfDeck_PutOnTopAndOnBottom(
					DecisionMaker,
					FindTurnTakerController(tt),
					tt.Deck,
					1,
					1,
					0
				),
				allowAutoDecide: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealCR);
			}

			yield break;
		}

		public override IEnumerator ActivateRewind()
		{
			// ...of the villain deck.
			List<SelectLocationDecision> chosenDeck = new List<SelectLocationDecision>();
			IEnumerator selectCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.RevealCardsFromDeck,
				(Location l) => (IsVillain(l.OwnerTurnTaker) || l.OwnerTurnTaker.IsVillainTeam)
					&& !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
				chosenDeck,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCR);
			}

			if (DidSelectDeck(chosenDeck))
			{
				TurnTaker villain = chosenDeck.FirstOrDefault().SelectedLocation.Location.OwnerTurnTaker;

				List<MoveCardDestination> list = new List<MoveCardDestination>();
				// Discard 1...
				list.Add(new MoveCardDestination(villain.Trash));
				// ...put 1 on top of the villain deck...
				list.Add(new MoveCardDestination(villain.Deck));
				// ...and put 1 on the bottom of the villain deck.
				list.Add(new MoveCardDestination(villain.Deck, true));

				List<Location> revealed = new List<Location>();
				revealed.Add(villain.Revealed);

				// Reveal the top 3 cards...
				IEnumerator revealCR = RevealCardsFromDeckToMoveToOrderedDestinations(
					FindTurnTakerController(villain),
					villain.Deck,
					list,
					fromBottom: false,
					sendCleanupMessageIfNecessary: true
				);
				IEnumerator cleanCR = CleanupCardsAtLocations(revealed, villain.Deck);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
					yield return GameController.StartCoroutine(cleanCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
					GameController.ExhaustCoroutine(cleanCR);
				}
			}

			yield break;
		}
	}
}