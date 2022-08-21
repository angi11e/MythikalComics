using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class TwoFistedJusticeCardController : CaptainCainBaseCardController
	{
		/*
		 * {CaptainCainCharacter} deals up to 5 targets 1 melee damage each.
		 * 
		 * 👊: This damage is irreducible.
		 * 
		 * 💧: {CaptainCainCharacter} regains 1 HP each time a target takes damage this way.
		 */

		public TwoFistedJusticeCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// 💧: {CaptainCainCharacter} regains 1 HP each time a target takes damage this way.
			ITrigger healTrigger = null;
			if (IsBloodActive)
			{
				healTrigger = AddTrigger(
					(DealDamageAction d) => d.DidDealDamage && d.CardSource != null && d.CardSource.CardController == this,
					(DealDamageAction d) => GameController.GainHP(
						this.CharacterCard,
						1,
						cardSource: GetCardSource()
					),
					new List<TriggerType> { TriggerType.GainHP },
					TriggerTiming.After
				);
			}

			// {CaptainCainCharacter} deals up to 5 targets 1 melee damage each.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				1,
				DamageType.Melee,
				5,
				false,
				0,
				// 👊: This damage is irreducible.
				IsFistActive,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			if (healTrigger != null)
			{
				RemoveTrigger(healTrigger);
			}

			yield break;
		}
	}
}