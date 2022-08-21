using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class ManOfThePeopleCardController : CaptainCainSetupCardController
	{
		/*
		 * When this card enters play, destroy any of your [u]blood[/u] cards in play.
		 * When this card is destroyed, another player may play a card.
		 * 
		 * Treat {Fist} effects as active.
		 * 
		 * When you draw a card, another player may draw a card.
		 */

		public ManOfThePeopleCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When you draw a card, another player may draw a card.
			AddTrigger(
				(DrawCardAction dca) => dca.HeroTurnTaker == this.HeroTurnTaker,
				(DrawCardAction dca) => GameController.SelectHeroToDrawCard(
					DecisionMaker,
					additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != this.TurnTaker),
					cardSource: GetCardSource()
				),
				TriggerType.Hidden,
				TriggerTiming.After
			);
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[1] {
			TriggerType.PlayCard
		};

		protected override IEnumerator SetupDestroyResponse(GameAction ga)
		{
			// When this card is destroyed, another player may play a card.
			IEnumerator playCardCR = GameController.SelectHeroToPlayCard(
				DecisionMaker,
				additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != this.TurnTaker),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playCardCR);
			}

			yield break;
		}
	}
}