using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Angille.Theurgy
{
	public class TheurgyTurnTakerController : HeroTurnTakerController
	{
		public TheurgyTurnTakerController(
			TurnTaker turnTaker,
			GameController gameController
		) : base(turnTaker, gameController)
		{
		}

		public override IEnumerator StartGame()
		{
			IEnumerable<string> villainsToAugment = new[] { "GloomWeaverCharacter", "AkashBhutaCharacter" };
			for (int i = 0; i < villainsToAugment.Count(); i++)
			{
				Card newNemesis = FindCardsWhere(
					(Card c) => c.Identifier == villainsToAugment.ElementAt(i)
				).FirstOrDefault();
				if (newNemesis != null)
				{
					CardController thisCCC = FindCardController(TurnTaker.CharacterCard);
					IEnumerator addNemesisCR = GameController.UpdateNemesisIdentifiers(
						GameController.FindCardController(newNemesis),
						TurnTaker.CharacterCard.NemesisIdentifiers,
						thisCCC.GetCardSource()
					);

					if (thisCCC.UseUnityCoroutines)
					{
						yield return thisCCC.GameController.StartCoroutine(addNemesisCR);
					}
					else
					{
						thisCCC.GameController.ExhaustCoroutine(addNemesisCR);
					}
				}
			}

			yield break;
		}
	}
}
