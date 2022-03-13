using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.WhatsHerFace
{
	public class SubtleInfluenceCardController : RecallBaseCardController
	{
		/*
		 * Play this card next to a character card.
		 * increase damage taken by targets in that character's play area by 1.
		 * reduce damage dealt by targets in that character's play area by 1.
		 * If that target leaves play, destroy this card.
		 */

		public SubtleInfluenceCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		// Play this card next to a character card.
		protected override LinqCardCriteria CustomCriteria => new LinqCardCriteria(
			(Card c) => c.IsCharacter && c.IsInPlayAndHasGameText && !c.IsIncapacitatedOrOutOfGame,
			"character card"
		);

		public override void AddTriggers()
		{
			// increase damage taken by targets in that character's play area by 1.
			AddIncreaseDamageTrigger(
				(DealDamageAction dd) =>
					dd.Target.Location.OwnerTurnTaker == base.Card.Location.OwnerTurnTaker,
				1
			);

			// reduce damage dealt by targets in that character's play area by 1.
			AddReduceDamageTrigger(
				(DealDamageAction dd) =>
					dd.DamageSource.IsOneOfTheseCards(base.Card.Location.OwnerTurnTaker.GetPlayAreaCards()),
				(DealDamageAction dd) => 1
			);

			// At the start of your turn, either discard a card or destroy this card.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.TurnTaker,
				DiscardOrDestroyResponse,
				new TriggerType[2] { TriggerType.DiscardCard, TriggerType.DestroySelf }
			);

			// If that target leaves play, destroy this card. (removed from card)
			// AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();

			base.AddTriggers();
		}

		private IEnumerator DiscardOrDestroyResponse(PhaseChangeAction pca)
		{
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = GameController.SelectAndDiscardCard(
				DecisionMaker,
				optional: true,
				storedResults: storedResults,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (!DidDiscardCards(storedResults))
			{
				IEnumerator destroyCR = GameController.DestroyCard(
					DecisionMaker,
					base.Card,
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
			}
		}
	}
}