using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public abstract class CharmBaseCardController : TheurgyBaseCardController
	{
		// Play this card next to a hero character card.
		// [add some unique triggers]
		// That hero gains the following power:
		// Power: [power has unique effect, then...]
		//  Destroy this card..

		public CharmBaseCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddAsPowerContributor();
		}

		protected abstract string CharmPowerText { get; }

		public override void AddTriggers()
		{
			base.AddTriggers();
			AddIfTheTargetThatThisCardIsNextToLeavesPlayDestroyThisCardTrigger();
		}

		public override IEnumerator DeterminePlayLocation(
			List<MoveCardDestination> storedResults,
			bool isPutIntoPlay,
			List<IDecision> decisionSources,
			Location overridePlayArea = null,
			LinqTurnTakerCriteria additionalTurnTakerCriteria = null
		)
		{
			//When this card enters play, put it next to a hero
			IEnumerator selectHeroCR = SelectCardThisCardWillMoveNextTo(
				new LinqCardCriteria(
					(Card c) => c.IsHeroCharacterCard && !c.IsIncapacitatedOrOutOfGame,
					"hero character"
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

		public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cc)
		{
			// this defines what displays in a green box in the UI

			// need to be prepared for both SW Sentinels AND Guise
			Card cardToCheck = GetCardThisCardIsNextTo();
			if (cardToCheck == null)
			{
				cardToCheck = base.Card.Location.OwnerTurnTaker.CharacterCard;
			}

			if (cc.Card == cardToCheck)
			{
				//If this card is next to a hero, they have this power
				return new Power[1]
				{
					new Power(
						cc.HeroTurnTakerController,
						cc,
						CharmPowerText,
						CharmPowerResponse(cc),
						0,
						null,
						GetCardSource()
					)
				};
			}
			return null;
		}

		protected abstract IEnumerator CharmPowerResponse(CardController cc);
	}
}