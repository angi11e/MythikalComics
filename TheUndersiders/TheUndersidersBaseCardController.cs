using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Collections;
using Handelabra;

namespace Angille.TheUndersiders
{
	public abstract class TheUndersidersBaseCardController : CardController
	{
		public TheUndersidersBaseCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}
		public IEnumerator FindVillain(string identifier, Card villain, bool needTarget = false, Card originator = null)
		{
			villain = TurnTaker.FindCard(identifier);
			if (!villain.IsFlipped && villain.IsInPlayAndNotUnderCard && villain.IsVillainTarget)
			{
				yield return villain;
			}
			else if (needTarget && base.GameController.Game.IsChallenge && !villain.IsUnderCard)
			{
				IEnumerable<Card> highestVillains = base.GameController.FindAllTargetsWithHighestHitPoints(
					1,
					(Card c) => c.IsVillainTarget && c.IsInPlayAndNotUnderCard,
					GetCardSource()
				);

				villain = highestVillains.FirstOrDefault();
				if (highestVillains.Count() > 1)
				{
					List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
					IEnumerator getHighestCR = base.GameController.SelectCardAndStoreResults(
						DecisionMaker,
						SelectionType.CardToDealDamage,
						new LinqCardCriteria((Card c) => highestVillains.Contains(c)),
						storedResults,
						false,
						cardSource: GetCardSource()
					);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(getHighestCR);
					}
					else
					{
						base.GameController.ExhaustCoroutine(getHighestCR);
					}
					villain = storedResults.FirstOrDefault().SelectedCard;
				}

				IEnumerator challengeCR = GameController.SendMessageAction(
					"Undersiders never forget their enemies.",
					Priority.Medium,
					GetCardSource()
				);
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(challengeCR);
				}
				else
				{
					base.GameController.ExhaustCoroutine(challengeCR);
				}

				yield return villain;
			}
			else
			{
				villain = null;
			}

			yield return null;
		}

	}
}
