using BabelRushR.Core.Card;
using BabelRushR.Mvvm.ViewModels;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Mvvm.Test;

// public class CardPileViewModelTests
// {
//     private static NotifiableList<ICard> NewHand(params ICard[] cards)
//     {
//         var hand = new NotifiableList<ICard>();
//         hand.AddRange(cards);
//         return hand;
//     }
//
//     private static int[] Costs(CardPileViewModel viewModel) => [..viewModel.Cards.Select(card => card.Cost)];
//
//     [Fact]
//     public void Picks_Up_Cards_Already_In_The_Hand()
//     {
//         var hand = NewHand(TestCards.New(1), TestCards.New(2), TestCards.New(3));
//
//         var viewModel = new CardPileViewModel(hand);
//
//         Assert.Equal(new[] { 1, 2, 3 }, Costs(viewModel));
//     }
//
//     [Fact]
//     public void Adding_Cards_Creates_ViewModels_In_Order()
//     {
//         var hand = NewHand(TestCards.New(1));
//         var viewModel = new CardPileViewModel(hand);
//
//         hand.Add(TestCards.New(2));
//         hand.Add(TestCards.New(3));
//
//         Assert.Equal(new[] { 1, 2, 3 }, Costs(viewModel));
//     }
//
//     [Fact]
//     public void Batch_Adding_Cards_Creates_ViewModels_In_Order()
//     {
//         var hand = NewHand(TestCards.New(1));
//         var viewModel = new CardPileViewModel(hand);
//
//         hand.AddRange(new List<ICard> { TestCards.New(4), TestCards.New(5), TestCards.New(6) });
//
//         Assert.Equal(new[] { 1, 4, 5, 6 }, Costs(viewModel));
//     }
//
//     [Fact]
//     public void Adding_A_Card_Keeps_Existing_ViewModels()
//     {
//         var hand = NewHand(TestCards.New(1));
//         var viewModel = new CardPileViewModel(hand);
//         var first = viewModel.Cards[0];
//
//         hand.Add(TestCards.New(2));
//
//         Assert.Same(first, viewModel.Cards[0]);
//     }
//
//     [Fact]
//     public void Removing_A_Card_Removes_And_Disposes_Its_ViewModel()
//     {
//         var removed = TestCards.New(1);
//         var hand = NewHand(removed, TestCards.New(2));
//         var viewModel = new CardPileViewModel(hand);
//         var removedViewModel = viewModel.Cards[0];
//         var notified = NotificationRecorder.PropertyNames(removedViewModel);
//
//         hand.Remove(removed);
//         TestCards.RaiseCost(removed, 2);
//
//         Assert.Equal(new[] { 2 }, Costs(viewModel));
//         Assert.Empty(notified);
//     }
//
//     [Fact]
//     public void Moving_A_Card_Keeps_The_Same_ViewModels()
//     {
//         var hand = NewHand(TestCards.New(1), TestCards.New(2), TestCards.New(3));
//         var viewModel = new CardPileViewModel(hand);
//         var first = viewModel.Cards[0];
//         var second = viewModel.Cards[1];
//         var third = viewModel.Cards[2];
//
//         hand.Move(0, 2);
//
//         Assert.Same(second, viewModel.Cards[0]);
//         Assert.Same(third, viewModel.Cards[1]);
//         Assert.Same(first, viewModel.Cards[2]);
//     }
//
//     [Fact]
//     public void Replacing_A_Card_Swaps_And_Disposes_The_Old_ViewModel()
//     {
//         var oldCard = TestCards.New(1);
//         var hand = NewHand(oldCard);
//         var viewModel = new CardPileViewModel(hand);
//         var oldViewModel = viewModel.Cards[0];
//         var notified = NotificationRecorder.PropertyNames(oldViewModel);
//
//         hand[0] = TestCards.New(5);
//         TestCards.RaiseCost(oldCard, 2);
//
//         Assert.Equal(new[] { 5 }, Costs(viewModel));
//         Assert.Empty(notified);
//     }
//
//     [Fact]
//     public void Clearing_The_Hand_Removes_And_Disposes_Every_ViewModel()
//     {
//         var first = TestCards.New(1);
//         var second = TestCards.New(2);
//         var hand = NewHand(first, second);
//         var viewModel = new CardPileViewModel(hand);
//         var firstNotified = NotificationRecorder.PropertyNames(viewModel.Cards[0]);
//         var secondNotified = NotificationRecorder.PropertyNames(viewModel.Cards[1]);
//
//         hand.Clear();
//         TestCards.RaiseCost(first, 1);
//         TestCards.RaiseCost(second, 1);
//
//         Assert.Empty(viewModel.Cards);
//         Assert.Empty(firstNotified);
//         Assert.Empty(secondNotified);
//     }
//
//     [Fact]
//     public void Card_Cost_Change_Flows_Through_The_ViewModel()
//     {
//         var card = TestCards.New(1);
//         var viewModel = new CardPileViewModel(NewHand(card));
//         var cardViewModel = viewModel.Cards[0];
//         var notified = NotificationRecorder.PropertyNames(cardViewModel);
//
//         TestCards.RaiseCost(card, 2);
//
//         Assert.Equal(3, cardViewModel.Cost);
//         Assert.Contains(nameof(CardViewModel.Cost), notified);
//     }
//
//     [Fact]
//     public void Disposing_Stops_Tracking_The_Hand_And_Disposes_Cards()
//     {
//         var card = TestCards.New(1);
//         var hand = NewHand(card);
//         var viewModel = new CardPileViewModel(hand);
//         var cardViewModel = viewModel.Cards[0];
//         var notified = NotificationRecorder.PropertyNames(cardViewModel);
//
//         viewModel.Dispose();
//         hand.Add(TestCards.New(2));
//         TestCards.RaiseCost(card, 1);
//
//         Assert.Empty(viewModel.Cards);
//         Assert.Empty(notified);
//     }
// }
