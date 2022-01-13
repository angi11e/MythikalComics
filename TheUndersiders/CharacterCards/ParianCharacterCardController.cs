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
    public class ParianCharacterCardController : TheUndersidersVillainCardController
    {
        public ParianCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        public override void AddSideTriggers()
        {
            if (!base.Card.IsFlipped)
            {

            }
            base.AddSideTriggers();
        }
    }
}
