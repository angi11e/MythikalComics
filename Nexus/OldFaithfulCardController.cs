using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Nexus
{
	public class OldFaithfulCardController : NexusOneShotCardController
	{
		/*
		 * {Nexus} deals 1 target 2 cold damage and 1 different target 2 fire damage, in either order.
		 * 
		 * You may discard a card. if you do, another player may play a card.
		 */

		public OldFaithfulCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController, DamageType.Cold, DamageType.Fire)
		{
		}

		public override IEnumerator Play()
		{
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(base.Play());
			}
			else
			{
				GameController.ExhaustCoroutine(base.Play());
			}

			// You may discard a card.
			List<DiscardCardAction> storedCards = new List<DiscardCardAction>();
			IEnumerator discardCR = SelectAndDiscardCards(
				HeroTurnTakerController,
				1,
				optional: true,
				null,
				storedCards
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(discardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(discardCR);
			}

			// if you do
			if (DidDiscardCards(storedCards, 1))
			{
				// another player may play a card.
				IEnumerator playCR = GameController.SelectHeroToPlayCard(
					DecisionMaker,
					additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != this.TurnTaker),
					cardSource: GetCardSource()
				);

				if (UseUnityCoroutines)
				{
					yield return GameController.StartCoroutine(playCR);
				}
				else
				{
					GameController.ExhaustCoroutine(playCR);
				}
			}

			yield break;
		}
	}
}