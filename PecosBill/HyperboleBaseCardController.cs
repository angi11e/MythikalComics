using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Angille.PecosBill
{
	public abstract class HyperboleBaseCardController : PecosBillBaseCardController
	{
		protected HyperboleBaseCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
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