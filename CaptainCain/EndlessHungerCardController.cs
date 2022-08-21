using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class EndlessHungerCardController : CaptainCainSetupCardController
	{
		/*
		 * When this card enters play, destroy any of your [u]fist[/u] cards in play.
		 * When this card is destroyed, you may move a [u]blood[/u] card from your trash to your hand.
		 * 
		 * Treat {Blood} effects as active.
		 * 
		 * increase HP recovery by {CaptainCainCharacter} by 1.
		 */

		public EndlessHungerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// increase HP recovery by {CaptainCainCharacter} by 1.
			AddTrigger(
				(GainHPAction gh) => gh.HpGainer == this.CharacterCard,
				(GainHPAction gh) => GameController.IncreaseHPGain(gh, 1, GetCardSource()),
				new TriggerType[2] { TriggerType.IncreaseHPGain, TriggerType.ModifyHPGain },
				TriggerTiming.Before
			);
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[1] {
			TriggerType.MoveCard
		};

		protected override IEnumerator SetupDestroyResponse(GameAction ga)
		{
			// When this card is destroyed, you may move a [u]blood[/u] card from your trash to your hand.
			IEnumerator moveCardCR = SearchForCards(
				DecisionMaker,
				false,
				true,
				0,
				1,
				IsBloodCriteria(),
				false,
				true,
				false,
				true
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveCardCR);
			}

			yield break;
		}
	}
}