using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PseudoBlackjack
{
    public partial class Form1 : Form
    {
        int moneypool = 1000;
        Hand[] hands = new Hands[10];
        int currenthand = 0;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
        private void Hit(object sender, EventArgs e)
        {
            dealCard();
            if (PlayerBust()) Stay(null, null);
        }
        private void Stay(object sender, EventArgs e)
        {
            if (hands[currenthand + 1] == null) {
                //shutdown all buttons
                DealerTurn();
            }
            else
            {
                currenthand += 1;
                Hit(null, null);
                //start from where bets were just placed for this hand
            }
        }
        private void DoubleDown(object sender, EventArgs e)
        {
            hands[currenthand].bet = hands[currenthand].bet * 2;
            Hit(null, null);
            CheckAces();
            Stay(null, null);
        }
        private void Split(object sender, EventArgs e) {
            Card c = hands[currenthand].cardslist.Lastentry();
            hands[currenthand].cardslist.remove(Lastentry);
            hands[currenthand + 1] = new Hand(c);
            Hit(null, null);
        }
        private void DealerTurn() {
            //code for reveal hand
            for (int i = 0; i < hands.Length; i++) {
                currenthand = i;
                bool allbust = true;
                if (hands[i] != null && !PlayerBust()) {
                    allbust = false;
                }
                if (allbust) return;

            }
            //code of Algorithm of dealer's turn
            if (DealerHand.value < 17)
            {
                //hit on dealer
            }
            else if (DealerHand.value == 17 && DealerHand.Cards.contains(Aces) && DealerHand.oneAce.equals(1))
            {
                //hit on dealer
            }
            else {
                //stay
            }
            ///////////////////////////////////
            if (hands[currenthand].value > 21) {
                moneypool -= hands[currenthand].bet;
            }
            else if (DealerHand.value > hands[currenthand].value)
            {
                moneypool -= hands[currenthand].bet;
            }
            else if (DealerHand.value < hands[currenthand].value)
            {
                moneypool += hands[currenthand].bet;
            }
        }
        private void dealCard() {
            //algorithm here
        }
        private bool PlayerBust() {
            CheckAces();
            if (hands[currenthand].value > 21)
            {
                Stay(null, null);
                return true;
            }
            else if (hands[currenthand].value = 21) {
                Stay(null, null);
                return false;
            }
            else
            {
                return false;
            }

        }
        private void CheckAces() {
            if (handvalue > 21) {
                Card[] aces = Acquire_aces_from_hand();
                for (int i = 0; i < aces.Length; i++) {
                    if (aces[i].value = 11)
                    {
                        aces[i].value = 1;
                        break;
                    } 
                }
            }
        }
    }
}
