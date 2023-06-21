using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Athena
{
	public class AthenaCharacterCardController : AthenaBaseCharacterCardController
	{
		public AthenaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Trash, IsManifestCriteria());
		}

		public override IEnumerator UsePower(int index = 0)
		{
			// You may move an [u]aspect[/u] card from your trash into play.
			IEnumerator moveCardCR = SearchForCards(
				DecisionMaker,
				false,
				true,
				0,
				1,
				IsManifestCriteria(),
				true,
				false,
				false,
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

			// {Athena} deals 1 target 1 radiant damage.
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);

			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Radiant,
				targetNumeral,
				false,
				targetNumeral,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One hero may deal 1 target 1 radiant damage.
					IEnumerator dealDamageCR = GameController.SelectHeroToSelectTargetAndDealDamage(
						DecisionMaker,
						1,
						DamageType.Radiant,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(dealDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(dealDamageCR);
					}
					break;
				case 1:
					// Select a target. Reduce the next damage dealt to that target by 2.
					List<SelectCardDecision> storedCard = new List<SelectCardDecision>();
					IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.ReduceNextDamageTaken,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && c.IsTarget,
							"target",
							useCardsSuffix: false
						),
						storedCard,
						optional: false,
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

					SelectCardDecision selectCardDecision = storedCard.FirstOrDefault();
					if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
					{
						ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(2);
						reduceDamageStatusEffect.NumberOfUses = 1;
						reduceDamageStatusEffect.TargetCriteria.IsSpecificCard = selectCardDecision.SelectedCard;
						IEnumerator reduceCR = AddStatusEffect(reduceDamageStatusEffect);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(reduceCR);
						}
						else
						{
							GameController.ExhaustCoroutine(reduceCR);
						}
					}
					break;
				case 2:
					// One hero may use a power now.
					IEnumerator usePowerCR = GameController.SelectHeroToUsePower(DecisionMaker);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(usePowerCR);
					}
					else
					{
						GameController.ExhaustCoroutine(usePowerCR);
					}
					break;
			}
			yield break;
		}
	}
}