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
    public class DescentOfTheSwarmCardController : TheUndersidersBaseCardController
	{
		public DescentOfTheSwarmCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
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
