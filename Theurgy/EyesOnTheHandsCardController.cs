using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class EyesOnTheHandsCardController : TheurgyBaseCardController
	{
		// Whenever you discard a card, you may put it into your hand instead.
		// If you do, either destroy this card or Theurgy deals herself 2 irreducible psychic damage.

		public EyesOnTheHandsCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever you discard a card...
			AddTrigger(
				(MoveCardAction d) => d.CardToMove.Owner == base.TurnTaker &&
				(d.Origin.IsHand || d.Origin.IsDeck || d.Origin.IsRevealed) &&
				d.Destination == d.CardToMove.Owner.Trash && d.IsDiscard && d.CanChangeDestination,
				DealWithTheDiscard,
				new TriggerType[2] {
					TriggerType.MoveCard,
					TriggerType.FlipCard
				},
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		private IEnumerator DealWithTheDiscard(GameAction a)
		{
			Card card = null;
			DiscardCardAction discardObj = null;
			MoveCardAction moveObj = null;
			DestroyCardAction destroyObj = null;

			if (a is DiscardCardAction)
			{
				discardObj = a as DiscardCardAction;
				card = discardObj.CardToDiscard;
			}
			else if (a is MoveCardAction)
			{
				moveObj = a as MoveCardAction;
				card = moveObj.CardToMove;
			}
			else if (a is DestroyCardAction)
			{
				destroyObj = a as DestroyCardAction;
				card = destroyObj.CardToDestroy.Card;
			}

			// you may put it into your hand instead.
			List<YesNoCardDecision> yesOrNo = new List<YesNoCardDecision>();
			IEnumerator yesNoInHandCR = base.GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.MoveCardToHand,
				card,
				null,
				yesOrNo,
				null,
				GetCardSource()
			);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(yesNoInHandCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(yesNoInHandCR);
			}

			if (yesOrNo.Count > 0 && yesOrNo.FirstOrDefault().Answer == true)
			{
				// If you do...
				if (a is DiscardCardAction)
				{
					discardObj.SetDestination(base.HeroTurnTaker.Hand);
				}
				else if (a is MoveCardAction)
				{
					moveObj.SetDestination(base.HeroTurnTaker.Hand);
				}
				else if (a is DestroyCardAction)
				{
					destroyObj.SetPostDestroyDestination(base.HeroTurnTaker.Hand);
				}

				// You may destroy this card...
				List<DestroyCardAction> destroyResult = new List<DestroyCardAction>();
				IEnumerator destroyCR = this.GameController.DestroyCard(
					this.DecisionMaker,
					this.Card,
					true,
					destroyResult,
					cardSource: this.GetCardSource(null)
				);
				if (this.UseUnityCoroutines)
				{
					yield return this.GameController.StartCoroutine(destroyCR);
				}
				else
				{
					this.GameController.ExhaustCoroutine(destroyCR);
				}

				if (destroyResult == null || destroyResult.Count == 0 || !destroyResult.First().WasCardDestroyed)
				{
					// if you did not...
					// Theurgy deals herself 2 irreducible psychic damage.

					IEnumerator selfHarmCR = base.DealDamage(
						base.CharacterCard,
						base.CharacterCard,
						2,
						DamageType.Psychic,
						true
					);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(selfHarmCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(selfHarmCR);
					}
				}
			}

			yield break;
		}
	}
}