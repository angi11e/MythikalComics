using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class ItsJustAScratchCardController : CaptainCainSetupCardController
	{
		/*
		 * When this card enters play, destroy any of your [u]fist[/u] cards in play.
		 * When this card is destroyed, {CaptainCainCharacter} regains 4 HP.
		 * 
		 * Treat {Blood} effects as active.
		 * 
		 * When {CaptainCainCharacter} drops to 0 or fewer HP, restore {CaptainCainCharacter} to 10 HP.
		 * Then, move this card to the bottom of your deck.
		 */

		public ItsJustAScratchCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When {CaptainCainCharacter} drops to 0 or fewer HP, restore {CaptainCainCharacter} to 10 HP.
			AddWhenHPDropsToZeroOrBelowRestoreHPTriggers(
				() => this.CharacterCard,
				() => 10,
				false,
				BuryResponse,
				preventDamage: false
			);
		}

		private IEnumerator BuryResponse(GameAction ga)
		{
			// Then, move this card to the bottom of your deck.
			IEnumerator buryCR = GameController.MoveCard(
				DecisionMaker,
				this.Card,
				this.TurnTaker.Deck,
				true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(buryCR);
			}
			else
			{
				GameController.ExhaustCoroutine(buryCR);
			}

			yield break;
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[1] {
			TriggerType.GainHP
		};

		protected override IEnumerator SetupDestroyResponse(GameAction ga)
		{
			// When this card is destroyed, {CaptainCainCharacter} regains 4 HP.
			IEnumerator healingCR = GameController.GainHP(
				this.CharacterCard,
				4,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healingCR);
			}

			yield break;
		}
	}
}