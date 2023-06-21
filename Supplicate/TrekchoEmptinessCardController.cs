using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class TrekchoEmptinessCardController : SupplicateBaseCardController
	{
		/*
		 * Discard your hand.
		 * for each card discarded this way, 1 hero target regains 1 hp.
		 * 
		 * draw X cards,
		 * where X = the number of yaojing card in play plus 1
		 * 
		 * if you drew fewer cards than you discarded,
		 * you may destroy an ongoing card.
		 */

		public TrekchoEmptinessCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsYaojingCriteria());
		}

		public override IEnumerator Play()
		{
			// for each card discarded this way, 1 hero target regains 1 hp.
			ITrigger healTrigger = AddTrigger(
				(MoveCardAction m) =>
					m.Destination == HeroTurnTaker.Trash
					&& m.Origin.IsHand
					&& m.IsDiscard
					&& m.CanChangeDestination,
				(MoveCardAction m) => GameController.SelectAndGainHP(
					DecisionMaker,
					1,
					additionalCriteria: (Card c) => IsHeroTarget(c),
					cardSource: GetCardSource()
				),
				TriggerType.GainHP,
				TriggerTiming.After
			);

			// Discard your hand.
			List<DiscardCardAction> storedDiscards = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.DiscardHand(
				DecisionMaker,
				false,
				storedDiscards,
				TurnTaker,
				GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			RemoveTrigger(healTrigger);

			// ...where X = the number of yaojing card in play plus 1
			int drawNumeral = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsYaojing(c)).Count() + 1;

			// draw X cards...
			List<DrawCardAction> storedDraws = new List<DrawCardAction>();
			IEnumerator drawCR = DrawCards(DecisionMaker, drawNumeral, storedResults: storedDraws);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(drawCR);
			}
			else
			{
				GameController.ExhaustCoroutine(drawCR);
			}

			// if you drew fewer cards than you discarded...
			if (
				storedDiscards.Where((DiscardCardAction d) => d.WasCardDiscarded).Count()
				> storedDraws.Where((DrawCardAction d) => d.DidDrawCard).Count()
			)
			{
				// ...you may destroy an ongoing card.
				IEnumerator destroyCR = GameController.SelectAndDestroyCard(
					DecisionMaker,
					new LinqCardCriteria((Card c) => IsOngoing(c), "ongoing"),
					true,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(destroyCR);
				}
			}

			yield break;
		}
	}
}