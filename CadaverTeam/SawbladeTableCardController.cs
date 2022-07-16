using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.CadaverTeam
{
	public class SawbladeTableCardController : CardController
	{
		private List<Card> _targetsDestroyed = new List<Card>();

		public SawbladeTableCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
			SpecialStringMaker.ShowLowestHP(cardCriteria: new LinqCardCriteria(
				(Card c) => !c.DoKeywordsContain("haunt")
			));
		}

		public override IEnumerator Play()
		{
			ITrigger destroyTrigger = AddTrigger(
				(DestroyCardAction dca) => dca.WasCardDestroyed,
				StoreDestroyedTargetResponse,
				TriggerType.Hidden,
				TriggerTiming.After
			);

			// {CadaverTeam} deals the non-haunt target with the lowest HP 5 melee damage.
			List<DealDamageAction> storedResults = new List<DealDamageAction>();
			IEnumerator dealDamageCR = DealDamageToLowestHP(
				this.CharacterCard,
				1,
				(Card c) => !c.DoKeywordsContain("haunt"),
				(Card c) => 5,
				DamageType.Melee,
				storedResults: storedResults
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(dealDamageCR);
			}
			else
			{
				GameController.ExhaustCoroutine(dealDamageCR);
			}
			RemoveTrigger(destroyTrigger);

			// If that target is destroyed this way...
			DealDamageAction dealDamageAction = storedResults.FirstOrDefault();
			if (
				dealDamageAction != null
				&& dealDamageAction.OriginalTarget != null &&
				_targetsDestroyed.Contains(dealDamageAction.OriginalTarget)
			)
			{
				// ...reveal cards from the top of {CadaverTeam}'s deck until a haunt card is revealed.
				// Put it into play. Shuffle the other revealed cards back into {CadaverTeam}'s deck.
				IEnumerator revealCR = RevealCards_MoveMatching_ReturnNonMatchingCards(
					this.TurnTakerController,
					this.TurnTaker.Deck,
					false,
					true,
					false,
					new LinqCardCriteria((Card c) => c.DoKeywordsContain("haunt")),
					1
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(revealCR);
				}
				else
				{
					GameController.ExhaustCoroutine(revealCR);
				}
			}

			_targetsDestroyed = new List<Card>();

			yield break;
		}

		private IEnumerator StoreDestroyedTargetResponse(DestroyCardAction dca)
		{
			_targetsDestroyed.Add(dca.CardToDestroy.Card);
			yield return null;
		}
	}
}
