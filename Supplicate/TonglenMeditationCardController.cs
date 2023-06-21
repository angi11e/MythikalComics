using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Supplicate
{
	public class TonglenMeditationCardController : SupplicateBaseCardController
	{
		/*
		 * after psychic damage is dealt to {Supplicate} or a yaojing card,
		 * that card regains 1 hp.
		 * 
		 * POWER:
		 * {Supplicate} and each yaojing target regains 1 hp.
		 * You may play a yaojing card.
		 */

		public TonglenMeditationCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			base.AddTriggers();

			// after psychic damage is dealt to {Supplicate} or a yaojing card,
			AddTrigger(
				(DealDamageAction dda) => dda.DamageType == DamageType.Psychic
					&& (dda.Target == this.CharacterCard || IsYaojing(dda.Target))
					&& dda.DidDealDamage
					&& !dda.DidDestroyTarget,
				// that card regains 1 hp.
				(DealDamageAction dda) => GameController.GainHP(
					dda.Target,
					1,
					cardSource: GetCardSource()
				),
				TriggerType.GainHP,
				TriggerTiming.After
			);
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// {Supplicate} and each yaojing target regains 1 hp.
			IEnumerator healCR = GameController.GainHP(
				DecisionMaker,
				(Card c) => c == this.CharacterCard || IsYaojing(c),
				1,
				cardSource: GetCardSource()
			);

			// You may play a yaojing card.
			IEnumerator playYaojingCR = SelectAndPlayCardFromHand(
				DecisionMaker,
				cardCriteria: new LinqCardCriteria((Card c) => IsYaojing(c), "yaojing")
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(healCR);
				yield return GameController.StartCoroutine(playYaojingCR);
			}
			else
			{
				GameController.ExhaustCoroutine(healCR);
				GameController.ExhaustCoroutine(playYaojingCR);
			}
			yield break;
		}
	}
}