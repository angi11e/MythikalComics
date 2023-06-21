using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Angille.TheVisionary
{
	public class MythikalInspireCharacterCardController : HeroCharacterCardController
	{
		public MythikalInspireCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			AddThisCardControllerToList(CardControllerListType.EnteringGameCheck);
			AddThisCardControllerToList(CardControllerListType.ModifiesKeywords);
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);
			bool damageDealt = false;
			bool noProblems = true;
			List<Card> alreadyGone = new List<Card>();

			while (noProblems && FindCardsWhere(
				(Card c) => c.IsAtLocationRecursive(this.TurnTaker.PlayArea)
				&& c.IsTarget
				&& c.IsInPlayAndHasGameText
				&& !alreadyGone.Contains(c)
			).Any())
			{
				// Each target in your play area...
				List<SelectCardDecision> storedResult = new List<SelectCardDecision>();
				IEnumerator pickTargetCR = GameController.SelectCardAndStoreResults(
					DecisionMaker,
					SelectionType.SelectTargetFriendly,
					new LinqCardCriteria(
						(Card c) => c.IsTarget
							&& c.IsInPlayAndNotUnderCard
							&& c.IsAtLocationRecursive(this.TurnTaker.PlayArea)
							&& !alreadyGone.Contains(c),
						"target in your play area",
						useCardsSuffix: false
					),
					storedResult,
					optional: false,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(pickTargetCR);
				}
				else
				{
					GameController.ExhaustCoroutine(pickTargetCR);
				}

				SelectCardDecision selection = storedResult.FirstOrDefault();
				if (selection != null && selection.SelectedCard != null)
				{
					// ...deals 1 target 1 psychic damage.
					List<DealDamageAction> storedDamage = new List<DealDamageAction>();
					IEnumerator damageInstanceCR = GameController.SelectTargetsAndDealDamage(
						DecisionMaker,
						new DamageSource(GameController, selection.SelectedCard),
						damageNumeral,
						DamageType.Psychic,
						targetNumeral,
						false,
						targetNumeral,
						storedResultsDamage: storedDamage,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(damageInstanceCR);
					}
					else
					{
						GameController.ExhaustCoroutine(damageInstanceCR);
					}

					damageDealt = damageDealt || DidDealDamage(storedDamage);
					alreadyGone.Add(selection.SelectedCard);
				}
				else
				{
					noProblems = false;
				}
			}

			// If no damage is dealt this way...
			if (!damageDealt)
			{
				// ...play the top card of your deck.
				IEnumerator playTopCR = GameController.PlayTopCard(
					DecisionMaker,
					this.TurnTakerController,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playTopCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playTopCR);
				}
			}
			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					break;

				case 1:
					break;

				case 2:
					break;
			}
			yield break;
		}

		public override void AddTriggers()
		{
			AddMaintainTargetTriggers(
				(Card c) => c.Owner == this.TurnTaker && c.DoKeywordsContain("phantasm", true, true),
				5,
				new List<string> { "phantasm" }
			);
			base.AddTriggers();
		}

		public override IEnumerator PerformEnteringGameResponse()
		{
			// Your ongoing and distortion cards instead have the keyword “phantasm”...
			List<Card> affectedCards = FindCardsWhere(new LinqCardCriteria(
				(Card c) => c.Owner == this.TurnTaker &&
				(c.DoKeywordsContain("ongoing", true, true) || c.DoKeywordsContain("distortion", true, true))
			)).ToList();
			foreach (Card card in affectedCards)
			{
				FieldInfo keywordsField = card.Definition.GetType().GetField(
					"_keywords",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
				);
				List<string> keywords = new List<string>();
				keywords.Add("phantasm");
				if (card.IsLimited)
				{
					keywords.Add("limited");
				}
				keywordsField.SetValue(card.Definition, keywords);
			}

			/*
						List<Card> affectedCards = FindCardsWhere(new LinqCardCriteria(
							(Card c) => c.Owner == this.TurnTaker && !c.IsCharacter && !c.IsOneShot
						)).ToList();

						IEnumerator addPhantasmCR = GameController.ModifyKeywords(
							"phantasm",
							true,
							affectedCards,
							GetCardSource()
						);
						IEnumerator removeDistortionCR = GameController.ModifyKeywords(
							"distortion",
							false,
							affectedCards,
							GetCardSource()
						);
						IEnumerator removeOngoingCR = GameController.ModifyKeywords(
							"ongoing",
							false,
							affectedCards,
							GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(addPhantasmCR);
							yield return GameController.StartCoroutine(removeDistortionCR);
							yield return GameController.StartCoroutine(removeOngoingCR);
						}
						else
						{
							GameController.ExhaustCoroutine(addPhantasmCR);
							GameController.ExhaustCoroutine(removeDistortionCR);
							GameController.ExhaustCoroutine(removeOngoingCR);
						}
			*/

			// ...and a max hp of 5.
			IEnumerator targetCR = GameController.MakeTargettable(
				DecisionMaker,
				(Card c) => c.DoKeywordsContain("phantasm", true, true) && c.Owner == this.TurnTaker,
				(Card c) => 5,
				GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(targetCR);
			}
			else
			{
				GameController.ExhaustCoroutine(targetCR);
			}

			yield break;
		}

/*
		public override bool AskIfCardContainsKeyword(
			Card card,
			string keyword,
			bool evenIfUnderCard = false,
			bool evenIfFaceDown = false
		)
		{
			if (card.Owner == this.TurnTaker && !card.IsCharacter && !card.IsOneShot)
			{
				if (keyword == "ongoing" || keyword == "distortion")
				{
					return false;
				}
				if (keyword == "phantasm")
				{
					return true;
				}
			}
			return base.AskIfCardContainsKeyword(card, keyword, evenIfUnderCard, evenIfFaceDown);
		}

		public override IEnumerable<string> AskForCardAdditionalKeywords(Card card)
		{
			if (card.Owner == this.TurnTaker && !card.IsCharacter && !card.IsOneShot)
			{
				return new string[1] { "phantasm" };
			}
			return base.AskForCardAdditionalKeywords(card);
		}
*/
	}
}