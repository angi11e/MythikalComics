using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public abstract class CaptainCainSetupCardController : CaptainCainBaseCardController
	{
		protected CaptainCainSetupCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
		{
		}

		protected abstract TriggerType[] DestructionTriggers { get; }

		public override void AddTriggers()
		{
			base.AddTriggers();

			// when this card is destroyed...
			AddWhenDestroyedTrigger(SetupDestroyResponse, DestructionTriggers);
		}

		public override IEnumerator Play()
		{
			IEnumerator destroyCR = GameController.DestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.Owner == this.TurnTaker && (IsBlood(this.Card) ? IsFist(c) : IsBlood(c))),
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

		protected abstract IEnumerator SetupDestroyResponse(GameAction ga);
	}
}