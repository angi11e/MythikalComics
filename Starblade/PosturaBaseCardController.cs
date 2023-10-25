using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Starblade
{
	public class PosturaBaseCardController : StarbladeBaseCardController
	{
		/*
		 * When this card enters play, destroy your other [u]postura[/u] cards,
		 * then put a [i]insert-construct-here[/i] into play from your trash.
		 */

		private readonly string _constructIdentifier;

		public PosturaBaseCardController(
			Card card,
			TurnTakerController turnTakerController,
			string constructIdentifier
		) : base(card, turnTakerController)
		{
			_constructIdentifier = constructIdentifier;
		}

		public override IEnumerator Play()
		{
			// When this card enters play, destroy your other [u]postura[/u] cards,
			IEnumerator destroyCR = GameController.DestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c != this.Card && IsPostura(c) && c.Owner == this.Card.Owner),
				cardSource: GetCardSource()
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(destroyCR);
			}
			else
			{
				GameController.ExhaustCoroutine(destroyCR);
			}

			// then put a [i]insert-construct-here[/i] into play from your trash.
			IEnumerator moveCardCR = SearchForCards(
				DecisionMaker,
				false,
				true,
				1,
				1,
				new LinqCardCriteria((Card c) => c.Identifier == _constructIdentifier),
				true,
				false,
				false,
				false
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(moveCardCR);
			}
			else
			{
				GameController.ExhaustCoroutine(moveCardCR);
			}

			yield break;
		}
	}
}