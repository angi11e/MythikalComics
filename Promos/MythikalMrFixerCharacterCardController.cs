using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.MrFixer
{
	public class MythikalMrFixerCharacterCardController : HeroCharacterCardController
	{
		public MythikalMrFixerCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 1);

			// If a hero target takes damage this way, their player may draw a card.
			ITrigger drawTrigger = AddTrigger(
				(DealDamageAction d) => d.DidDealDamage
					&& d.CardSource != null
					&& d.CardSource.CardController == this
					&& IsHero(d.Target)
					&& d.Target.Owner.IsHero
					&& !d.Target.Owner.IsIncapacitatedOrOutOfGame,
				(DealDamageAction d) => GameController.DrawCard(
					d.Target.Owner.ToHero(),
					true,
					cardSource: GetCardSource()
				),
				new List<TriggerType> { TriggerType.DrawCard },
				TriggerTiming.After
			);

			// [i]Mantra[/i] deals 1 target 1 melee damage.
			IEnumerator dealDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
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

			if (drawTrigger != null)
			{
				RemoveTrigger(drawTrigger);
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			switch (index)
			{
				case 0:
					// One Player may play a card.
					IEnumerator playCR = SelectHeroToPlayCard(DecisionMaker);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(playCR);
					}
					else
					{
						GameController.ExhaustCoroutine(playCR);
					}
					break;
				case 1:
					// Each target regains 1 HP.
					IEnumerator gainHPCR = GameController.GainHP(
						DecisionMaker,
						(Card c) => true,
						1,
						cardSource: GetCardSource()
					);
					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(gainHPCR);
					}
					else
					{
						GameController.ExhaustCoroutine(gainHPCR);
					}
					break;
				case 2:
					// Select a Target.
					List<SelectCardDecision> cardSelection = new List<SelectCardDecision>();
					IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.SelectTargetFriendly,
						new LinqCardCriteria(
							(Card c) => c.IsInPlay && c.IsTarget,
							"target",
							useCardsSuffix: false,
							plural: "targets"
						),
						cardSelection,
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

					SelectCardDecision selected = cardSelection.FirstOrDefault();
					if (selected != null && selected.SelectedCard != null)
					{
						// Increase the next damage dealt by that Target by 1...
						IncreaseDamageStatusEffect increaseDamageStatusEffect = new IncreaseDamageStatusEffect(1);
						increaseDamageStatusEffect.NumberOfUses = 1;
						increaseDamageStatusEffect.SourceCriteria.IsSpecificCard = selected.SelectedCard;
						increaseDamageStatusEffect.UntilTargetLeavesPlay(selected.SelectedCard);
						IEnumerator increaseStatusCR = AddStatusEffect(increaseDamageStatusEffect);

						// ...and reduce the next damage dealt to that Target by 1.
						ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(1);
						reduceDamageStatusEffect.NumberOfUses = 1;
						reduceDamageStatusEffect.TargetCriteria.IsSpecificCard = selected.SelectedCard;
						reduceDamageStatusEffect.UntilTargetLeavesPlay(selected.SelectedCard);
						IEnumerator decreaseStatusCR = AddStatusEffect(reduceDamageStatusEffect);

						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(increaseStatusCR);
							yield return GameController.StartCoroutine(decreaseStatusCR);
						}
						else
						{
							GameController.ExhaustCoroutine(increaseStatusCR);
							GameController.ExhaustCoroutine(decreaseStatusCR);
						}
					}
					break;
			}
			yield break;
		}
	}
}