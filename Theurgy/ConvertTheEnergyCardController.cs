using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Theurgy
{
	public class ConvertTheEnergyCardController : CharmBaseCardController
	{
		// Play this card next to a hero character card.
		// At the start of their turn, they may select a damage type.
		//  Until the start of their next turn, all damage they deal is that type.
		// That hero gains the following power:
		// Power: Play 1 card or use 1 power.
		//  If doing so deals damage, that damage is irreducible.
		//  Destroy this card..

		public ConvertTheEnergyCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		protected override string CharmPowerText => "Play 1 card or use 1 power. All damage is irreducible until this card leaves play. Destroy convert the energy.";

		public override void AddTriggers()
		{
			base.AddTriggers();

			// At the start of their turn, they may select a damage type.
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == base.Card.Location.OwnerTurnTaker,
				SelectDamageTypeResponse,
				TriggerType.SelectDamageType
			);
		}

		private IEnumerator SelectDamageTypeResponse(PhaseChangeAction phaseChange)
		{
			HeroTurnTakerController httc = phaseChange.DecisionMaker;

			// At the start of their turn, they may select a damage type.
			List<SelectDamageTypeDecision> storedResults = new List<SelectDamageTypeDecision>();
			IEnumerator chooseDamageCR = base.GameController.SelectDamageType(
				httc,
				storedResults,
				cardSource: GetCardSource()
			);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(chooseDamageCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(chooseDamageCR);
			}

			// should handle both SW Sentinels and Guise
			Card targetHero = GetCardThisCardIsNextTo();
			if ( !targetHero.IsHeroCharacterCard )
			{
				targetHero = base.Card.Location.OwnerTurnTaker.CharacterCard;
			}

			// Until the start of their next turn, all damage they deal is that type.
			DamageType? damageType = storedResults.First((SelectDamageTypeDecision d) => d.Completed).SelectedDamageType;

			ChangeDamageTypeStatusEffect changeDamageTypeStatusEffect = new ChangeDamageTypeStatusEffect(damageType.Value);
			changeDamageTypeStatusEffect.SourceCriteria.IsSpecificCard = targetHero;
			changeDamageTypeStatusEffect.UntilStartOfNextTurn(targetHero.Owner);
			changeDamageTypeStatusEffect.BeforeOrAfter = BeforeOrAfter.Before;
			changeDamageTypeStatusEffect.CardDestroyedExpiryCriteria.Card = targetHero;

			IEnumerator changeTypeCR = AddStatusEffect(changeDamageTypeStatusEffect);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(changeTypeCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(changeTypeCR);
			}
			yield break;
		}

		protected override IEnumerator CharmPowerResponse(CardController cc)
		{
			int playNumeral = GetPowerNumeral(0, 1);
			int powerNumeral = GetPowerNumeral(1, 1);

			HeroTurnTakerController httc = cc.DecisionMaker;

			// make all damage irreducible
			MakeDamageIrreducibleStatusEffect effect = new MakeDamageIrreducibleStatusEffect();
			effect.UntilCardLeavesPlay(base.Card);
			effect.UntilEndOfPhase(httc.TurnTaker, Phase.End);
			effect.CreateImplicitExpiryConditions();
			IEnumerator makeIrreducibleCR = AddStatusEffect(effect);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(makeIrreducibleCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(makeIrreducibleCR);
			}

			// choose to play card or use power
			List<Function> functionList = new List<Function>();

			// play a card?
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"Play " + playNumeral + " card" + (playNumeral != 1 ? "s" : ""),
					SelectionType.PlayCard,
					() => base.SelectAndPlayCardsFromHand(
						httc,
						playNumeral
					)
				)
			);

			// or use a power?
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"Use " + powerNumeral + " power" + (powerNumeral != 1 ? "s" : ""),
					SelectionType.UsePower,
					() => SelectAndUsePowers(
						cc,
						playNumeral
					)
				)
			);

			// play the card or use the power
			SelectFunctionDecision selectFunction = new SelectFunctionDecision(
				base.GameController,
				httc,
				functionList,
				false,
				cardSource: base.GetCardSource()
			);

			IEnumerator selectFunctionCR = base.GameController.SelectAndPerformFunction(selectFunction);
			if (UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(selectFunctionCR);
			}
			else
			{
				base.GameController.ExhaustCoroutine(selectFunctionCR);
			}

			// reverse irreducible condition
			// which, uh, hopefully happens on destruction?
			// think it might get weird with Null Point, but should also destroy at the end of the phase

			// destroy this card
			IEnumerator destructionCR = GameController.DestroyCard(
				httc,
				base.Card,
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

		private IEnumerator SelectAndUsePowers(CardController cc, int powerQuantity)
		{
			IEnumerator powerCR = null;

			while (powerQuantity > 0)
			{
				powerCR = base.SelectAndUsePower(cc);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(powerCR);
				}
				else
				{
					GameController.ExhaustCoroutine(powerCR);
				}
				powerQuantity--;
			}

			yield break;
		}
	}
}