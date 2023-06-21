using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Spoiler
{
	public class CausalityDecouplerCardController : SpoilerEquipmentCardController
	{
		public CausalityDecouplerCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
			SpecialStringMaker.ShowListOfCardsInPlay(
				new LinqCardCriteria((Card c) => IsOngoing(c) && c.Owner == this.TurnTaker, "ongoing")
			);
		}

		public override void AddTriggers()
		{
			// At the start of your turn, you may activate a [u]rewind[/i] text.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				(PhaseChangeAction pca) => GameController.SelectAndActivateAbility(
					DecisionMaker,
					"rewind",
					optional: true,
					cardSource: GetCardSource()
				),
				TriggerType.ActivateTriggers
			);

			base.AddTriggers();
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int psychicNumeral = GetPowerNumeral(0, 1);
			int energyNumeral = GetPowerNumeral(1, 1);

			// {Spoiler} deals each target 1 psychic damage.
			IEnumerator psychicDamageCR = DealDamage(
				this.CharacterCard,
				(Card c) => c.IsTarget,
				psychicNumeral,
				DamageType.Psychic
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(psychicDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(psychicDamageCR);
			}

			// {Spoiler} deals each villain target 1 energy damage.
			IEnumerator energyDamageCR = DealDamage(
				this.CharacterCard,
				(Card c) => IsVillainTarget(c),
				energyNumeral,
				DamageType.Energy
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(energyDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(energyDamageCR);
			}

			// Destroy this card.
			IEnumerator destructionCR = GameController.DestroyCard(
				DecisionMaker,
				this.Card,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destructionCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destructionCR);
			}

			yield break;
		}
	}
}