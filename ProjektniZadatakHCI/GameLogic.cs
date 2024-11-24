using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektniZadatakHCI
{
   public class GameLogic
{
    private List<CardModel> _cards;
    private CardModel _firstCard;
    private CardModel _secondCard;
    public event Action GameWon;
    public event Action CardsUpdated;

    public List<CardModel> InitializeGame(int numberOfPairs)
    {
        _cards = new List<CardModel>();

       
        for (int i = 0; i < numberOfPairs; i++)
        {
            _cards.Add(new CardModel { Id = i, ImagePath = $"C:\\Users\\pc\\Desktop\\picturesOfCars\\image{i+1}.png", IsMatched = false });
            _cards.Add(new CardModel { Id = i, ImagePath = $"C:\\Users\\pc\\Desktop\\picturesOfCars\\image{i+1}.png", IsMatched = false });
        }

        _cards = _cards.OrderBy(x => Guid.NewGuid()).ToList();

        return _cards;
    }

        public async void FlipCard(CardModel card)
        {
            if (_firstCard == null)
            {
                _firstCard = card;
                card.IsFlipped = true;

               
                CardsUpdated?.Invoke();
            }
            else if (_secondCard == null)
            {
                _secondCard = card;
                card.IsFlipped = true;

              
                CardsUpdated?.Invoke();

                
                if (_firstCard.Id == _secondCard.Id)
                {
                    _firstCard.IsMatched = true;
                    _secondCard.IsMatched = true;
                    ResetSelection();
                }
                else
                {
                   
                    await Task.Delay(300);

                    _firstCard.IsFlipped = false;
                    _secondCard.IsFlipped = false;
                    ResetSelection();
                    CardsUpdated?.Invoke();
                }
            }
            if (_cards.All(c => c.IsMatched))
            {
                GameWon?.Invoke();
            }
        }

        private void ResetSelection()
    {
        _firstCard = null;
        _secondCard = null;
    }
}

}
