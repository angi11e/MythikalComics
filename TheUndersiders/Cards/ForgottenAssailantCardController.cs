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
	public class ForgottenAssailantCardController : TheUndersidersBaseCardController
	{
		public ForgottenAssailantCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// The villain character card with the lowest HP deals the hero character card with the most cards in their trash {H - 2} melee damage.
			List<Card> lowestVillain = new List<Card>();
			IEnumerator findLowestCR = GameController.FindTargetWithLowestHitPoints(
				1,
				(Card c) => c.IsVillainCharacterCard && !c.IsIncapacitatedOrOutOfGame,
				lowestVillain,
				cardSource: GetCardSource()
			);

			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(findLowestCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(findLowestCR);
			}

			Card lowestVillainCard = lowestVillain.FirstOrDefault();
			if (lowestVillainCard != null)
			{
				List<TurnTaker> heroList = new List<TurnTaker>();
				IEnumerator trashHeroCR = FindHeroWithMostCardsInTrash(heroList);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(trashHeroCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(trashHeroCR);
				}
				Card trashHero = heroList.FirstOrDefault().CharacterCard;

				if (trashHero.IsTarget)
				{
					IEnumerator trashDamageCR = DealDamage(
						lowestVillainCard,
						trashHero,
						base.H - 2,
						DamageType.Melee
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(trashDamageCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(trashDamageCR);
					}
				}
			}

			// Mask: {ImpCharacter} deals the hero she's in front of 1 melee damage and 1 toxic damage.
			if (IsEnabled("mask"))
			{
				Card maybeImp = ImpCharacter;
				Card heroTarget = null;
				if (!maybeImp.IsFlipped)
				{
					heroTarget = ImpCharacter.Location.OwnerTurnTaker.CharacterCard;
				}
				else
				{
					IEnumerator promptCR = GameController.SendMessageAction(
						"Who recalls Imp fondly?",
						Priority.Medium,
						GetCardSource()
					);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(promptCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(promptCR);
					}

					List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
					IEnumerator pickHeroCR = GameController.SelectHeroCharacterCard(
						DecisionMaker,
						SelectionType.CharacterCard,
						storedResults,
						cardSource: GetCardSource()
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(pickHeroCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(pickHeroCR);
					}

					heroTarget = storedResults.FirstOrDefault().SelectedCard;

					List<Card> villainList = new List<Card>();
					IEnumerator findVillainCR = GameController.FindTargetWithHighestHitPoints(
						1,
						(Card c) => c.IsVillainCharacterCard,
						villainList,
						cardSource: GetCardSource()
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(findVillainCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(findVillainCR);
					}

					maybeImp = villainList.FirstOrDefault();
				}

				if (maybeImp.IsTarget && heroTarget.IsTarget)
				{
					List<DealDamageAction> damageInfo = new List<DealDamageAction>
					{
						new DealDamageAction(
							GetCardSource(),
							new DamageSource(base.GameController, maybeImp),
							null,
							1,
							DamageType.Melee
						),
						new DealDamageAction(
							GetCardSource(),
							new DamageSource(base.GameController, maybeImp),
							null,
							1,
							DamageType.Toxic
						)
					};

					IEnumerator forgottenStrikeCR = DealMultipleInstancesOfDamage(
						damageInfo,
						(Card c) => c == heroTarget,
						numberOfTargets: 1
					);

					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(forgottenStrikeCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(forgottenStrikeCR);
					}
				}
			}

			// Crown: Any hero targets damaged this turn deal themselves 2 psychic damage.
			if (IsEnabled("crown"))
			{
				List<Card> affectedList = GameController.Game.Journal.DealDamageEntriesThisTurn().Select(
					ddje => ddje.TargetCard
				).Distinct().ToList();

				IEnumerator confusedCR = GameController.SelectTargetsToDealDamageToSelf(
					DecisionMaker,
					2,
					DamageType.Psychic,
					null,
					false,
					null,
					additionalCriteria: (Card c) => affectedList.Contains(c) && c.IsHero,
					allowAutoDecide: true,
					cardSource: GetCardSource()
				);

				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(confusedCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(confusedCR);
				}
			}
			yield break;
		}
	}
}
