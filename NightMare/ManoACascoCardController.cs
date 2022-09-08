using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.NightMare
{
	public class ManoACascoCardController : NightMareBaseCardController
	{
		/*
		 * Play this card next to a non-Hero target.
		 * Damage dealt to that target is irreducible.
		 * Damage dealt by that target is irreducible and redirected to {NightMare}.
		 * At the start of your turn, you may destroy this card.
		 * 
		 * DISCARD
		 * Choose a target.
		 * Choose a target. the next damage dealt to that target is irreducible.
		 */

		public ManoACascoCardController(
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
			// Play this card next to a non-Hero target.
			IEnumerator selectHeroCR = SelectCardThisCardWillMoveNextTo(
				new LinqCardCriteria(
					(Card c) => !c.IsHero && c.IsTarget,
					"non-hero targets",
					useCardsSuffix: false
				),
				storedResults,
				isPutIntoPlay,
				decisionSources
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectHeroCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectHeroCR);
			}
			yield break;
		}

		public override void AddTriggers()
		{
			// Damage dealt to that target is irreducible.
			// Damage dealt by that target is irreducible...
			AddMakeDamageIrreducibleTrigger((DealDamageAction dd) =>
				dd.Target == GetCardThisCardIsNextTo()
				|| (dd.DamageSource.IsCard && dd.DamageSource.Card == GetCardThisCardIsNextTo())
			);

			// ...and redirected to {NightMare}.
			AddRedirectDamageTrigger(
				(DealDamageAction dd) =>
					GetCardThisCardIsNextTo() != null
					&& dd.DamageSource.IsCard
					&& dd.DamageSource.Card == GetCardThisCardIsNextTo(),
				() => base.CharacterCard
			);

			// At the start of your turn, you may destroy this card.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == TurnTaker,
				YouMayDestroyThisCardResponse,
				TriggerType.DestroyCard
			);

			AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false);
			base.AddTriggers();
		}

		protected override IEnumerator DiscardResponse(GameAction ga)
		{
			// Choose a target.
			List<Card> targetList = GameController.FindTargetsInPlay((Card c) => true).ToList();
			List<SelectTargetDecision> targets = new List<SelectTargetDecision>();
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				DecisionMaker,
				targetList,
				targets,
				selectionType: SelectionType.Custom,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectTargetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectTargetCR);
			}

			// The next damage dealt to that target is irreducible.
			Card theTarget = targets.FirstOrDefault().SelectedCard;
			if (theTarget.IsTarget)
			{
				MakeDamageIrreducibleStatusEffect dealtToSE = new MakeDamageIrreducibleStatusEffect();
				dealtToSE.NumberOfUses = 1;
				dealtToSE.TargetCriteria.IsSpecificCard = theTarget;

				IEnumerator dealtToCR = AddStatusEffect(dealtToSE);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealtToCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealtToCR);
				}
			}

			yield break;
		}

		public override CustomDecisionText GetCustomDecisionText(IDecision decision)
		{
			return new CustomDecisionText(
				"select a target to make damage irreducible against",
				"they are selecting a target to make damage irreducible against",
				"vote for a target to make damage irreducible against",
				"target to make damage irreducible against"
			);
		}
	}
}