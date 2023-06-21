using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class LegerdemainCardController : TheurgyBaseCardController
	{
		// Put any number of your charm cards back into your hand from play.
		// You may play up to that many cards now.

		public LegerdemainCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			int beforeRetrieval = HeroTurnTaker.Hand.NumberOfCards;

			// choose the charm cards
			var selectCards = new SelectCardsDecision(
				GameController,
				DecisionMaker,
				(Card c) => c.IsInPlayAndHasGameText && IsCharm(c) && c.Owner == this.TurnTaker,
				SelectionType.ReturnToHand,
				CharmCardsInPlay,
				requiredDecisions: 0,
				cardSource: GetCardSource()
			);

			List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
			IEnumerator selectCardsCR = GameController.SelectCardsAndDoAction(
				selectCards,
				(SelectCardDecision scd) => GameController.MoveCard(
					DecisionMaker,
					scd.SelectedCard,
					HeroTurnTaker.Hand,
					cardSource: GetCardSource()
				),
				storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCardsCR);
			}

			// play that many
			int cardsToPlay = storedResults.Where((SelectCardDecision scd) => scd.SelectedCard != null).Count();
			if (cardsToPlay > 0)
			{
				IEnumerator playCardsCR = GameController.SelectAndPlayCardsFromHand(
					DecisionMaker,
					cardsToPlay,
					true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardsCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardsCR);
				}
			}
			yield break;
		}
	}
}