using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
    public class TheUndersidersInstructionsCardController : VillainCharacterCardController
	{
		// At the start of the Villain turn, move the top card from beneath Warlords of Brockton into the villain play area.
		// At the end of the Villain turn, if there are no environment cards in play, play the top card of the environment deck. Flip this card.
		// Whenever there are no villain character targets in play, the heroes win.
		// Advanced: When a villain character card enters play, play the top card of the villain deck.
		////////////////////////////////////////////////////////
		// At the start of the villain turn, play the top card of the villain deck.
        // When a villain character card is incapacitated, destroy all environment cards. The villain target with the highest HP deals the hero character target with the lowest HP {H} melee damage. Flip this card.
        // Whenever there are no villain character targets in play, the heroes win."
		// Advanced: Increase damage taken by hero targets by 1.


		public TheUndersidersInstructionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
		{
			base.SpecialStringMaker.ShowNumberOfCardsInPlay(
				new LinqCardCriteria((Card c) => c.IsVillainTarget && c.IsVillainCharacterCard, "villain character target")
			);
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

		public override void AddDependentSpecialStrings()
		{
			Card warlords = base.TurnTaker.FindCard("WarlordsOfBrockton");
			if (warlords != null)
			{
				base.SpecialStringMaker.ShowNumberOfCardsUnderCard(warlords);
			}
		}

		public override void AddSideTriggers()
		{
			AddSideTrigger(AddTrigger(
				(GameAction g) => base.GameController.HasGameStarted
					&& !(g is GameOverAction)
					&& !(g is IncrementAchievementAction)
					&& FindCardsWhere(
						(Card c) => c.IsInPlayAndHasGameText && c.IsVillainTarget && c.IsVillainCharacterCard
					).Count() == 0,
				(GameAction g) => DefeatedResponse(g),
				new TriggerType[2]
				{
					TriggerType.GameOver,
					TriggerType.Hidden
				},
				TriggerTiming.After
			));

			if (!base.Card.IsFlipped)
			{
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					(PhaseChangeAction p) => base.GameController.MoveIntoPlay(
						base.TurnTakerController,
						FindCard("WarlordsOfBrockton").UnderLocation.TopCard,
						base.TurnTaker,
						GetCardSource()
					),
					TriggerType.MoveCard
				));

				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					(PhaseChangeAction p) => FrontSideFlip(p),
					TriggerType.FlipCard,
					(PhaseChangeAction p) => FindCardsWhere((Card c) => c.IsEnvironment && c.IsInPlay).Count() <= 0
				));

				if (base.GameController.Game.IsAdvanced)
				{
					AddSideTrigger(AddTrigger(
						(CardEntersPlayAction p) => p.CardEnteringPlay != base.Card
							&& p.CardEnteringPlay.IsVillainCharacterCard,
						(CardEntersPlayAction p) => PlayTheTopCardOfTheVillainDeckWithMessageResponse(p),
						TriggerType.PlayCard,
						TriggerTiming.After
					));
				}

				return;
			}
			/* old Ennead stuff - still need 
			AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, delegate
			{
				GameController gameController = base.GameController;
				HeroTurnTakerController decisionMaker = DecisionMaker;
				Func<Card, bool> criteria = (Card c) => c.IsVillainTarget;
				int amount = base.H - 2;
				CardSource cardSource = GetCardSource();
				return gameController.GainHP(decisionMaker, criteria, amount, null, optional: false, null, null, null, cardSource);
			}, TriggerType.GainHP));
			if (base.GameController.Game.IsAdvanced)
			{
				AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dd) => dd.DamageSource.IsVillainTarget, 1));
			}
			*/
		}

		private IEnumerator FrontSideFlip(GameAction triggerAction)
		{
			IEnumerator discardEnvCR = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(triggerAction);
			IEnumerator flipCR = base.GameController.FlipCard(
				this,
				treatAsPlayed: false,
				treatAsPutIntoPlay: false,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(discardEnvCR);
				yield return base.GameController.StartCoroutine(flipCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(discardEnvCR);
				base.GameController.ExhaustCoroutine(flipCR);
			}
			yield break;
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			return card == base.Card;
		}
	}
}
