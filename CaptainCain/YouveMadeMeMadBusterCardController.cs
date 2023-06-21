using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class YouveMadeMeMadBusterCardController : CaptainCainSetupCardController
	{
		/*
		 * When this card enters play, destroy any of your [u]blood[/u] cards in play.
		 * When this card is destroyed, {CaptainCainCharacter} deals each non-hero target 1 melee damage.
		 * 
		 * Treat {Fist} effects as active.
		 * 
		 * You may use an additional power during your power phase.
		 */

		public YouveMadeMeMadBusterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.IncreasePhaseActionCount);
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// You may use an additional power during your power phase.
			AddAdditionalPhaseActionTrigger((TurnTaker tt) => tt == this.TurnTaker, Phase.UsePower, 1);
		}

		public override IEnumerator Play()
		{
			IEnumerator morePowerCR = IncreasePhaseActionCountIfInPhase(
				(TurnTaker tt) => tt == this.TurnTaker,
				Phase.UsePower,
				1
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(base.Play());
				yield return GameController.StartCoroutine(morePowerCR);
			}
			else
			{
				GameController.ExhaustCoroutine(base.Play());
				GameController.ExhaustCoroutine(morePowerCR);
			}
		}

		public override bool AskIfIncreasingCurrentPhaseActionCount()
		{
			if (GameController.ActiveTurnPhase.IsUsePower)
			{
				return GameController.ActiveTurnTaker == this.TurnTaker;
			}
			return false;
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[1] {
			TriggerType.DealDamage
		};

		protected override IEnumerator SetupDestroyResponse(GameAction ga)
		{
			// When this card is destroyed, {CaptainCainCharacter} deals each non-hero target 1 melee damage.
			GameController.AddCardControllerToList(CardControllerListType.CanCauseDamageOutOfPlay, this);
			GameController.AddInhibitorException(this, (GameAction g) => true);

			IEnumerator damageCR = DealDamage(
				this.CharacterCard,
				(Card c) => !IsHero(c),
				1,
				DamageType.Melee
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			GameController.RemoveCardControllerFromList(CardControllerListType.CanCauseDamageOutOfPlay, this);
			GameController.RemoveInhibitorException(this);

			yield break;
		}
	}
}