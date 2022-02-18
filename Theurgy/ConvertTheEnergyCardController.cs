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
		//  Their player selects a damage type. Damage of that type dealt by or to this hero is irreducible.
		// That hero gains the [b]power:[/b] destroy this card.
		// Before this card is destroyed, the hero it's next to may play 1 card or use 1 power.
		//  All damage is irreducible until this card leaves play.

		public ConvertTheEnergyCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			// Their player selects a damage type.
			Card targetHero = CharmedHero();
			HeroTurnTaker htt = targetHero.Owner.ToHero();
			HeroTurnTakerController httc = FindHeroTurnTakerController(htt);

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

			DamageType? damageType = storedResults.First(
				(SelectDamageTypeDecision d) => d.Completed
			).SelectedDamageType;

			// Damage of that type dealt by or to this hero is irreducible.
			if (damageType != null)
			{
				MakeDamageIrreducibleStatusEffect dealtToSE = new MakeDamageIrreducibleStatusEffect();
				MakeDamageIrreducibleStatusEffect dealtBySE = new MakeDamageIrreducibleStatusEffect();
				dealtToSE.UntilCardLeavesPlay(base.Card);
				dealtBySE.UntilCardLeavesPlay(base.Card);
				dealtToSE.CreateImplicitExpiryConditions();
				dealtBySE.CreateImplicitExpiryConditions();
				dealtToSE.DamageTypeCriteria.AddType(damageType.Value);
				dealtBySE.DamageTypeCriteria.AddType(damageType.Value);
				dealtToSE.TargetCriteria.IsSpecificCard = targetHero;
				dealtBySE.SourceCriteria.IsSpecificCard = targetHero;

				IEnumerator dealtToCR = AddStatusEffect(dealtToSE);
				IEnumerator dealtByCR = AddStatusEffect(dealtBySE);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(dealtToCR);
					yield return GameController.StartCoroutine(dealtByCR);
				}
				else
				{
					GameController.ExhaustCoroutine(dealtToCR);
					GameController.ExhaustCoroutine(dealtByCR);
				}
			}

			yield break;
		}

		protected override IEnumerator CharmDestroyResponse(GameAction ga)
		{
			HeroTurnTakerController httc = FindHeroTurnTakerController(CharmedHero().Owner.ToHero());

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
					"Play a card",
					SelectionType.PlayCard,
					() => base.SelectAndPlayCardsFromHand(
						httc,
						1
					)
				)
			);

			// or use a power?
			functionList.Add(
				new Function(
					this.DecisionMaker,
					"Use a power",
					SelectionType.UsePower,
					() => SelectAndUsePower(FindCardController(CharmedHero()))
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

			yield break;
		}
	}
}