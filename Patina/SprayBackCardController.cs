using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class SprayBackCardController : PatinaBaseCardController
	{
		/*
		 * When a villain target deals damage to a hero target, you may destroy this card.
		 * If you do so, {Patina} deals the source of that damage X projectile damage,
		 *  where X = the number of water cards in play plus 1.
		 */

		public SprayBackCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria((Card c) => !c.IsOneShot));
		}

		public override void AddTriggers()
		{
			// When a villain target deals damage to a hero target...
			AddTrigger(
				(DealDamageAction dd) =>
					dd.Target.IsHero
					&& dd.DamageSource.IsVillainTarget
					&& dd.DidDealDamage,
				RetributionResponse,
				TriggerType.DealDamage,
				TriggerTiming.After,
				ActionDescription.DamageTaken
			);

			base.AddTriggers();
		}

		private IEnumerator RetributionResponse(DealDamageAction dd)
		{
			// ...you may destroy this card.
			List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
			IEnumerator yesNoCR = GameController.MakeYesNoCardDecision(
				DecisionMaker,
				SelectionType.DestroySelf,
				this.Card,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(yesNoCR);
			}
			else
			{
				GameController.ExhaustCoroutine(yesNoCR);
			}

			// If you do so...
			if (DidPlayerAnswerYes(storedResults))
			{
				// ...where X = the number of water cards in play plus 1.
				int damageNumeral = FindCardsWhere(
					(Card c) => c.IsInPlayAndHasGameText && IsWater(c) && !c.IsOneShot
				).Count() + 1;

				// ...{Patina} deals the source of that damage X projectile damage...
				IEnumerator dealDamageCR = DealDamage(
					this.CharacterCard,
					(Card c) => c.IsTarget && c == dd.DamageSource.Card,
					damageNumeral,
					DamageType.Projectile
				);
				IEnumerator destroyCR = GameController.DestroyCard(
					DecisionMaker,
					this.Card,
					optional: false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
					GameController.ExhaustCoroutine(destroyCR);
				}
			}

			yield break;
		}
	}
}