using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Chest
{
    class Program
    {
        static void Main(string[] args)
        {

            List<string> Players = new List<string>() { "Максим", "Мария" };
            CardGame Game = new CardGame(Players);

            Game.StartGame(5);


            Console.ReadKey();
        }
    }

    class CardGame
    {
        private Random firstturn = new Random();
        private CardDeck _Deck = new CardDeck() ;
        private List<Player> Players = new List<Player>();
        private int TurnCounter = 0;
        private int CurrentTurn = 0;

        public CardGame( List<string> players )
        {
            foreach( string player in players)
            {
                this.Players.Add(new Player(player));
            }

            Console.WriteLine(" Итак, Игроки в сборе\n");
            Thread.Sleep(2000);
        }
        
        public void StartGame( int CardsOnHands  )
        {
            //if ( TurnCounter == Players.Count ) { this.TurnCounter -= Players.Count; }

            foreach ( Player player in Players)
            {
                player.SetPersonalDeck( _Deck, CardsOnHands);
                player.CheckChest(player.name);
            }

            Console.WriteLine(" Карты разданы, игра начинается!\n");
            Thread.Sleep(3000);

            Console.Clear();

            while ( _Deck._acutalDeck.Count > 0 )
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" При выборе карты выбирайте номер карты, а не саму карту !!!\n");
                Console.ResetColor();

                this.CurrentTurn = (this.TurnCounter > 0) ? this.CurrentTurn : FirstTurn();
                int SecondPlayer = (CurrentTurn + 1) % Players.Count;
                /*
                 Было
                int SecondPlayer = (this.CurrentTurn == (Players.Count-1)) 
                ? this.CurrentTurn - (Players.Count-1) 
                : this.CurrentTurn + 1;
                 
                 */

                Console.WriteLine($" Ходит Игрок '{Players[CurrentTurn].name}'\n");
                Thread.Sleep(3000);

                string card = Players[CurrentTurn].PlayerTurn(Players[CurrentTurn]);

                // Если ход пропущен (рука пуста) — просто передаём ход дальше
                if (card != null)
                {
                    Players[CurrentTurn].UpdateDecks(_Deck, card, Players[CurrentTurn], Players[SecondPlayer]);
                    Players[CurrentTurn].CheckChest(Players[CurrentTurn].name);
                }

                

                // Добор карт для обоих игроков, а не только для второго,
                // чтобы никто не остался с пустой рукой
                Players[CurrentTurn].CheckIfDeckZero(_Deck);
                Players[SecondPlayer].CheckIfDeckZero(_Deck);

                this.CurrentTurn = SecondPlayer;
                TurnCounter++;

                Console.Clear();
            }

            Console.WriteLine(" Игра окончена!\n");
            Console.WriteLine($" Всего было сделано ходов '{TurnCounter}'!\n");
            Thread.Sleep(1000);
            Results();
        }

        public int FirstTurn()
        {
            int turn = firstturn.Next(0, Players.Count);
            return turn;
        }

        public void Results()
        {
            List<int> Chests = new List<int>();

            foreach ( Player player in Players)
            {
                Chests.Add(player._CollectedChests);
            }

            if ( Chests.Sum() == Chests.Max() * Players.Count)
            {
                Console.WriteLine($" Игра завершается ничьей, все собрали сундучков '{Chests.Max()}'!");
            }
            else
            {
                Console.WriteLine(" Больше всего очков набрал");
                foreach( Player player in Players)
                {
                    if ( player._CollectedChests == Chests.Max())
                    {
                        Console.WriteLine($" Игрок {player.name}");
                    }
                }
            }

        }

    }

    class Player
    {
        public List<string> _PlayableDeck { get; private set; }
        public int _CollectedChests { get; private set; }
        public string name;

        public Player( string name )
        {
            this.name = name;
            _PlayableDeck = new List<string>();
        }
        
        // ход игрока
        public string PlayerTurn( Player player )
        {

            if (_PlayableDeck.Count == 0)
            {
                Console.WriteLine($" {player.name}, у вас нет карт. Пропуск хода.\n");
                Thread.Sleep(2000);
                return null;
            }

            Console.WriteLine($" {player.name}, ваш ход!\n Ваша карта...\n" );
            Thread.Sleep(1000);

            Console.ForegroundColor = ConsoleColor.Green;

            foreach ( string card in _PlayableDeck)
            {
                Console.Write(" " + card.Substring(0, card.IndexOf("_")));
            }
            Console.ResetColor();

            Console.WriteLine();

            // Проверка на корректность введенного номера
            while (true)
            {
                try
                {

                    Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop + 1);

                    int number = Int32.Parse(Console.ReadLine());
                    Thread.Sleep(1000);
                    Console.WriteLine($" Ваша карта '{_PlayableDeck[number-1].Split('_')[0]}'\n");
                    return _PlayableDeck[number - 1];
                }
                catch
                {
                    Console.WriteLine(" Такой карты нет в вашей колоде!\n");
                    continue;
                }
            }
        }

        
        public void SetPersonalDeck( CardDeck Deck, int CardsCount )
        {
            for ( int i = 0; i < CardsCount; i++)
            {
                _PlayableDeck.Add(Deck._acutalDeck.Dequeue());
            }
        }

        
        public void UpdateDecks( CardDeck Deck, string card, Player asking, Player answering )
        {
            if ( answering._PlayableDeck.Select( element => element.Substring( 0, element.IndexOf("_") ) ).Contains( card.Substring(0, card.IndexOf("_") ) ))
            {

                Console.WriteLine($" У игрока '{answering.name}' есть такая карта!\n Нажмите любую клавишу, чтобы взять карту у соперника\n");
                Console.ReadKey();

                asking._PlayableDeck.AddRange(answering._PlayableDeck.Where(element => element.Substring(0, element.IndexOf("_")) == card.Substring(0, element.IndexOf("_"))));

                answering._PlayableDeck.RemoveAll(element => element.Substring(0, element.IndexOf("_")) == card.Substring(0, card.IndexOf("_")));

                Console.WriteLine();

            }
            else
            {
                if (Deck._acutalDeck.Count > 0)
                {
                    Console.WriteLine($" У игрока '{answering.name}' нет такой карты\n Нажмите любую клавишу, чтобы взять карту из колоды, {asking.name}\n");
                    
                    //Console.WriteLine();
                    //answering.CheckPlayer(Deck);

                    Console.ReadKey();

                    asking._PlayableDeck.Add(Deck._acutalDeck.Dequeue());
                }
            }
        }

        public int CheckChest( string name )
        {
            List<string> Exps = new List<string>() ;
            List<string> Chests = _PlayableDeck

                .GroupBy(s => s.Substring(0, s.IndexOf("_")))  // Группируем по первой части до подчеркивания
                .Where(g => g.Count() == 4)  // Выбираем только те, которые встречаются 4 раза
                .Select(g => g.Key )  // Выбираем сами значения
                .ToList();

            foreach ( string word in Chests)
            {
                _PlayableDeck = _PlayableDeck
                .Where(s => s.Substring(0, s.IndexOf("_")) != word)
                .ToList();

                // Было _PlayableDeck = _PlayableDeck.Where(s => !s.Contains(word)).ToList(); 
            }

            if (Chests.Count > 0)
            {
                Console.WriteLine($" {name}, вы собрали новый сундучок!\n Нажмите любую клавишу для продолжения\n");
                Console.ReadKey();
            } 
            _CollectedChests += Chests.Count;
            return Chests.Count;
            
        }

        public void CheckIfDeckZero( CardDeck Deck )
        {
            if ( this._PlayableDeck.Count == 0  &&  Deck._acutalDeck.Count != 0 )
            {
                this._PlayableDeck.Add(Deck._acutalDeck.Dequeue());
            }   
        }


        public void CheckPlayer(CardDeck Deck)
        {
            foreach (string item in _PlayableDeck)
            {
                Console.WriteLine(item);
            }
        }

        public void CheckDeck( CardDeck Deck )
        {
            foreach (string item in Deck._acutalDeck)
            {
                Console.WriteLine(item);
            }
        }

    }

    // Класс отвечающий за карточную колоду

    class CardDeck
    {
        // Поля, рандом, список дефолтных карт и реальная колода
        private Random rand = new Random();
        private List<string> _DefaultCards;
        private Queue<string> _AcutalDeck ;

        // Свойство задающее логику для играбельной колоды.
        public Queue<string> _acutalDeck
        {
            get { return _AcutalDeck ; }
            private set { _AcutalDeck = value; }
        }

        // public string NextItem => _AcutalDeck.Dequeue();


        public CardDeck()
        {
            // Дефолтная колода
            _DefaultCards = new List<string> {

            "2_hearts","2_diamonds","2_Crosses","2_Spades",
            "3_hearts","3_diamonds","3_Crosses","3_Spades",
            "4_hearts","4_diamonds","4_Crosses","4_Spades",
            "5_hearts","5_diamonds","5_Crosses","5_Spades",
            "6_hearts","6_diamonds","6_Crosses","6_Spades",
            "7_hearts","7_diamonds","7_Crosses","7_Spades",
            "8_hearts","8_diamonds","8_Crosses","8_Spades",
            "9_hearts","9_diamonds","9_Crosses","9_Spades",
            "10_hearts","10_diamonds","10_Crosses","10_Spades",
            "Jack_hearts","Jack_diamonds","Jack_Crosses","Jack_Spades",
            "Queen_hearts","Queen_diamonds","Queen_Crosses","Queen_Spades",
            "King_hearts","King_diamonds","King_Crosses","King_Spades",
            "Ace_hearts","Ace_diamonds","Ace_Crosses","Ace_Spades",
            
             };

            // Играбельная колода
            _acutalDeck = new Queue<string>(_DefaultCards);

            // При создании экземпляра колоды, карты в ней автоматически перемешиваются
            MixCardsDeck();

        }
        
        
        private void MixCardsDeck()
        {
            // метод, рандомно мешающий карты
            string temp;
            for ( int i = 0; i < _DefaultCards.Count; i++)
            {
                temp = _DefaultCards[i];
                int randomIndex = rand.Next(0, _DefaultCards.Count);
                _DefaultCards[i] = _DefaultCards[randomIndex];
                _DefaultCards[randomIndex] = temp;
            }
            _acutalDeck = new Queue<string>(_DefaultCards);
        }
        
        public void Check()
        {
            foreach ( string item in _AcutalDeck)
            {
                Console.WriteLine(item);
            }
        }
      
    }
}
