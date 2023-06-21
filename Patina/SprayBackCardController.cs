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
					IsHeroTarget(dd.Target)
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
			List<DestroyCardAction> actions = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				optional: true,
				actions,
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

			// If you do so...
			if (actions.Any() && actions.FirstOrDefault().WasCardDestroyed)
			{
				GameController.AddCardControllerToList(CardControllerListType.CanCauseDamageOutOfPlay, this);
				GameController.AddInhibitorException(this, (GameAction g) => true);


				// ...{Patina} deals the source of that damage X projectile damage...
				IEnumerator dealDamageCR = DealDamage(
					this.CharacterCard,
					(Card c) => c.IsTarget && c == dd.DamageSource.Card,
					// ...where X = the number of water cards in play plus 1.
					WaterCardsInPlay + 1,
					DamageType.Projectile
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}

				GameController.RemoveCardControllerFromList(CardControllerListType.CanCauseDamageOutOfPlay, this);
				GameController.RemoveInhibitorException(this);
			}

			yield break;
		}
	}
}