using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class ForgotAboutHerCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a non-hero target.
		 * That target cannot affect or be affected by any of your cards or effects, aside from this one.
		 * At the start of your turn, you may destroy this card.
		 */

		public ForgotAboutHerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowSpecialString(
				() => GetCardThisCardIsNextTo().Title
					+ " has forgotten "
					+ base.TurnTaker.NameRespectingVariant
					+ " and cannot affect or be affected by her.",
				() => true
			).Condition = () => base.Card.IsInPlayAndHasGameText;

			AddThisCardControllerToList(CardControllerListType.ChangesVisibility);
		}

		// Play this card next to a non-hero target.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => !c.IsHero && c.IsTarget && c.IsInPlayAndHasGameText,
			"non-hero target"
		);

		public override IEnumerator Play()
		{
			if (base.Card.Location.IsNextToCard)
			{
				IEnumerator alertCR = base.GameController.SendMessageAction(
					base.Card.Title
						+ " will prevent "
						+ GetCardThisCardIsNextTo().Title
						+ " from affecting "
						+ base.TurnTaker.NameRespectingVariant
						+ ", and vice versa.",
					Priority.High,
					GetCardSource(),
					showCardSource: true
				);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(alertCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(alertCR);
				}
			}

			yield break;
		}

		public override void AddTriggers()
		{
			// That target cannot affect or be affected by any of your cards or effects, aside from this one.
			// NOTE: use Miss Information's Isolated Hero for reference
			// STILL NEEDS LOTS OF WORK BEFORE GOLIVE
			AddTrigger(
				(MakeDecisionsAction md) =>
					md.CardSource != null
					&& (md.CardSource.Card.Owner == TurnTaker || md.CardSource.Card == GetCardThisCardIsNextTo()),
				RemoveDecisionsFromMakeDecisionsResponse,
				TriggerType.RemoveDecision,
				TriggerTiming.Before
			);

			// At the start of your turn, you may destroy this card.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == TurnTaker,
				YouMayDestroyThisCardResponse,
				TriggerType.DestroySelf
			);

			base.AddTriggers();
		}

		public override bool AskIfActionCanBePerformed(GameAction g)
		{
			if (GetCardThisCardIsNextTo() != null)
			{
				bool? flag = g.DoesFirstCardAffectSecondCard(
					(Card c) => c.Owner == TurnTaker && c != base.Card,
					(Card c) => c == GetCardThisCardIsNextTo()
				);

				bool? flag2 = g.DoesFirstCardAffectSecondCard(
					(Card c) => c == GetCardThisCardIsNextTo(),
					(Card c) => c.Owner == TurnTaker && c != base.Card
				);

				if (
					(flag.HasValue && flag.Value)
					|| (flag2.HasValue && flag2.Value)
				)
				{
					return false;
				}
			}
			return true;
		}

		private IEnumerator RemoveDecisionsFromMakeDecisionsResponse(MakeDecisionsAction md)
		{
			md.RemoveDecisions(
				(IDecision d) =>
					d.SelectedCard == GetCardThisCardIsNextTo()
					&& d.HeroTurnTakerController.TurnTaker == TurnTaker
			);

			md.RemoveDecisions(
				(IDecision d) =>
					d.SelectedCard.Owner == TurnTaker
					&& d.SelectedCard != base.Card
					&& d.CardSource.Card == GetCardThisCardIsNextTo()
			);

			yield return DoNothing();
		}

		private TurnTaker GetForgettingCard()
		{
			if (base.Card.Location.OwnerCard != null)
			{
				return base.Card.Location.OwnerCard.Owner;
			}
			return null;
		}

		public override bool? AskIfTurnTakerIsVisibleToCardSource(TurnTaker tt, CardSource cardSource)
		{
			if (cardSource != null && cardSource.Card == GetCardThisCardIsNextTo() && tt == TurnTaker)
			{
				return false;
			}
			return true;
		}
	}
}