using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.Athena
{
	public class AspectBaseCardController : AthenaBaseCardController
	{
		/*
		 * When this card enters play, destroy any other [u]aspect[/u] cards.
		 */

		public AspectBaseCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override IEnumerator Play()
		{
			IEnumerator destroyCR = GameController.DestroyCards(
				DecisionMaker,
				new LinqCardCriteria((Card c) => c != base.Card && IsAspect(c)),
//				cancelDecisionsIfTrue: () => !base.CardWithoutReplacements.IsInPlayAndHasGameText,
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

			yield break;
		}
	}
}