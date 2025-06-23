using Wpf.Ui.Common.Interfaces;

namespace WorkTools.Views.Pages
{
    /// <summary>
    /// RecipePage.xaml 的交互逻辑
    /// </summary>
    public partial class RecipePage : INavigableView<ViewModels.RecipeViewModel>
    {
        public ViewModels.RecipeViewModel ViewModel
        {
            get;
        }

        public RecipePage(ViewModels.RecipeViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }

        private void BtnExcute_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
