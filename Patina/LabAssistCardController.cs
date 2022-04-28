using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class LabAssistCardController : PatinaBaseCardController
	{
		/*
		 * Whenever a hero uses a Power on an equipment card, you may increase
		 *  by 1 or reduce by 1 one of the numerals in the text of that Power.
		 * 
		 * POWER
		 * 1 Hero may discard a card. Any who do so may use 1 power.
		 */

		public LabAssistCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// Whenever a hero uses a Power on an equipment card...
			AddTrigger(
				(UsePowerAction p) =>
					base.TurnTakerController != null
					&& p.Power.CardController != null
					&& IsEquipment(p.Power.CardController.Card)
					&& p.Power.Index >= 0
// && p.Power.CardSource.CardController.CardWithoutReplacements.Location.HighestRecursiveLocation == base.TurnTaker.PlayArea
					&& ((p.Power.CopiedFromCardController != null
						&& p.Power.CopiedFromCardController.HasPowerNumerals())
						|| p.Power.CardController.HasPowerNumerals()),
				ModifyNumeralResponse,
				TriggerType.ModifyNumeral,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		private IEnumerator ModifyNumeralResponse(UsePowerAction p)
		{
			// ...you may increase by 1 or reduce by 1 one of the numerals in the text of that Power.
			CardController cc = (
				(p.Power.CopiedFromCardController != null) ? p.Power.CopiedFromCardController : (
					(p.Power.CardSource.CardController == null
					|| p.Power.CardSource.CardController == p.Power.CardController) ?
					p.Power.CardController : p.Power.CardSource.CardController
				)
			);

			IEnumerable<string> powerNumeralStrings = cc.GetPowerNumeralStrings(p.Power, p.Power.Index);
			List<SelectWordDecision> storedResults = new List<SelectWordDecision>();
			Card[] associatedCards = new Card[1] { cc.Card };

			IEnumerator selectionCR = GameController.SelectWord(
				DecisionMaker,
				powerNumeralStrings,
				SelectionType.SelectNumeral,
				storedResults,
				optional: true,
				associatedCards,
				GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectionCR);
			}

			SelectWordDecision selectWordDecision = storedResults.FirstOrDefault();
			if (selectWordDecision != null && selectWordDecision.Index.HasValue)
			{
				int index = selectWordDecision.Index.Value;
				int num = cc.GetPowerNumerals(p.Power, p.Power.Index).ElementAt(index);
				string displayText =
					$"Increase by 1: {selectWordDecision.SelectedWord.Replace(num.ToString(), (num + 1).ToString())}";
				string displayText2 =
					$"Reduce by 1: {selectWordDecision.SelectedWord.Replace(num.ToString(), (num - 1).ToString())}";

				IEnumerable<Function> functionChoices = new Function[2]
				{
					new Function(
						base.HeroTurnTakerController,
						displayText,
						SelectionType.ModifyNumeral,
						() => ModifyFunction(p.Power, index, 1)
					),
					new Function(
						base.HeroTurnTakerController,
						displayText2,
						SelectionType.ModifyNumeral,
						() => ModifyFunction(p.Power, index, -1)
					)
				};
				SelectFunctionDecision selectFunction = new SelectFunctionDecision(
					GameController,
					base.HeroTurnTakerController,
					functionChoices,
					optional: true,
					null,
					null,
					associatedCards,
					GetCardSource()
				);

				IEnumerator adjustCR = GameController.SelectAndPerformFunction(selectFunction, null, associatedCards);
				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(adjustCR);
				}
				else
				{
					GameController.ExhaustCoroutine(adjustCR);
				}
			}
			yield break;
		}

		private IEnumerator ModifyFunction(Power p, int numeralIndex, int amount)
		{
			CardController cardController = p.CardController;
			string identifier = p.CardController.Card.Identifier;
			if (p.CopiedFromCardController != null)
			{
				cardController = p.CopiedFromCardController;
			}
			else if (p.IsContributionFromCardSource)
			{
				cardController = p.CardSource.CardController;
				identifier = cardController.Card.Identifier;
			}
			cardController.AddPowerNumeralModification(identifier, numeralIndex, amount);
			yield return null;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// 1 player may discard 2 cards.
			int heroNumeral = GetPowerNumeral(0, 1);
			int discardNumeral = GetPowerNumeral(1, 2);
			int powerNumeral = GetPowerNumeral(2, 1);

			return GameController.SelectTurnTakersAndDoAction(
				DecisionMaker,
				new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && tt.ToHero().HasCardsInHand),
				SelectionType.DiscardCard,
				(TurnTaker tt) => DiscardForPower(tt, discardNumeral, powerNumeral),
				heroNumeral,
				false,
				heroNumeral,
				cardSource: GetCardSource()
			);
		}

		private IEnumerator DiscardForPower(TurnTaker tt, int discardNumeral, int powerNumeral)
		{
			// Any who do so may use 1 power.
			HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());

			List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				httc,
				discardNumeral,
				optional: false,
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

			if (DidDiscardCards(storedResults, discardNumeral))
			{
				for (int i = 0; i < powerNumeral; i++)
				{
					IEnumerator powerCR = GameController.SelectAndUsePower(
						httc,
						optional: true,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(powerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(powerCR);
					}
				}
			}
			else
			{
				IEnumerator messageCR = GameController.SendMessageAction(
					tt.Name + " did not discard enough cards for powers.",
					Priority.High,
					GetCardSource(),
					null,
					showCardSource: true
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(messageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(messageCR);
				}
			}

			yield break;
		}
	}
}