using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;

namespace Angille.TheUndersiders
{
	public abstract class TheUndersidersVillainCardController : VillainCharacterCardController
	{
		public TheUndersidersVillainCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override bool CanBeDestroyed
		{
			get { return false; }
		}

		public override void AddSideTriggers()
		{
			if (this.Card.IsFlipped)
			{
				AddSideTrigger(AddCannotDealDamageTrigger((Card c) => c == this.Card));
			}
		}

		public override IEnumerator BeforeFlipCardImmediateResponse(FlipCardAction flip)
		{
			CardSource cardSource = flip.CardSource;
			if (cardSource == null && flip.ActionSource != null)
			{
				cardSource = flip.ActionSource.CardSource;
			}
			if (cardSource == null)
			{
				cardSource = GetCardSource();
			}

			if (!flip.CardToFlip.Card.IsFlipped) {
				IEnumerator untargetCR = GameController.RemoveTarget(
					this.Card,
					leavesPlayIfInPlay: true,
					cardSource
				);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(untargetCR);
				}
				else
				{
					GameController.ExhaustCoroutine(untargetCR);
				}
			}

			yield break;
		}

		public override IEnumerator DestroyAttempted(DestroyCardAction destroyCard)
		{
			FlipCardAction action = new FlipCardAction(
				GameController,
				this,
				treatAsPlayedIfFaceUp: false,
				treatAsPutIntoPlayIfFaceUp: false,
				destroyCard.ActionSource
			);

			IEnumerator flipInsteadCR = DoAction(action);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(flipInsteadCR);
			}
			else
			{
				GameController.ExhaustCoroutine(flipInsteadCR);
			}
		}
	}
}
