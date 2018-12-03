using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Media;

namespace Blackjack01
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    
    public partial class Game : Window
    {
        public static int onTop;
        String[] suits = new String[] {"H","D","C","S"};
        String[] numbers = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        public Card[] Deck = new Card[52];
        int moneypool;
        Hand[] hands = new Hand[4];
        Label[] WinStatus;
        int currenthand = 0;
        int Deck_Marker = 51;
        int bet = 0;
        Hand DealerH;
        static SoundPlayer Menu = new SoundPlayer(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Blackjack01.Resources.title-screen.wav"));
        public Game()
        {
            InitializeComponent();
            WinStatus = new Label[] { Label1, Label2, Label3, Label4, Label5 };
            for (int i = 0; i < WinStatus.Length; i++) {
                WinStatus[i].Content = "";
            }
            //Create the deck and position it
            int index = 0;
            for (int s = 0; s < suits.Length; s++) {
                for (int n = 0; n < numbers.Length; n++)
                {
                    Card a = new Card(numbers[n] + suits[s]);
                    Deck[index] = a;
                    this.grd_Game.Children.Add(a.CardPic);
                    a.CardPic.Height = 100;
                    a.CardPic.Width = 100;
                    a.CardPic.Margin = new Thickness(70 + index/2, 0,0,0);
                    index += 1;
                }
            }
            //Shuffle
            Shuffle();
            btn_Hit.Visibility = Visibility.Hidden;
            btn_Stay.Visibility = Visibility.Hidden;
            btn_Split.Visibility = Visibility.Hidden;
            btn_DblDown.Visibility = Visibility.Hidden;
            btn_Reset.Visibility = Visibility.Hidden;
            try {
                FileStream banker = new FileStream("bank.txt", FileMode.Open);
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(int));
                moneypool = (int) js.ReadObject(banker);
            }
            catch (IOException nonexist) {
                FileStream banker = new FileStream("bank.txt", FileMode.Create);
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(int));
                moneypool = 1000;
                js.WriteObject(banker, moneypool);
            }
            lb_Balance.Content = "$" + moneypool;
            lbl_Money.Content = "$" + bet;
            
            Menu.PlayLooping();
        }
        private void Shuffle() {
            Random r = new Random();
            LinkedList<Card> a = new LinkedList<Card>();
            for (int i = 0; i < Deck.Length; i++) {
                a.AddLast(Deck[i]);
                Deck[i].Conceal();
            }
            Card[] temp = new Card[52];
            int index = 0;
            while (a.Any()) {
                int x = (int)(r.NextDouble() * a.Count());
                temp[index] = a.ElementAt(x);
                a.Remove(a.ElementAt(x));
                index += 1;
            }
            Deck = temp;
            for (int i = 0; i < Deck.Length; i++) {
                Deck[i].CardPic.Margin = new Thickness(70 + i/2, 0, 0, 0);
                Deck[i].CardPic.SetValue(Canvas.ZIndexProperty, Game.onTop);
                Game.onTop++;
            }
            Deck_Marker = 51;
            System.Reflection.Assembly aa = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream s = aa.GetManifestResourceStream("Blackjack01.Resources.shuffe.wav");
            SoundPlayer player = new SoundPlayer(s);
            player.Play();
        }
        private void btn_Hit_Click(object sender, RoutedEventArgs e)
        {
            btn_Add.Visibility = Visibility.Hidden;
            btn_Subtract.Visibility = Visibility.Hidden;
            hands[currenthand].bet += bet;
            bet = 0;
            lbl_Money.Content = "$0";
            btn_DblDown.Visibility = Visibility.Hidden;
            btn_Split.Visibility = Visibility.Hidden;
            dealCard(hands[currenthand], true);
            CheckAces(hands[currenthand]);
            if (PlayerBust(hands[currenthand]) || hands[currenthand].handvalue() == 21) { Stay(null, null); }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Deal(object sender, RoutedEventArgs e) {
            btn_Deal.Visibility = Visibility.Hidden;
            hands[currenthand] = new Hand(currenthand, WinStatus[currenthand], bet);
            DealerH = new Hand(-1, WinStatus[4], 0);
            dealCard(DealerH, false);
            dealCard(DealerH, true);
            dealCard(hands[currenthand], true);
            dealCard(hands[currenthand], true);
            btn_Hit.Visibility = Visibility.Visible;
            btn_Stay.Visibility = Visibility.Visible;
            btn_DblDown.Visibility = Visibility.Visible;
            //btn_Subtract.Visibility = Visibility.Hidden;
            //btn_Add.Visibility = Visibility.Hidden;
            bet = 0;
            lbl_Money.Content = "$0";
            //Determine if split available
            Card[] pair = hands[currenthand].cards.ToArray();
            if (pair[0].value == pair[1].value || pair[0].name().Substring(0, 1).Equals(pair[1].name().Substring(0, 1))) {
                //first condition is same value, second is a pair
                btn_Split.Visibility = Visibility.Visible;
            }
            if (DealerH.handvalue() == 21) {
                Stay(null, null);
            }
            btn_Split.Visibility = Visibility.Visible; //LOL DELETE THIS
            //Check for blackjack//
        }
        private void Stay(object sender, RoutedEventArgs e)
        {
            btn_Add.Visibility = Visibility.Hidden;
            btn_Subtract.Visibility = Visibility.Hidden;
            hands[currenthand].bet += bet;
            bet = 0;
            lbl_Money.Content = "$0";
            if (currenthand + 1 == 4) {
                //shutdown all buttons
                btn_Stay.Visibility = Visibility.Hidden;
                btn_DblDown.Visibility = Visibility.Hidden;
                btn_Split.Visibility = Visibility.Hidden;
                btn_Hit.Visibility = Visibility.Hidden;
                DealerTurn();
                return;
            }
            if (hands[currenthand + 1] == null)
            {
                //shutdown all buttons
                btn_Stay.Visibility = Visibility.Hidden;
                btn_DblDown.Visibility = Visibility.Hidden;
                btn_Split.Visibility = Visibility.Hidden;
                btn_Hit.Visibility = Visibility.Hidden;
                DealerTurn();
            }
            else
            {
                currenthand += 1;
                btn_Hit_Click(null, null);
                btn_DblDown.Visibility = Visibility.Visible;
                Card[] pair = hands[currenthand].cards.ToArray();
                if (pair[0].value == pair[1].value || pair[0].name().Substring(0, 1).Equals(pair[1].name().Substring(0, 1)))
                {
                    //first condition is same value, second is a pair
                    btn_Split.Visibility = Visibility.Visible;
                }
                //start from where bets were just placed for this hand
            }
        }

        private void DblDown(object sender, RoutedEventArgs e)
        {
            btn_Add.Visibility = Visibility.Hidden;
            btn_Subtract.Visibility = Visibility.Hidden;
            hands[currenthand].bet += bet;
            bet = 0;
            lbl_Money.Content = "$0";
            hands[currenthand].bet = hands[currenthand].bet * 2;
            moneypool -= hands[currenthand].bet;
            btn_Hit_Click(null, null);
            CheckAces(hands[currenthand]);
            if (!PlayerBust(hands[currenthand]))
            {
                Stay(null, null);
            }
        }
        private void Split(object sender, RoutedEventArgs e)
        {
            btn_Add.Visibility = Visibility.Hidden;
            btn_Subtract.Visibility = Visibility.Hidden;
            hands[currenthand].bet += bet;
            bet = 0;
            lbl_Money.Content = "$0";
            int temp = 0;
            for (int i = 0; i < hands.Length; i++) {
                if (hands[i] == null) {
                    temp = i;
                    break;
                }
            }
            Card c = hands[currenthand].cards.Last();
            hands[currenthand].cards.Remove(c);
            hands[temp] = new Hand(temp, WinStatus[temp], bet);
            moneypool -= hands[temp].bet;
            hands[temp].AddCard(c);
            btn_Hit_Click(null, null);
            //enable split again if the first hand is capable of splitting
            Card[] pair = hands[currenthand].cards.ToArray();
            if ((pair[0].value == pair[1].value || pair[0].name().Substring(0, 1).Equals(pair[1].name().Substring(0, 1))) && (moneypool - hands[currenthand].bet >= 0) && hands[3] == null)
            {
                //first condition is same value, second is a pair
                btn_Split.Visibility = Visibility.Visible;
            }
            else {
                btn_Split.Visibility = Visibility.Hidden;
            }
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream s = a.GetManifestResourceStream("Blackjack01.Resources.split.wav");
            SoundPlayer player = new SoundPlayer(s);
            player.Play();
            lb_Balance.Content = "$" + moneypool;
        }
        private void DealerTurn()
        {
            //code for reveal hand
            DealerH.cards.ElementAt(0).Reveal();
            bool allbust = true;
            for (int i = 0; i < hands.Length; i++) {
                if (hands[i] != null && !PlayerBust(hands[i]))
                {
                    allbust = false;
                }
                
            }
            //code of Algorithm of dealer's turn
            while (DealerH.handvalue() < 17 || (DealerH.handvalue() == 17 && DealerH.getAces().Length != 0) && !allbust) {
                if (DealerH.handvalue() < 17)
                {
                    //hit on dealer
                    dealCard(DealerH, true);
                    CheckAces(DealerH);
                }
                else if (DealerH.handvalue() == 17 && DealerH.getAces().Length != 0)
                {
                    Card[] aces = DealerH.getAces();
                    
                    for (int i = 0; i < aces.Length; i++)
                    {
                        if (aces[i].value == 11)
                        {
                            //hit on dealer
                            dealCard(DealerH, true);
                            CheckAces(DealerH);
                            allbust = false; //safety condition to prevent infinite loop under special circumstances
                            break;
                        }
                        else {
                            allbust = true;
                        }
                    }
                }
                else
                {
                    //stay
                }
            }
            ///////////////////////////////////
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream s = a.GetManifestResourceStream("Blackjack01.Resources.winnv.wav");
            SoundPlayer player = new SoundPlayer(s);
            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i] != null)
                {
                    if (hands[i].handvalue() > 21)
                    {
                        //moneypool -= hands[currenthand].bet;
                        hands[i].SetStatus("Player busted");
                    }
                    else if (DealerH.handvalue() > 21)
                    {
                        moneypool += hands[currenthand].bet * 2;
                        hands[i].SetStatus("Dealer Busts!");
                        player.Play();
                    }
                    else if (DealerH.handvalue() > hands[i].handvalue())
                    {
                        //moneypool -= hands[currenthand].bet;
                        hands[i].SetStatus("Player Loses!");
                    }
                    else if (DealerH.handvalue() < hands[i].handvalue())
                    {
                        moneypool += hands[currenthand].bet * 2;
                        hands[i].SetStatus("Player Wins!");
                        player.Play();
                    }
                    else if (DealerH.handvalue() == hands[i].handvalue()) {
                        moneypool += hands[currenthand].bet;
                        hands[i].SetStatus("Push!");
                    }
                    
                }
            }
            FileStream banker = new FileStream("bank.txt", FileMode.Create);
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(int));
            js.WriteObject(banker, moneypool);
            lb_Balance.Content = "$" + moneypool;
            lbl_Money.Content = "$" + bet;
            banker.Close();
            btn_Reset.Visibility = Visibility.Visible;
        }
        private void dealCard(Hand hand, bool faceUP)
        {
            //
            hand.AddCard(Deck[Deck_Marker]);
            if (faceUP)
            {
                Deck[Deck_Marker].Reveal();
            }
            else {
                Deck[Deck_Marker].Conceal();
            }
            Deck_Marker--;
        }
        private void Reset(object sender, RoutedEventArgs e)
        {
            Menu.PlayLooping();
            btn_Hit.Visibility = Visibility.Hidden;
            btn_Stay.Visibility = Visibility.Hidden;
            btn_Split.Visibility = Visibility.Hidden;
            btn_DblDown.Visibility = Visibility.Hidden;
            btn_Deal.Visibility = Visibility.Visible;
            btn_Reset.Visibility = Visibility.Hidden;
            Label1.Content = "";
            Label2.Content = "";
            Label3.Content = "";
            Label4.Content = "";
            Label5.Content = "";
            currenthand = 0;
            for (int i = 0; i < hands.Length; i++) {
                hands[i] = null;
            }
            for (int i = Deck_Marker; i < Deck.Length; i++) {
                Deck[i].CardPic.Margin = new Thickness(370 - (i - Deck_Marker) / 2, 0, 0, 0);
            }
            if (Deck_Marker < 12) {
                Shuffle();
            }
            lbl_Money.Content = "$0";
            bet = 0;
            btn_Add.Visibility = Visibility.Visible;
            btn_Subtract.Visibility = Visibility.Visible;
        }

        private bool PlayerBust(Hand hand)
        {
            CheckAces(hand);
            if (hands[currenthand].handvalue() > 21)
            {
                //Stay(null, null);
                return true;
            }
            else if (hands[currenthand].handvalue() == 21)
            {
                //Stay(null, null);
                return false;
            }
            else
            {
                return false;
            }

        }
        private void CheckAces(Hand hand)
        {
            if (hand.handvalue() > 21)
            {
                Card[] aces = hand.getAces();
                for (int i = 0; i < aces.Length; i++)
                {
                    if (aces[i].value == 11)
                    {
                        aces[i].value = 1;
                        break;
                    }
                }
            }
        }

        private void btn_Quit_Click(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream s = a.GetManifestResourceStream("Blackjack01.Resources.quit.wav");
            SoundPlayer player = new SoundPlayer(s);
            player.Play();
            //Close this window
            this.Close();
        }

        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {

            if (moneypool == 0) { }
            else if (moneypool < 10)
            {
                bet += moneypool;
                moneypool = 0;
            }
            else {
                bet += 10;
                moneypool -= 10;
            }
            lbl_Money.Content = "$" + bet;
            lb_Balance.Content = "$" + moneypool;
        }

        private void btn_Subtract_Click(object sender, RoutedEventArgs e)
        {
            if (bet == 0) { }
            else if (bet < 10)
            {
                moneypool += bet;
                bet = 0;
            }
            else {
                moneypool += 10;
                bet -= 10;
            }
            lbl_Money.Content = "$" + bet;
            lb_Balance.Content = "$" + moneypool;
        }
    }
}
