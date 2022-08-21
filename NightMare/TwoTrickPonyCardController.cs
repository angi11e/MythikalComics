using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class TwoTrickPonyCardController : NightMareBaseCardController
	{
		/*
		 * Whenever you use a power, draw a card.
		 * Whenever you play a card, draw a card.
		 * 
		 * POWER
		 * Discard 1 card. Play 1 card.
		 * 
		 * DISCARD
		 * You may return this card to your hand.
		 */

		public TwoTrickPonyCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever you use a power, draw a card.
			AddTrigger(
				(UsePowerAction p) =>
					p.Power != null
					&& p.Power.TurnTakerController != null
					&& p.Power.TurnTakerController == DecisionMaker,
				(UsePowerAction p) => DrawCard(HeroTurnTaker),
				TriggerType.DrawCard,
				TriggerTiming.After
			);

			// Whenever you play a card, draw a card.
			AddTrigger(
				(PlayCardAction p) =>
					p.CardToPlay != null
					&& p.DecisionMaker == DecisionMaker,
				(PlayCardAction p) => DrawCard(HeroTurnTaker),
				TriggerType.DrawCard,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int discardNumeral = GetPowerNumeral(0, 1);
			int playNumeral = GetPowerNumeral(1, 1);

			// Discard 1 card. Play 1 card.
			IEnumerator discardCR = SelectAndDiscardCards(DecisionMaker, discardNumeral);
			IEnumerator playCardsCR = SelectAndPlayCardsFromHand(DecisionMaker, playNumeral);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
				yield return GameController.StartCoroutine(playCardsCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
				GameController.ExhaustCoroutine(playCardsCR);
			}

			yield break;
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// You may return this card to your hand.
			List<YesNoCardDecision> yesOrNo = new List<YesNoCardDecision>();
			IEnumerator yesNoInHandCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.MoveCardToHand,
				base.Card,
				null,
				yesOrNo,
				null,
				GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesNoInHandCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesNoInHandCR);
			}

			if (yesOrNo.Count > 0 && yesOrNo.FirstOrDefault().Answer == true)
			{
				// If you do...
				/* old code using Timing.Before
				if (ga is DiscardCardAction)
				{
					(ga as DiscardCardAction).SetDestination(base.HeroTurnTaker.Hand);
				}
				else if (ga is MoveCardAction)
				{
					(ga as MoveCardAction).SetDestination(base.HeroTurnTaker.Hand);
				}
				else if (ga is DestroyCardAction)
				{
					(ga as DestroyCardAction).SetPostDestroyDestination(base.HeroTurnTaker.Hand);
				}
				*/

				// new code using Timing.After
				IEnumerator moveCardCR = GameController.MoveCard(
					this.TurnTakerController,
					this.Card,
					this.HeroTurnTaker.Hand,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCardCR);
				}
			}

			yield break;
		}
	}
}