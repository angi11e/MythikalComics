using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class NamchoCycleCardController : SupplicateBaseCardController
	{
		/*
		 * whenever one of your ongoing cards is destroyed,
		 * you may move it to the bottom of your deck,
		 * then move a yaojing from your trash to your hand.
		 * 
		 * whenever one of your yaojing cards is destroyed,
		 * you may move it to the bottom of your deck,
		 * then move an ongoing from your trash to your hand.
		 */

		public NamchoCycleCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsAtLocation(
				HeroTurnTaker.Trash,
				new LinqCardCriteria((Card c) => IsOngoing(c), "ongoing")
			);
			SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Trash, IsYaojingCriteria());
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// whenever one of your ongoing cards is destroyed,
			// whenever one of your yaojing cards is destroyed,
			AddTrigger(
				(DestroyCardAction d) => d.WasCardDestroyed
					&& (IsYaojing(d.CardToDestroy.Card) || IsOngoing(d.CardToDestroy.Card))
					&& d.CardToDestroy.Card.Owner == this.TurnTaker,
				RecycleResponse,
				TriggerType.MoveCard,
				TriggerTiming.After
			);
		}

		private IEnumerator RecycleResponse(DestroyCardAction dca)
		{
			List<Card> cards = new List<Card>();
			if (IsOngoing(dca.CardToDestroy.Card))
			{
				cards = this.TurnTaker.Trash.Cards.Where((Card c) => IsYaojing(c)).ToList();
			}
			else
			{
				cards = this.TurnTaker.Trash.Cards.Where((Card c) => IsOngoing(c)).ToList();
			}

			// you may move it to the bottom of your deck,
			List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
			IEnumerator yesNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.MoveCardOnBottomOfDeck,
				dca.CardToDestroy.Card,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesNoCR);
			}

			if (DidPlayerAnswerYes(storedResults))
			{
				IEnumerator buryCardCR = GameController.MoveCard(
					DecisionMaker,
					dca.CardToDestroy.Card,
					this.TurnTaker.Deck,
					true,
					cardSource: GetCardSource()
				);

				// then move a yaojing from your trash to your hand.
				// then move an ongoing from your trash to your hand.
				IEnumerator recoverCardCR = GameController.SelectAndMoveCard(
					DecisionMaker,
					(Card c) => cards.Contains(c),
					this.HeroTurnTaker.Hand,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(buryCardCR);
					yield return GameController.StartCoroutine(recoverCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(buryCardCR);
					GameController.ExhaustCoroutine(recoverCardCR);
				}
			}

			yield break;
		}
	}
}