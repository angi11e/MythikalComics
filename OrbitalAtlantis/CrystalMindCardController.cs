using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class CrystalMindCardController : CardController
	{
		/*
		 * at the start of the environment turn,
		 * you may remove a token from a zone card's bias pool.
		 * if you do, this card deals each villain target 2 psychic damage.
		 * otherwise, this card deals each hero target 2 psychic damage.
		 */

		public CrystalMindCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{
			// at the start of the environment turn...
			AddStartOfTurnTrigger(
				(TurnTaker tt) => tt == this.TurnTaker,
				BiasResponse,
				TriggerType.AddTokensToPool
			);

			base.AddTriggers();
		}

		private IEnumerator BiasResponse(PhaseChangeAction p)
		{
			// ...you may remove a token from a zone card's bias pool.
			List<RemoveTokensFromPoolAction> tokenResults = new List<RemoveTokensFromPoolAction>();
			List<SelectCardDecision> zoneResults = new List<SelectCardDecision>();

			IEnumerator selectCardCR = GameController.SelectCardAndStoreResults(
				DecisionMaker,
				SelectionType.RemoveTokens,
				new LinqCardCriteria(
					(Card c) => c.IsInPlayAndNotUnderCard
						&& c.DoKeywordsContain("zone")
						&& c.FindTokenPool("bias") != null
						&& c.FindTokenPool("bias").CurrentValue > 0,
					"zone cards with bias tokens"
				),
				zoneResults,
				true,
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(selectCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(selectCardCR);
			}

			if (DidSelectCard(zoneResults))
			{
				IEnumerator removeTokensCR = GameController.RemoveTokensFromPool(
					zoneResults.FirstOrDefault().SelectedCard.FindTokenPool("bias"),
					1,
					tokenResults,
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(removeTokensCR);
				}
				else
				{
					GameController.ExhaustCoroutine(removeTokensCR);
				}
			}

			// otherwise, this card deals each hero target 2 psychic damage.
			bool allPaidUp = false;
			if (DidRemoveTokens(tokenResults))
			{
				// if you do, this card deals each villain target 2 psychic damage.
				allPaidUp = true;
			}

			IEnumerator mindBlastCR = DealDamage(
				this.Card,
				(Card c) => allPaidUp ? IsVillainTarget(c) : IsHeroTarget(c),
				2,
				DamageType.Psychic
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(mindBlastCR);
			}
			else
			{
				GameController.ExhaustCoroutine(mindBlastCR);
			}

			yield break;
		}
	}
}