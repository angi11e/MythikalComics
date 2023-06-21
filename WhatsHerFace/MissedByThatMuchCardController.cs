using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class MissedByThatMuchCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a hero ongoing or equipment card.
		 * When that card would be destroyed, prevent that destruction, then destroy this card.
		 * If that card leaves play in any other way, return this card to your hand.
		 */

		public MissedByThatMuchCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		// Play this card next to a hero ongoing or equipment card.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => c.IsInPlayAndHasGameText && IsHero(c) && (IsOngoing(c) || IsEquipment(c)),
			"hero ongoing or equipment"
		);

		public override void AddTriggers()
		{
			// When that card would be destroyed, prevent that destruction, then destroy this card.
			AddTrigger(
				(DestroyCardAction dca) => dca.CardToDestroy.Card == GetCardThisCardIsNextTo(),
				DestroyButNotResponse,
				TriggerType.CancelAction,
				TriggerTiming.Before,
				priority: TriggerPriority.High
			);

			// If that card leaves play in any other way, return this card to your hand.
			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToYourHandTrigger();

			base.AddTriggers();
		}

		private IEnumerator DestroyButNotResponse(DestroyCardAction dca)
		{
			// When that card would be destroyed, prevent that destruction...
			IEnumerator cancelCR = CancelAction(dca);

			// ...then destroy this card.
			IEnumerator destructionCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(cancelCR);
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(cancelCR);
				GameController.ExhaustCoroutine(destructionCR);
			}

			yield break;
		}
	}
}