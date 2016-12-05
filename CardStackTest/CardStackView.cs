using System;
using Xamarin.Forms;
using System.Collections.Generic;
namespace CardStackTest
{
	public class CardStackView : ContentView
	{
		public class Item
		{
			public string Name { get; set; }
			public string Photo { get; set; }
			public string Location { get; set; }
			public string Description { get; set; }
		}

		const float BackCardScale = 0.8f;
		const int AnimLength = 250;
		const float DegreesToRadians = 57.2957795f;
		const float CardRotationAdjuster = 0.3f;

		public int CardMoveDistance { get; set; }

		const int NumCards = 2;
		CardView[] cards = new CardView[NumCards];

		int topCardIndex;

		float cardDistance = 0;

		int itemIndex = 0;

		bool ignoreTouch = false;

		public Action<int> SwipedRight = null;
		public Action<int> SwipedLeft = null;

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(System.Collections.IList), typeof(CardStackView), null, propertyChanged: OnItemsSourcePropertyChanged);

		public List<Item> ItemsSource
		{
			get
			{
				return (List<Item>)GetValue(ItemsSourceProperty);
			}
			set
			{
				SetValue(ItemsSourceProperty, value);
				itemIndex = 0;
			}
		}

		private static void OnItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((CardStackView)bindable).Setup();
		}

		public CardStackView()
		{
			RelativeLayout view = new RelativeLayout();
			for (int i = 0; i < NumCards; i++)
			{
				var card = new CardView();
				cards[i] = card;
				card.InputTransparent = true;
				card.IsVisible = false;

				view.Children.Add(
					card,
					Constraint.Constant(0),
					Constraint.Constant(0),
					Constraint.RelativeToParent((parent) =>
				{
					return parent.Width;
				}),
					Constraint.RelativeToParent((parent) =>
				{
					return parent.Height;
				})
				);
			}
			this.BackgroundColor = Color.Black;
			this.Content = view;

			var panGesture = new PanGestureRecognizer();
			panGesture.PanUpdated += OnPanUpdated;
			GestureRecognizers.Add(panGesture);
			}

		void Setup()
		{
			topCardIndex = 0;
			for (int i = 0; i < Math.Min(NumCards, ItemsSource.Count); i++)
			{
				if (itemIndex >= ItemsSource.Count) break;
				var card = cards[i];
				card.Name.Text = ItemsSource[itemIndex].Name;
				card.Location.Text = ItemsSource[itemIndex].Location;
				card.Description.Text = ItemsSource[itemIndex].Description;
				card.Photo.Source = ImageSource.FromFile(ItemsSource[itemIndex].Photo);
				card.IsVisible = true;
				card.Scale = GetScale(i);
				card.RotateTo(0, 0);
				card.TranslateTo(0, -card.Y, 0);
				((RelativeLayout)this.Content).LowerChild(card);
				itemIndex++;
			}
		}

		void OnPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			switch (e.StatusType)
			{
				case GestureStatus.Started:
					HandleTouchStart();
					break;
				case GestureStatus.Running:
					HandleTouch((float)e.TotalX);
					break;
				case GestureStatus.Completed:
					HandleTouchEnd();
					break;
			}
		}

		public void HandleTouchStart()
		{
			cardDistance = 0;
		}

		public void HandleTouch(float diff_x)
		{
			if (ignoreTouch) return;

			var topCard = cards[topCardIndex];
			var backCard = cards[PrevCardIndex(topCardIndex)];

			if (topCard.IsVisible)
			{
				topCard.TranslationX = (diff_x);
				float rotationAngle = (float)(CardRotationAdjuster * Math.Min(diff_x / this.Width, 1.0f));
				topCard.Rotation = rotationAngle * DegreesToRadians;
				cardDistance = diff_x;
			}
			if (backCard.IsVisible)
				backCard.Scale = Math.Min(BackCardScale + Math.Abs((cardDistance / CardMoveDistance) * (1.0f - BackCardScale)), 1.0f);
		}

		public async void HandleTouchEnd()
		{
			ignoreTouch = true;
			var topCard = cards[topCardIndex];

			if (Math.Abs((int)cardDistance) > CardMoveDistance)
			{
				await topCard.TranslateTo(cardDistance > 0 ? this.Width : -this.Width, 0, AnimLength / 2, Easing.SpringOut);
				topCard.IsVisible = false;
				if (SwipedRight != null && cardDistance > 0)
				{
					SwipedRight(itemIndex);
				}
				else if (SwipedLeft != null)
				{
					SwipedLeft(itemIndex);
				}

				ShowNextCard();
			}
			else
			{
				topCard.TranslateTo((-topCard.X), -topCard.Y, AnimLength, Easing.SpringOut);
				topCard.RotateTo(0, AnimLength, Easing.SpringOut);
			}
			ignoreTouch = false;
		}

		void ShowNextCard()
		{
			if (!cards[0].IsVisible && !cards[1].IsVisible)
			{
				Setup();
				return;
			}

			var topCard = cards[topCardIndex];
			topCardIndex = NextCardIndex(topCardIndex);

			if (itemIndex < ItemsSource.Count)
			{
				((RelativeLayout)this.Content).LowerChild(topCard);

				topCard.Scale = BackCardScale;
				topCard.RotateTo(0, 0);
				topCard.TranslateTo(0, -topCard.Y, 0);

				topCard.Name.Text = ItemsSource[topCardIndex].Name;
				topCard.Location.Text = ItemsSource[topCardIndex].Location;
				topCard.Description.Text = ItemsSource[topCardIndex].Description;
				topCard.Photo.Source = ImageSource.FromFile(ItemsSource[topCardIndex].Photo);

				topCard.IsVisible = true;
				itemIndex++;
			}
		}

		int NextCardIndex(int topIndex)
		{
			return topIndex == 0 ? 1 : 0;
		}

		int PrevCardIndex(int topIndex)
		{
			return topIndex > 0 ? topIndex-- : NumCards-1;
		}

		float GetScale(int index)
		{
			return (index == topCardIndex) ? 1.0f : BackCardScale;
		}
						
	}
}
