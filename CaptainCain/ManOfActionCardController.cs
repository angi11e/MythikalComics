using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.CaptainCain
{
	public class ManOfActionCardController : CaptainCainBaseCardController
	{
		/*
		 * {CaptainCainCharacter} deals 1 target 2 melee damage,
		 *  then deals a second target melee damage equal to the amount of damage dealt to the first target.
		 * 
		 * 👊: {CaptainCainCharacter} may deal a third target projectile damage
		 *  equal to the amount of damage dealt to the second target.
		 * 
		 * 💧: When a target is destroyed this way, you may draw a card.
		 */

		public ManOfActionCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			ITrigger destroyTrigger = null;

			if (IsBloodActive)
			{
				// 💧: When a target is destroyed this way, you may draw a card.
				destroyTrigger = AddTrigger(
					(DestroyCardAction d) => d.WasCardDestroyed
						&& d.CardSource != null
						&& d.CardSource.Card == this.CharacterCard,
					(DestroyCardAction d) => DrawCard(this.HeroTurnTaker, true),
					TriggerType.DrawCard,
					TriggerTiming.After
				);
			}

			// {CaptainCainCharacter} deals 1 target 2 melee damage,
			DamageSource damageSource = new DamageSource(GameController, this.CharacterCard);
			List<DealDamageAction> firstDamage = new List<DealDamageAction>();
			IEnumerator firstDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				damageSource,
				2,
				DamageType.Melee,
				1,
				false,
				1,
				storedResultsDamage: firstDamage,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(firstDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(firstDamageCR);
			}

			// then deals a second target melee damage equal to the amount of damage dealt to the first target.
			if (firstDamage.Any())
			{
				Card firstTarget = firstDamage.FirstOrDefault().Target;
				List<DealDamageAction> secondDamage = new List<DealDamageAction>();
				IEnumerator secondDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					damageSource,
					firstDamage.FirstOrDefault().Amount,
					DamageType.Melee,
					1,
					false,
					1,
					additionalCriteria: (Card c) => c != firstTarget,
					storedResultsDamage: secondDamage,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(secondDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(secondDamageCR);
				}

				if (IsFistActive && secondDamage.Any())
				{
					// 👊: {CaptainCainCharacter} may deal a third target projectile damage
					// equal to the amount of damage dealt to the second target.
					Card secondTarget = secondDamage.FirstOrDefault().Target;
					IEnumerator thirdDamageCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						damageSource,
						secondDamage.FirstOrDefault().Amount,
						DamageType.Projectile,
						1,
						false,
						0,
						additionalCriteria: (Card c) => c != firstTarget && c != secondTarget,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(thirdDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(thirdDamageCR);
					}
				}
			}

			if (destroyTrigger != null)
			{
				RemoveTrigger(destroyTrigger);
			}

			yield break;
		}
	}
}