using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Setback
{
	public class MythikalSetbackCharacterCardController : HeroCharacterCardController
	{
		public MythikalSetbackCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int addNumeral = GetPowerNumeral(0, 2);
			int removeNumeral = GetPowerNumeral(1, 2);
			int damageNumeral = GetPowerNumeral(2, 2);

			// Destroy an ongoing card.
			List<DestroyCardAction> storedResults = new List<DestroyCardAction>();
			IEnumerator destroyCR = GameController.SelectAndDestroyCard(
				HeroTurnTakerController,
				new LinqCardCriteria(
					(Card c) => IsOngoing(c),
					"ongoing"
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

			// If a hero card was destroyed...
			if (DidDestroyCard(storedResults) && storedResults.First().CardToDestroy.Card.IsHero)
			{
				// ...add 2 tokens to your unlucky pool...
				IEnumerator addTokenCR = GameController.AddTokensToPool(
					this.Card.FindTokenPool(TokenPool.UnluckyPoolIdentifier),
					addNumeral,
					GetCardSource()
				);

				// ...and draw a card.
				IEnumerator drawCR = DrawCard(this.HeroTurnTaker);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(addTokenCR);
					yield return GameController.StartCoroutine(drawCR);
				}
				else
				{
					GameController.ExhaustCoroutine(addTokenCR);
					GameController.ExhaustCoroutine(drawCR);
				}
			}
			else if (DidDestroyCard(storedResults) && !storedResults.First().CardToDestroy.Card.IsHero)
			{
				// Otherwise, remove 2 tokens from your unlucky pool...
				IEnumerator removeTokenCR = GameController.RemoveTokensFromPool(
					this.Card.FindTokenPool(TokenPool.UnluckyPoolIdentifier),
					removeNumeral,
					cardSource: GetCardSource()
				);

				// ...and {Setback} deals himself 2 melee damage.
				IEnumerator selfDamageCR = DealDamage(
					this.CharacterCard,
					this.CharacterCard,
					damageNumeral,
					DamageType.Melee,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokenCR);
					yield return GameController.StartCoroutine(selfDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokenCR);
					GameController.ExhaustCoroutine(selfDamageCR);
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Hero may use a power.
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
				case 1:
					// Select a Target. Reduce the next damage dealt to that Target by 2.
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
					// Destroy up to two ongoing cards. Only one may be a villain card.
					LinqCardCriteria ongoingCriteria = new LinqCardCriteria(
						(Card c) => IsOngoing(c) && c.IsInPlay,
						"ongoing"
					);
					List<DestroyCardAction> destroySelection = new List<DestroyCardAction>();
					IEnumerator destroyFirstCR = GameController.SelectAndDestroyCard(
						DecisionMaker,
						ongoingCriteria,
						true,
						destroySelection,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(destroyFirstCR);
					}
					else
					{
						GameController.ExhaustCoroutine(destroyFirstCR);
					}

					if (DidDestroyCard(destroySelection))
					{
						if (destroySelection.FirstOrDefault().CardToDestroy.Card.IsVillain)
						{
							ongoingCriteria = new LinqCardCriteria(
								(Card c) => IsOngoing(c) && c.IsInPlay && !c.IsVillain,
								"ongoing"
							);
						}

						IEnumerator destroySecondCR = GameController.SelectAndDestroyCard(
							DecisionMaker,
							ongoingCriteria,
							true,
							cardSource: GetCardSource()
						);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(destroySecondCR);
						}
						else
						{
							GameController.ExhaustCoroutine(destroySecondCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}