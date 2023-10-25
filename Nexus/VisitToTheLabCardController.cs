using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class VisitToTheLabCardController : CardController
	{
		/*
		 * you may shuffle your trash into your deck.
		 * 
		 * reveal cards from the top of your deck until a non-limited equipment card is revealed.
		 * put it into play. discard the other revealed cards.
		 * 
		 * you may move an ongoing card from your trash to your hand.
		 */

		public VisitToTheLabCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// you may shuffle your trash into your deck.
			List<YesNoCardDecision> storedYesNoResults = new List<YesNoCardDecision>();
			IEnumerator askTrashCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.ShuffleTrashIntoDeck,
				this.Card,
				storedResults: storedYesNoResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(askTrashCR);
			}
			else
			{
				GameController.ExhaustCoroutine(askTrashCR);
			}

			if (DidPlayerAnswerYes(storedYesNoResults))
			{
				// Shuffle trash into deck
				IEnumerator shuffleCR = GameController.ShuffleTrashIntoDeck(
					this.TurnTakerController,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(shuffleCR);
				}
				else
				{
					GameController.ExhaustCoroutine(shuffleCR);
				}
			}

			// reveal cards from the top of your deck until a non-limited equipment card is revealed.
			// put it into play. discard the other revealed cards.
			IEnumerator revealCR = RevealCards_SelectSome_MoveThem_DiscardTheRest(
				HeroTurnTakerController,
				TurnTakerController,
				this.TurnTaker.Deck,
				(Card c) => IsEquipment(c) && !c.IsLimited,
				1,
				1,
				false,
				true,
				true,
				"non-limited equipment"
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(revealCR);
			}
			else
			{
				GameController.ExhaustCoroutine(revealCR);
			}

			// you may move an ongoing card from your trash to your hand.
			IEnumerator moveCardCR = SearchForCards(
				DecisionMaker,
				false,
				true,
				0,
				1,
				new LinqCardCriteria((Card c) => IsOngoing(c), "ongoing"),
				false,
				true,
				false,
				true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveCardCR);
			}

			yield break;
		}
	}
}