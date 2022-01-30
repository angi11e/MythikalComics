using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class WhereDidSheComeFromCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a target.
		 * At the start of your turn, {WhatsHerFace} deals that target 2 projectile damage.
		 * At the end of your turn, move this card next to a new target.
		 * If the card this card is next to leaves play, destroy this card.
		 */

		public WhereDidSheComeFromCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		// Play this card next to a target.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => c.IsTarget && c.IsInPlayAndHasGameText,
			"target"
		);

		public override void AddTriggers()
		{
			// At the start of your turn, {WhatsHerFace} deals that target 2 projectile damage.
			AddDealDamageAtStartOfTurnTrigger(
				base.TurnTaker,
				base.CharacterCard,
				(Card c) => c == GetCardThisCardIsNextTo(),
				TargetType.All,
				2,
				DamageType.Projectile
			);

			// At the end of your turn, move this card next to a new target.
			AddEndOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				MoveCardResponse,
				TriggerType.MoveCard
			);

			// If the card this card is next to leaves play, destroy this card.
			AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();

			base.AddTriggers();
		}

		private IEnumerator MoveCardResponse(PhaseChangeAction pca)
		{
			Card notThisOne = GetCardThisCardIsNextTo();

			List<Card> targetList = GameController.FindTargetsInPlay((Card c) => c != notThisOne).ToList();
			List<SelectTargetDecision> targets = new List<SelectTargetDecision>();
			IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
				DecisionMaker,
				targetList,
				targets,
				selectionType: SelectionType.MoveCardNextToCard,
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

			Card newHome = targets.FirstOrDefault().SelectedCard;
			if (newHome.IsTarget)
			{
				IEnumerator moveCR = GameController.MoveCard(
					base.TurnTakerController,
					base.Card,
					newHome.NextToLocation,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCR);
				}
			}

			yield break;
		}
	}
}