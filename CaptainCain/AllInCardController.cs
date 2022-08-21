using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class AllInCardController : CaptainCainBaseCardController
	{
		/*
		 * {CaptainCainCharacter} deals 1 target 3 melee damage.
		 * That target deals {CaptainCainCharacter} 2 melee damage.
		 * 
		 * 👊: {CaptainCainCharacter} may deal up to 2 additional targets 1 melee damage each.
		 * 
		 * 💧: When {CaptainCainCharacter} would take damage, he regains that many HP instead.
		 */

		public AllInCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			ITrigger healTrigger = null;
			if (IsBloodActive)
			{
				// When {CaptainCainCharacter} would take damage, he regains that many HP instead.
				healTrigger = AddPreventDamageTrigger(
					(DealDamageAction dda) => dda.Target == this.CharacterCard,
					(DealDamageAction dda) => GameController.GainHP(
						this.CharacterCard,
						dda.Amount,
						cardSource: GetCardSource()
					),
					new TriggerType[1] { TriggerType.GainHP },
					true
				);
			}

			// {CaptainCainCharacter} deals 1 target 3 melee damage.
			List<DealDamageAction> theTarget = new List<DealDamageAction>();
			IEnumerator damageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				3,
				DamageType.Melee,
				1,
				false,
				1,
				storedResultsDamage: theTarget,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(damageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(damageCR);
			}

			Card damageSource = null;

			// That target deals {CaptainCainCharacter} 2 melee damage.
			if (theTarget.Any() && !theTarget.FirstOrDefault().DidDestroyTarget)
			{
				damageSource = theTarget.FirstOrDefault().Target;
				IEnumerator reflectDamageCR = DealDamage(
					damageSource,
					this.CharacterCard,
					2,
					DamageType.Melee,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(reflectDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(reflectDamageCR);
				}
			}

			if (IsFistActive)
			{
				// {CaptainCainCharacter} may deal up to 2 additional targets 1 melee damage each.
				IEnumerator fistDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					1,
					DamageType.Melee,
					2,
					false,
					0,
					additionalCriteria: (Card c) => damageSource == null || c != damageSource,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(fistDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(fistDamageCR);
				}
			}

			if (healTrigger != null)
			{
				RemoveTrigger(healTrigger);
			}

			yield break;
		}
	}
}