using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public class DescentOfTheSwarmCardController : TheUndersidersBaseCardController
	{
		public DescentOfTheSwarmCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();
		}

		public override IEnumerator Play()
		{
			// Put all the swarm cards from the villain trash into play.
			IEnumerator fromTrashCR = PlayCardsFromLocation(
				TurnTaker.Trash,
				new LinqCardCriteria((Card c) => c.DoKeywordsContain("swarm"), "swarm"),
				useFixedList: true
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(fromTrashCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(fromTrashCR);
			}

			// Spider: Reveal cards from the top of the villain deck until a swarm card is revealed. Put it into play. Shuffle the other revealed cards back into the villain deck.
			if (IsEnabled("spider"))
			{
				IEnumerator fromDeckCR = RevealCards_MoveMatching_ReturnNonMatchingCards(
					base.TurnTakerController,
					base.TurnTaker.Deck,
					playMatchingCards: false,
					putMatchingCardsIntoPlay: true,
					moveMatchingCardsToHand: false,
					new LinqCardCriteria((Card c) => c.DoKeywordsContain("swarm") && c.IsVillain, "swarm"),
					1
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(fromDeckCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(fromDeckCR);
				}
			}

			// Skull: Each villain character card regains {H} HP.
			if (IsEnabled("skull"))
			{
				IEnumerator healCR = GameController.GainHP(
					DecisionMaker,
					(Card c) => c.IsVillainCharacterCard && !c.IsFlipped && c.IsInPlayAndNotUnderCard,
					base.H,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(healCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(healCR);
				}
			}

			yield break;
		}
	}
}
