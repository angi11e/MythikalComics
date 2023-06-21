using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Angille.Athena
{
	public class BrandNewAthenaCharacterCardController : AthenaBaseCharacterCardController
	{
		public BrandNewAthenaCharacterCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int radiantNumeral = GetPowerNumeral(1, 1);
			int meleeNumeral = GetPowerNumeral(2, 1);
			int drawNumeral = GetPowerNumeral(3, 2);
			int discardNumeral = GetPowerNumeral(4, 1);

			// If there is an [u]aspect[/u] card in play,
			if (HeroTurnTaker.GetCardsWhere(
				(Card c) => c.IsInPlayAndNotUnderCard && IsManifest(c)
			).Count() > 0)
			{
				// {Athena} deals 1 target 1 radiant and 1 melee damage.
				List<DealDamageAction> damages = new List<DealDamageAction>();
				damages.Add(new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.CharacterCard),
					null,
					radiantNumeral,
					DamageType.Radiant
				));
				damages.Add(new DealDamageAction(
					GetCardSource(),
					new DamageSource(GameController, this.CharacterCard),
					null,
					meleeNumeral,
					DamageType.Melee
				));

				IEnumerator dealDamageCR = SelectTargetsAndDealMultipleInstancesOfDamage(
					damages,
					minNumberOfTargets: targetNumeral,
					maxNumberOfTargets: targetNumeral
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealDamageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealDamageCR);
				}
			}
			else
			{
				// If not, draw 2 cards, then discard 1 card.
				IEnumerator drawCR = DrawCards(this.HeroTurnTakerController, drawNumeral);
				IEnumerator discardCR = SelectAndDiscardCards(this.HeroTurnTakerController, discardNumeral);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(drawCR);
					yield return GameController.StartCoroutine(discardCR);
				}
				else
				{
					GameController.ExhaustCoroutine(drawCR);
					GameController.ExhaustCoroutine(discardCR);
				}
			}

			yield break;
		}

		public override IEnumerator UseIncapacitatedAbility(int index)
		{
			IEnumerator incapCR = DoNothing();
			switch (index)
			{
				case 0:
					// One player may draw a card.
					incapCR = GameController.SelectHeroToDrawCard(
						this.HeroTurnTakerController,
						additionalCriteria: new LinqTurnTakerCriteria(
							(TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame && !tt.IsIncapacitated
						),
						cardSource: GetCardSource()
					);
					break;
				case 1:
					// One player may play a card.
					incapCR = SelectHeroToPlayCard(
						this.HeroTurnTakerController,
						heroCriteria: new LinqTurnTakerCriteria(
							(TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitated
						)
					);
					break;
				case 2:
					// Increase the next damage dealt by a hero target by 2.
					incapCR = AddStatusEffect(new IncreaseDamageStatusEffect(2)
					{
						SourceCriteria = { IsHero = new bool?(true) },
						TargetCriteria = { IsTarget = new bool?(true) },
						NumberOfUses = new int?(1)
					});
					break;
			}

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(incapCR);
			}
			else
			{
				GameController.ExhaustCoroutine(incapCR);
			}

			yield break;
		}
	}
}