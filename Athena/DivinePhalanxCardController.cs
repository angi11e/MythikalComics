﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class DivinePhalanxCardController : AthenaBaseCardController
	{
		/*
		 * Choose a target.
		 * Athena and up to 2 other hero targets deal that target 1 radiant damage each.
		 * If there is an [u]aspect[/u] card in play, this damage is irreducible.
		 */

		public DivinePhalanxCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Choose a target.
			List<SelectTargetDecision> selectedTarget = new List<SelectTargetDecision>();
			IEnumerable<Card> choices = base.FindCardsWhere(
				new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText)
			);
			IEnumerator selectTargetCR = base.GameController.SelectTargetAndStoreResults(
				base.HeroTurnTakerController,
				choices,
				selectedTarget,
				selectionType: SelectionType.SelectTarget,
				cardSource: base.GetCardSource()
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

					// Athena and up to 2 other hero targets deal that target 1 radiant damage each.
					IEnumerator firstDamageCR = DealDamage(
						this.CharacterCard,
						theCard,
						1,
						DamageType.Radiant,
						// If there is an [u]aspect[/u] card in play, this damage is irreducible.
						isIrreducible: AspectInPlay,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(firstDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(firstDamageCR);
					}

					List<SelectCardsDecision> heroCards = new List<SelectCardsDecision>();
					IEnumerator selectHeroCardsCR = GameController.SelectCardsAndStoreResults(
						DecisionMaker,
						SelectionType.CardToDealDamage,
						(Card c) => c.IsHero && c.IsTarget && c.IsInPlay && c != this.CharacterCard,
						2,
						heroCards,
						optional: false,
						0,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(selectHeroCardsCR);
					}
					else
					{
						GameController.ExhaustCoroutine(selectHeroCardsCR);
					}

					SelectCardsDecision selectCardsDecision = heroCards.FirstOrDefault();
					if (selectCardsDecision != null && selectCardsDecision.SelectCardDecisions != null)
					{
						foreach (SelectCardDecision selectCardDecision in selectCardsDecision.SelectCardDecisions)
						{
							if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
							{
								IEnumerator iterativeDamageCR = DealDamage(
									selectCardDecision.SelectedCard,
									theCard,
									1,
									DamageType.Radiant,
									// If there is an [u]aspect[/u] card in play, this damage is irreducible.
									isIrreducible: AspectInPlay,
									cardSource: GetCardSource()
								);

								if (UseUnityCoroutines)
								{
									yield return GameController.StartCoroutine(iterativeDamageCR);
								}
								else
								{
									GameController.ExhaustCoroutine(iterativeDamageCR);
								}
							}
						}
					}
					/* old version with all hero characters
					IEnumerator mainDamageCR = MultipleDamageSourcesDealDamage(
						new LinqCardCriteria((Card c) =>
							c.IsHeroCharacterCard
							&& c.IsTarget
							&& !c.IsFlipped
							&& c.IsInPlayAndNotUnderCard
						),
						TargetType.All,
						1,
						new LinqCardCriteria((Card c) => c == theCard),
						1,
						DamageType.Radiant,
						// If there is an [u]aspect[/u] card in play, this damage is irreducible.
						isIrreducible: AspectInPlay
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(mainDamageCR);
					}
					else
					{
						GameController.ExhaustCoroutine(mainDamageCR);
					}
					*/
				}
			}

			yield break;
		}
	}
}