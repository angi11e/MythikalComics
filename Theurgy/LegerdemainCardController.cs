using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class LegerdemainCardController : TheurgyBaseCardController
	{
		// Put any number of charm cards back into your hand from play.
		// You may play up to that many cards now.

		public LegerdemainCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(IsCharmCriteria());
		}

		public override IEnumerator Play()
		{
			int beforeRetrieval = base.HeroTurnTaker.Hand.NumberOfCards;

			// choose the charm cards
			var selectCards = new SelectCardsDecision(
				GameController,
				DecisionMaker,
				(Card c) => c.IsInPlayAndHasGameText && IsCharm(c) && c.Owner == this.TurnTaker,
				SelectionType.ReturnToHand,
				FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsCharm(c)).Count(),
				requiredDecisions: 1,
				cardSource: GetCardSource()
			);

			IEnumerator selectCardsCR = GameController.SelectCardsAndDoAction(
				selectCards,
				MoveCardsToHand,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectCardsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectCardsCR);
			}

			// play that many
			int cardsToPlay = base.HeroTurnTaker.Hand.NumberOfCards - beforeRetrieval;
			if (cardsToPlay > 0)
            {
				IEnumerator playCardsCR = GameController.SelectAndPlayCardsFromHand(
					DecisionMaker,
					cardsToPlay,
					true,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(playCardsCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(playCardsCR);
				}
			}
			yield break;
		}

		private IEnumerator MoveCardsToHand(SelectCardDecision scd)
        {
			// put them in-hand
			IEnumerator moveCardsCR = GameController.MoveCard(
				DecisionMaker,
				scd.SelectedCard,
				HeroTurnTaker.Hand,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(moveCardsCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(moveCardsCR);
			}

			yield break;
        }
	}
}