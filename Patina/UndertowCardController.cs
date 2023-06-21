using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Patina
{
	public class UndertowCardController : PatinaBaseCardController
	{
		/*
		 * Destroy an ongoing or equipment card.
		 * 
		 * If it was a hero card,
		 *  {Patina} deals 1 target 1 cold damage and X psychic damage in either order,
		 *  where X = the number of [u]water[/u] cards in play plus 1.
		 */

		public UndertowCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsInPlay(IsWaterCriteria((Card c) => !c.IsOneShot));
		}

		public override IEnumerator Play()
		{
			// Destroy an ongoing or equipment card.
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				this.HeroTurnTakerController,
				new LinqCardCriteria(
					(Card c) => IsOngoing(c) || IsEquipment(c),
					"ongoing or equipment"
				),
				optional: false,
				storedResults,
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

			// If it was a hero card...
			if (DidDestroyCard(storedResults) && IsHero(storedResults.FirstOrDefault().CardToDestroy.Card)) {
				// {Patina} deals 1 target...
				List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
				IEnumerable<Card> choices = FindCardsWhere(
					new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText)
				);
				IEnumerator selectTargetCR = GameController.SelectTargetAndStoreResults(
					this.HeroTurnTakerController,
					choices,
					selectedTarget,
					selectionType: SelectionType.SelectTarget,
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

				if (selectedTarget != null && selectedTarget.Any())
				{
					SelectTargetDecision selectedTargetDecision = selectedTarget.FirstOrDefault();
					if (selectedTargetDecision != null && selectedTargetDecision.SelectedCard != null)
					{
						Card theCard = selectedTargetDecision.SelectedCard;

						// ...in either order.
						List<SelectDamageTypeDecision> chosenType = new List<SelectDamageTypeDecision>();
						IEnumerator chooseDamageCR = GameController.SelectDamageType(
							DecisionMaker,
							chosenType,
							new DamageType[] { DamageType.Cold, DamageType.Psychic },
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(chooseDamageCR);
						}
						else
						{
							GameController.ExhaustCoroutine(chooseDamageCR);
						}

						DamageType damageType = chosenType.First(
							(SelectDamageTypeDecision d) => d.Completed
						).SelectedDamageType ?? DamageType.Cold;

						// ...1 cold damage and X psychic damage...
						IEnumerator dealColdCR = DealDamage(
							this.CharacterCard,
							(Card c) => c == theCard,
							1,
							DamageType.Cold
						);

						IEnumerator dealPsychicCR = DealDamage(
							this.CharacterCard,
							(Card c) => c == theCard,
							// ...where X = the number of water cards in play plus 1.
							WaterCardsInPlay + 1,
							DamageType.Psychic
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine((damageType == DamageType.Cold) ? dealColdCR : dealPsychicCR);
							yield return GameController.StartCoroutine((damageType == DamageType.Cold) ? dealPsychicCR : dealColdCR);
						}
						else
						{
							GameController.ExhaustCoroutine((damageType == DamageType.Cold) ? dealColdCR : dealPsychicCR);
							GameController.ExhaustCoroutine((damageType == DamageType.Cold) ? dealPsychicCR : dealColdCR);
						}
					}
				}
			}

			yield break;
		}
	}
}