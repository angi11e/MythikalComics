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
			// Whenever there are no villain character targets in play, the heroes win.
			AddSideTrigger(AddTrigger(
				(GameAction g) => base.GameController.HasGameStarted
					&& base.Game.Round > 1
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
				// At the start of the first Villain turn, move H-2 cards from warlords into play
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker && base.Game.Round == 1,
					(PhaseChangeAction p) => StartOfGamePlayVillains(p),
					TriggerType.PlayCard
				));

				// At the start of the Villain turn, move the top card from beneath Warlords of Brockton into the villain play area.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					(PhaseChangeAction p) => base.GameController.PlayCard(
						base.TurnTakerController,
						FindCard("WarlordsOfBrockton").UnderLocation.TopCard,
						true
					),
					TriggerType.PlayCard
				));

				// At the end of the Villain turn, play the top card of the environment deck. Flip this card.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					(PhaseChangeAction p) => FrontSideFlip(p),
					TriggerType.FlipCard
//					(PhaseChangeAction p) => FindCardsWhere((Card c) => c.IsEnvironment && c.IsInPlay).Count() <= 0
				));

				// Advanced: When a villain character card enters play, play the top card of the villain deck.
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
			else
			{
				// At the start of the villain turn, play the top card of the villain deck.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == base.TurnTaker,
					base.PlayTheTopCardOfTheVillainDeckWithMessageResponse,
					TriggerType.PlayCard
				));

				// When a villain character card is incapacitated, destroy all environment cards. The villain target with the highest HP deals the hero character target with the lowest HP {H} melee damage. Flip this card.
				AddSideTrigger(AddTrigger(
					(FlipCardAction fc) => fc.CardToFlip.Card != base.Card && fc.CardToFlip.Card.IsVillainCharacterCard,
					(FlipCardAction fc) => BackSideFlip(fc),
					new TriggerType[]
					{
						TriggerType.DestroyCard,
						TriggerType.DealDamage,
						TriggerType.FlipCard
					},
					TriggerTiming.After
				));
				
				// Advanced: Increase damage taken by hero targets by 1.
				if (base.GameController.Game.IsAdvanced)
				{
					AddSideTrigger(AddIncreaseDamageTrigger(
						(DealDamageAction dd) => dd.Target.IsHero,
						1
					));
				}
			}
		}

		private IEnumerator StartOfGamePlayVillains(PhaseChangeAction pca)
		{
			Card warlords = FindCard("WarlordsOfBrockton");
			List<Function> functionList = new List<Function>();

			// randomize
			functionList.Add(
				new Function(
					FindHeroTurnTakerController(Game.HeroTurnTakers.FirstOrDefault()),
					"Randomize villains",
					SelectionType.ShuffleDeck,
					() => DoNothing()
				)
			);

			// masters
			functionList.Add(
				new Function(
					DecisionMaker,
					"masters",
					SelectionType.MoveCardBelowCard,
					() => ReorderWarlords(new List<string>
					{
						"ImpCharacter",
						"FoilCharacter",
						"GrueCharacter",
						"TattletaleCharacter",
						"RegentCharacter",
						"ParianCharacter",
						"BitchCharacter",
						"SkitterCharacter"
					})
				)
			);

			// thinkers
			functionList.Add(
				new Function(
					DecisionMaker,
					"thinkers",
					SelectionType.MoveCardBelowCard,
					() => ReorderWarlords(new List<string>
					{
						"BitchCharacter",
						"FoilCharacter",
						"ParianCharacter",
						"GrueCharacter",
						"ImpCharacter",
						"RegentCharacter",
						"SkitterCharacter",
						"TattletaleCharacter"
					})
				)
			);

			// strangers
			functionList.Add(
				new Function(
					DecisionMaker,
					"strangers",
					SelectionType.MoveCardBelowCard,
					() => ReorderWarlords(new List<string>
					{
						"BitchCharacter",
						"ParianCharacter",
						"FoilCharacter",
						"TattletaleCharacter",
						"SkitterCharacter",
						"RegentCharacter",
						"GrueCharacter",
						"ImpCharacter"
					})
				)
			);
			
			// strikers
			functionList.Add(
				new Function(
					DecisionMaker,
					"strikers",
					SelectionType.MoveCardBelowCard,
					() => ReorderWarlords(new List<string>
					{
						"ParianCharacter",
						"TattletaleCharacter",
						"ImpCharacter",
						"RegentCharacter",
						"SkitterCharacter",
						"BitchCharacter",
						"GrueCharacter",
						"FoilCharacter"
					})
				)
			);

			// ask for which one
			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				base.GameController,
				this.DecisionMaker,
				functionList,
				false,
				cardSource: GetCardSource() // new CardSource(FindCardController(FindCard("TheUndersidersInstructions")))
			);

			IEnumerator selectFunctionCR = base.GameController.SelectAndPerformFunction(selectFunction);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectFunctionCR);
			}

			for (int i = 0; i < base.H - 2; i++)
			{
				IEnumerator playVillainCR = base.GameController.PlayCard(
					TurnTakerController,
					warlords.UnderLocation.TopCard,
					true
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(playVillainCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(playVillainCR);
				}
			}
		}

		private IEnumerator ReorderWarlords(List<string> order)
		{
			Card warlords = FindCard("WarlordsOfBrockton");
			for (int i = order.Count() - 1; i >= 0; i--)
			{
				Log.Debug("moving " + order[i]);
				IEnumerator moveCardCR = GameController.MoveCard(
					TurnTakerController,
					FindCard(order[i]),
					warlords.UnderLocation,
					true
				);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(moveCardCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(moveCardCR);
				}
			}
		}

		private IEnumerator FrontSideFlip(GameAction triggerAction)
		{
			IEnumerator playEnvCR = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(triggerAction);
			IEnumerator flipCR = base.GameController.FlipCard(
				this,
				treatAsPlayed: false,
				treatAsPutIntoPlay: false,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(playEnvCR);
				yield return base.GameController.StartCoroutine(flipCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(playEnvCR);
				base.GameController.ExhaustCoroutine(flipCR);
			}
			yield break;
		}

		private IEnumerator BackSideFlip(GameAction triggerAction)
		{
			IEnumerator destroyEnviromentCR = GameController.DestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c.IsEnvironment),
				cardSource: GetCardSource()
			);

			IEnumerator retributionCR = DealDamageToLowestHP(
				null,
				1,
				(Card c) => c.IsHero,
				(Card c) => base.H,
				DamageType.Melee,
				damageSourceInfo: new TargetInfo(
					HighestLowestHP.HighestHP,
					1,
					1,
					new LinqCardCriteria((Card c) => c.IsVillainTarget, "The villain target with the highest HP")
				)
			);

			IEnumerator flipCR = base.GameController.FlipCard(
				base.CharacterCardController,
				treatAsPlayed: false,
				treatAsPutIntoPlay: false,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(destroyEnviromentCR);
				yield return base.GameController.StartCoroutine(retributionCR);
				yield return base.GameController.StartCoroutine(flipCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(destroyEnviromentCR);
				base.GameController.ExhaustCoroutine(retributionCR);
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
