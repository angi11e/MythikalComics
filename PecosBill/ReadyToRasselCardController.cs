using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class ReadyToRasselCardController : PecosBillBaseCardController
	{
		/*
		 * You may activate a [u]tall tale[/u] text.
		 * 
		 * Select a target.
		 * {PecosBill} deals that target 2 melee damage.
		 * [i]Loyal Lightning[/i] deals that target 2 lightning damage.
		 * [i]Shake the Snake[/i] deals that target 2 toxic damage.
		 * [i]Tamed Twister[/i] deals that target 2 projectile damage.
		 */

		public ReadyToRasselCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// You may activate a [u]tall tale[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"tall tale",
				optional: true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			// {PecosBill} deals that target 2 melee damage.
			List<DealDamageAction> damageInfo = new List<DealDamageAction>();
			damageInfo.Add(new DealDamageAction(
				GetCardSource(),
				new DamageSource(GameController, this.CharacterCard),
				null,
				2,
				DamageType.Melee
			));

			// [i]Loyal Lightning[/i] deals that target 2 lightning damage.
			Card lightning = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Identifier == "LoyalLightning").FirstOrDefault();
			if (lightning != null && lightning.IsInPlayAndNotUnderCard && lightning.IsTarget)
			{
				damageInfo.Add(new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, lightning),
					null,
					2,
					DamageType.Lightning
				));
			}

			// [i]Shake the Snake[/i] deals that target 2 toxic damage.
			Card shake = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Identifier == "ShakeTheSnake").FirstOrDefault();
			if (shake != null && shake.IsInPlayAndNotUnderCard && shake.IsTarget)
			{
				damageInfo.Add(new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, shake),
					null,
					2,
					DamageType.Toxic
				));
			}

			// [i]Tamed Twister[/i] deals that target 2 projectile damage.
			Card twister = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Identifier == "TamedTwister").FirstOrDefault();
			if (twister != null && twister.IsInPlayAndNotUnderCard && twister.IsTarget)
			{
				damageInfo.Add(new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, twister),
					null,
					2,
					DamageType.Projectile
				));
			}

			// Select a target.
			if (damageInfo.Count() > 1)
			{
				IEnumerator dealDamageCR = SelectTargetsAndDealMultipleInstancesOfDamage(
					damageInfo,
					minNumberOfTargets: 1,
					maxNumberOfTargets: 1
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}
			}
			else
			{
				// because SelectTargetsAndDealMultipleInstancesOfDamage does NOT like one instance
				IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
					DecisionMaker,
					new DamageSource(GameController, this.CharacterCard),
					2,
					DamageType.Melee,
					1,
					false,
					1,
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
			}

			yield break;
		}
	}
}