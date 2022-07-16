using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class HypnoticPendulumCardController : CardController
	{
		private const string _FirstCard = "FirstCard";

		public HypnoticPendulumCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowHasBeenUsedThisTurn(
				_FirstCard,
				"{0} has already influenced a hero this turn.",
				"{0} has not yet influenced a hero this turn."
			);
		}

		public override void AddTriggers()
		{
			// The first time each turn a hero card enters play...
			AddTrigger(
				(CardEntersPlayAction c) => c.CardEnteringPlay.IsHero && !IsPropertyTrue(_FirstCard),
				PlayCardResponse,
				new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard },
				TriggerTiming.After
			);

			AddAfterLeavesPlayAction(
				(GameAction ga) => ResetFlagAfterLeavesPlay(_FirstCard),
				TriggerType.Hidden
			);

			// If a hero target deals itself damage, destroy this card.
			AddTrigger(
				(DealDamageAction dda) => dda.Target.IsHero && dda.Target == dda.DamageSource.Card,
				(DealDamageAction dda) => GameController.DestroyCard(
					DecisionMaker,
					base.Card,
					cardSource: GetCardSource()
				),
				TriggerType.DestroySelf,
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator PlayCardResponse(CardEntersPlayAction cepa)
		{
			SetCardPropertyToTrueIfRealAction(_FirstCard);

			// ...its player must either discard 2 cards or play the top card of a villain deck.
			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				FindHeroTurnTakerController(cepa.CardEnteringPlay.Owner.ToHero()),
				2,
				optional: true,
				null,
				storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			if (!DidDiscardCards(storedResults, 2))
			{
				IEnumerator playVillainCR = PlayTheTopCardOfTheVillainDeckWithMessageResponse(null);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playVillainCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playVillainCR);
				}
			}

			yield break;
		}
	}
}
