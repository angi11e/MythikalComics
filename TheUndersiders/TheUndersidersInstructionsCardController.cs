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
			SpecialStringMaker.ShowNumberOfCardsInPlay(
				new LinqCardCriteria((Card c) => IsVillainTarget(c) && c.IsVillainCharacterCard, "villain character target")
			);
			AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
		}

		public override void AddDependentSpecialStrings()
		{
			Card warlords = TurnTaker.FindCard("WarlordsOfBrockton");
			if (warlords != null)
			{
				SpecialStringMaker.ShowNumberOfCardsUnderCard(warlords);
			}
		}

		public override void AddSideTriggers()
		{
			// Whenever there are no villain character targets in play, the heroes win.
			AddSideTrigger(AddTrigger(
				(GameAction g) => GameController.HasGameStarted
					&& !(g is GameOverAction)
					&& !(g is IncrementAchievementAction)
					&& FindCardsWhere(
						(Card c) => c.IsInPlayAndHasGameText && IsVillainTarget(c) && c.IsVillainCharacterCard
					).Count() == 0
					&& FindCardsWhere((Card c) => c.IsVillainCharacterCard && c.IsFlipped).Count() > 0,
				(GameAction g) => DefeatedResponse(g),
				new TriggerType[2]
				{
					TriggerType.GameOver,
					TriggerType.Hidden
				},
				TriggerTiming.After
			));

			if (!this.Card.IsFlipped)
			{
				// At the start of the first Villain turn, move H-2 cards from warlords into play
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker && !FindCardsWhere(
						(Card c) => c.IsInPlayAndHasGameText && c.IsVillainCharacterCard && (IsVillainTarget(c) || c.IsFlipped)
					).Any(),
					(PhaseChangeAction p) => StartOfGamePlayVillains(p),
					TriggerType.PlayCard
				));

				// At the start of the Villain turn, move the top card from beneath Warlords of Brockton into the villain play area.
				AddSideTrigger(AddStartOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					(PhaseChangeAction p) => GameController.PlayCard(
						this.TurnTakerController,
						FindCard("WarlordsOfBrockton").UnderLocation.TopCard,
						true
					),
					TriggerType.PlayCard
				));

				// At the end of the Villain turn, play the top card of the environment deck. Flip this card.
				AddSideTrigger(AddEndOfTurnTrigger(
					(TurnTaker tt) => tt == this.TurnTaker,
					(PhaseChangeAction p) => FrontSideFlip(p),
					TriggerType.FlipCard
				));

				// Advanced: When a villain character card enters play, play the top card of the villain deck.
				if (base.GameController.Game.IsAdvanced)
				{
					AddSideTrigger(AddTrigger(
						(CardEntersPlayAction p) => p.CardEnteringPlay != this.Card
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
					(TurnTaker tt) => tt == this.TurnTaker,
					PlayTheTopCardOfTheVillainDeckWithMessageResponse,
					TriggerType.PlayCard
				));

				// When a villain character card is incapacitated, destroy all environment cards. The villain target with the highest HP deals the hero character target with the lowest HP {H} melee damage. Flip this card.
				AddSideTrigger(AddTrigger(
					(FlipCardAction fc) => fc.CardToFlip.Card != this.Card && fc.CardToFlip.Card.IsVillainCharacterCard,
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
				if (Game.IsAdvanced)
				{
					AddSideTrigger(AddIncreaseDamageTrigger(
						(DealDamageAction dd) => IsHero(dd.Target),
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
				GameController,
				DecisionMaker,
				functionList,
				false,
				cardSource: GetCardSource() // new CardSource(FindCardController(FindCard("TheUndersidersInstructions")))
			);

			IEnumerator selectFunctionCR = GameController.SelectAndPerformFunction(selectFunction);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectFunctionCR);
			}

			for (int i = 0; i < H - 2; i++)
			{
				IEnumerator playVillainCR = GameController.PlayCard(
					TurnTakerController,
					warlords.UnderLocation.TopCard,
					true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playVillainCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playVillainCR);
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
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(moveCardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(moveCardCR);
				}
			}
		}

		private IEnumerator FrontSideFlip(GameAction triggerAction)
		{
			IEnumerator playEnvCR = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(triggerAction);
			IEnumerator flipCR = GameController.FlipCard(
				this,
				treatAsPlayed: false,
				treatAsPutIntoPlay: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playEnvCR);
				yield return GameController.StartCoroutine(flipCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playEnvCR);
				GameController.ExhaustCoroutine(flipCR);
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
				(Card c) => IsHeroTarget(c),
				(Card c) => H,
				DamageType.Melee,
				damageSourceInfo: new TargetInfo(
					HighestLowestHP.HighestHP,
					1,
					1,
					new LinqCardCriteria((Card c) => IsVillainTarget(c), "The villain target with the highest HP")
				)
			);

			IEnumerator flipCR = GameController.FlipCard(
				this.CharacterCardController,
				treatAsPlayed: false,
				treatAsPutIntoPlay: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyEnviromentCR);
				yield return GameController.StartCoroutine(retributionCR);
				yield return GameController.StartCoroutine(flipCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyEnviromentCR);
				GameController.ExhaustCoroutine(retributionCR);
				GameController.ExhaustCoroutine(flipCR);
			}

			yield break;
		}

		public override bool AskIfCardIsIndestructible(Card card)
		{
			return card == this.Card;
		}
	}
}
