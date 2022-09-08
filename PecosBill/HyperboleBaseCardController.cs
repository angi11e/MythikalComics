using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public abstract class HyperboleBaseCardController : PecosBillBaseCardController
	{
		/*
		 * Play this card next to <_folkIdentifier>.
		 * If <_folkIdentifier> is ever not in play, destroy this card.
		 * 
		 * TALL TALE
		 * activateable ability -- wanna make sure this is identified here as it's named the same on all
		 */

		private readonly string _folkIdentifier;

		protected HyperboleBaseCardController(
			Card card,
			TurnTakerController turnTakerController,
			string folkIdentifier
		) : base(card, turnTakerController)
		{
			_folkIdentifier = folkIdentifier;
		}

		public override IEnumerator DeterminePlayLocation(
			List<MoveCardDestination> storedResults,
			bool isPutIntoPlay,
			List<IDecision> decisionSources,
			Location overridePlayArea = null,
			LinqTurnTakerCriteria additionalTurnTakerCriteria = null
		)
		{
			//Place this card next to <_folkIdentifier>
			IEnumerator selectCardCR = SelectCardThisCardWillMoveNextTo(
				new LinqCardCriteria(
					(Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.Identifier == _folkIdentifier,
					_folkIdentifier
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
			base.AddTriggers();

			/*
			AddTrigger<MoveCardAction>(
				(MoveCardAction m) => m.CardToMove.Identifier == _folkIdentifier
					&& m.Origin.IsHeroPlayAreaRecursive
					&& (m.Destination.IsHand || m.Destination.IsTrash),
				RequiredCardMissingDestroySelfResponse,
				TriggerType.DestroySelf,
				TriggerTiming.After
			);
			*/
			AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();
		}

		public override IEnumerator Play()
		{
			IEnumerable<Card> folk = GameController.FindCardsWhere(
				(Card c) => c.IsInPlayAndHasGameText && c.Identifier == _folkIdentifier,
				visibleToCard: GetCardSource()
			);

			if (!folk.Any())
			{
				IEnumerator destroyCR = RequiredCardMissingDestroySelfResponse(null);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(destroyCR);
				}
				else
				{
					GameController.ExhaustCoroutine(destroyCR);
				}
			}

			yield break;
		}

		private IEnumerator RequiredCardMissingDestroySelfResponse(GameAction ga)
		{
			Card folk = FindCard(_folkIdentifier);
			IEnumerator messageCR = GameController.SendMessageAction(
				$"{folk.Title} is not in play, {Card.Title} will be destroyed.",
				Priority.Medium,
				GetCardSource(),
				showCardSource: true
			);

			IEnumerator destroySelfCR = GameController.DestroyCard(
				DecisionMaker,
				Card,
				actionSource: ga,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(messageCR);
				yield return GameController.StartCoroutine(destroySelfCR);
			}
			else
			{
				GameController.ExhaustCoroutine(messageCR);
				GameController.ExhaustCoroutine(destroySelfCR);
			}

			yield break;
		}

		public override IEnumerator ActivateAbilityEx(CardDefinition.ActivatableAbilityDefinition definition)
		{
			if (definition.Name == "tall tale")
			{
				return ActivateTallTale();
			}

			return base.ActivateAbilityEx(definition);
		}

		public virtual IEnumerator ActivateTallTale()
		{
			yield return null;
		}
	}
}