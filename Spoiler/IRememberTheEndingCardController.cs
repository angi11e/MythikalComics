using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class IRememberTheEndingCardController : SpoilerOngoingCardController
	{
		public IRememberTheEndingCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// when this card enters play, reveal and replace the top card of each hero deck.
			IEnumerator revealCR = GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria(
					(TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt),
					"active hero"
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
			// One player...
			List<SelectLocationDecision> chosenDeck = new List<SelectLocationDecision>();
			IEnumerator selectCR = GameController.SelectADeck(
				DecisionMaker,
				SelectionType.RevealCardsFromDeck,
				(Location l) => IsHero(l.OwnerTurnTaker) && !l.OwnerTurnTaker.IsIncapacitatedOrOutOfGame,
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
				HeroTurnTaker hero = chosenDeck.FirstOrDefault().SelectedLocation.Location.OwnerTurnTaker.ToHero();

				List<MoveCardDestination> list = new List<MoveCardDestination>();
				// Discard 1...
				list.Add(new MoveCardDestination(hero.Trash));
				// ...put 1 on top of their deck...
				list.Add(new MoveCardDestination(hero.Deck));
				// ...and put 1 in their hand.
				list.Add(new MoveCardDestination(hero.Hand));

				List<Location> revealed = new List<Location>();
				revealed.Add(hero.Revealed);

				// ...reveals the top 3 cards of their deck.
				IEnumerator revealCR = RevealCardsFromDeckToMoveToOrderedDestinations(
					FindTurnTakerController(hero),
					hero.Deck,
					list,
					fromBottom: false,
					sendCleanupMessageIfNecessary: true
				);
				IEnumerator cleanCR = CleanupCardsAtLocations(revealed, hero.Deck);

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