using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public class JustYouNMePardnerCardController : PecosBillBaseCardController
	{
		/*
		 * You may activate a [u]tall tale[/u] text.
		 * 
		 * Destroy all your [u]hyperbole[/u] cards, then all your [u]folk[/u] cards.
		 * {PecosBill} regains X HP, then deals 1 target X melee damage,
		 * where X = the number of cards destroyed this way times 2.
		 */

		public JustYouNMePardnerCardController(
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

			// Destroy all your [u]hyperbole[/u] cards...
			List<DestroyCardAction> destroyedHyperboles = new List<DestroyCardAction>();
			IEnumerator destroyHyperboleCR = GameController.DestroyCards(
				DecisionMaker,
				IsHyperboleCriteria(),
				storedResults: destroyedHyperboles,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyHyperboleCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyHyperboleCR);
			}

			// ...then all your [u]folk[/u] cards.
			List<DestroyCardAction> destroyedFolks = new List<DestroyCardAction>();
			IEnumerator destroyFolkCR = GameController.DestroyCards(
				DecisionMaker,
				IsFolkCriteria(),
				storedResults: destroyedFolks,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyFolkCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyFolkCR);
			}

			// ...where X = the number of cards destroyed this way times 2.
			int damageNumeral = (destroyedHyperboles.Where(
				(DestroyCardAction d) => d.CardToDestroy != null && d.WasCardDestroyed
			).Count() + destroyedFolks.Where(
				(DestroyCardAction d) => d.CardToDestroy != null && d.WasCardDestroyed
			).Count()) * 2;

			// {PecosBill} regains X HP...
			IEnumerator healingCR = GameController.GainHP(
				this.CharacterCard,
				damageNumeral,
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

			// ...then deals 1 target X melee damage...
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
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

			yield break;
		}
	}
}