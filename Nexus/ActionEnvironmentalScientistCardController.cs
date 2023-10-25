using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class ActionEnvironmentalScientistCardController : CardController
	{
		/*
		 * whenever you destroy an environment target, you may draw a card.
		 * 
		 * the first time each turn you destroy a villain target, you may play a card.
		 */

		private const string HasPlayedCard = "HasPlayedCard";

		public ActionEnvironmentalScientistCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// whenever you destroy an environment target,
			// the first time each turn you destroy a villain target,
			AddTrigger(
				(DestroyCardAction destroy) =>
					destroy.CardSource != null
					&& destroy.CardToDestroy.CanBeDestroyed
					&& destroy.WasCardDestroyed
					&& destroy.CardSource.Card.Owner == this.TurnTaker
					&& destroy.CardToDestroy.Card.IsTarget
					&& destroy.PostDestroyDestinationCanBeChanged
					&& (destroy.DealDamageAction == null || destroy.DealDamageAction.DamageSource.Card == this.CharacterCard)
					&& (destroy.DealDamageAction != null || destroy.CardSource.Card.IsOneShot || destroy.CardSource.Card.HasPowers),
				DestroyCardResponse,
				new TriggerType[2] { TriggerType.DrawCard, TriggerType.PlayCard },
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(HasPlayedCard),
				TriggerType.Hidden
			);

			base.AddTriggers();
		}

		private IEnumerator DestroyCardResponse(DestroyCardAction dca)
		{
			if (dca.CardToDestroy.Card.IsEnvironmentTarget)
			{
				// you may draw a card. (environment)
				IEnumerator drawCR = DrawCard(HeroTurnTaker, true);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
				}
			}
			else if (IsVillainTarget(dca.CardToDestroy.Card) && !IsPropertyTrue(HasPlayedCard))
			{
				SetCardPropertyToTrueIfRealAction(HasPlayedCard);

				// you may play a card. (villain, first time)
				IEnumerator playCardCR = SelectAndPlayCardFromHand(this.HeroTurnTakerController);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCardCR);
				}
			}

			yield break;
		}
	}
}