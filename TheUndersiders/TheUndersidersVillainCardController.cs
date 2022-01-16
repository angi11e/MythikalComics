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
			if (base.Card.IsFlipped)
			{
				AddSideTrigger(AddCannotDealDamageTrigger((Card c) => c == base.Card));
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
				IEnumerator untargetCR = base.GameController.RemoveTarget(
					base.Card,
					leavesPlayIfInPlay: true,
					cardSource
				);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(untargetCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(untargetCR);
				}
			}

			yield break;
		}

		public override IEnumerator DestroyAttempted(DestroyCardAction destroyCard)
		{
			FlipCardAction action = new FlipCardAction(
				base.GameController,
				this,
				treatAsPlayedIfFaceUp: false,
				treatAsPutIntoPlayIfFaceUp: false,
				destroyCard.ActionSource
			);

			IEnumerator flipInsteadCR = DoAction(action);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(flipInsteadCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(flipInsteadCR);
			}
		}
	}
}
