using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class InTheNickOfTimeCardController : CaptainCainSetupCardController
	{
		/*
		 * When this card enters play, destroy any of your [u]blood[/u] cards in play.
		 * When this card is destroyed, you may destroy an ongoing card.
		 * 
		 * Treat {Fist} effects as active.
		 * 
		 * When a hero target would be dealt damage, you may redirect the damage to {CaptainCainCharacter}.
		 */

		public InTheNickOfTimeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// When a hero target would be dealt damage, you may redirect the damage to {CaptainCainCharacter}.
			AddRedirectDamageTrigger(
				(DealDamageAction dd) => dd.Target != this.CharacterCard && dd.Target.IsHero,
				() => this.CharacterCard,
				optional: true
			);
		}

		protected override TriggerType[] DestructionTriggers => new TriggerType[1] {
			TriggerType.DestroyCard
		};

		protected override IEnumerator SetupDestroyResponse(GameAction ga)
		{
			// When this card is destroyed, you may destroy an ongoing card.
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing"),
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

			yield break;
		}
	}
}