using Xamarin.Forms;

namespace CardStackTest
{
	public partial class CardStackTestPage : ContentPage
	{
		CardStackView cardStack;
		MainPageViewModel viewModel = new MainPageViewModel();

		public CardStackTestPage()
		{
			InitializeComponent();
			this.BindingContext = viewModel;
			this.BackgroundColor = Color.Black;

			RelativeLayout view = new RelativeLayout();

			cardStack = new CardStackView();
			cardStack.SetBinding(CardStackView.ItemsSourceProperty, "ItemsList");
			cardStack.SwipedLeft += SwipedLeft;
			cardStack.SwipedRight += SwipedRight;

			view.Children.Add(cardStack,
							  Constraint.Constant(30),
							  Constraint.Constant(60),
							  Constraint.RelativeToParent((parent) =>
							  {
								  return parent.Width - 60;
							  }),
							  Constraint.RelativeToParent((parent) =>
							  {	
								  return parent.Height - 140;
						}));

			this.LayoutChanged += (object sender, System.EventArgs e) =>
			{
				cardStack.CardMoveDistance = (int)(this.Width * 0.60f);
			};

			this.Content = view;
		}

		void SwipedLeft(int index)
		{
			DisplayAlert("TEST", "Elemento rechazado", "ok", "ko");
		}

		void SwipedRight(int index)
		{
			DisplayAlert("TEST", "Elemento agregado", "ok", "ko");
		}
	}
}
