using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class HeartflowInducerCardController : CaptainCainBaseCardController
	{
		/*
		 * When you play an ongoing card from your hand, you may draw a card.
		 * 
		 * POWER:
		 * your ongoing cards are indestructible while this power resolves.
		 * Play 1 card.
		 */

		public HeartflowInducerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

		private bool duringPower = false;

		public override void AddTriggers()
		{
			// When you play an ongoing card from your hand, you may draw a card.
			AddTrigger(
				(CardEntersPlayAction p) =>
					!p.IsPutIntoPlay
					&& p.TurnTakerController == this.TurnTakerController
					&& p.Origin == this.HeroTurnTaker.Hand
					&& IsOngoing(p.CardEnteringPlay),
				DrawCardResponse,
				TriggerType.DrawCard,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator DrawCardResponse(CardEntersPlayAction c)
		{
			string message = $"{this.Card.Title} allows {this.TurnTaker.Name} to draw a card.";
			IEnumerator messageCR = GameController.SendMessageAction(message, Priority.Medium, GetCardSource());
			IEnumerator drawCardCR = DrawCard(null, true);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(messageCR);
				yield return GameController.StartCoroutine(drawCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(messageCR);
				GameController.ExhaustCoroutine(drawCardCR);
			}
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			if (duringPower && IsOngoing(card) && card.Owner == this.Card.Owner)
			{
				return true;
			}
			return false;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int playNumeral = GetPowerNumeral(0, 1);

			// your ongoing cards are indestructible while this power resolves.
			duringPower = true;

			// Play 1 card.
			IEnumerator playCardsCR = GameController.SelectAndPlayCardsFromHand(
				DecisionMaker,
				playNumeral,
				false,
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

			duringPower = false;
			yield break;
		}
	}
}