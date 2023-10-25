using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class WeightedNetCardController : StarbladeConstructCardController
	{
		/*
		 * play this card next to a target.
		 * reduce damage dealt by that target by 1.
		 * 
		 * TECHNIQUE
		 * this card deals the card this card is next to 1 melee damage and 1 psychic damage.
		 */

		public WeightedNetCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator DeterminePlayLocation(
			List<MoveCardDestination> storedResults,
			bool isPutIntoPlay,
			List<IDecision> decisionSources,
			Location overridePlayArea = null,
			LinqTurnTakerCriteria additionalTurnTakerCriteria = null
		)
		{
			// play this card next to a target.
			IEnumerator selectCardCR = SelectCardThisCardWillMoveNextTo(
				new LinqCardCriteria(
					(Card c) => c.IsTarget && c.IsInPlayAndHasGameText,
					"target"
				),
				storedResults,
				isPutIntoPlay,
				decisionSources
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCardCR);
			}
			yield break;
		}
		public override void AddTriggers()
		{
			// reduce damage dealt by that target by 1.
			AddReduceDamageTrigger(
				(DealDamageAction dd) => dd.DamageSource.IsCard && dd.DamageSource.Card == GetCardThisCardIsNextTo(),
				(DealDamageAction dd) => 1
			);

			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(
				alsoRemoveTriggersFromThisCard: true,
				GetCardThisCardIsNextTo() != null && !GetCardThisCardIsNextTo().IsHeroCharacterCard
			);

			base.AddTriggers();
		}

		public override IEnumerator ActivateTechnique()
		{
			// ...1 melee damage and 1 psychic damage.
			List<DealDamageAction> theDamages = new List<DealDamageAction>
			{
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController,this.Card),
					null,
					1,
					DamageType.Melee
				),
				new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController,this.Card),
					null,
					1,
					DamageType.Psychic
				)
			};

			// this card deals...
			IEnumerator dealDamageCR = DealMultipleInstancesOfDamage(
				theDamages,
				// ...the card this card is next to...
				(Card c) => c == GetCardThisCardIsNextTo()
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