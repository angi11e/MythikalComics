using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.OrbitalAtlantis
{
	public class EmergeFromTheSeaCardController : CardController
	{
		/*
		 * 
		 */

		public EmergeFromTheSeaCardController(
			Card card,
			TurnTakerController turnTakerController
		) : base(card, turnTakerController)
		{
		}

		public override void AddTriggers()
		{

			base.AddTriggers();
		}

		public override IEnumerator Play()
		{

			yield break;
		}
	}
}