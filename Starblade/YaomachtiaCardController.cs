using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class YaomachtiaCardController : PosturaBaseCardController
	{
		/*
		 * when this card enters play, destroy your other postura cards,
		 * then put a [i]bow and arrow[/i] into play from your trash.
		 * 
		 * whenever a construct card is destroyed,
		 * restore a construct card to its max hp.
		 * 
		 * POWER
		 * you may play a construct card.
		 * {Starblade} deals 1 target 1 melee and 1 energy damage.
		 * activate a [u]technique[/u] text.
		 */

		public YaomachtiaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "BowAndArrow")
		{
		}

		public override void AddTriggers()
		{
			// whenever a construct card is destroyed,
			AddTrigger(
				(DestroyCardAction d) => d.CardToDestroy.Card.IsConstruct && d.WasCardDestroyed,
				RestoreResponse,
				new List<TriggerType> { TriggerType.DealDamage },
				TriggerTiming.After
			);

			base.AddTriggers();
		}

		private IEnumerator RestoreResponse(DestroyCardAction dda)
		{
			// restore a construct card to its max hp.
			List<SelectCardDecision> storedCard = new List<SelectCardDecision>();
			IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.GainHP,
				new LinqCardCriteria(
					(Card c) => c.IsInPlay && c.IsTarget && c.IsConstruct,
					"construct",
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
				IEnumerator restoreCR = GameController.SetHP(
					selectCardDecision.SelectedCard,
					selectCardDecision.SelectedCard.MaximumHitPoints.Value,
					GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(restoreCR);
				}
				else
				{
					GameController.ExhaustCoroutine(restoreCR);
				}
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int meleeNumeral = GetPowerNumeral(1, 1);
			int energyNumeral = GetPowerNumeral(2, 1);

			// you may play a construct card.
			IEnumerator playConstructCR = SelectAndPlayCardFromHand(
				DecisionMaker,
				cardCriteria: new LinqCardCriteria((Card c) => c.IsConstruct, "construct")
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playConstructCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playConstructCR);
			}

			// {Starblade} deals 1 target 1 melee and 1 energy damage.
			List<DealDamageAction> damages = new List<DealDamageAction>();
			damages.Add(new DealDamageAction(
				GetCardSource(),
				new DamageSource(GameController, this.CharacterCard),
				null,
				meleeNumeral,
				DamageType.Melee
			));
			damages.Add(new DealDamageAction(
				GetCardSource(),
				new DamageSource(GameController, this.CharacterCard),
				null,
				energyNumeral,
				DamageType.Energy
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

			// activate a [u]technique[/u] text.
			IEnumerator activateCR = GameController.SelectAndActivateAbility(
				DecisionMaker,
				"technique",
				optional: false,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(activateCR);
			}
			else
			{
				GameController.ExhaustCoroutine(activateCR);
			}

			yield break;
		}
	}
}