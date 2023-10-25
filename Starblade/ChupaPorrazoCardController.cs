using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class ChupaPorrazoCardController : PosturaBaseCardController
	{
		/*
		 * when this card enters play, destroy your other postura cards,
		 * then put a [i]weighted net[/i] into play from your trash.
		 * 
		 * whenever a hero target would be dealt damage by a villain card,
		 * you may redirect that damage to {Starblade} or a construct card.
		 * 
		 * POWER
		 * {Starblade} deals herself and 1 target 3 irreducible melee damage.
		 * activate a [u]technique[/u] text.
		 */

		public ChupaPorrazoCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, "WeightedNet")
		{
		}

		public override void AddTriggers()
		{
			// whenever a hero target would be dealt damage by a villain card,
			AddTrigger(
				(DealDamageAction dd) =>
					IsHero(dd.Target)
					&& dd.DamageSource.IsCard
					&& IsVillain(dd.DamageSource.Card),
				RedirectResponse,
				TriggerType.RedirectDamage,
				TriggerTiming.Before
			);

			base.AddTriggers();
		}

		private IEnumerator RedirectResponse(DealDamageAction dd)
		{
			// you may redirect that damage to {Starblade} or a construct card.
			List<SelectCardDecision> storedCard = new List<SelectCardDecision>();
			IEnumerator selectTargetCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.RedirectDamage,
				new LinqCardCriteria(
					(Card c) => (c == this.CharacterCard || c.DoKeywordsContain("construct")) && c.IsInPlayAndHasGameText,
					"target",
					useCardsSuffix: false
				),
				storedCard,
				optional: true,
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
				IEnumerator redirectCR = GameController.RedirectDamage(
					dd,
					selectCardDecision.SelectedCard,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(redirectCR);
				}
				else
				{
					GameController.ExhaustCoroutine(redirectCR);
				}
			}

			yield break;
		}

		public override IEnumerator UsePower(int index = 0)
		{
			int targetNumeral = GetPowerNumeral(0, 1);
			int damageNumeral = GetPowerNumeral(1, 3);

			// {Starblade} deals herself and 1 target 3 irreducible melee damage.
			IEnumerator selfDamageCR = DealDamage(
				this.CharacterCard,
				this.CharacterCard,
				damageNumeral,
				DamageType.Melee,
				true,
				cardSource: GetCardSource()
			);

			IEnumerator otherDamageCR = GameController.SelectTargetsAndDealDamage(
				DecisionMaker,
				new DamageSource(GameController, this.CharacterCard),
				damageNumeral,
				DamageType.Melee,
				targetNumeral,
				false,
				targetNumeral,
				true,
				additionalCriteria: (Card c) => c != this.CharacterCard,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selfDamageCR);
				yield return GameController.StartCoroutine(otherDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selfDamageCR);
				GameController.ExhaustCoroutine(otherDamageCR);
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