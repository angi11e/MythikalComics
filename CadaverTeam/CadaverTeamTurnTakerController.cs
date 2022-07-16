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
	public class CadaverTeamTurnTakerController : TurnTakerController
	{
		public CadaverTeamTurnTakerController(TurnTaker turnTaker, GameController gameController)
			: base(turnTaker, gameController)
		{
		}

		public override IEnumerator StartGame()
		{
			// At the start of the game, {CadaverTeam} enters play “Corrupted Conjurer” side up.
			// {H - 3} copies of [i]Mesmerized Audience[/i] are put into play. The villain deck is shuffled.
			if (this.H < 4)
			{
				yield break;
			}
			
			IEnumerator playAudienceCR = PutCardsIntoPlay(
				new LinqCardCriteria(
					(Card c) => c.Identifier == "MesmerizedAudience", "cards named Mesmerized Audience"
				),
				this.H - 3
			);

			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(playAudienceCR);
			}
			else
			{
				GameController.ExhaustCoroutine(playAudienceCR);
			}
		}
	}
}
