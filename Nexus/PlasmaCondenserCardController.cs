using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class PlasmaCondenserCardController : NexusEquipmentCardController
	{
		/*
		 * the first time each turn {Nexus} deals fire damage to a target,
		 * she also deals that target 1 radiant damage.
		 * 
		 * POWER
		 * destroy a hero ongoing or equipment card.
		 * if a card is destroyed this way, {Nexus} deals each non-hero target 1 fire damage.
		 * if [i]Hydrokinesis[/i] is in play, move the destroyed card to its player's hand.
		 */

		public PlasmaCondenserCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Fire, DamageType.Radiant)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int damageNumeral = GetPowerNumeral(0, 1);

			// destroy a hero ongoing or equipment card.
			List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
			IEnumerator destructionCR = GameController.SelectAndDestroyCard(
				DecisionMaker,
				new LinqCardCriteria(
					(Card c) => IsHero(c) && (IsOngoing(c) || IsEquipment(c)),
					"hero ongoing or equipment"
				),
				optional: false,
				destroyResults,
				null,
				GetCardSource()
			);
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}

			// if a card is destroyed this way
			if (DidDestroyCards(destroyResults, 1))
			{
				// {Nexus} deals each non-hero target 1 fire damage.
				IEnumerator damageCR = DealDamage(
					this.CharacterCard,
					(Card c) => !IsHeroTarget(c),
					damageNumeral,
					DamageType.Fire
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(damageCR);
				}
				else
				{
					GameController.ExhaustCoroutine(damageCR);
				}

				// if [i]Hydrokinesis[/i] is in play
				if (FindCardsWhere(c => c.Identifier == "Hydrokinesis" && c.IsInPlayAndHasGameText).Any())
				{
					Card trashed = destroyResults.FirstOrDefault().CardToDestroy.Card;

					// move the destroyed card to its player's hand.
					IEnumerator returnCardCR = GameController.MoveCard(
						TurnTakerController,
						trashed,
						trashed.Owner.ToHero().Hand,
						cardSource: GetCardSource()
					);

					if (UseUnityCoroutines)
					{
						yield return GameController.StartCoroutine(returnCardCR);
					}
					else
					{
						GameController.ExhaustCoroutine(returnCardCR);
					}
				}
			}

			yield break;
		}
	}
}